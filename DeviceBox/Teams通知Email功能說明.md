# Teams 通知 Email 功能說明

## 📧 功能概述

DeviceBox 系統在發送 Teams 通知時，可以在 JSON 中包含**多組**聯絡人的 Email 地址。此功能主要用於 **Power Automate** 模式，方便後續的郵件通知或其他自動化處理。

**✨ 新功能：支援多組人員！**
- 支援單一或多個 Email 地址
- 自動解析逗號或分號分隔的 Email
- 在 JSON 中以陣列形式傳送，方便 Power Automate 處理

---

## 📝 設定方法

### 1. 在 config.xml 中設定 Email

#### 單一收件者
```xml
<TeamsNotification>
  <Enabled>true</Enabled>
  <WebhookUrl>YOUR_WEBHOOK_URL_HERE</WebhookUrl>
  <Email>maintenance@company.com</Email>
</TeamsNotification>
```

#### 多個收件者（用逗號分隔）
```xml
<Email>user1@company.com,user2@company.com,user3@company.com</Email>
```

#### 多個收件者（用分號分隔）
```xml
<Email>user1@company.com;user2@company.com;user3@company.com</Email>
```

#### 混合使用逗號和分號
```xml
<Email>user1@company.com,user2@company.com;user3@company.com</Email>
```

### 支援的格式

| 格式 | 範例 | 結果 |
|------|------|------|
| 單一 Email | `maintenance@company.com` | `["maintenance@company.com"]` |
| 逗號分隔 | `user1@company.com,user2@company.com` | `["user1@company.com", "user2@company.com"]` |
| 分號分隔 | `user1@company.com;user2@company.com` | `["user1@company.com", "user2@company.com"]` |
| 混合分隔 | `a@c.com,b@c.com;c@c.com` | `["a@c.com", "b@c.com", "c@c.com"]` |
| 空白 | (留空) | `[]` |

⚠️ **注意**：
- Email 地址前後的空格會自動移除
- 空的 Email 項目會自動過濾
- 不驗證 Email 格式的正確性（請確保格式正確）

---

## 🔧 技術實作

### JSON 結構（Power Automate 模式）

發送到 Power Automate 的 Adaptive Card JSON 包含 `mail` 欄位（陣列格式）：

#### 範例 1：單一 Email
```json
{
  "mail": ["maintenance@company.com"],
  "type": "AdaptiveCard",
  "version": "1.4",
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "body": [
	{
	  "type": "TextBlock",
	  "text": "⚠️ 空壓超限警報",
	  "weight": "Bolder",
	  "size": "Large",
	  "color": "Attention"
	},
	{
	  "type": "FactSet",
	  "facts": [
		{"title": "來源", "value": "工程大樓"},
		{"title": "當前數值", "value": "8.5"}
	  ]
	}
  ]
}
```

#### 範例 2：多個 Email
```json
{
  "mail": [
	"user1@company.com",
	"user2@company.com",
	"user3@company.com"
  ],
  "type": "AdaptiveCard",
  ...
}
```

#### 範例 3：空白
```json
{
  "mail": [],
  "type": "AdaptiveCard",
  ...
}
```

### 在 Power Automate 中使用 Email 陣列

#### 方法 1：取得完整陣列
```
triggerBody()?['mail']
```
結果：`["user1@company.com", "user2@company.com"]`

#### 方法 2：取得第一個 Email
```
first(triggerBody()?['mail'])
```
結果：`user1@company.com`

#### 方法 3：遍歷所有 Email（使用 Apply to Each）
```
觸發器 Body: triggerBody()?['mail']
當前項目: @{items('Apply_to_each')}
```

#### 方法 4：轉換為分號分隔字串（用於 Outlook）
```
join(triggerBody()?['mail'], ';')
```
結果：`user1@company.com;user2@company.com;user3@company.com`

---

## 🎯 使用場景

### 場景 1：Teams 通知 + 批量郵件通知

```
[DeviceBox 偵測到警報]
		 ↓
[發送到 Power Automate]
		 ↓
	┌────┴────┐
	↓         ↓
[Teams 卡片] [Apply to Each]
			 ↓
		  [發送郵件給每個人]
```

**Power Automate Flow 設定：**
```
1. HTTP 觸發器
2. Post card in Teams
3. Apply to Each（來源：triggerBody()?['mail']）
   └─ 發送電子郵件
	  收件者：@{items('Apply_to_each')}
```

