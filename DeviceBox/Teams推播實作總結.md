# Teams 推播通知實作總結

## 📋 實作內容

### 1. 新增檔案

#### TeamsNotificationService.cs
完整的 Microsoft Teams 通知服務類別，包含：
- `SendPressureAlertAsync()` - 發送空壓超限警報
- `SendTemperatureAlertAsync()` - 發送溫度超限警報
- `TestConnectionAsync()` - 測試連線功能
- 內建 5 分鐘冷卻機制防止重複通知

### 2. 修改檔案

#### Config.cs
- ✅ 新增 `TeamsWebhookUrl` 屬性
- ✅ 新增 `TeamsNotificationEnabled` 屬性
- ✅ 新增 `LoadTeamsNotificationSettings()` 方法
- ✅ 在 `LoadConfig()` 中整合 Teams 配置讀取

#### MainForm.cs
- ✅ 新增 `teamsNotificationService` 欄位
- ✅ 新增 `InitializeTeamsNotification()` 方法
- ✅ 實作 `OnPressureOverLimit()` 方法（使用 Teams 推播）
- ✅ 實作 `OnTempOverLimit()` 方法（使用 Teams 推播）
- ✅ 在建構函式中呼叫初始化

### 3. 文件檔案
- ✅ `Teams推播通知設定說明.md` - 完整使用說明
- ✅ `config_teams_example.xml` - 配置範例

## 🎯 功能特點

1. **自動推播**：空壓或溫度超限時自動發送 Teams 通知
2. **防止洪水**：5 分鐘冷卻時間機制
3. **非同步處理**：不阻塞主程式運行
4. **美觀格式**：使用 Teams MessageCard 格式
5. **錯誤處理**：完善的錯誤捕捉和日誌記錄
6. **可開關**：可透過 config.xml 啟用/停用

## ⚙️ 配置步驟

1. **取得 Webhook URL**
   - Teams 頻道 → 連接器 → Incoming Webhook → 複製 URL

2. **編輯 config.xml**
   ```xml
   <TeamsNotification>
	 <Enabled>true</Enabled>
	 <WebhookUrl>YOUR_WEBHOOK_URL</WebhookUrl>
   </TeamsNotification>
   ```

3. **重新啟動程式**
   - 系統會自動載入設定並初始化 Teams 服務

## 📊 通知範例

### 空壓超限
```
⚠️ 空壓超限警報
維護課 DeviceBox 系統

來源：廠區A
當前數值：8.5 bar
上限：8.0 bar
下限：6.0 bar
時間：2024-01-15 14:30:25
```

### 溫度超限
```
🌡️ 溫度超限警報
維護課 DeviceBox 系統

來源：廠區B
當前數值：85 °C
上限：80 °C
下限：0 °C
時間：2024-01-15 14:31:10
```

## 🔧 技術細節

### 通知流程
```
感測器數值超限
	↓
OnPressureOverLimit / OnTempOverLimit 被呼叫
	↓
檢查 Teams 服務是否啟用
	↓
Task.Run (非同步執行)
	↓
檢查冷卻時間
	↓
發送 HTTP POST 到 Teams Webhook
	↓
記錄日誌
```

### 冷卻機制
```csharp
private static DateTime _lastNotificationTime = DateTime.MinValue;
private static readonly TimeSpan _notificationCooldown = TimeSpan.FromMinutes(5);

// 檢查是否在冷卻時間內
if (DateTime.Now - _lastNotificationTime < _notificationCooldown)
{
	return; // 跳過通知
}

_lastNotificationTime = DateTime.Now; // 更新時間
```

## 🐛 除錯訊息

程式執行時會在 Debug 輸出顯示：

```
[MainForm] Teams 通知服務已啟用
[Config] Teams Notification Enabled: True
[Config] Teams Webhook URL: https://outlook.office.com/webhook/...
[Teams通知] 空壓超限通知已發送
[Teams通知] 溫度超限通知已發送
[Teams通知] 冷卻時間內，跳過通知
```

## ✅ 建置狀態

- ✅ **編譯成功**：無錯誤、無警告
- ✅ **相依套件**：System.Net.Http（.NET Framework 內建）
- ✅ **目標框架**：.NET Framework 4.8

## 📝 使用範例

### 啟用通知
```xml
<TeamsNotification>
  <Enabled>true</Enabled>
  <WebhookUrl>https://outlook.office.com/webhook/...</WebhookUrl>
</TeamsNotification>
```

### 停用通知
```xml
<TeamsNotification>
  <Enabled>false</Enabled>
  <WebhookUrl></WebhookUrl>
</TeamsNotification>
```

### 測試連線（可選）
```csharp
// 在程式中添加測試按鈕
private async void btnTestTeams_Click(object sender, EventArgs e)
{
	if (teamsNotificationService != null)
	{
		bool result = await teamsNotificationService.TestConnectionAsync();
		MessageBox.Show(
			result ? "Teams 連線測試成功！" : "Teams 連線測試失敗！",
			"測試結果",
			MessageBoxButtons.OK,
			result ? MessageBoxIcon.Information : MessageBoxIcon.Error
		);
	}
}
```

## 🔒 安全建議

1. 將 Webhook URL 視為敏感資訊
2. 不要將 config.xml 提交到公開版本控制
3. 定期更換 Webhook URL
4. 限制可訪問 Webhook 的 IP 範圍（Teams 管理）

## 📚 參考文件

- Microsoft Teams Incoming Webhook 文件
- MessageCard 格式說明
- 完整說明：`Teams推播通知設定說明.md`
- 配置範例：`config_teams_example.xml`

---

**實作完成** ✅  
**建置成功** ✅  
**可立即使用** ✅
