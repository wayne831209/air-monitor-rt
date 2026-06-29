-- ====================================================
-- 空壓機排程管理系統 - 資料庫表結構
-- Database: ycm_energy
-- Created: 2024
-- ====================================================

-- ====================================================
-- Table: schedule_modes (排程模式表)
-- 說明：儲存不同的排程模式（如一般模式、高負荷模式、手動模式等）
-- ====================================================
CREATE TABLE IF NOT EXISTS `schedule_modes` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '模式ID',
  `name` VARCHAR(100) NOT NULL COMMENT '模式名稱',
  `description` VARCHAR(500) NULL COMMENT '模式說明',
  `is_default` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否為預設模式 (0=否, 1=是)',
  `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否啟用 (0=停用, 1=啟用)',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新時間',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='排程模式表';

-- ====================================================
-- Table: schedule_items (排程項目表)
-- 說明：儲存具體的排程設定，包含時間範圍、設備資訊等
-- ====================================================
CREATE TABLE IF NOT EXISTS `schedule_items` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '排程ID',
  `factory_id` INT NOT NULL COMMENT '廠區ID',
  `factory_name` VARCHAR(100) NOT NULL COMMENT '廠區名稱',
  `device_name` VARCHAR(100) NOT NULL COMMENT '設備名稱',
  `machine_no` INT NOT NULL COMMENT '機台編號',
  `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否啟用 (0=停用, 1=啟用)',
  `is_span_mode` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '排程模式 (0=重複模式, 1=跨日模式)',
  `start_day` TINYINT NOT NULL DEFAULT 1 COMMENT '開始星期 (0=Sunday, 1=Monday, ..., 6=Saturday)',
  `start_time` TIME NOT NULL COMMENT '開始時間',
  `end_day` TINYINT NOT NULL DEFAULT 5 COMMENT '結束星期',
  `end_time` TIME NOT NULL COMMENT '結束時間',
  `repeat_days` VARCHAR(50) NULL COMMENT '重複日期 (逗號分隔，如: 1,2,3,4,5)',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新時間',
  PRIMARY KEY (`id`),
  KEY `idx_factory_device` (`factory_id`, `device_name`, `machine_no`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='排程項目表';

-- ====================================================
-- Table: mode_schedule_mapping (模式排程關聯表)
-- 說明：關聯模式和排程項目的多對多關係
-- ====================================================
CREATE TABLE IF NOT EXISTS `mode_schedule_mapping` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '關聯ID',
  `mode_id` INT NOT NULL COMMENT '模式ID',
  `schedule_id` INT NOT NULL COMMENT '排程ID',
  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序順序',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_mode_schedule` (`mode_id`, `schedule_id`),
  KEY `idx_mode_id` (`mode_id`),
  KEY `idx_schedule_id` (`schedule_id`),
  CONSTRAINT `fk_mapping_mode` FOREIGN KEY (`mode_id`) REFERENCES `schedule_modes` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_mapping_schedule` FOREIGN KEY (`schedule_id`) REFERENCES `schedule_items` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='模式排程關聯表';

-- ====================================================
-- 初始資料：建立預設模式
-- ====================================================
INSERT INTO `schedule_modes` (`name`, `description`, `is_default`, `enabled`) 
VALUES ('一般模式', '標準作業排程', 1, 1)
ON DUPLICATE KEY UPDATE `description` = VALUES(`description`);

-- ====================================================
-- 索引優化建議
-- ====================================================
-- 如果查詢效能有問題，可考慮新增以下複合索引：
-- CREATE INDEX idx_schedule_factory_device ON schedule_items(factory_id, device_name, machine_no, enabled);
-- CREATE INDEX idx_mode_enabled ON schedule_modes(enabled, is_default);

-- ====================================================
-- 查詢範例
-- ====================================================
-- 1. 取得預設模式及其所有排程
-- SELECT m.*, s.* 
-- FROM schedule_modes m
-- LEFT JOIN mode_schedule_mapping msm ON m.id = msm.mode_id
-- LEFT JOIN schedule_items s ON msm.schedule_id = s.id
-- WHERE m.is_default = 1 AND m.enabled = 1;

-- 2. 取得特定設備的所有排程
-- SELECT s.* 
-- FROM schedule_items s
-- INNER JOIN mode_schedule_mapping msm ON s.id = msm.schedule_id
-- INNER JOIN schedule_modes m ON msm.mode_id = m.id
-- WHERE m.enabled = 1 AND s.factory_id = 6 AND s.device_name = 'CO-33';

-- 3. 取得所有模式清單
-- SELECT * FROM schedule_modes WHERE enabled = 1 ORDER BY is_default DESC, name ASC;
