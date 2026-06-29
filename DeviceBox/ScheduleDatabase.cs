using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace DeviceBox
{
    /// <summary>
    /// 排程資料庫管理類別
    /// 負責所有與排程相關的資料庫操作
    /// </summary>
    public class ScheduleDatabase
    {
        private readonly string _connectionString;

        public ScheduleDatabase(string server, string database, string user, string password)
        {
            _connectionString = $"server={server};database={database};uid={user};pwd={password};Connect Timeout=10;CharSet=utf8mb4;";
        }

        #region Mode Operations

        /// <summary>
        /// 從資料庫載入所有模式
        /// </summary>
        public List<ScheduleMode> LoadModesFromDatabase()
        {
            var modes = new List<ScheduleMode>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // 載入所有啟用的模式
                    string sql = @"
                        SELECT id, name, description, is_default, enabled 
                        FROM schedule_modes 
                        WHERE enabled = 1 
                        ORDER BY is_default DESC, name ASC";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var mode = new ScheduleMode
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                                IsDefault = reader.GetBoolean("is_default")
                            };
                            modes.Add(mode);
                        }
                    }

                    // 為每個模式載入其排程
                    foreach (var mode in modes)
                    {
                        mode.Schedules = LoadSchedulesForMode(conn, mode.Id);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] Loaded {modes.Count} modes from database");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] LoadModesFromDatabase failed: {ex.Message}");
            }

            return modes;
        }

        /// <summary>
        /// 取得預設模式
        /// </summary>
        public ScheduleMode GetDefaultMode()
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT id, name, description, is_default, enabled 
                        FROM schedule_modes 
                        WHERE is_default = 1 AND enabled = 1 
                        LIMIT 1";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var mode = new ScheduleMode
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                                IsDefault = true
                            };

                            reader.Close();
                            mode.Schedules = LoadSchedulesForMode(conn, mode.Id);
                            return mode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] GetDefaultMode failed: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 載入特定模式的所有排程
        /// </summary>
        private List<ModeScheduleItem> LoadSchedulesForMode(MySqlConnection conn, int modeId)
        {
            var schedules = new List<ModeScheduleItem>();

            string sql = @"
                SELECT s.id, s.factory_id, s.factory_name, s.device_name, s.machine_no, 
                       s.enabled, s.is_span_mode, s.start_day, s.start_time, 
                       s.end_day, s.end_time, s.repeat_days
                FROM schedule_items s
                INNER JOIN mode_schedule_mapping msm ON s.id = msm.schedule_id
                WHERE msm.mode_id = @modeId
                ORDER BY msm.sort_order, s.id";

            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@modeId", modeId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var schedule = new ModeScheduleItem
                        {
                            FactoryId = reader.GetInt32("factory_id"),
                            FactoryName = reader.GetString("factory_name"),
                            DeviceName = reader.GetString("device_name"),
                            MachineNo = reader.GetInt32("machine_no"),
                            Enabled = reader.GetBoolean("enabled"),
                            IsSpanMode = reader.GetBoolean("is_span_mode"),
                            StartDay = (DayOfWeek)reader.GetByte("start_day"),
                            StartTime = reader.GetTimeSpan("start_time"),
                            EndDay = (DayOfWeek)reader.GetByte("end_day"),
                            EndTime = reader.GetTimeSpan("end_time")
                        };

                        // 解析重複日期
                        if (!reader.IsDBNull(reader.GetOrdinal("repeat_days")))
                        {
                            string repeatDaysStr = reader.GetString("repeat_days");
                            if (!string.IsNullOrEmpty(repeatDaysStr))
                            {
                                schedule.RepeatDays = repeatDaysStr.Split(',')
                                    .Select(d => (DayOfWeek)int.Parse(d.Trim()))
                                    .ToList();
                            }
                        }

                        schedules.Add(schedule);
                    }
                }
            }

            return schedules;
        }

        /// <summary>
        /// 儲存或更新模式
        /// </summary>
        public bool SaveMode(ScheduleMode mode)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            int modeId = mode.Id;

                            // 如果是新模式（Id = 0），插入新記錄
                            if (modeId == 0)
                            {
                                // 如果設為預設，先取消其他預設
                                if (mode.IsDefault)
                                {
                                    string clearDefaultSql = "UPDATE schedule_modes SET is_default = 0 WHERE is_default = 1";
                                    using (var clearCmd = new MySqlCommand(clearDefaultSql, conn, transaction))
                                    {
                                        clearCmd.ExecuteNonQuery();
                                    }
                                }

                                string insertSql = @"
                                    INSERT INTO schedule_modes (name, description, is_default, enabled) 
                                    VALUES (@name, @description, @isDefault, 1);
                                    SELECT LAST_INSERT_ID();";

                                using (var cmd = new MySqlCommand(insertSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@name", mode.Name);
                                    cmd.Parameters.AddWithValue("@description", mode.Description ?? "");
                                    cmd.Parameters.AddWithValue("@isDefault", mode.IsDefault);

                                    modeId = Convert.ToInt32(cmd.ExecuteScalar());
                                    mode.Id = modeId;
                                }
                            }
                            else
                            {
                                // 更新現有模式
                                if (mode.IsDefault)
                                {
                                    string clearDefaultSql = "UPDATE schedule_modes SET is_default = 0 WHERE is_default = 1 AND id != @id";
                                    using (var clearCmd = new MySqlCommand(clearDefaultSql, conn, transaction))
                                    {
                                        clearCmd.Parameters.AddWithValue("@id", modeId);
                                        clearCmd.ExecuteNonQuery();
                                    }
                                }

                                string updateSql = @"
                                    UPDATE schedule_modes 
                                    SET name = @name, description = @description, is_default = @isDefault 
                                    WHERE id = @id";

                                using (var cmd = new MySqlCommand(updateSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@id", modeId);
                                    cmd.Parameters.AddWithValue("@name", mode.Name);
                                    cmd.Parameters.AddWithValue("@description", mode.Description ?? "");
                                    cmd.Parameters.AddWithValue("@isDefault", mode.IsDefault);
                                    cmd.ExecuteNonQuery();
                                }

                                // 刪除舊的排程關聯和排程項目
                                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] Deleting old schedules for mode {modeId}");

                                // 先取得此模式相關的 schedule_id
                                string getScheduleIdsSql = "SELECT schedule_id FROM mode_schedule_mapping WHERE mode_id = @modeId";
                                var scheduleIds = new List<int>();
                                using (var cmd = new MySqlCommand(getScheduleIdsSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@modeId", modeId);
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            scheduleIds.Add(reader.GetInt32(0));
                                        }
                                    }
                                }

                                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] Found {scheduleIds.Count} old schedule items to delete");

                                // 刪除關聯
                                string deleteMappingSql = "DELETE FROM mode_schedule_mapping WHERE mode_id = @modeId";
                                using (var cmd = new MySqlCommand(deleteMappingSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@modeId", modeId);
                                    int mappingDeleted = cmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] Deleted {mappingDeleted} mapping records");
                                }

                                // 刪除排程項目（只刪除不被其他模式使用的）
                                foreach (var scheduleId in scheduleIds)
                                {
                                    // 檢查此 schedule_id 是否還被其他模式使用
                                    string checkUsageSql = "SELECT COUNT(*) FROM mode_schedule_mapping WHERE schedule_id = @scheduleId";
                                    using (var cmd = new MySqlCommand(checkUsageSql, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@scheduleId", scheduleId);
                                        int usageCount = Convert.ToInt32(cmd.ExecuteScalar());

                                        if (usageCount == 0)
                                        {
                                            // 沒有其他模式使用，可以刪除
                                            string deleteScheduleSql = "DELETE FROM schedule_items WHERE id = @scheduleId";
                                            using (var delCmd = new MySqlCommand(deleteScheduleSql, conn, transaction))
                                            {
                                                delCmd.Parameters.AddWithValue("@scheduleId", scheduleId);
                                                delCmd.ExecuteNonQuery();
                                                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] Deleted unused schedule_item {scheduleId}");
                                            }
                                        }
                                    }
                                }
                            }

                            // 儲存排程項目
                            if (mode.Schedules != null && mode.Schedules.Count > 0)
                            {
                                SaveSchedulesForMode(conn, transaction, modeId, mode.Schedules);
                            }

                            transaction.Commit();
                            System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] Mode '{mode.Name}' saved successfully");
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] SaveMode failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 儲存模式的排程項目
        /// </summary>
        private void SaveSchedulesForMode(MySqlConnection conn, MySqlTransaction transaction, int modeId, List<ModeScheduleItem> schedules)
        {
            int sortOrder = 0;
            foreach (var schedule in schedules)
            {
                // 插入或更新排程項目
                string insertScheduleSql = @"
                    INSERT INTO schedule_items 
                    (factory_id, factory_name, device_name, machine_no, enabled, is_span_mode, 
                     start_day, start_time, end_day, end_time, repeat_days)
                    VALUES 
                    (@factoryId, @factoryName, @deviceName, @machineNo, @enabled, @isSpanMode, 
                     @startDay, @startTime, @endDay, @endTime, @repeatDays);
                    SELECT LAST_INSERT_ID();";

                int scheduleId;
                using (var cmd = new MySqlCommand(insertScheduleSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@factoryId", schedule.FactoryId);
                    cmd.Parameters.AddWithValue("@factoryName", schedule.FactoryName);
                    cmd.Parameters.AddWithValue("@deviceName", schedule.DeviceName);
                    cmd.Parameters.AddWithValue("@machineNo", schedule.MachineNo);
                    cmd.Parameters.AddWithValue("@enabled", schedule.Enabled);
                    cmd.Parameters.AddWithValue("@isSpanMode", schedule.IsSpanMode);
                    cmd.Parameters.AddWithValue("@startDay", (byte)schedule.StartDay);
                    cmd.Parameters.AddWithValue("@startTime", schedule.StartTime);
                    cmd.Parameters.AddWithValue("@endDay", (byte)schedule.EndDay);
                    cmd.Parameters.AddWithValue("@endTime", schedule.EndTime);

                    string repeatDaysStr = schedule.RepeatDays != null && schedule.RepeatDays.Count > 0
                        ? string.Join(",", schedule.RepeatDays.Select(d => (int)d))
                        : null;
                    cmd.Parameters.AddWithValue("@repeatDays", (object)repeatDaysStr ?? DBNull.Value);

                    scheduleId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 建立關聯
                string insertMappingSql = @"
                    INSERT INTO mode_schedule_mapping (mode_id, schedule_id, sort_order) 
                    VALUES (@modeId, @scheduleId, @sortOrder)";

                using (var cmd = new MySqlCommand(insertMappingSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@modeId", modeId);
                    cmd.Parameters.AddWithValue("@scheduleId", scheduleId);
                    cmd.Parameters.AddWithValue("@sortOrder", sortOrder++);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 刪除模式
        /// </summary>
        public bool DeleteMode(int modeId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // 檢查是否為預設模式
                    string checkSql = "SELECT is_default FROM schedule_modes WHERE id = @id";
                    using (var cmd = new MySqlCommand(checkSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", modeId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && Convert.ToBoolean(result))
                        {
                            System.Diagnostics.Debug.WriteLine("[ScheduleDatabase] Cannot delete default mode");
                            return false;
                        }
                    }

                    // 軟刪除（設為停用）
                    string deleteSql = "UPDATE schedule_modes SET enabled = 0 WHERE id = @id";
                    using (var cmd = new MySqlCommand(deleteSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", modeId);
                        cmd.ExecuteNonQuery();
                    }

                    System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] Mode {modeId} deleted successfully");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] DeleteMode failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 測試資料庫連線
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] Connection test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 檢查資料庫表是否存在
        /// </summary>
        public bool CheckTablesExist()
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT COUNT(*) FROM information_schema.tables 
                        WHERE table_schema = DATABASE() 
                        AND table_name IN ('schedule_modes', 'schedule_items', 'mode_schedule_mapping')";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count == 3;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleDatabase] CheckTablesExist failed: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
