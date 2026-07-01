-- ========================================
-- 資料庫診斷 SQL
-- 用於檢查遷移是否成功
-- ========================================

USE ycm_energy;

-- 1. 檢查表是否存在
SHOW TABLES LIKE 'factories';
SHOW TABLES LIKE 'device_config';
SHOW TABLES LIKE 'alarm_limits';

-- 2. 檢查工廠資料
SELECT '=== 工廠資料 ===' AS '';
SELECT * FROM factories;
SELECT COUNT(*) AS '工廠總數' FROM factories;

-- 3. 檢查設備資料
SELECT '=== 設備資料 ===' AS '';
SELECT f.name AS 工廠名稱, d.device_type AS 設備類型, d.name AS 設備名稱, 
	   d.machine_no AS 機台編號, d.enabled AS 是否啟用
FROM device_config d
LEFT JOIN factories f ON d.factory_id = f.id
ORDER BY f.id, d.device_type, d.machine_no;
SELECT COUNT(*) AS '設備總數' FROM device_config;

-- 4. 檢查警報上下限
SELECT '=== 警報上下限 ===' AS '';
SELECT f.name AS 工廠名稱, 
	   a.pressure_upper AS 壓力上限, 
	   a.pressure_lower AS 壓力下限,
	   a.temp_upper AS 溫度上限,
	   a.temp_lower AS 溫度下限
FROM alarm_limits a
LEFT JOIN factories f ON a.factory_id = f.id;
SELECT COUNT(*) AS '上下限設定數' FROM alarm_limits;

-- 5. 統計摘要
SELECT '=== 統計摘要 ===' AS '';
SELECT 
	(SELECT COUNT(*) FROM factories) AS 工廠數量,
	(SELECT COUNT(*) FROM device_config) AS 設備數量,
	(SELECT COUNT(*) FROM alarm_limits) AS 上下限設定數量;
