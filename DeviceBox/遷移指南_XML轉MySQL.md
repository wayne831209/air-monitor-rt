# 設備配置從 XML 遷移到 MySQL 資料庫 - 完整指南

## 📋 概述
本指南將協助您將 `config.xml` 中的工廠、設備和警報上下限設定遷移到 MySQL 資料庫。

---

## 🔧 遷移步驟

### 步驟 1: 在 MySQL 中建立資料庫表

#### 方法 A: 使用 MySQL Workbench (推薦)

1. 開啟 MySQL Workbench
2. 連接到您的 MySQL 伺服器 (192.168.102.182)
3. 選擇資料庫 `ycm_energy`
4. 開啟檔案 `DeviceBox\Database\1_執行資料庫建表.sql`
5. 點擊 ⚡ 執行按鈕

#### 方法 B: 使用命令列

```bash
mysql -h 192.168.102.182 -u Client -p ycm_energy < "DeviceBox\Database\1_執行資料庫建表.sql"
```

輸入密碼: `root`

#### 驗證表是否建立成功

執行以下 SQL 確認:

```sql
USE ycm_energy;
SHOW TABLES LIKE '%factories%';
SHOW TABLES LIKE '%device_config%';
SHOW TABLES LIKE '%alarm_limits%';
```

應該會看到 3 個表:
- `factories` - 工廠資訊
- `device_config` - 設備配置
- `alarm_limits` - 警報上下限

---

### 步驟 2: 執行資料遷移

#### 方法 A: 使用批次檔 (最簡單)

1. 雙擊執行 `DeviceBox\2_執行資料遷移.bat`
2. 按任意鍵繼續
3. 會自動開啟遷移工具視窗
4. 點擊「1. 建立資料庫表」(如果尚未執行步驟1)
5. 點擊「2. 執行資料遷移」
6. 等待遷移完成

#### 方法 B: 使用 Visual Studio

1. 在 Visual Studio 中開啟專案
2. 右鍵點擊專案 → 屬性 → 偵錯
3. 在「命令列引數」中輸入: `--migrate`
4. 按 F5 啟動
5. 會開啟遷移工具視窗
6. 點擊「2. 執行資料遷移」

#### 方法 C: 使用命令列

```powershell
cd "DeviceBox\bin\Debug"
.\DeviceBox.exe --migrate
```

---

### 步驟 3: 驗證遷移結果

執行以下 SQL 檢查資料:

```sql
-- 查看所有工廠
SELECT * FROM factories;

-- 查看所有設備
SELECT f.name AS 工廠名稱, d.device_type AS 設備類型, d.name AS 設備名稱, d.machine_no AS 機台編號
FROM device_config d
JOIN factories f ON d.factory_id = f.id
ORDER BY f.id, d.device_type, d.machine_no;

-- 查看警報上下限
SELECT f.name AS 工廠名稱, 
	   a.pressure_upper AS 壓力上限, 
	   a.pressure_lower AS 壓力下限,
	   a.temp_upper AS 溫度上限,
	   a.temp_lower AS 溫度下限
FROM alarm_limits a
JOIN factories f ON a.factory_id = f.id;

-- 統計資料
SELECT 
	(SELECT COUNT(*) FROM factories) AS 工廠數量,
	(SELECT COUNT(*) FROM device_config) AS 設備數量,
	(SELECT COUNT(*) FROM alarm_limits) AS 上下限設定數量;
```

---

## 🎯 遷移後使用方式

### 正常啟動程式

遷移完成後,直接啟動程式即可:

```powershell
.\DeviceBox.exe
```

程式會自動從資料庫載入所有設備配置。

### 動態新增設備

現在您可以直接在資料庫中新增設備,程式會自動偵測並更新介面(每5秒檢查一次)!

#### 新增工廠範例:

```sql
INSERT INTO factories (id, name, modbus_ip, modbus_port, enabled, sort_order)
VALUES (7, '新工廠', '192.168.210.120', '502', 1, 7);
```

#### 新增空壓機範例:

```sql
INSERT INTO device_config 
(factory_id, device_type, machine_no, name, enabled, 
 run_di, alarm_di, fault_di, control_do, is_ready, is_remote)
VALUES 
(1, 'Compressor', 2, 'CO-39', 1, 10, 11, 12, 1, 3, 4);
```

#### 設定警報上下限範例:

```sql
INSERT INTO alarm_limits (factory_id, pressure_upper, pressure_lower, temp_upper, temp_lower)
VALUES (7, 8.5, 6.5, 45.0, 10.0)
ON DUPLICATE KEY UPDATE
	pressure_upper = VALUES(pressure_upper),
	pressure_lower = VALUES(pressure_lower),
	temp_upper = VALUES(temp_upper),
	temp_lower = VALUES(temp_lower);
```

