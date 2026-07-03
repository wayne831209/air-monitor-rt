-- ========================================
-- 診斷場域同步問題
-- ========================================

USE ycm_energy;

-- 1. 檢查場域配置表結構
DESCRIBE site_config;

-- 2. 查看所有場域的當前狀態
SELECT 
	site_id AS '場域ID',
	site_name AS '場域名稱',
	current_mode_id AS '當前模式ID',
	config_version AS '配置版本',
	last_updated_by AS '最後更新者',
	updated_at AS '更新時間'
FROM site_config
ORDER BY updated_at DESC;

-- 3. 檢查是否有模式資料
SELECT 
	id AS '模式ID',
	name AS '模式名稱',
	description AS '描述',
	is_default AS '是否預設'
FROM schedule_modes
ORDER BY id;

-- 4. 模擬場域更新並觀察
-- 4.1 記錄更新前的狀態
SELECT '=== 更新前狀態 ===' AS '';
SELECT site_id, site_name, current_mode_id, config_version 
FROM site_config;

-- 4.2 更新"其他場域"到模式2
UPDATE site_config 
SET current_mode_id = 2,
	config_version = config_version + 1,
	last_updated_by = 'TEST-B',
	updated_at = CURRENT_TIMESTAMP
WHERE site_id = 'other';

-- 4.3 檢查更新後的狀態
SELECT '=== 更新"其他場域"後 ===' AS '';
SELECT 
	site_id AS '場域ID',
	site_name AS '場域名稱', 
	current_mode_id AS '當前模式',
	config_version AS '版本',
	last_updated_by AS '更新者'
FROM site_config;

-- 5. 驗證"鑄造廠"是否受影響
SELECT '=== 鑄造廠應該不變 ===' AS '';
SELECT 
	site_id,
	current_mode_id,
	config_version,
	last_updated_by,
	updated_at
FROM site_config
WHERE site_id = 'foundry';

-- 6. 重置測試
-- 恢復初始狀態
UPDATE site_config SET current_mode_id = NULL, config_version = 0 WHERE site_id = 'other';
UPDATE site_config SET current_mode_id = NULL, config_version = 0 WHERE site_id = 'foundry';

SELECT '=== 已重置所有場域 ===' AS '';
SELECT * FROM site_config;

-- ========================================
-- 診斷建議：
-- ========================================
-- 如果 SQL 測試顯示"鑄造廠"確實不會被"其他場域"的更新影響，
-- 那問題可能在：
-- 
-- 1. 同一台電腦上兩個軟體實例共用了某些全域變數
-- 2. SiteManager.Instance 的 CurrentSiteId 被意外修改
-- 3. ConfigSyncService 被錯誤地建立多次，或使用了錯誤的 siteId
-- 
-- 請在測試時檢查輸出視窗的 Debug 訊息：
-- - [ConfigSyncService] Initialized for site: XXX
-- - [MainForm] Config updated for site XXX
-- - [MainForm] Ignoring config update from different site
