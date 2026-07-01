-- ====================================================
-- 診斷通知設定
-- ====================================================

USE ycm_energy;

-- 1. 檢查 notification_settings 表是否存在
SHOW TABLES LIKE 'notification_settings';

-- 2. 查看表結構
DESCRIBE notification_settings;

-- 3. 查看所有通知設定
SELECT * FROM notification_settings;

-- 4. 查看特定的 Teams 設定
SELECT 
	setting_key,
	setting_value,
	description,
	updated_at
FROM notification_settings
WHERE setting_key IN ('teams_enabled', 'teams_webhook_url', 'teams_email')
ORDER BY setting_key;

-- 5. 統計資料
SELECT 
	COUNT(*) as total_settings,
	SUM(CASE WHEN setting_key LIKE 'teams_%' THEN 1 ELSE 0 END) as teams_settings
FROM notification_settings;

-- ====================================================
-- 測試插入範例資料 (如果表是空的)
-- ====================================================
-- 執行以下 SQL 來插入範例通知設定:
/*
INSERT INTO notification_settings (setting_key, setting_value, description) VALUES
('teams_enabled', 'true', 'Teams 通知是否啟用'),
('teams_webhook_url', 'https://example.webhook.url', 'Teams Webhook URL'),
('teams_email', 'test@example.com', 'Teams 通知郵件地址')
ON DUPLICATE KEY UPDATE 
	setting_value = VALUES(setting_value),
	description = VALUES(description);
*/
