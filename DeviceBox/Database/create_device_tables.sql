-- ====================================================
-- 設備配置管理系統 - 資料庫表結構
-- Database: ycm_energy
-- Created: 2024
-- Description: 儲存工廠、設備配置和警報上下限設定
-- ====================================================

-- ====================================================
-- Table: factories (工廠基本資訊表)
-- 說明:儲存所有工廠的基本資訊和 Modbus 連線設定
-- ====================================================
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

-- ====================================================
-- Table: alarm_limits (警報上下限設定表)
-- 說明:儲存每個工廠的壓力和溫度警報上下限
-- ====================================================
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

-- ====================================================
-- Table: device_config (設備配置表)
-- 說明:儲存所有設備的配置資訊,包含類型、名稱和IO設定
-- ====================================================
CREATE TABLE IF NOT EXISTS `device_config` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '設備ID',
  `factory_id` INT NOT NULL COMMENT '工廠ID',
  `device_type` VARCHAR(50) NOT NULL COMMENT '設備類型 (Compressor/Precooler/Dryer/Fan/PressureSensor/ReadyStatus)',
  `machine_no` INT NOT NULL COMMENT '機台編號',
  `name` VARCHAR(100) NOT NULL COMMENT '設備名稱',
  `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否啟用 (0=停用, 1=啟用)',

  -- IO 設定欄位 (各設備類型使用不同欄位,未使用的保持NULL)
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

-- ====================================================
-- Table: notification_settings (通知設定表)
-- 說明:儲存系統通知設定(Teams、Email等)
-- ====================================================
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
-- 索引優化建議
-- ====================================================
-- 如果查詢效能有問題,可考慮新增以下複合索引:
-- CREATE INDEX idx_device_factory_enabled ON device_config(factory_id, enabled, device_type);

-- ====================================================
-- 查詢範例
-- ====================================================
-- 1. 取得所有啟用的工廠及其設備
-- SELECT f.*, d.* 
-- FROM factories f
-- LEFT JOIN device_config d ON f.id = d.factory_id
-- WHERE f.enabled = 1 AND (d.enabled = 1 OR d.enabled IS NULL)
-- ORDER BY f.sort_order, d.sort_order;

-- 2. 取得特定工廠的所有 Compressor 設備
-- SELECT * FROM device_config 
-- WHERE factory_id = 1 AND device_type = 'Compressor' AND enabled = 1;

-- 3. 取得 Teams 通知設定
-- SELECT setting_key, setting_value 
-- FROM notification_settings 
-- WHERE setting_key IN ('teams_enabled', 'teams_webhook_url', 'teams_email');

-- 3. 取得工廠的警報上下限設定
-- SELECT f.name, a.* 
-- FROM alarm_limits a
-- INNER JOIN factories f ON a.factory_id = f.id
-- WHERE f.enabled = 1;

-- 4. 列出所有工廠及其警報設定 (包含沒有設定的工廠)
-- SELECT f.*, a.pressure_upper, a.pressure_lower, a.temp_upper, a.temp_lower
-- FROM factories f
-- LEFT JOIN alarm_limits a ON f.id = a.factory_id
-- WHERE f.enabled = 1
-- ORDER BY f.sort_order;
