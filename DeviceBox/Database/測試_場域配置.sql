-- ========================================
-- 快速驗證場域配置表
-- ========================================

USE ycm_energy;

-- 1. 檢查 site_config 表是否存在
SELECT 'Checking site_config table...' as status;
SHOW TABLES LIKE 'site_config';

-- 2. 查看所有場域
SELECT 'Available sites:' as status;
SELECT * FROM site_config;

-- 3. 測試更新場域模式
SELECT 'Testing mode update for site: other' as status;
UPDATE site_config 
SET current_mode_id = 1,
	config_version = config_version + 1,
	last_updated_by = 'SETUP_TEST'
WHERE site_id = 'other';

-- 4. 查看更新結果
SELECT * FROM site_config WHERE site_id = 'other';

-- 5. 重置為 NULL(準備使用)
UPDATE site_config 
SET current_mode_id = NULL,
	last_updated_by = 'SYSTEM'
WHERE site_id IN ('other', 'foundry');

SELECT 'Setup complete! Both sites reset to NULL mode.' as status;
SELECT * FROM site_config;
