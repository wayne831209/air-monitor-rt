using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeviceBox
{
    /// <summary>
    /// Microsoft Teams 通知服務
    /// 用於發送警報訊息到 Teams 頻道
    /// </summary>
    public class TeamsNotificationService
    {
        private readonly string _webhookUrl;
        private readonly string[] _notificationEmails; // 支援多組 Email
        private readonly HttpClient _httpClient;
        private static readonly object _lockObj = new object();
        private static DateTime _lastNotificationTime = DateTime.MinValue;
        private readonly TimeSpan _notificationCooldown; // 推播間隔時間（可動態設定）
        private readonly bool _usePowerAutomate; // true: Power Automate, false: Teams Incoming Webhook

        public TeamsNotificationService(string webhookUrl, string notificationEmail = "", int cooldownMinutes = 5)
        {
            _webhookUrl = webhookUrl;
            _notificationCooldown = TimeSpan.FromMinutes(cooldownMinutes);

            // 解析 Email 字串（支援逗號、分號分隔）
            _notificationEmails = ParseEmailAddresses(notificationEmail);

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            // 自動偵測 webhook 類型
            _usePowerAutomate = webhookUrl?.Contains("powerplatform.com") ?? false;

            System.Diagnostics.Debug.WriteLine($"[Teams通知] 使用 {(_usePowerAutomate ? "Power Automate" : "Teams Incoming Webhook")} 模式");
            System.Diagnostics.Debug.WriteLine($"[Teams通知] 推播間隔時間: {cooldownMinutes} 分鐘");
            if (_notificationEmails.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 通知聯絡人數量: {_notificationEmails.Length}");
                foreach (var email in _notificationEmails)
                {
                    System.Diagnostics.Debug.WriteLine($"[Teams通知]   - {email}");
                }
            }
        }

        /// <summary>
        /// 解析 Email 地址字串，支援逗號或分號分隔
        /// </summary>
        private string[] ParseEmailAddresses(string emailString)
        {
            if (string.IsNullOrWhiteSpace(emailString))
                return new string[0];

            // 支援逗號或分號分隔
            char[] separators = new[] { ',', ';' };

            return emailString
                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToArray();
        }

        /// <summary>
        /// 發送空壓超限通知到 Teams
        /// </summary>
        public async Task SendPressureAlertAsync(string source, string currentValue, double upperLimit, double lowerLimit)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                System.Diagnostics.Debug.WriteLine("[Teams通知] Webhook URL 未設定");
                return;
            }

            // 檢查冷卻時間，避免頻繁通知
            lock (_lockObj)
            {
                if (DateTime.Now - _lastNotificationTime < _notificationCooldown)
                {
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 冷卻時間內，跳過通知");
                    return;
                }
                _lastNotificationTime = DateTime.Now;
            }

            try
            {
                string title = "⚠️ 空壓超限警報";
                string color = "FF0000"; // 紅色
                var facts = new[]
                {
                    ("設備", source),
                    ("當前數值", currentValue),
                    ("上限", upperLimit == double.MaxValue ? "未設定" : upperLimit.ToString("F2")),
                    ("下限", lowerLimit == double.MinValue ? "未設定" : lowerLimit.ToString("F2")),
                    ("時間", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };

                // 如果有多個 Email，用 for 迴圈分別發送
                if (_notificationEmails.Length > 0)
                {
                    for (int i = 0; i < _notificationEmails.Length; i++)
                    {
                        string email = _notificationEmails[i];
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送給: {email} ({i + 1}/{_notificationEmails.Length})");

                        string message = await SendTeamsNotificationAsync(title, color, facts, email);
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 空壓超限通知已發送給 {email}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[Teams通知] 已完成發送，共 {_notificationEmails.Length} 位收件者");
                }
                else
                {
                    // 沒有 Email 的情況，發送空字串
                    string message = await SendTeamsNotificationAsync(title, color, facts, "");
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 空壓超限通知已發送（無 Email）");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送空壓超限通知失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 發送溫度超限通知到 Teams
        /// </summary>
        public async Task SendTemperatureAlertAsync(string source, string currentValue, double upperLimit, double lowerLimit)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                System.Diagnostics.Debug.WriteLine("[Teams通知] Webhook URL 未設定");
                return;
            }

            // 檢查冷卻時間，避免頻繁通知
            lock (_lockObj)
            {
                if (DateTime.Now - _lastNotificationTime < _notificationCooldown)
                {
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 冷卻時間內，跳過通知");
                    return;
                }
                _lastNotificationTime = DateTime.Now;
            }

            try
            {
                string title = "🌡️ 溫度超限警報";
                string color = "FFA500"; // 橘色
                var facts = new[]
                {
                    ("設備", source),
                    ("當前數值", currentValue),
                    ("上限", upperLimit == double.MaxValue ? "未設定" : upperLimit.ToString("F2")),
                    ("下限", lowerLimit == double.MinValue ? "未設定" : lowerLimit.ToString("F2")),
                    ("時間", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };

                // 如果有多個 Email，用 for 迴圈分別發送
                if (_notificationEmails.Length > 0)
                {
                    for (int i = 0; i < _notificationEmails.Length; i++)
                    {
                        string email = _notificationEmails[i];
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送給: {email} ({i + 1}/{_notificationEmails.Length})");

                        string message = await SendTeamsNotificationAsync(title, color, facts, email);
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 溫度超限通知已發送給 {email}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[Teams通知] 已完成發送，共 {_notificationEmails.Length} 位收件者");
                }
                else
                {
                    // 沒有 Email 的情況，發送空字串
                    string message = await SendTeamsNotificationAsync(title, color, facts, "");
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 溫度超限通知已發送（無 Email）");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送溫度超限通知失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 發送設備警報通知到 Teams
        /// </summary>
        public async Task SendDeviceAlarmAsync(string deviceName, string status)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                System.Diagnostics.Debug.WriteLine("[Teams通知] Webhook URL 未設定");
                return;
            }

            // 檢查冷卻時間，避免頻繁通知
            lock (_lockObj)
            {
                if (DateTime.Now - _lastNotificationTime < _notificationCooldown)
                {
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 冷卻時間內，跳過通知");
                    return;
                }
                _lastNotificationTime = DateTime.Now;
            }

            try
            {
                string title = "⚠️ 設備警報";
                string color = "FFA500"; // 橘色
                var facts = new[]
                {
                    ("設備名稱", deviceName),
                    ("狀態", status),
                    ("時間", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };

                if (_notificationEmails.Length > 0)
                {
                    for (int i = 0; i < _notificationEmails.Length; i++)
                    {
                        string email = _notificationEmails[i];
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送給: {email} ({i + 1}/{_notificationEmails.Length})");

                        string message = await SendTeamsNotificationAsync(title, color, facts, email);
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 設備警報通知已發送給 {email}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[Teams通知] 已完成發送，共 {_notificationEmails.Length} 位收件者");
                }
                else
                {
                    string message = await SendTeamsNotificationAsync(title, color, facts, "");
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 設備警報通知已發送（無 Email）");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送設備警報通知失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 發送設備故障通知到 Teams
        /// </summary>
        public async Task SendDeviceFaultAsync(string deviceName, string status)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                System.Diagnostics.Debug.WriteLine("[Teams通知] Webhook URL 未設定");
                return;
            }

            // 檢查冷卻時間，避免頻繁通知
            lock (_lockObj)
            {
                if (DateTime.Now - _lastNotificationTime < _notificationCooldown)
                {
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 冷卻時間內，跳過通知");
                    return;
                }
                _lastNotificationTime = DateTime.Now;
            }

            try
            {
                string title = "🚨 設備故障";
                string color = "DC143C"; // 深紅色
                var facts = new[]
                {
                    ("設備名稱", deviceName),
                    ("狀態", status),
                    ("時間", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };

                if (_notificationEmails.Length > 0)
                {
                    for (int i = 0; i < _notificationEmails.Length; i++)
                    {
                        string email = _notificationEmails[i];
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送給: {email} ({i + 1}/{_notificationEmails.Length})");

                        string message = await SendTeamsNotificationAsync(title, color, facts, email);
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 設備故障通知已發送給 {email}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[Teams通知] 已完成發送，共 {_notificationEmails.Length} 位收件者");
                }
                else
                {
                    string message = await SendTeamsNotificationAsync(title, color, facts, "");
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 設備故障通知已發送（無 Email）");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送設備故障通知失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 發送合併的異常警報到 Teams
        /// </summary>
        public async Task SendCombinedAbnormalAlertAsync(List<string> abnormalMessages)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                System.Diagnostics.Debug.WriteLine("[Teams通知] Webhook URL 未設定");
                return;
            }

            if (abnormalMessages == null || abnormalMessages.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[Teams通知] 沒有異常訊息需要發送");
                return;
            }

            // 檢查冷卻時間，避免頻繁通知
            lock (_lockObj)
            {
                if (DateTime.Now - _lastNotificationTime < _notificationCooldown)
                {
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 冷卻時間內，跳過通知");
                    return;
                }
                _lastNotificationTime = DateTime.Now;
            }

            try
            {
                string title = "🚨 設備異常警報";
                string color = "DC143C"; // 深紅色

                // 將異常訊息轉換為 facts 格式
                var facts = new List<(string Name, string Value)>
                {
                    ("異常時間", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };

                // 加入每個異常類型
                for (int i = 0; i < abnormalMessages.Count; i++)
                {
                    facts.Add(($"異常 {i + 1}", abnormalMessages[i]));
                }

                if (_notificationEmails.Length > 0)
                {
                    for (int i = 0; i < _notificationEmails.Length; i++)
                    {
                        string email = _notificationEmails[i];
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送給: {email} ({i + 1}/{_notificationEmails.Length})");

                        string message = await SendTeamsNotificationAsync(title, color, facts.ToArray(), email);
                        System.Diagnostics.Debug.WriteLine($"[Teams通知] 合併異常通知已發送給 {email}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[Teams通知] 已完成發送，共 {_notificationEmails.Length} 位收件者");
                }
                else
                {
                    string message = await SendTeamsNotificationAsync(title, color, facts.ToArray(), "");
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 合併異常通知已發送（無 Email）");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送合併異常通知失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 發送通知到 Teams (支援兩種格式)
        /// </summary>
        private async Task<string> SendTeamsNotificationAsync(string title, string themeColor, (string Name, string Value)[] facts, string email)
        {
            string json;

            if (_usePowerAutomate)
            {
                // Power Automate: 使用 Adaptive Card 格式
                json = BuildAdaptiveCardJson(title, themeColor, facts, email);
            }
            else
            {
                // Teams Incoming Webhook: 使用 MessageCard 格式
                json = BuildMessageCardJson(title, themeColor, facts);
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            System.Diagnostics.Debug.WriteLine($"[Teams通知] 準備發送 POST 請求");
            if (!string.IsNullOrEmpty(email))
            {
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 收件者: {email}");
            }

            var response = await _httpClient.PostAsync(_webhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 回應錯誤: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 錯誤內容: {responseBody}");
                throw new Exception($"Teams API 回應錯誤: {response.StatusCode} - {responseBody}");
            }

            System.Diagnostics.Debug.WriteLine("[Teams通知] 發送成功");
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 建立 Adaptive Card JSON (用於 Power Automate)
        /// </summary>
        private string BuildAdaptiveCardJson(string title, string themeColor, (string Name, string Value)[] facts, string email)
        {
            var factsJson = new StringBuilder();
            for (int i = 0; i < facts.Length; i++)
            {
                factsJson.Append($@"
                    {{
                        ""title"": ""{EscapeJson(facts[i].Name)}"",
                        ""value"": ""{EscapeJson(facts[i].Value)}""
                    }}");
                if (i < facts.Length - 1)
                    factsJson.Append(",");
            }

            string colorName = GetColorName(themeColor);

            return $@"
            {{
                ""mail"": ""{EscapeJson(email)}"",
                ""type"": ""AdaptiveCard"",
                ""version"": ""1.4"",
                ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                ""body"": [
                    {{
                        ""type"": ""TextBlock"",
                        ""text"": ""{EscapeJson(title)}"",
                        ""weight"": ""Bolder"",
                        ""size"": ""Large"",
                        ""color"": ""{colorName}""
                    }},
                    {{
                        ""type"": ""TextBlock"",
                        ""text"": ""空壓設備系統"",
                        ""isSubtle"": true,
                        ""spacing"": ""None""
                    }},
                    {{
                        ""type"": ""FactSet"",
                        ""facts"": [{factsJson}]
                    }}
                ]
            }}";
        }

        /// <summary>
        /// 建立 MessageCard JSON (用於 Teams Incoming Webhook)
        /// </summary>
        private string BuildMessageCardJson(string title, string themeColor, (string Name, string Value)[] facts)
        {
            var factsJson = new StringBuilder();
            for (int i = 0; i < facts.Length; i++)
            {
                factsJson.Append($@"
                    {{
                        ""name"": ""{EscapeJson(facts[i].Name)}"",
                        ""value"": ""{EscapeJson(facts[i].Value)}""
                    }}");
                if (i < facts.Length - 1)
                    factsJson.Append(",");
            }

            return $@"
            {{
                ""@type"": ""MessageCard"",
                ""@context"": ""https://schema.org/extensions"",
                ""themeColor"": ""{themeColor}"",
                ""summary"": ""{EscapeJson(title)}"",
                ""sections"": [
                    {{
                        ""activityTitle"": ""{EscapeJson(title)}"",
                        ""activitySubtitle"": """",
                        ""facts"": [{factsJson}],
                        ""markdown"": true
                    }}
                ]
            }}";
        }

        /// <summary>
        /// 轉換顏色代碼為 Adaptive Card 顏色名稱
        /// </summary>
        private string GetColorName(string hexColor)
        {
            switch (hexColor.ToUpper())
            {
                case "FF0000":
                case "DC3545":
                    return "Attention"; // 紅色
                case "FFA500":
                case "FD7E14":
                    return "Warning"; // 橘色
                case "0078D4":
                case "0D6EFD":
                    return "Accent"; // 藍色
                case "28A745":
                    return "Good"; // 綠色
                default:
                    return "Default";
            }
        }

        /// <summary>
        /// 轉義 JSON 字串中的特殊字元
        /// </summary>
        private string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// 測試 Teams 通知連線
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                System.Diagnostics.Debug.WriteLine("[Teams通知] Webhook URL 未設定");
                return false;
            }

            try
            {
                // 如果有多個 Email，測試時只發送給第一個
                string testEmail = _notificationEmails.Length > 0 ? _notificationEmails[0] : "";

                if (!string.IsNullOrEmpty(testEmail))
                {
                    System.Diagnostics.Debug.WriteLine($"[Teams通知] 測試連線 - 發送給: {testEmail}");
                }

                string json;

                if (_usePowerAutomate)
                {
                    // Power Automate 使用 Adaptive Card 格式
                    json = @"
                    {
                        ""mail"": """ + EscapeJson(testEmail) + @""",
                        ""type"": ""AdaptiveCard"",
                        ""version"": ""1.4"",
                        ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                        ""body"": [
                            {
                                ""type"": ""TextBlock"",
                                ""text"": ""✅ Teams 通知測試"",
                                ""weight"": ""Bolder"",
                                ""size"": ""Large"",
                                ""color"": ""Good""
                            },
                            {
                                ""type"": ""TextBlock"",
                                ""text"": """",
                                ""isSubtle"": true,
                                ""spacing"": ""None""
                            },
                            {
                                ""type"": ""FactSet"",
                                ""facts"": [
                                    {
                                        ""title"": ""狀態"",
                                        ""value"": ""連線成功""
                                    },
                                    {
                                        ""title"": ""時間"",
                                        ""value"": """ + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"""
                                    }
                                ]
                            }
                        ]
                    }";
                }
                else
                {
                    // Teams Incoming Webhook 使用 MessageCard 格式
                    json = @"
                    {
                        ""@type"": ""MessageCard"",
                        ""@context"": ""https://schema.org/extensions"",
                        ""themeColor"": ""28A745"",
                        ""summary"": ""Teams 通知測試"",
                        ""sections"": [
                            {
                                ""activityTitle"": ""✅ Teams 通知測試"",
                                ""activitySubtitle"": """",
                                ""facts"": [
                                    {
                                        ""name"": ""狀態"",
                                        ""value"": ""連線成功""
                                    },
                                    {
                                        ""name"": ""時間"",
                                        ""value"": """ + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"""
                                    }
                                ]
                            }
                        ]
                    }";
                }

                System.Diagnostics.Debug.WriteLine($"[Teams通知] 測試連線 - 發送 POST 請求");
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 使用格式: {(_usePowerAutomate ? "Adaptive Card" : "MessageCard")}");
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 測試 JSON: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_webhookUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[Teams通知] 測試失敗: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"[Teams通知] 錯誤內容: {responseBody}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[Teams通知] 測試成功");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Teams通知] 連線測試失敗: {ex.Message}");
                return false;
            }
        }
    }
}