---

## ⚠️ 注意事項

### 遷移前準備

1. **備份 config.xml**
   ```powershell
   Copy-Item config.xml config.xml.backup
   ```

2. **確認資料庫連線**
   - 伺服器: 192.168.102.182
   - 資料庫: ycm_energy
   - 使用者: Client
   - 密碼: root

3. **確認 config.xml 格式正確**

### 遷移中注意

- 如果工廠名稱重複,會跳過該工廠
- 如果設備已存在(相同 factory_id + device_type + machine_no),會跳過該設備
- 遷移過程中請勿關閉程式

### 遷移後

- **保留 config.xml** - 建議保留作為備份
- **資料庫連線設定** - 仍然在 config.xml 中的 `<Database>` 區段
- **Teams 通知設定** - 仍然在 config.xml 中的 `<TeamsNotification>` 區段

---

## 🔍 常見問題

### Q1: 遷移失敗,顯示「資料庫連線失敗」

**解答:**
1. 檢查 config.xml 中的資料庫設定
2. 測試資料庫連線:
   ```powershell
   mysql -h 192.168.102.182 -u Client -p ycm_energy
   ```
3. 確認防火牆規則

### Q2: 遷移後程式沒有顯示設備

**解答:**
1. 檢查資料庫中是否有資料:
   ```sql
   SELECT COUNT(*) FROM factories;
   SELECT COUNT(*) FROM device_config;
   ```
2. 檢查 enabled 欄位是否為 1
3. 重新啟動程式

### Q3: 如何重新遷移?

**解答:**
如果需要重新遷移,先清空資料庫:

```sql
-- 小心:這會刪除所有資料!
DELETE FROM device_config;
DELETE FROM alarm_limits;
DELETE FROM factories;

-- 重置自動遞增ID
ALTER TABLE factories AUTO_INCREMENT = 1;
ALTER TABLE device_config AUTO_INCREMENT = 1;
ALTER TABLE alarm_limits AUTO_INCREMENT = 1;
```

然後重新執行遷移工具。

### Q4: 如何在資料庫中修改設備設定?

**解答:**
直接在資料庫中修改即可,程式會在5秒內自動更新:

```sql
-- 修改設備名稱
UPDATE device_config 
SET name = 'CO-28-NEW'
WHERE factory_id = 1 AND device_type = 'Compressor' AND machine_no = 1;

-- 停用設備
UPDATE device_config 
SET enabled = 0
WHERE id = 5;

-- 修改警報上限
UPDATE alarm_limits 
SET pressure_upper = 9.0
WHERE factory_id = 1;
```

### Q5: 可以同時使用 XML 和資料庫嗎?

**解答:**
不行,程式會優先使用資料庫。如果要恢復使用 XML,需要修改程式碼將 `LoadFactoriesFromDatabase()` 改回 `LoadFactorySettings()`。

---

## 📊 資料庫結構說明

### factories 表 (工廠資訊)
| 欄位名稱 | 類型 | 說明 |
|---------|------|------|
| id | INT | 工廠ID (主鍵) |
| name | VARCHAR(100) | 工廠名稱 (唯一) |
| modbus_ip | VARCHAR(50) | Modbus IP位址 |
| modbus_port | VARCHAR(10) | Modbus Port |
| enabled | TINYINT(1) | 是否啟用 (0/1) |
| sort_order | INT | 排序順序 |

### device_config 表 (設備配置)
| 欄位名稱 | 類型 | 說明 |
|---------|------|------|
| id | INT | 設備ID (主鍵) |
| factory_id | INT | 工廠ID (外鍵) |
| device_type | VARCHAR(50) | 設備類型 |
| machine_no | INT | 機台編號 |
| name | VARCHAR(100) | 設備名稱 |
| enabled | TINYINT(1) | 是否啟用 |
| run_di, alarm_di, ... | INT | IO 設定 |

### alarm_limits 表 (警報上下限)
| 欄位名稱 | 類型 | 說明 |
|---------|------|------|
| id | INT | 設定ID (主鍵) |
| factory_id | INT | 工廠ID (外鍵,唯一) |
| pressure_upper | DECIMAL(10,2) | 壓力上限 |
| pressure_lower | DECIMAL(10,2) | 壓力下限 |
| temp_upper | DECIMAL(10,2) | 溫度上限 |
| temp_lower | DECIMAL(10,2) | 溫度下限 |

---

## 📞 技術支援

如有任何問題,請聯繫開發團隊或參考:
- 專案文件: `README.md`
- 程式碼: `DeviceDatabase.cs`, `Config.cs`, `DeviceMigrationTool.cs`

---

**最後更新:** 2024-01
**版本:** 1.0