### 場景 2：發送給所有人（單一郵件）

```
[DeviceBox 警報]
	  ↓
[Power Automate]
	  ↓
  [發送郵件]
   收件者：user1@c.com;user2@c.com;user3@c.com
```

**Power Automate Flow 設定：**
```
1. HTTP 觸發器
2. 撰寫動作
   輸入：join(triggerBody()?['mail'], ';')
3. 發送電子郵件
   收件者：@{outputs('撰寫')}
```

### 場景 3：通知不同工廠的負責人

不同工廠可能有不同的負責人，您可以在不同環境使用不同的 config.xml：

**裝配一廠：**
```xml
<Email>factory1@company.com,supervisor1@company.com</Email>
```

**裝配二廠：**
```xml
<Email>factory2@company.com,supervisor2@company.com</Email>
```

### 場景 4：警報等級控制

```
[一般警報] → 發送給維護人員
[嚴重警報] → 發送給維護人員 + 主管
```

在 Power Automate 中可以根據警報內容決定收件者。

---

## 🔍 Power Automate Flow 範例

### 基礎版：Teams 卡片 + 批量郵件

```
1. 📥 [HTTP 觸發器: 當收到 HTTP 要求時]
   ↓
2. 💬 [Post card in a chat or channel]
   Team: 選擇您的 Teams
   Channel: 選擇頻道
   Message: @triggerBody()
   ↓
3. 🔄 [Apply to each]
   選取先前步驟的輸出: @triggerBody()?['mail']
   ↓
   └─ 📧 [發送電子郵件 (V2)]
	  收件者: @{items('Apply_to_each')}
	  主旨: DeviceBox 警報通知
	  內文: 
		警報標題: @{triggerBody()?['body'][0]['text']}
		詳細資訊: (從 facts 提取)
```

### 進階版：單一郵件發送給所有人

```
1. 📥 [HTTP 觸發器]
   ↓
2. 📊 [初始化變數]
   名稱: EmailList
   類型: 字串
   值: @{join(triggerBody()?['mail'], ';')}
   ↓
3. 💬 [Post card in Teams]
   Message: @triggerBody()
   ↓
4. ❓ [條件]
   @variables('EmailList') 不等於 空字串
   ↓
   是 → 📧 [發送電子郵件]
		收件者: @{variables('EmailList')}
   否 → ⏭️ [跳過]
```

### 進階版：條件式通知（只在有 Email 時發送）

```
1. 📥 [HTTP 觸發器]
   ↓
2. 💬 [Post card in Teams]
   ↓
3. ❓ [條件]
   @length(triggerBody()?['mail']) 大於 0
   ↓
   是 → 🔄 [Apply to each]
		└─ 📧 發送郵件
   否 → ⏭️ 跳過
```

### 超進階：包含 Adaptive Card 內容的郵件

```
1. 📥 [HTTP 觸發器]
   ↓
2. 📊 [撰寫 - 建立郵件內容]
   輸入:
   <html>
   <body>
   <h2>@{triggerBody()?['body'][0]['text']}</h2>
   <table>
   @{join(
	 items('解析_Facts')?['title'], 
	 ': ',
	 items('解析_Facts')?['value'],
	 '<br>'
   )}
   </table>
   </body>
   </html>
   ↓
3. 🔄 [Apply to each]
   ↓
   └─ 📧 [發送 HTML 格式郵件]
```

---

## 🧪 測試步驟

### 1. 設定多組測試 Email

```xml
<Email>your-email1@company.com,your-email2@company.com,your-email3@company.com</Email>
```

### 2. 觸發警報

調整閾值以觸發警報

### 3. 檢查 Debug 輸出

```
[Config] Teams Notification Email: user1@company.com,user2@company.com,user3@company.com
[Teams通知] 通知聯絡人數量: 3
[Teams通知]   - user1@company.com
[Teams通知]   - user2@company.com
[Teams通知]   - user3@company.com
[Teams通知] JSON: {"mail":["user1@company.com","user2@company.com","user3@company.com"],...}
```

### 4. 檢查 Power Automate 執行記錄

在 Flow 執行歷程中：
1. 展開 HTTP 觸發器的輸出
2. 確認 `mail` 欄位是陣列格式
3. 確認陣列包含所有設定的 Email

### 5. 驗證郵件發送

如果設定了 Apply to Each：
- 每個 Email 應該收到一封郵件
- 檢查郵件內容是否正確

