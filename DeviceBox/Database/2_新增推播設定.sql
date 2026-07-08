-- ====================================================
-- 步驟 2: 新增推播設定項目到 notification_settings 表
-- 請使用 MySQL Workbench 或其他 MySQL 客戶端工具執行
-- 資料庫: ycm_energy
-- ====================================================

USE ycm_energy;

-- 新增推播間隔時間設定（預設 5 分鐘）
INSERT INTO notification_settings (setting_key, setting_value, description)
VALUES ('notification_cooldown_minutes', '5', '推播間隔時間（分鐘），防止短時間內重複推播')
ON DUPLICATE KEY UPDATE 
	description = VALUES(description),
	updated_at = CURRENT_TIMESTAMP;

-- 新增超限延遲推播時間設定（預設 0 分鐘，即立即推播）
INSERT INTO notification_settings (setting_key, setting_value, description)
VALUES ('alarm_delay_minutes', '0', '設定值超過上下限後，持續多久才推播（分鐘），0 表示立即推播')
ON DUPLICATE KEY UPDATE 
	description = VALUES(description),
	updated_at = CURRENT_TIMESTAMP;

-- 檢查現有的推播設定
SELECT 
	setting_key AS '設定鍵值',
	setting_value AS '設定值',
	description AS '說明',
	updated_at AS '更新時間'
FROM notification_settings
WHERE setting_key IN (
	'teams_enabled',
	'teams_webhook_url',
	'teams_email',
	'notification_cooldown_minutes',
	'alarm_delay_minutes'
)
ORDER BY setting_key;

-- 完成!
SELECT '推播設定已更新！' AS message;
