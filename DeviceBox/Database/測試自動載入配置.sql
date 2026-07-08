-- ========================================
-- 自動載入場域配置測試資料設定
-- ========================================

-- 查看當前場域配置
SELECT 
	site_id as '場域ID',
	site_name as '場域名稱',
	current_mode_id as '當前模式ID',
	config_version as '配置版本',
	last_updated_by as '最後更新者',
	updated_at as '更新時間'
FROM site_config
ORDER BY site_id;

-- ========================================
-- 測試 1：設定其他廠域為模式一 (ID: 15)
-- ========================================

-- 設定其他廠域
UPDATE site_config
SET current_mode_id = 15,
	config_version = config_version + 1,
	last_updated_by = 'TestUser_A',
	updated_at = NOW()
WHERE site_id = 'other';

-- 驗證更新
SELECT 
	site_name as '場域',
	current_mode_id as '模式ID',
	last_updated_by as '更新者',
	updated_at as '更新時間'
FROM site_config
WHERE site_id = 'other';

-- ========================================
-- 測試 2：設定兩個場域不同模式
-- ========================================

-- 其他廠域設為模式一 (ID: 15)
UPDATE site_config
SET current_mode_id = 15,
	config_version = config_version + 1,
	last_updated_by = 'TestUser_A',
	updated_at = NOW()
WHERE site_id = 'other';

-- 鑄造廠設為模式二 (ID: 20)
UPDATE site_config
SET current_mode_id = 20,
	config_version = config_version + 1,
	last_updated_by = 'TestUser_B',
	updated_at = NOW()
WHERE site_id = 'foundry';

-- 驗證兩個場域的設定
SELECT 
	site_id as '場域ID',
	site_name as '場域名稱',
	current_mode_id as '模式ID',
	config_version as '版本',
	last_updated_by as '更新者'
FROM site_config
ORDER BY site_id;

-- ========================================
-- 測試 3：查看可用的模式 ID
-- ========================================

-- 如果您的資料庫有 modes 或 schedule_modes 資料表，可以查詢：
-- SELECT id, name, description FROM modes ORDER BY id;

-- 常見的模式 ID (根據您的實際資料調整)：
-- ID: 15 - 模式一
-- ID: 20 - 模式二
-- ID: 25 - 模式三
-- ID: 1  - 手動模式

-- ========================================
-- 測試 4：模擬多人協作情境
-- ========================================

-- A 人員設定其他廠域為模式一
UPDATE site_config
SET current_mode_id = 15,
	config_version = config_version + 1,
	last_updated_by = 'UserA',
	updated_at = NOW()
WHERE site_id = 'other';

-- 等待一段時間後...

-- B 人員啟動軟體應該看到模式一
-- (直接啟動軟體測試即可)

-- ========================================
-- 測試 5：清除場域設定(恢復預設)
-- ========================================

-- 將模式設為 NULL (無設定)
UPDATE site_config
SET current_mode_id = NULL,
	config_version = config_version + 1,
	last_updated_by = 'Reset',
	updated_at = NOW()
WHERE site_id = 'other';

-- 驗證清除結果
SELECT 
	site_name as '場域',
	current_mode_id as '模式ID (應為NULL)',
	updated_at as '更新時間'
FROM site_config
WHERE site_id = 'other';

-- ========================================
-- 查詢配置歷史(如果有 audit/log 表)
-- ========================================

-- 如果您想追蹤變更歷史，可以考慮建立 audit table：
/*
CREATE TABLE IF NOT EXISTS site_config_audit (
	id INT AUTO_INCREMENT PRIMARY KEY,
	site_id VARCHAR(50),
	site_name VARCHAR(100),
	old_mode_id INT,
	new_mode_id INT,
	changed_by VARCHAR(100),
	changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	INDEX idx_site_time (site_id, changed_at)
);
*/

-- ========================================
-- 快速測試腳本
-- ========================================

-- 場景 A：設定兩個不同的模式，測試切換
UPDATE site_config 
SET current_mode_id = 15, 
	config_version = config_version + 1,
	last_updated_by = 'Test',
	updated_at = NOW()
WHERE site_id = 'other';

UPDATE site_config 
SET current_mode_id = 20,
	config_version = config_version + 1,
	last_updated_by = 'Test',
	updated_at = NOW()
WHERE site_id = 'foundry';

-- 立即查看結果
SELECT 
	site_id,
	site_name,
	current_mode_id,
	config_version,
	DATE_FORMAT(updated_at, '%Y-%m-%d %H:%i:%s') as updated_time
FROM site_config
ORDER BY site_id;

-- ========================================
-- 進階：建立場域配置快照
-- ========================================

-- 備份當前配置
CREATE TABLE IF NOT EXISTS site_config_backup AS
SELECT 
	site_id,
	site_name,
	current_mode_id,
	config_data,
	config_version,
	last_updated_by,
	updated_at,
	NOW() as backup_at
FROM site_config;

-- 查看備份
SELECT * FROM site_config_backup;

-- 從備份恢復
-- UPDATE site_config sc
-- INNER JOIN site_config_backup scb ON sc.site_id = scb.site_id
-- SET sc.current_mode_id = scb.current_mode_id,
--     sc.config_version = scb.config_version;

-- ========================================
-- 疑難排解
-- ========================================

-- 問題 1：載入的模式 ID 不存在
-- 檢查 current_mode_id 是否有效
SELECT 
	sc.site_id,
	sc.current_mode_id,
	'mode exists?' as check_result
FROM site_config sc
WHERE sc.current_mode_id IS NOT NULL;
-- 然後對照您的 modes 資料表確認 ID 是否存在

-- 問題 2：配置沒有更新
-- 檢查 updated_at 時間戳
SELECT 
	site_id,
	site_name,
	updated_at,
	TIMESTAMPDIFF(MINUTE, updated_at, NOW()) as minutes_ago
FROM site_config
ORDER BY updated_at DESC;

-- 問題 3：版本號異常
-- 檢查版本號是否正常遞增
SELECT 
	site_id,
	config_version,
	last_updated_by,
	updated_at
FROM site_config
ORDER BY site_id;

-- 如果需要重置版本號
-- UPDATE site_config SET config_version = 1 WHERE site_id = 'other';
-- UPDATE site_config SET config_version = 1 WHERE site_id = 'foundry';
