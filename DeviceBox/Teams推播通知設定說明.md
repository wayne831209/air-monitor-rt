# Microsoft Teams 推播通知設定說明

## 功能概述

本系統實作了 Microsoft Teams 推播通知功能，當空壓或溫度超過設定的上下限時，會自動發送警報訊息到指定的 Teams 頻道。

## 主要特性

- ✅ **空壓超限警報**：當空壓數值超過上下限時發送通知
- ✅ **溫度超限警報**：當溫度數值超過上下限時發送通知
- ✅ **防止重複通知**：內建 5 分鐘冷卻時間機制
- ✅ **非同步處理**：不會阻塞主程式運行
- ✅ **格式化卡片**：使用 Teams MessageCard 格式，顯示清晰美觀
- ✅ **錯誤處理**：完善的錯誤處理和日誌記錄

## 配置步驟

### 1. 取得 Teams Webhook URL

1. 開啟 Microsoft Teams，選擇要接收通知的頻道
2. 點擊頻道名稱旁的 `•••` (更多選項)
3. 選擇「連接器」(Connectors) 或「管理頻道」
4. 搜尋並新增「Incoming Webhook」
5. 設定 Webhook 名稱（例如：DeviceBox 警報）
6. 複製產生的 Webhook URL（格式類似：`https://outlook.office.com/webhook/...`）

### 2. 修改 config.xml

在 `config.xml` 檔案的 `<Config>` 根節點中添加以下配置：

```xml
<Config>
  <!-- 現有的 Database 配置 -->
  <Database>
	...
  </Database>

  <!-- 現有的 Factories 配置 -->
  <Factories>
	...
  </Factories>

  <!-- 新增 Teams 通知配置 -->
  <TeamsNotification>
	<Enabled>true</Enabled>
	<WebhookUrl>YOUR_WEBHOOK_URL_HERE</WebhookUrl>
  </TeamsNotification>
</Config>
```

**重要**：將 `YOUR_WEBHOOK_URL_HERE` 替換為步驟 1 中取得的實際 Webhook URL。

### 3. 配置範例

```xml
<TeamsNotification>
  <Enabled>true</Enabled>
  <WebhookUrl>https://outlook.office.com/webhook/12345678-1234-1234-1234-123456789abc@12345678-1234-1234-1234-123456789abc/IncomingWebhook/1234567890abcdef1234567890abcdef/12345678-1234-1234-1234-123456789abc</WebhookUrl>
</TeamsNotification>
```

**啟用/停用通知**：
- `<Enabled>true</Enabled>`：啟用 Teams 通知
- `<Enabled>false</Enabled>`：停用 Teams 通知

## 通知訊息格式

### 空壓超限警報
```
⚠️ 空壓超限警報
維護課 DeviceBox 系統

來源：[廠區名稱]
當前數值：[數值]
上限：[上限值]
下限：[下限值]
時間：[發生時間]
```

### 溫度超限警報
```
🌡️ 溫度超限警報
維護課 DeviceBox 系統

來源：[廠區名稱]
當前數值：[數值]
上限：[上限值]
下限：[下限值]
時間：[發生時間]
```

## 程式碼架構

### 新增檔案
- `DeviceBox\TeamsNotificationService.cs`：Teams 通知服務類別

### 修改檔案
- `DeviceBox\Config.cs`：
  - 新增 `TeamsWebhookUrl` 和 `TeamsNotificationEnabled` 屬性
  - 新增 `LoadTeamsNotificationSettings` 方法

- `DeviceBox\MainForm.cs`：
  - 新增 `teamsNotificationService` 欄位
  - 新增 `InitializeTeamsNotification` 方法
  - 實作 `OnPressureOverLimit` 方法
  - 實作 `OnTempOverLimit` 方法

## 防重複通知機制

為避免短時間內重複發送相同警報，系統內建 **5 分鐘冷卻時間**：

