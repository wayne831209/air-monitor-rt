# config.xml Email 設定快速參考

## 📋 基本設定

```xml
<?xml version="1.0" encoding="utf-8"?>
<Setting>
  <Database>
	<!-- 資料庫設定 -->
  </Database>

  <Factories>
	<!-- 工廠設定 -->
  </Factories>

  <TeamsNotification>
	<Enabled>true</Enabled>
	<WebhookUrl>YOUR_WEBHOOK_URL_HERE</WebhookUrl>
	<Email>maintenance@company.com</Email>
  </TeamsNotification>
</Setting>
```

## ⚠️ 重要提醒

### Webhook URL 中的 & 符號

❌ **錯誤**
```xml
<WebhookUrl>https://example.com?param1=1&param2=2&param3=3</WebhookUrl>
```

✅ **正確**
```xml
<WebhookUrl>https://example.com?param1=1&amp;param2=2&amp;param3=3</WebhookUrl>
```

## 📧 Email 設定範例

### 單一收件者
```xml
<Email>maintenance@company.com</Email>
```
**結果 JSON**: `["maintenance@company.com"]`

### 多個收件者（用逗號分隔）✨ 推薦
```xml
<Email>user1@company.com,user2@company.com,user3@company.com</Email>
```
**結果 JSON**: `["user1@company.com", "user2@company.com", "user3@company.com"]`

### 多個收件者（用分號分隔）
```xml
<Email>user1@company.com;user2@company.com;user3@company.com</Email>
```
**結果 JSON**: 同上

### 不需要郵件通知
```xml
<Email></Email>
```
**結果 JSON**: `[]`

## 🔍 設定驗證

### 檢查點 1：XML 格式
- [ ] XML 檔案可以正常被解析
- [ ] 沒有 XML 錯誤訊息

### 檢查點 2：Webhook URL
- [ ] URL 中的 `&` 已替換為 `&amp;`
- [ ] URL 完整且沒有被截斷
- [ ] URL 格式正確

### 檢查點 3：Email
- [ ] Email 格式正確
- [ ] 沒有多餘的前後空格（會自動處理）
- [ ] 多個 Email 用逗號或分號分隔

## 🧪 測試設定

### 步驟 1：檢查 Debug 輸出
啟動程式後，在 Visual Studio 的輸出視窗中應該看到：

```
[Config] Teams Notification Enabled: True
[Config] Teams Webhook URL: https://...
[Config] Teams Notification Email: user1@company.com,user2@company.com
[Teams通知] 使用 Power Automate 模式
[Teams通知] 通知聯絡人數量: 2
[Teams通知]   - user1@company.com
[Teams通知]   - user2@company.com
```

### 步驟 2：觸發測試警報
手動調整閾值來觸發警報

### 步驟 3：檢查通知
- Teams 頻道應該收到卡片通知
- Power Automate 執行記錄中應該有 `mail` 欄位

## 🔧 常見錯誤修正

### 錯誤 1：XML 解析失敗
```
'=' 是未預期的語彙基元
```
**解決方法**：檢查 URL 中的 `&` 是否已替換為 `&amp;`

### 錯誤 2：Email 未傳送
**檢查項目**：
1. Email 欄位是否正確設定
2. 使用的是 Power Automate 還是 Incoming Webhook（後者不支援）
3. Power Automate Flow 是否正確設定

### 錯誤 3：程式無法啟動
**檢查項目**：
1. config.xml 是否存在於 bin\Debug 目錄
2. XML 格式是否正確
3. 檔案編碼是否為 UTF-8

## 📚 延伸閱讀

- `Teams通知設定指南.md` - 完整的設定說明
- `Teams通知Email功能說明.md` - Email 功能詳細說明
- `config_teams_example.xml` - 設定範例檔案

---

**更新日期**：2024-01-19  
**版本**：2.0 (支援多組人員)
