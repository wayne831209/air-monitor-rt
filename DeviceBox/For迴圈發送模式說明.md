# For 迴圈發送模式說明

## 📋 功能概述

DeviceBox 現在使用 **for 迴圈** 在程式端處理多組人員的通知發送，每次只發送給一個 Email 地址。

## ✨ 優點

### 對比方案

| 方案 | 處理位置 | 優點 | 缺點 |
|------|----------|------|------|
| **For 迴圈發送（當前）** | 程式端 | Flow 簡單，不需要 Apply to each | 發送多次請求 |
| Array 發送 | Power Automate | 只發送一次請求 | Flow 需要 Apply to each |

### 當前方案的優勢

✅ **Power Automate Flow 保持簡單**
- 不需要 Apply to each 動作
- 直接使用 `@triggerBody()?['mail']` 取得單一 Email
- Flow 設定更直觀

✅ **程式端控制發送邏輯**
- 可以在程式中控制發送順序
- 可以針對每個收件者記錄發送狀態
- 容易添加錯誤處理和重試機制

✅ **詳細的發送日誌**
- 每個收件者都有獨立的發送記錄
- 容易追蹤哪些發送成功、哪些失敗

---

## 🔧 技術實作

### 程式碼邏輯

```csharp
// 如果有多個 Email，用 for 迴圈分別發送
if (_notificationEmails.Length > 0)
{
	for (int i = 0; i < _notificationEmails.Length; i++)
	{
		string email = _notificationEmails[i];
		System.Diagnostics.Debug.WriteLine($"[Teams通知] 發送給: {email} ({i + 1}/{_notificationEmails.Length})");

		string message = await SendTeamsNotificationAsync(title, color, facts, email);
		System.Diagnostics.Debug.WriteLine($"[Teams通知] 已發送給 {email}");
	}

	System.Diagnostics.Debug.WriteLine($"[Teams通知] 已完成發送，共 {_notificationEmails.Length} 位收件者");
}
```

### JSON 格式

每次發送的 JSON 包含**單一** Email：

```json
{
  "mail": "user1@company.com",
  "type": "AdaptiveCard",
  "version": "1.4",
  "body": [...]
}
```

### 發送流程

```
[觸發警報]
	↓
[讀取 Email 列表]
	↓
[For 迴圈開始]
	↓
[發送給 user1@company.com] → Power Automate Flow 1
	↓
[發送給 user2@company.com] → Power Automate Flow 2
	↓
[發送給 user3@company.com] → Power Automate Flow 3
	↓
[For 迴圈結束]
	↓
[記錄完成日誌]
```

---

## 📊 Debug 輸出範例

### 設定 3 個收件者

```xml
<Email>user1@company.com,user2@company.com,user3@company.com</Email>
```

### 觸發警報後的輸出

```
[Teams通知] 通知聯絡人數量: 3
[Teams通知]   - user1@company.com
[Teams通知]   - user2@company.com
[Teams通知]   - user3@company.com
[Teams通知] 發送給: user1@company.com (1/3)
[Teams通知] 準備發送 POST 請求
[Teams通知] 收件者: user1@company.com
[Teams通知] 發送成功
[Teams通知] 空壓超限通知已發送給 user1@company.com
[Teams通知] 發送給: user2@company.com (2/3)
[Teams通知] 準備發送 POST 請求
[Teams通知] 收件者: user2@company.com
[Teams通知] 發送成功
[Teams通知] 空壓超限通知已發送給 user2@company.com
[Teams通知] 發送給: user3@company.com (3/3)
[Teams通知] 準備發送 POST 請求
[Teams通知] 收件者: user3@company.com
[Teams通知] 發送成功
[Teams通知] 空壓超限通知已發送給 user3@company.com
[Teams通知] 已完成發送，共 3 位收件者
```

---

## 🚀 Power Automate Flow 設定（超簡單）

### 完整 Flow

```
1. 📥 [HTTP 觸發器: 當收到 HTTP 要求時]
   ↓
2. 💬 [Post card in a chat or channel]
   Team: 選擇您的 Teams
   Channel: 選擇頻道
   Message: @triggerBody()
   ↓
3. 📧 [發送電子郵件 (V2)]
   收件者: @{triggerBody()?['mail']}
   主旨: DeviceBox 警報通知
   內文: 
	 標題: @{triggerBody()?['body'][0]['text']}
	 詳細資訊: (從 body 提取)
```