- 發送通知後，5 分鐘內不會再發送相同類型的警報
- 5 分鐘後，如果問題仍存在，會再次發送通知
- 冷卻時間可在 `TeamsNotificationService.cs` 中調整 `_notificationCooldown` 參數

```csharp
private static readonly TimeSpan _notificationCooldown = TimeSpan.FromMinutes(5);
```

## 測試方法

### 1. 測試連線功能（可選）

可以在程式中呼叫測試方法：

```csharp
var testResult = await teamsNotificationService.TestConnectionAsync();
if (testResult)
{
	MessageBox.Show("Teams 通知連線測試成功！");
}
else
{
	MessageBox.Show("Teams 通知連線測試失敗！");
}
```

### 2. 模擬超限警報

暫時調整警報上下限：
1. 將空壓或溫度的上下限設定為很低的值
2. 等待系統偵測到超限
3. 檢查 Teams 頻道是否收到通知
4. 恢復正常的上下限設定

### 3. 檢查日誌

在 Visual Studio 的「輸出」視窗中查看 Debug 訊息：
- `[Teams通知] 空壓超限通知已發送`
- `[Teams通知] 溫度超限通知已發送`
- `[Teams通知] 冷卻時間內，跳過通知`
- `[Teams通知] 發送通知失敗: [錯誤訊息]`

## 常見問題排除

### Q1: 沒有收到通知訊息

**檢查項目**：
1. 確認 `config.xml` 中 `<Enabled>` 設為 `true`
2. 確認 Webhook URL 正確無誤
3. 檢查網路連線是否正常
4. 確認 Teams 頻道的 Webhook 連接器未被停用
5. 查看程式日誌是否有錯誤訊息

### Q2: Webhook URL 失效

**解決方法**：
- 重新在 Teams 頻道中建立新的 Incoming Webhook
- 更新 `config.xml` 中的 URL
- 重新啟動程式

### Q3: 收到過多重複通知

**解決方法**：
- 調整冷卻時間（預設 5 分鐘）
- 檢查警報上下限設定是否合理
- 確認感測器數值是否穩定

### Q4: 建置錯誤：找不到 System.Net.Http

**解決方法**：
在專案中加入 `System.Net.Http` 參考：
1. 在方案總管中右鍵點擊「參考」
2. 選擇「新增參考」
3. 勾選 `System.Net.Http`
4. 點擊「確定」

## 進階設定

### 修改通知顏色

在 `TeamsNotificationService.cs` 中：

```csharp
// 空壓超限（紅色）
string color = "FF0000";

// 溫度超限（橘色）
string color = "FFA500";

// 可改為其他顏色，例如：
// 藍色："0078D4"
// 綠色："00FF00"
// 黃色："FFFF00"
```

### 自訂通知訊息

修改 `TeamsNotificationService.cs` 中的 `SendTeamsNotificationAsync` 方法，調整 facts 陣列的內容：

```csharp
new[]
{
	("來源", source),
	("當前數值", currentValue),
	("上限", upperLimit.ToString("F2")),
	("下限", lowerLimit.ToString("F2")),
	("時間", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
	// 可新增更多欄位
	("處理建議", "請立即檢查設備狀態")
}
```

## 安全注意事項

🔒 **Webhook URL 保護**：
- Webhook URL 包含敏感資訊，請勿公開分享
- 建議將 `config.xml` 加入 `.gitignore`，避免上傳到版本控制系統
- 定期更換 Webhook URL

## 技術規格

- **通知協定**：HTTPS POST
- **訊息格式**：Microsoft Teams MessageCard JSON
- **逾時時間**：10 秒
- **冷卻時間**：5 分鐘（可調整）
- **執行方式**：非同步（Task.Run）
- **相依套件**：System.Net.Http

## 版本資訊

- **建立日期**：2024
- **適用版本**：.NET Framework 4.8
- **作者**：維護課 DeviceBox 開發團隊

---

如有問題或需要協助，請聯絡系統管理員。
