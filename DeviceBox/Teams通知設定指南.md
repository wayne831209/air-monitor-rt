# Teams 通知設定指南

## 📋 概述

DeviceBox 系統現在支援兩種 Teams 通知方式：
1. **Power Automate Flow**（較複雜，但功能更強大，支援郵件通知）
2. **Teams Incoming Webhook**（簡單直接，推薦）

系統會自動偵測 webhook URL 類型並使用對應的格式。

## 🆕 新功能：Email 通知

在 Power Automate 模式下，通知 JSON 中會包含 `mail` 欄位，您可以在 Flow 中使用此欄位：
- 發送郵件通知給相關人員
- 觸發其他自動化流程
- 記錄聯絡人資訊

---

## 方案 1：Power Automate Flow（當前使用）

### ⚠️ 重要設定步驟

您目前使用的 Power Automate webhook URL：
```
https://default03259ba3b3ec47e38585f76b24aee3.4b.environment.api.powerplatform.com:443/...
```

### 必須這樣設定您的 Flow：

#### 步驟 1：HTTP 觸發器
- **觸發器**: 當收到 HTTP 要求時
- **方法**: POST
- **不要設定請求本文 JSON 結構描述**（留空，讓它自動接收）

#### 步驟 2：Post card 動作
- **動作**: Post card in a chat or channel
- **Team**: 選擇您的 Teams
- **Channel**: 選擇頻道
- **Message**: 點擊動態內容，選擇 `Body`（來自 HTTP 觸發器）

或直接輸入運算式：
```
triggerBody()
```

### ✅ 正確的 Flow 結構

```
[HTTP 觸發器: 當收到 HTTP 要求時]
		  ↓
[Post card in a chat or channel]
  Message: @triggerBody()
```

### ❌ 常見錯誤

**錯誤做法 1**: 在 Adaptive Card Designer 中手動建立卡片
- 不要這樣做！程式碼已經發送完整的 Adaptive Card JSON

**錯誤做法 2**: 設定複雜的 JSON 結構描述
- 不需要！直接使用 `triggerBody()` 即可

---

## 方案 2：Teams Incoming Webhook（推薦，更簡單）

### 設定步驟

#### 步驟 1：在 Teams 中新增 Incoming Webhook

1. 打開您的 Teams 頻道
2. 點擊頻道名稱旁的 **⋯** (更多選項)
3. 選擇 **連接器** (Connectors)
4. 搜尋並新增 **Incoming Webhook**
5. 設定名稱（例如：DeviceBox 警報）
6. 複製 Webhook URL

#### 步驟 2：更新 config.xml

將您的 webhook URL 替換為 Teams Incoming Webhook URL：

```xml
<TeamsNotification>
  <Enabled>1</Enabled>
  <WebhookUrl>https://outlook.office.com/webhook/xxxxx...</WebhookUrl>
  <Email>maintenance@company.com</Email>
</TeamsNotification>
```

⚠️ **注意事項**
- 記得將 URL 中的 `&` 替換為 `&amp;`
- Email 欄位在 Incoming Webhook 模式下不會被使用，但建議填寫以便日後切換到 Power Automate

#### 優點
- ✅ 設定簡單，不需要 Power Automate
- ✅ 穩定可靠
- ✅ 不需要額外的授權
- ✅ 系統會自動偵測並使用 MessageCard 格式

---

## 🧪 測試通知

### 方法 1：使用程式內建測試功能
（如果您的程式有測試按鈕）

### 方法 2：觸發警報
調整壓力或溫度閾值來觸發警報

### 方法 3：查看 Debug 輸出
在 Visual Studio 的輸出視窗中會看到詳細的通知日誌：

```
[Teams通知] 使用 Power Automate 模式
[Teams通知] 準備發送 POST 請求
[Teams通知] URL: https://...
[Teams通知] JSON: {...}
[Teams通知] 發送成功
```

---

## 🔧 故障排除

### 問題：收到 "Property 'type' must be 'AdaptiveCard'" 錯誤

**原因**: Power Automate Flow 設定不正確

**解決方法**:
1. 確認 "Post card" 動作的 Message 欄位使用 `@triggerBody()`
2. 不要在 Adaptive Card Designer 中手動建立卡片
3. 或改用 Teams Incoming Webhook（方案 2）

### 問題：收到 "expected 'POST' and actual 'GET'" 錯誤

**原因**: 有人在瀏覽器中打開了 webhook URL

**解決方法**: 確保只有程式發送請求，不要在瀏覽器中測試 URL

### 問題：XML 解析錯誤（'=' is unexpected token）

**原因**: config.xml 中的 webhook URL 包含未轉義的 `&` 字元

**解決方法**: 將 URL 中的 `&` 替換為 `&amp;`

例如：
```xml
<!-- 錯誤 ❌ -->
<WebhookUrl>https://example.com?param1=1&param2=2</WebhookUrl>

<!-- 正確 ✅ -->
<WebhookUrl>https://example.com?param1=1&amp;param2=2</WebhookUrl>
```

---

## 📊 通知格式

### Adaptive Card（Power Automate）
- 標題：大字體，帶顏色（紅色=警報，橘色=警告）
- 副標題：維護課 DeviceBox 系統
- 資料表格：名稱-值對

### MessageCard（Incoming Webhook）
- 標題：警報類型
- 副標題：系統名稱
- 資料表格：詳細資訊

---

## 🔄 自動偵測機制

程式會自動偵測 webhook URL 類型：

```csharp
if (url.Contains("powerplatform.com"))
	使用 Adaptive Card 格式
else
	使用 MessageCard 格式
```

---

## 📝 建議的設定順序

**對於新用戶（推薦）**:
1. 使用 Teams Incoming Webhook（方案 2）
2. 設定簡單且穩定

**對於需要進階功能**:
1. 使用 Power Automate（方案 1）
2. 按照本文件的步驟正確設定 Flow
3. 確保使用 `@triggerBody()`

---

## 📞 技術支援

如有問題，請檢查：
1. Debug 輸出視窗的詳細日誌
2. Power Automate Flow 的執行歷程記錄
3. Teams 頻道的權限設定

更新日期：2024-01-19
