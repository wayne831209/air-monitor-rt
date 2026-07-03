-- ====================================================
-- 步驟 1: 在 MySQL 中執行此腳本建立資料庫表
-- 請使用 MySQL Workbench 或其他 MySQL 客戶端工具執行
-- 資料庫: ycm_energy
-- ====================================================

USE ycm_energy;

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

-- 建立 notification_settings 表
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

-- 建立 site_config 表
CREATE TABLE IF NOT EXISTS `site_config` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '配置ID',
  `site_id` VARCHAR(50) NOT NULL COMMENT '場域ID (例如: other, foundry)',
  `site_name` VARCHAR(100) NOT NULL COMMENT '場域名稱 (例如: 其他場域, 鑄造廠)',
  `current_mode_id` INT NULL COMMENT '當前排程模式ID',
  `config_data` TEXT NULL COMMENT '其他場域特定配置(JSON格式)',
  `config_version` INT NOT NULL DEFAULT 1 COMMENT '配置版本號(用於同步)',
  `last_updated_by` VARCHAR(100) NULL COMMENT '最後更新者(電腦名稱或用戶)',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新時間',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_site_id` (`site_id`),
  KEY `idx_config_version` (`config_version`),
  KEY `idx_updated_at` (`updated_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='場域配置表';

-- 插入預設場域
INSERT INTO site_config (site_id, site_name, current_mode_id, last_updated_by)
VALUES 
  ('other', '其他場域', NULL, 'SYSTEM'),
  ('foundry', '鑄造廠', NULL, 'SYSTEM')
ON DUPLICATE KEY UPDATE updated_at = updated_at;

-- 完成!
SELECT '資料庫表建立完成!' AS message;
