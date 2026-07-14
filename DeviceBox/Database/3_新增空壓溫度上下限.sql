-- ====================================================
-- 步驟 3: 新增空壓溫度上下限欄位到 alarm_limits 表
-- 請使用 MySQL Workbench 或其他 MySQL 客戶端工具執行
-- 資料庫: ycm_energy
-- ====================================================

USE ycm_energy;

-- 檢查並新增 pressuretemp_upper 和 pressuretemp_lower 欄位
ALTER TABLE `alarm_limits`
ADD COLUMN IF NOT EXISTS `pressuretemp_upper` DECIMAL(10,2) NULL COMMENT '空壓溫度上限 (°C)' AFTER `temp_lower`,
ADD COLUMN IF NOT EXISTS `pressuretemp_lower` DECIMAL(10,2) NULL COMMENT '空壓溫度下限 (°C)' AFTER `pressuretemp_upper`;

-- 查詢更新後的表結構
DESC alarm_limits;

-- 查詢現有資料（確認現有工廠的空壓溫度限制預設為 NULL）
SELECT 
	al.id,
	f.name AS factory_name,
	al.pressure_upper,
	al.pressure_lower,
	al.temp_upper,
	al.temp_lower,
	al.pressuretemp_upper,
	al.pressuretemp_lower,
	al.updated_at
FROM alarm_limits al
LEFT JOIN factories f ON al.factory_id = f.id
ORDER BY f.id;

-- 如果需要為現有工廠設定預設值，可執行以下語句（依需求調整數值）:
-- UPDATE alarm_limits SET pressuretemp_upper = 50.0, pressuretemp_lower = 10.0 WHERE factory_id = 1;
-- UPDATE alarm_limits SET pressuretemp_upper = 50.0, pressuretemp_lower = 10.0 WHERE factory_id = 2;