---

## ⚠️ 注意事項

### 1. 模式差異

| 功能 | Power Automate | Incoming Webhook |
|------|----------------|------------------|
| Mail 欄位 | ✅ 包含在 JSON 中 | ❌ 不支援 |
| 自動郵件 | ✅ 可設定 | ❌ 不可 |
| 設定複雜度 | 較高 | 較低 |

### 2. Email 格式驗證

程式**不會**驗證 Email 格式的正確性，請確保：
- Email 地址正確無誤
- 沒有多餘的空格
- 使用正確的分隔符號（逗號）

### 3. 隱私考量

Email 地址會包含在發送的 JSON 中，請確保：
- Webhook URL 的安全性
- 不要在公開場合洩漏 Webhook URL

---

## 🔧 常見問題

### Q1: Email 可以留空嗎？

**答**：可以。如果留空，`mail` 欄位會是空陣列 `[]`。在 Power Automate 中可以用條件判斷 `length(triggerBody()?['mail']) > 0` 來決定是否發送郵件。

### Q2: 如何設定多個收件者？

**答**：
- **方式 1（推薦）**：用逗號分隔
  ```xml
  <Email>user1@company.com,user2@company.com,user3@company.com</Email>
  ```
- **方式 2**：用分號分隔
  ```xml
  <Email>user1@company.com;user2@company.com;user3@company.com</Email>
  ```
- **方式 3**：混合使用（會自動處理）

### Q3: 如何在 Power Automate 中發送給所有人？

**答**：使用 `join()` 函數：
```
join(triggerBody()?['mail'], ';')
```
結果：`user1@company.com;user2@company.com;user3@company.com`

然後直接放入「發送電子郵件」動作的「收件者」欄位。

### Q4: 如何分別發送給每個人？

**答**：使用 **Apply to each** 動作：
```
1. Apply to each
   來源: triggerBody()?['mail']
2. 在迴圈內：
   發送電子郵件
   收件者: @{items('Apply_to_each')}
```

### Q5: Teams Incoming Webhook 支援多個 Email 嗎？

**答**：功能支援，但 MessageCard 格式不會使用此欄位。如需 Email 功能，請使用 Power Automate。

### Q6: Email 地址前後有空格會影響嗎？

**答**：不會。程式會自動移除每個 Email 前後的空格。例如：
```xml
<Email> user1@c.com , user2@c.com , user3@c.com </Email>
```
會被正確解析為：`["user1@c.com", "user2@c.com", "user3@c.com"]`

### Q7: 如何驗證 Email 格式？

**答**：程式**不會**驗證 Email 格式。請確保：
- Email 地址正確無誤
- 使用正確的分隔符號（逗號或分號）
- 沒有不可見的特殊字符

### Q8: 能設定多少個 Email？

**答**：技術上沒有限制，但建議：
- 一般使用：2-5 個
- 最多建議：10-20 個

太多收件者可能會影響郵件發送效能。

---

## 📊 JSON Schema 參考

### 完整的通知 JSON 結構

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
	"mail": {
	  "type": "array",
	  "description": "聯絡人 Email 地址陣列",
	  "items": {
		"type": "string",
		"format": "email"
	  },
	  "examples": [
		[],
		["maintenance@company.com"],
		["user1@company.com", "user2@company.com", "user3@company.com"]
	  ]
	},
	"type": {
	  "type": "string",
	  "const": "AdaptiveCard"
	},
	"version": {
	  "type": "string",
	  "const": "1.4"
	},
	"body": {
	  "type": "array",
	  "items": {
		"type": "object"
	  }
	}
  },
  "required": ["mail", "type", "version", "body"]
}
```

### Mail 欄位的可能值

```json
// 空陣列（沒有設定 Email）
"mail": []

// 單一 Email
"mail": ["maintenance@company.com"]

// 多個 Email
"mail": [
  "user1@company.com",
  "user2@company.com",
  "user3@company.com"
]

// 實際範例
"mail": [
  "factory1.maintenance@company.com",
  "supervisor@company.com",
  "manager@company.com"
]
```

---

## 📞 技術支援

如有問題，請檢查：
1. config.xml 中 Email 欄位的設定
2. Debug 輸出中的 Email 值
3. Power Automate Flow 執行歷程
4. Teams 通知設定指南.md

更新日期：2024-01-19
版本：2.0 (支援多組人員)
