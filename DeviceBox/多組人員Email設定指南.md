# 多組人員 Email 通知設定快速指南

## ✨ 新功能亮點

DeviceBox 現在支援**多組人員**接收警報通知！

- ✅ 支援單一或多個 Email 地址
- ✅ 自動解析逗號或分號分隔的 Email
- ✅ 在 JSON 中以陣列形式傳送
- ✅ 方便 Power Automate 批量或個別發送郵件

---

## 🚀 快速開始

### 1. 設定 config.xml

```xml
<TeamsNotification>
  <Enabled>true</Enabled>
  <WebhookUrl>YOUR_POWER_AUTOMATE_WEBHOOK_URL</WebhookUrl>
  <Email>user1@company.com,user2@company.com,user3@company.com</Email>
</TeamsNotification>
```

### 2. 啟動程式，檢查 Debug 輸出

```
[Teams通知] 通知聯絡人數量: 3
[Teams通知]   - user1@company.com
[Teams通知]   - user2@company.com
[Teams通知]   - user3@company.com
```

### 3. 觸發警報，檢查 JSON

發送的 JSON 格式：
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

---

## 📋 支援的設定格式

| 說明 | 設定範例 | 結果 |
|------|----------|------|
| 單一人員 | `admin@c.com` | `["admin@c.com"]` |
| 逗號分隔 | `a@c.com,b@c.com,c@c.com` | `["a@c.com", "b@c.com", "c@c.com"]` |
| 分號分隔 | `a@c.com;b@c.com;c@c.com` | `["a@c.com", "b@c.com", "c@c.com"]` |
| 混合分隔 | `a@c.com,b@c.com;c@c.com` | `["a@c.com", "b@c.com", "c@c.com"]` |
| 帶空格 | ` a@c.com , b@c.com ` | `["a@c.com", "b@c.com"]` |
| 空白 | (留空) | `[]` |

---

## 🔧 Power Automate 設定

### 方案 A：發送給所有人（單一郵件）

```
1. HTTP 觸發器
   ↓
2. Post card in Teams
   Message: @triggerBody()
   ↓
3. 初始化變數
   名稱: EmailList
   值: @{join(triggerBody()?['mail'], ';')}
   ↓
4. 發送電子郵件
   收件者: @{variables('EmailList')}
   主旨: DeviceBox 警報通知
```

**結果**：所有人收到同一封郵件

---

### 方案 B：分別發送（每人一封）

```
1. HTTP 觸發器
   ↓
2. Post card in Teams
   Message: @triggerBody()
   ↓
3. Apply to each
   來源: @triggerBody()?['mail']
   ↓
   └─ 發送電子郵件
	  收件者: @{items('Apply_to_each')}
	  主旨: DeviceBox 警報通知 - 給 @{items('Apply_to_each')}
```

**結果**：每個人收到個別的郵件

---

### 方案 C：條件式發送（只在有 Email 時）

```
1. HTTP 觸發器
   ↓
2. Post card in Teams
   ↓
3. 條件判斷
   @length(triggerBody()?['mail']) 大於 0
   ↓
   是 → Apply to each + 發送郵件
   否 → 跳過
```

**結果**：只在設定了 Email 時才發送郵件

---

## 💡 實際應用場景

### 場景 1：不同工廠不同團隊

**裝配一廠**
```xml
<Email>factory1.maintenance@company.com,factory1.supervisor@company.com</Email>
```

**裝配二廠**
```xml
<Email>factory2.maintenance@company.com,factory2.supervisor@company.com</Email>
```

### 場景 2：按警報等級通知

在 Power Automate 中根據警報內容決定：
- 一般警報 → 維護人員
- 嚴重警報 → 維護人員 + 主管 + 經理

### 場景 3：多層級通知

```xml
<Email>
  on-duty@company.com,
  maintenance-team@company.com,
  supervisor@company.com,
  manager@company.com
</Email>
```

使用 Power Automate 的 **延遲** 動作實現升級機制：
- 立即通知值班人員
- 5 分鐘後如果未處理 → 通知維護團隊
- 15 分鐘後如果未處理 → 通知主管
- 30 分鐘後如果未處理 → 通知經理

---

## 🧪 測試檢查清單

### 設定檢查
- [ ] config.xml 中 Email 欄位已正確設定
- [ ] URL 中的 `&` 已替換為 `&amp;`
- [ ] Email 地址格式正確

### 程式執行檢查
- [ ] Debug 輸出顯示正確的 Email 數量
- [ ] 每個 Email 都正確列出
- [ ] 沒有錯誤訊息

### 通知測試
- [ ] Teams 收到卡片通知
- [ ] Power Automate Flow 成功執行
- [ ] 所有設定的 Email 都收到郵件

---

## ❓ 常見問題快速解答

### Q: 最多可以設定幾個 Email？
**A**: 技術上沒有限制，但建議 2-20 個。

### Q: 可以動態改變 Email 嗎？
**A**: 需要修改 config.xml 並重啟程式。

### Q: 如何測試不發送真實郵件？
**A**: 
1. 在 Power Automate 中使用測試 Email
2. 或暫時註解掉「發送電子郵件」動作
3. 只檢查 Flow 執行記錄中的 mail 陣列

### Q: Email 陣列是空的怎麼辦？
**A**: 在 Power Automate 中用條件判斷：
```
@length(triggerBody()?['mail']) > 0
```

### Q: 如何知道是誰收到警報了？
**A**: 
1. 在郵件主旨或內文中包含收件者 Email
2. 或在 Power Automate 中記錄到資料庫/Excel

---

## 📚 相關文件

- `Teams通知設定指南.md` - 完整的 Teams 通知設定
- `Teams通知Email功能說明.md` - Email 功能詳細說明
- `config_email_快速參考.md` - 設定快速參考
- `config_teams_example.xml` - 設定範例檔案

---

## 🎯 最佳實踐

### ✅ 推薦做法
1. 使用逗號分隔（最清楚）
2. 每個環境獨立的 config.xml
3. 定期測試郵件發送功能
4. 在 Power Automate 中記錄發送日誌

### ❌ 避免的做法
1. 過多的收件者（>20）
2. 使用個人 Email（應使用群組或角色郵箱）
3. 沒有測試就上線
4. Email 地址包含特殊字符

---

**更新日期**：2024-01-19  
**版本**：2.0 (支援多組人員)
