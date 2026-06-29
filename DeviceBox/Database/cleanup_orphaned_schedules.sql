-- 清理重複的 schedule_items
-- 此腳本會移除不被任何模式使用的排程項目

-- 1. 查看目前的狀況
SELECT 
	'1. Total schedule_items' as Info,
	COUNT(*) as Count
FROM schedule_items
UNION ALL
SELECT 
	'2. Used by modes' as Info,
	COUNT(DISTINCT schedule_id) as Count
FROM mode_schedule_mapping
UNION ALL
SELECT 
	'3. Orphaned (unused)' as Info,
	COUNT(*) as Count
FROM schedule_items si
WHERE si.id NOT IN (SELECT DISTINCT schedule_id FROM mode_schedule_mapping);

-- 2. 顯示孤立的排程項目（不被任何模式使用的）
SELECT 
	si.id,
	si.factory_name,
	si.device_name,
	si.start_time,
	si.end_time
FROM schedule_items si
WHERE si.id NOT IN (SELECT DISTINCT schedule_id FROM mode_schedule_mapping)
ORDER BY si.id;

-- 3. 刪除孤立的排程項目（執行此行前請先確認上面的查詢結果）
-- DELETE FROM schedule_items 
-- WHERE id NOT IN (SELECT DISTINCT schedule_id FROM mode_schedule_mapping);

-- 4. 查看每個模式使用了多少排程
SELECT 
	sm.id as mode_id,
	sm.name as mode_name,
	COUNT(msm.schedule_id) as schedule_count
FROM schedule_modes sm
LEFT JOIN mode_schedule_mapping msm ON sm.id = msm.mode_id
GROUP BY sm.id, sm.name
ORDER BY sm.id;

-- 5. 重設 AUTO_INCREMENT（如果需要重新開始編號）
-- ALTER TABLE schedule_modes AUTO_INCREMENT = 1;
-- ALTER TABLE schedule_items AUTO_INCREMENT = 1;
