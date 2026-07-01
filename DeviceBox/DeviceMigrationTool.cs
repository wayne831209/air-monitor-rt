using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DeviceBox
{
    /// <summary>
    /// 設備配置資料遷移工具
    /// 將 config.xml 中的工廠、設備和警報上下限資料遷移到資料庫
    /// </summary>
    public class DeviceMigrationTool
    {
        private readonly string configFilePath;
        private readonly DeviceDatabase deviceDatabase;

        public DeviceMigrationTool(string dbIP, string dbName, string dbUser, string dbPassword)
        {
            configFilePath = Path.Combine(Application.StartupPath, "config.xml");
            deviceDatabase = new DeviceDatabase(dbIP, dbName, dbUser, dbPassword);
        }

        /// <summary>
        /// 執行完整遷移流程
        /// </summary>
        public MigrationResult Migrate()
        {
            var result = new MigrationResult();

            try
            {
                // 1. 測試資料庫連線
                if (!deviceDatabase.TestConnection())
                {
                    result.Success = false;
                    result.ErrorMessage = "資料庫連線失敗,請檢查連線設定";
                    return result;
                }

                // 2. 檢查 config.xml 是否存在
                if (!File.Exists(configFilePath))
                {
                    result.Success = false;
                    result.ErrorMessage = $"找不到 config.xml 檔案: {configFilePath}";
                    return result;
                }

                // 3. 讀取 XML 檔案
                XDocument doc = XDocument.Load(configFilePath);
                var factoriesElement = doc.Root?.Element("Factories");

                if (factoriesElement == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "config.xml 格式錯誤:找不到 Factories 節點";
                    return result;
                }

                // 4. 遷移每個工廠
                foreach (var factoryElement in factoriesElement.Elements("Factory"))
                {
                    var factoryResult = MigrateFactory(factoryElement);
                    result.Factories.Add(factoryResult);

                    if (factoryResult.Success)
                        result.SuccessCount++;
                    else
                        result.FailureCount++;
                }

                // 5. 遷移通知設定
                try
                {
                    var teamsElement = doc.Root?.Element("TeamsNotification");
                    if (teamsElement != null)
                    {
                        MigrateNotificationSettings(teamsElement);
                        result.Message = $"遷移完成: 成功 {result.SuccessCount} 個工廠, 失敗 {result.FailureCount} 個工廠 (包含通知設定)";
                    }
                    else
                    {
                        result.Message = $"遷移完成: 成功 {result.SuccessCount} 個工廠, 失敗 {result.FailureCount} 個工廠 (未找到通知設定)";
                    }
                }
                catch (Exception notifEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[DeviceMigration] Notification migration warning: {notifEx.Message}");
                    result.Message = $"遷移完成: 成功 {result.SuccessCount} 個工廠, 失敗 {result.FailureCount} 個工廠 (通知設定遷移失敗)";
                }

                result.Success = result.SuccessCount > 0;

                System.Diagnostics.Debug.WriteLine($"[DeviceMigration] {result.Message}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"遷移過程發生錯誤: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DeviceMigration] Error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 遷移單一工廠的資料
        /// </summary>
        private FactoryMigrationResult MigrateFactory(XElement factoryElement)
        {
            var result = new FactoryMigrationResult();

            try
            {
                // 解析工廠基本資訊
                int id = int.Parse(factoryElement.Attribute("id")?.Value ?? "0");
                string name = factoryElement.Attribute("name")?.Value ?? "";
                string modbusIp = factoryElement.Attribute("modbusIp")?.Value ?? "";
                string modbusPort = factoryElement.Attribute("modbusPort")?.Value ?? "";

                result.FactoryName = name;

                var factory = new FactoryConfig
                {
                    Id = id,
                    Name = name,
                    ModbusIp = modbusIp,
                    ModbusPort = modbusPort
                };

                // 新增工廠到資料庫
                if (!deviceDatabase.InsertFactory(factory))
                {
                    result.Success = false;
                    result.ErrorMessage = $"無法新增工廠 {name} 到資料庫 (可能已存在)";
                    return result;
                }

                result.FactoryId = factory.Id;

                // 遷移警報上下限
                var alarmLimitsElement = factoryElement.Element("AlarmLimits");
                if (alarmLimitsElement != null)
                {
                    var alarmLimits = ParseAlarmLimits(alarmLimitsElement);
                    deviceDatabase.SaveAlarmLimits(factory.Id, alarmLimits);
                }

                // 遷移設備
                var devicesElement = factoryElement.Element("Devices");
                if (devicesElement != null)
                {
                    foreach (var deviceElement in devicesElement.Elements("Device"))
                    {
                        var device = ParseDevice(deviceElement);
                        if (device != null)
                        {
                            if (deviceDatabase.InsertDevice(factory.Id, device))
                            {
                                result.DeviceCount++;
                            }
                            else
                            {
                                result.Warnings.Add($"設備 {device.Name} 新增失敗");
                            }
                        }
                    }
                }

                result.Success = true;
                result.Message = $"工廠 {name} 遷移成功 ({result.DeviceCount} 個設備)";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"遷移工廠時發生錯誤: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 解析設備配置
        /// </summary>
        private DeviceConfig ParseDevice(XElement deviceElement)
        {
            try
            {
                string typeStr = deviceElement.Attribute("type")?.Value;
                DeviceType deviceType;
                if (!Enum.TryParse(typeStr, true, out deviceType))
                    return null;

                var device = new DeviceConfig
                {
                    Type = deviceType,
                    MachineNo = int.Parse(deviceElement.Attribute("machineNo")?.Value ?? "1"),
                    Name = deviceElement.Attribute("name")?.Value ?? "",
                    Enabled = deviceElement.Attribute("enabled")?.Value != "false",
                    IO = new IOConfig()
                };

                // 解析 IO 設定
                var ioElement = deviceElement.Element("IO");
                if (ioElement != null)
                {
                    if (ioElement.Attribute("runDI") != null)
                        device.IO.RunDI = int.Parse(ioElement.Attribute("runDI").Value);
                    if (ioElement.Attribute("alarmDI") != null)
                        device.IO.AlarmDI = int.Parse(ioElement.Attribute("alarmDI").Value);
                    if (ioElement.Attribute("faultDI") != null)
                        device.IO.FaultDI = int.Parse(ioElement.Attribute("faultDI").Value);
                    if (ioElement.Attribute("controlDO") != null)
                        device.IO.ControlDO = int.Parse(ioElement.Attribute("controlDO").Value);
                    if (ioElement.Attribute("isReady") != null)
                        device.IO.IsReadyDI = int.Parse(ioElement.Attribute("isReady").Value);
                    if (ioElement.Attribute("isRemote") != null)
                        device.IO.IsRemoteDI = int.Parse(ioElement.Attribute("isRemote").Value);
                    if (ioElement.Attribute("onDI") != null)
                        device.IO.OnDI = int.Parse(ioElement.Attribute("onDI").Value);
                    if (ioElement.Attribute("offDI") != null)
                        device.IO.OffDI = int.Parse(ioElement.Attribute("offDI").Value);
                    if (ioElement.Attribute("readyDI") != null)
                        device.IO.ReadyDI = int.Parse(ioElement.Attribute("readyDI").Value);
                    if (ioElement.Attribute("adam") != null)
                        device.IO.Adam = ioElement.Attribute("adam").Value;
                }

                return device;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceMigration] ParseDevice error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析警報上下限
        /// </summary>
        private AlarmLimitsConfig ParseAlarmLimits(XElement element)
        {
            var limits = new AlarmLimitsConfig();

            string pressureUpperStr = element.Attribute("pressureUpper")?.Value;
            if (!string.IsNullOrEmpty(pressureUpperStr))
            {
                double val;
                if (double.TryParse(pressureUpperStr, out val))
                    limits.PressureUpperLimit = val;
            }

            string pressureLowerStr = element.Attribute("pressureLower")?.Value;
            if (!string.IsNullOrEmpty(pressureLowerStr))
            {
                double val;
                if (double.TryParse(pressureLowerStr, out val))
                    limits.PressureLowerLimit = val;
            }

            string tempUpperStr = element.Attribute("tempUpper")?.Value;
            if (!string.IsNullOrEmpty(tempUpperStr))
            {
                double val;
                if (double.TryParse(tempUpperStr, out val))
                    limits.TempUpperLimit = val;
            }

            string tempLowerStr = element.Attribute("tempLower")?.Value;
            if (!string.IsNullOrEmpty(tempLowerStr))
            {
                double val;
                if (double.TryParse(tempLowerStr, out val))
                    limits.TempLowerLimit = val;
            }

            return limits;
        }

        /// <summary>
        /// 遷移通知設定
        /// </summary>
        private void MigrateNotificationSettings(XElement teamsElement)
        {
            try
            {
                // 解析 Teams 通知設定
                string enabledStr = teamsElement.Element("Enabled")?.Value ?? "false";
                string webhookUrl = teamsElement.Element("WebhookUrl")?.Value ?? "";
                string email = teamsElement.Element("Email")?.Value ?? "";

                // 儲存到資料庫
                deviceDatabase.SaveNotificationSetting("teams_enabled", enabledStr, "Teams 通知是否啟用");
                deviceDatabase.SaveNotificationSetting("teams_webhook_url", webhookUrl, "Teams Webhook URL");
                deviceDatabase.SaveNotificationSetting("teams_email", email, "Teams 通知郵件地址");

                System.Diagnostics.Debug.WriteLine($"[DeviceMigration] Notification settings migrated: enabled={enabledStr}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceMigration] MigrateNotificationSettings error: {ex.Message}");
                throw;
            }
        }
    }

    #region Result Classes

    /// <summary>
    /// 遷移結果
    /// </summary>
    public class MigrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<FactoryMigrationResult> Factories { get; set; } = new List<FactoryMigrationResult>();
    }

    /// <summary>
    /// 工廠遷移結果
    /// </summary>
    public class FactoryMigrationResult
    {
        public bool Success { get; set; }
        public int FactoryId { get; set; }
        public string FactoryName { get; set; }
        public int DeviceCount { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    #endregion
}
