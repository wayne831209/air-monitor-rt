-- ====================================================
-- 完整建表並匯入範例資料
-- 執行此腳本將建立所有表並從 config.xml 匯入範例通知設定
-- ====================================================

USE ycm_energy;

-- ====================================================
-- 1. 建立所有必要的表
-- ====================================================

-- 建立 factories 表
CREATE TABLE IF NOT EXISTS `factories` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '工廠ID',
  `name` VARCHAR(100) NOT NULL COMMENT '工廠名稱',
  `modbus_ip` VARCHAR(50) NOT NULL COMMENT 'Modbus IP位址',
  `modbus_port` VARCHAR(10) NOT NULL COMMENT 'Modbus Port',
  `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否啟用 (0=停用, 1=啟用)',
  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序順序',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新時間',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_name` (`name`),
  KEY `idx_enabled` (`enabled`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='工廠基本資訊表';

-- 建立 alarm_limits 表
CREATE TABLE IF NOT EXISTS `alarm_limits` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '設定ID',
  `factory_id` INT NOT NULL COMMENT '工廠ID',
  `pressure_upper` DECIMAL(10,2) NULL COMMENT '壓力上限 (kg/cm²)',
  `pressure_lower` DECIMAL(10,2) NULL COMMENT '壓力下限 (kg/cm²)',
  `temp_upper` DECIMAL(10,2) NULL COMMENT '溫度上限 (°C)',
  `temp_lower` DECIMAL(10,2) NULL COMMENT '溫度下限 (°C)',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新時間',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_factory_id` (`factory_id`),
  CONSTRAINT `fk_alarm_limits_factory` FOREIGN KEY (`factory_id`) REFERENCES `factories` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='警報上下限設定表';

-- 建立 device_config 表
CREATE TABLE IF NOT EXISTS `device_config` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '設備ID',
  `factory_id` INT NOT NULL COMMENT '工廠ID',
  `device_type` VARCHAR(50) NOT NULL COMMENT '設備類型 (Compressor/Precooler/Dryer/Fan/PressureSensor/ReadyStatus)',
  `machine_no` INT NOT NULL COMMENT '機台編號',
  `name` VARCHAR(100) NOT NULL COMMENT '設備名稱',
  `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否啟用 (0=停用, 1=啟用)',

  -- IO 設定欄位
  `run_di` INT NULL COMMENT 'Compressor: 運轉DI',
  `alarm_di` INT NULL COMMENT 'Compressor: 警報DI',
  `fault_di` INT NULL COMMENT 'Compressor/其他: 故障DI',
  `control_do` INT NULL COMMENT 'Compressor: 控制DO',
  `is_ready` INT NULL COMMENT 'Compressor: 就緒狀態',
  `is_remote` INT NULL COMMENT 'Compressor: 遠端模式',

  `on_di` INT NULL COMMENT 'Precooler/Dryer/Fan: 開啟DI',
  `off_di` INT NULL COMMENT 'Precooler/Dryer/Fan: 關閉DI',

  `ready_di` INT NULL COMMENT 'ReadyStatus: 就緒DI',
  `adam` VARCHAR(20) NULL COMMENT 'ReadyStatus: ADAM型號',

  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序順序',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新時間',

  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_factory_device` (`factory_id`, `device_type`, `machine_no`),
  KEY `idx_factory_type` (`factory_id`, `device_type`),
  KEY `idx_enabled` (`enabled`),
  CONSTRAINT `fk_device_factory` FOREIGN KEY (`factory_id`) REFERENCES `factories` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='設備配置表';

-- 建立 notification_settings 表 (新增)
CREATE TABLE IF NOT EXISTS `notification_settings` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '設定ID',
  `setting_key` VARCHAR(100) NOT NULL COMMENT '設定鍵值 (teams_enabled, teams_webhook_url, teams_email等)',
  `setting_value` TEXT NULL COMMENT '設定值',
  `description` VARCHAR(255) NULL COMMENT '設定說明',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新時間',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_setting_key` (`setting_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='通知設定表';

-- ====================================================
-- 2. 插入範例通知設定(根據您的 config.xml)
-- ====================================================

INSERT INTO notification_settings (setting_key, setting_value, description) VALUES
('teams_enabled', 'true', 'Teams 通知是否啟用'),
('teams_webhook_url', 'https://default03259ba3b3ec47e38585f76b24aee3.4b.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/e4d9a397f1834619b8e4d0a2b3a851a6/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=IFSA__dJ74eWQiMfEVICiBOJXueasRgEwmCBKuqpgjE', 'Teams Webhook URL'),
('teams_email', 'wayne.li@ycmcnc.com,wayne.li@ycmcnc.com', 'Teams 通知郵件地址')
ON DUPLICATE KEY UPDATE 
	setting_value = VALUES(setting_value),
	description = VALUES(description),
	updated_at = CURRENT_TIMESTAMP;

-- ====================================================
-- 3. 驗證建立結果
-- ====================================================

-- 顯示所有表
SHOW TABLES LIKE '%factories%';
SHOW TABLES LIKE '%device_config%';
SHOW TABLES LIKE '%alarm_limits%';
SHOW TABLES LIKE '%notification_settings%';

-- 顯示通知設定
SELECT '=== 通知設定 ===' as Info;
SELECT * FROM notification_settings;

-- 統計
SELECT '=== 統計資訊 ===' as Info;
SELECT 
	(SELECT COUNT(*) FROM factories) as factories_count,
	(SELECT COUNT(*) FROM device_config) as devices_count,
	(SELECT COUNT(*) FROM alarm_limits) as alarm_limits_count,
	(SELECT COUNT(*) FROM notification_settings) as notification_settings_count;

SELECT '建表完成! 現在請執行 DeviceBox.exe --migrate 來匯入工廠和設備資料' as Message;