### 就這麼簡單！

- ❌ 不需要 Apply to each
- ❌ 不需要 join() 函數
- ❌ 不需要條件判斷
- ✅ 直接使用 `@triggerBody()?['mail']`

---

## ⚙️ 效能考量

### 發送次數

- **1 個 Email** → 發送 1 次
- **3 個 Email** → 發送 3 次
- **5 個 Email** → 發送 5 次

### HTTP 請求數量

每個警報 = Email 數量 × 1 次請求

例如：
- 3 個收件者 → 3 次 HTTP POST 請求
- Power Automate 執行 3 次

### 冷卻機制

由於使用同一個 `_lastNotificationTime`，冷卻時間適用於整個警報事件，而非單一收件者。

5 分鐘內不會重複發送同一警報（無論有多少收件者）。

---

## 🎯 適用場景

### ✅ 推薦使用（當前模式）

- 收件者數量較少（1-10 人）
- 希望 Power Automate Flow 保持簡單
- 需要詳細的發送日誌
- 每個收件者可能有不同的處理邏輯

### ⚠️ 可能需要考慮的情況

- 收件者數量很多（>20 人）
  - 考慮使用批次發送
  - 或改用 Array 模式 + Apply to each
- 需要嚴格控制請求頻率
  - 可能需要添加發送間隔

---

## 📝 設定範例

### config.xml

```xml
<TeamsNotification>
  <Enabled>true</Enabled>
  <WebhookUrl>YOUR_WEBHOOK_URL</WebhookUrl>
  <Email>user1@company.com,user2@company.com,user3@company.com</Email>
</TeamsNotification>
```

### 支援格式

| 格式 | 結果 |
|------|------|
| `user@c.com` | 發送 1 次 |
| `a@c.com,b@c.com` | 發送 2 次 |
| `a@c.com;b@c.com;c@c.com` | 發送 3 次 |
| (空白) | 發送 1 次（mail = ""） |

---

## 🔍 錯誤處理

### 單一收件者失敗

如果某個收件者的發送失敗，不會影響其他收件者：

```
[Teams通知] 發送給: user1@company.com (1/3)
[Teams通知] 發送成功
[Teams通知] 發送給: user2@company.com (2/3)
[Teams通知] 回應錯誤: BadRequest
[Teams通知] 發送給: user3@company.com (3/3)
[Teams通知] 發送成功
```

### Try-Catch 保護

整個發送過程被 try-catch 包裹，任何錯誤都會被記錄但不會中斷程式。

---

## 🧪 測試步驟

### 1. 設定多個測試 Email

```xml
<Email>test1@company.com,test2@company.com</Email>
```

### 2. 觸發警報

調整閾值觸發警報

### 3. 檢查 Debug 輸出

確認：
- [ ] 顯示正確的收件者數量
- [ ] 每個收件者都有發送記錄
- [ ] 顯示完成訊息

### 4. 檢查 Power Automate

確認：
- [ ] Flow 執行了 2 次（對應 2 個 Email）
- [ ] 每次執行的 mail 欄位都是單一 Email
- [ ] Teams 收到 2 張卡片（如果發送到同一頻道）

### 5. 檢查郵件

確認：
- [ ] 每個 Email 都收到郵件
- [ ] 郵件內容正確

---

## 💡 最佳實踐

### ✅ 推薦

1. **適量的收件者**：建議 1-10 人
2. **明確的 Email**：使用角色信箱而非個人信箱
3. **記錄發送狀態**：保留 Debug 日誌以便追蹤
4. **定期測試**：確保所有收件者都能收到

### ❌ 避免

1. **過多收件者**：超過 20 人可能影響效能
2. **頻繁觸發**：依賴冷卻機制避免重複發送
3. **無效 Email**：定期清理無效的 Email 地址

---

## 📚 相關文件

- `Teams通知設定指南.md` - 總體設定說明
- `Teams通知Email功能說明.md` - Email 功能說明（v2.0 Array 模式）
- `多組人員Email設定指南.md` - 多組人員設定
- `config_email_快速參考.md` - 快速參考

---

**更新日期**：2024-01-19  
**版本**：3.0 (For 迴圈發送模式)  
**模式**：程式端迴圈處理
