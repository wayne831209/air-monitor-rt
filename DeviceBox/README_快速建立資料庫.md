# 資料庫建立快速指南

## 🎯 目標
建立資料庫表並匯入通知設定到 MySQL

---

## 📋 方法一:使用 MySQL Workbench (推薦)

### 步驟:

1. **開啟 MySQL Workbench**

2. **連線到資料庫**
   - Host: `192.168.102.182`
   - Database: `ycm_energy`
   - User: `Client`
   - Password: `root`

3. **執行 SQL 腳本**

   打開檔案:
   ```
   DeviceBox\Database\完整建表並匯入範例資料.sql
   ```

   按下執行按鈕 (⚡ 閃電圖示)

4. **查看結果**

   應該會看到:
   - ✅ 4 個表建立成功
   - ✅ 3 筆通知設定匯入成功

---

## 📋 方法二:使用命令列

### Windows CMD:

```cmd
cd "路徑\到\DeviceBox"
mysql -h 192.168.102.182 -u Client -proot ycm_energy < "Database\完整建表並匯入範例資料.sql"
```

### PowerShell:

```powershell
cd "路徑\到\DeviceBox"
Get-Content "Database\完整建表並匯入範例資料.sql" | mysql -h 192.168.102.182 -u Client -proot ycm_energy
```

---

## 📋 方法三:使用應用程式遷移工具

### 步驟:

1. **開啟遷移工具**

   雙擊執行:
   ```
   DeviceBox\bin\Debug\DeviceBox.exe --migrate
   ```

   或在命令列:
   ```cmd
   cd DeviceBox
   .\bin\Debug\DeviceBox.exe --migrate
   ```

2. **在遷移視窗中**

   - 點擊「建立資料表」按鈕
   - 等待建立完成
   - 點擊「開始遷移」按鈕
   - 查看遷移結果

---

## ✅ 驗證建立結果

### 使用 SQL 查詢:

```sql
-- 1. 檢查表是否存在
USE ycm_energy;
SHOW TABLES;

-- 應該看到:
-- alarm_limits
-- device_config
-- factories
-- notification_settings

-- 2. 檢查通知設定
SELECT * FROM notification_settings;

-- 應該看到 3 筆資料:
-- teams_enabled: true
-- teams_webhook_url: https://...
-- teams_email: wayne.li@ycmcnc.com,...

-- 3. 檢查表結構
DESCRIBE notification_settings;
```

### 使用診斷工具:

```cmd
.\bin\Debug\DeviceBox.exe --diagnostic
```

---

## 📊 建立的表結構

| 表名 | 說明 | 用途 |
|------|------|------|
| `factories` | 工廠資訊 | 儲存工廠名稱、Modbus IP/Port |
| `device_config` | 設備配置 | 儲存各種設備的 IO 設定 |
| `alarm_limits` | 警報上下限 | 儲存壓力和溫度的告警閾值 |
| `notification_settings` | 通知設定 | 儲存 Teams/Email 等通知配置 |

---

## 🔧 已匯入的通知設定

根據您的 `config.xml`,已自動匯入以下設定:

| 設定項 | 值 |
|--------|-----|
| `teams_enabled` | `true` (啟用) |
| `teams_webhook_url` | Power Automate Webhook URL |
| `teams_email` | `wayne.li@ycmcnc.com,wayne.li@ycmcnc.com` |

---

## 🚀 下一步驟

資料庫表建立完成後,請執行以下步驟匯入工廠和設備資料:

### 1. 執行遷移工具

```cmd
.\bin\Debug\DeviceBox.exe --migrate
```

或雙擊:
```
2_執行資料遷移.bat
```

### 2. 啟動應用程式

```cmd
.\bin\Debug\DeviceBox.exe
```

應用程式會自動從資料庫載入所有設定!

---

## ❓ 常見問題

### Q: mysql 命令找不到?

**A:** 請使用 MySQL Workbench 方法,或確認 MySQL 已安裝並加入 PATH

### Q: 權限錯誤?

**A:** 確認資料庫帳號 `Client` 有建表權限

### Q: 表已存在錯誤?

**A:** SQL 腳本使用 `CREATE TABLE IF NOT EXISTS`,不會覆蓋現有表。如需重建:

```sql
DROP TABLE IF EXISTS notification_settings;
DROP TABLE IF EXISTS device_config;
DROP TABLE IF EXISTS alarm_limits;
DROP TABLE IF EXISTS factories;
```

然後重新執行建表腳本。

---

## 📞 需要協助?

如果遇到問題:

1. 查看 `問題排查指南.md`
2. 執行診斷工具: `.\bin\Debug\DeviceBox.exe --diagnostic`
3. 檢查 SQL 腳本: `Database\診斷_檢查通知設定.sql`
