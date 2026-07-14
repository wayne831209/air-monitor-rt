using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace DeviceBox
{
    /// <summary>
    /// 設備資料庫操作類別
    /// 負責處理工廠、設備配置和警報上下限的資料庫操作
    /// </summary>
    public class DeviceDatabase
    {
        private readonly string connectionString;

        public DeviceDatabase(string ip, string database, string user, string password)
        {
            connectionString = $"server={ip};database={database};uid={user};pwd={password};CharSet=utf8mb4;";
        }

        #region Factory Operations

        /// <summary>
        /// 從資料庫載入所有工廠
        /// </summary>
        public List<FactoryConfig> LoadFactories()
        {
            var factories = new List<FactoryConfig>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        SELECT id, name, modbus_ip, modbus_port, enabled, sort_order
                        FROM factories
                        WHERE enabled = 1
                        ORDER BY sort_order, id";

                    using (var command = new MySqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var factory = new FactoryConfig
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                ModbusIp = reader.GetString("modbus_ip"),
                                ModbusPort = reader.GetString("modbus_port"),
                                Devices = new List<DeviceConfig>(),
                                AlarmLimits = new AlarmLimitsConfig()
                            };
                            factories.Add(factory);
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] Loaded {factories.Count} factories from database");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] LoadFactories failed: {ex.Message}");
            }

            return factories;
        }

        /// <summary>
        /// 新增工廠到資料庫
        /// </summary>
        public bool InsertFactory(FactoryConfig factory)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        INSERT INTO factories (name, modbus_ip, modbus_port, enabled, sort_order)
                        VALUES (@name, @modbus_ip, @modbus_port, @enabled, @sort_order)";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@name", factory.Name);
                        command.Parameters.AddWithValue("@modbus_ip", factory.ModbusIp);
                        command.Parameters.AddWithValue("@modbus_port", factory.ModbusPort);
                        command.Parameters.AddWithValue("@enabled", 1);
                        command.Parameters.AddWithValue("@sort_order", factory.Id);

                        int result = command.ExecuteNonQuery();

                        // 取得新增的 ID
                        command.CommandText = "SELECT LAST_INSERT_ID()";
                        factory.Id = Convert.ToInt32(command.ExecuteScalar());

                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] InsertFactory failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Device Operations

        /// <summary>
        /// 從資料庫載入指定工廠的所有設備
        /// </summary>
        public List<DeviceConfig> LoadDevices(int factoryId)
        {
            var devices = new List<DeviceConfig>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        SELECT id, factory_id, device_type, machine_no, name, enabled,
                               run_di, alarm_di, fault_di, control_do, is_ready, is_remote,
                               on_di, off_di, ready_di, adam, sort_order
                        FROM device_config
                        WHERE factory_id = @factory_id AND enabled = 1
                        ORDER BY sort_order, id";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@factory_id", factoryId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var device = new DeviceConfig
                                {
                                    Type = ParseDeviceType(reader.GetString("device_type")),
                                    MachineNo = reader.GetInt32("machine_no"),
                                    Name = reader.GetString("name"),
                                    Enabled = reader.GetBoolean("enabled"),
                                    IO = new IOConfig()
                                };

                                // 根據設備類型載入對應的 IO 設定
                                LoadDeviceIOSettings(reader, device);

                                devices.Add(device);
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] Loaded {devices.Count} devices for factory {factoryId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] LoadDevices failed: {ex.Message}");
            }

            return devices;
        }

        /// <summary>
        /// 載入設備的 IO 設定
        /// </summary>
        private void LoadDeviceIOSettings(MySqlDataReader reader, DeviceConfig device)
        {
            switch (device.Type)
            {
                case DeviceType.Compressor:
                    if (!reader.IsDBNull(reader.GetOrdinal("run_di")))
                        device.IO.RunDI = reader.GetInt32("run_di");
                    if (!reader.IsDBNull(reader.GetOrdinal("alarm_di")))
                        device.IO.AlarmDI = reader.GetInt32("alarm_di");
                    if (!reader.IsDBNull(reader.GetOrdinal("fault_di")))
                        device.IO.FaultDI = reader.GetInt32("fault_di");
                    if (!reader.IsDBNull(reader.GetOrdinal("control_do")))
                        device.IO.ControlDO = reader.GetInt32("control_do");
                    if (!reader.IsDBNull(reader.GetOrdinal("is_ready")))
                        device.IO.IsReadyDI = reader.GetInt32("is_ready");
                    if (!reader.IsDBNull(reader.GetOrdinal("is_remote")))
                        device.IO.IsRemoteDI = reader.GetInt32("is_remote");
                    break;

                case DeviceType.Precooler:
                case DeviceType.Dryer:
                case DeviceType.Fan:
                    if (!reader.IsDBNull(reader.GetOrdinal("on_di")))
                        device.IO.OnDI = reader.GetInt32("on_di");
                    if (!reader.IsDBNull(reader.GetOrdinal("off_di")))
                        device.IO.OffDI = reader.GetInt32("off_di");
                    if (!reader.IsDBNull(reader.GetOrdinal("fault_di")))
                        device.IO.FaultDI = reader.GetInt32("fault_di");
                    break;

                case DeviceType.ReadyStatus:
                    if (!reader.IsDBNull(reader.GetOrdinal("ready_di")))
                        device.IO.ReadyDI = reader.GetInt32("ready_di");
                    if (!reader.IsDBNull(reader.GetOrdinal("adam")))
                        device.IO.Adam = reader.GetString("adam");
                    break;

                case DeviceType.PressureSensor:
                    // PressureSensor 沒有 IO 設定
                    break;
            }
        }

        /// <summary>
        /// 新增設備到資料庫
        /// </summary>
        public bool InsertDevice(int factoryId, DeviceConfig device)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        INSERT INTO device_config 
                        (factory_id, device_type, machine_no, name, enabled, 
                         run_di, alarm_di, fault_di, control_do, is_ready, is_remote,
                         on_di, off_di, ready_di, adam, sort_order)
                        VALUES 
                        (@factory_id, @device_type, @machine_no, @name, @enabled,
                         @run_di, @alarm_di, @fault_di, @control_do, @is_ready, @is_remote,
                         @on_di, @off_di, @ready_di, @adam, @sort_order)";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@factory_id", factoryId);
                        command.Parameters.AddWithValue("@device_type", device.Type.ToString());
                        command.Parameters.AddWithValue("@machine_no", device.MachineNo);
                        command.Parameters.AddWithValue("@name", device.Name);
                        command.Parameters.AddWithValue("@enabled", device.Enabled ? 1 : 0);

                        // IO 設定參數 (使用 DBNull 處理 null 值)
                        command.Parameters.AddWithValue("@run_di", device.IO.RunDI >= 0 ? (object)device.IO.RunDI : DBNull.Value);
                        command.Parameters.AddWithValue("@alarm_di", device.IO.AlarmDI >= 0 ? (object)device.IO.AlarmDI : DBNull.Value);
                        command.Parameters.AddWithValue("@fault_di", device.IO.FaultDI >= 0 ? (object)device.IO.FaultDI : DBNull.Value);
                        command.Parameters.AddWithValue("@control_do", device.IO.ControlDO >= 0 ? (object)device.IO.ControlDO : DBNull.Value);
                        command.Parameters.AddWithValue("@is_ready", device.IO.IsReadyDI >= 0 ? (object)device.IO.IsReadyDI : DBNull.Value);
                        command.Parameters.AddWithValue("@is_remote", device.IO.IsRemoteDI >= 0 ? (object)device.IO.IsRemoteDI : DBNull.Value);
                        command.Parameters.AddWithValue("@on_di", device.IO.OnDI >= 0 ? (object)device.IO.OnDI : DBNull.Value);
                        command.Parameters.AddWithValue("@off_di", device.IO.OffDI >= 0 ? (object)device.IO.OffDI : DBNull.Value);
                        command.Parameters.AddWithValue("@ready_di", device.IO.ReadyDI >= 0 ? (object)device.IO.ReadyDI : DBNull.Value);
                        command.Parameters.AddWithValue("@adam", !string.IsNullOrEmpty(device.IO.Adam) ? (object)device.IO.Adam : DBNull.Value);
                        command.Parameters.AddWithValue("@sort_order", 0);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] InsertDevice failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Alarm Limits Operations

        /// <summary>
        /// 從資料庫載入指定工廠的警報上下限
        /// </summary>
        public AlarmLimitsConfig LoadAlarmLimits(int factoryId)
        {
            var limits = new AlarmLimitsConfig();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        SELECT pressure_upper, pressure_lower, temp_upper, temp_lower,compressedtemp_upper,compressedtemp_lower
                        FROM alarm_limits
                        WHERE factory_id = @factory_id";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@factory_id", factoryId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                limits.PressureUpperLimit = reader.IsDBNull(0) ? double.MaxValue : reader.GetDouble(0);
                                limits.PressureLowerLimit = reader.IsDBNull(1) ? double.MinValue : reader.GetDouble(1);
                                limits.TempUpperLimit = reader.IsDBNull(2) ? double.MaxValue : reader.GetDouble(2);
                                limits.TempLowerLimit = reader.IsDBNull(3) ? double.MinValue : reader.GetDouble(3);
                                limits.CompressedTempUpperLimit = reader.IsDBNull(4) ? double.MaxValue : reader.GetDouble(4);
                                limits.CompressedTempLowerLimit = reader.IsDBNull(5) ? double.MinValue : reader.GetDouble(5);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] LoadAlarmLimits failed: {ex.Message}");
            }

            return limits;
        }

        /// <summary>
        /// 儲存警報上下限到資料庫
        /// </summary>
        public bool SaveAlarmLimits(int factoryId, AlarmLimitsConfig limits)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        INSERT INTO alarm_limits (factory_id, pressure_upper, pressure_lower, temp_upper, temp_lower, compressedtemp_upper, compressedtemp_lower)
                        VALUES (@factory_id, @pressure_upper, @pressure_lower, @temp_upper, @temp_lower, @compressedtemp_upper, @compressedtemp_lower)
                        ON DUPLICATE KEY UPDATE
                            pressure_upper = VALUES(pressure_upper),
                            pressure_lower = VALUES(pressure_lower),
                            temp_upper = VALUES(temp_upper),
                            temp_lower = VALUES(temp_lower),
                            compressedtemp_upper = VALUES(compressedtemp_upper),
                            compressedtemp_lower = VALUES(compressedtemp_lower)";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@factory_id", factoryId);
                        command.Parameters.AddWithValue("@pressure_upper", 
                            limits.PressureUpperLimit == double.MaxValue ? (object)DBNull.Value : limits.PressureUpperLimit);
                        command.Parameters.AddWithValue("@pressure_lower", 
                            limits.PressureLowerLimit == double.MinValue ? (object)DBNull.Value : limits.PressureLowerLimit);
                        command.Parameters.AddWithValue("@temp_upper", 
                            limits.TempUpperLimit == double.MaxValue ? (object)DBNull.Value : limits.TempUpperLimit);
                        command.Parameters.AddWithValue("@temp_lower", 
                            limits.TempLowerLimit == double.MinValue ? (object)DBNull.Value : limits.TempLowerLimit);
                        command.Parameters.AddWithValue("@compressedtemp_upper", 
                            limits.CompressedTempUpperLimit == double.MaxValue ? (object)DBNull.Value : limits.CompressedTempUpperLimit);
                        command.Parameters.AddWithValue("@compressedtemp_lower", 
                            limits.CompressedTempLowerLimit == double.MinValue ? (object)DBNull.Value : limits.CompressedTempLowerLimit);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] SaveAlarmLimits failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 解析設備類型字串
        /// </summary>
        private DeviceType ParseDeviceType(string typeString)
        {
            DeviceType type;
            if (Enum.TryParse(typeString, true, out type))
                return type;

            return DeviceType.Compressor; // 預設值
        }

        /// <summary>
        /// 測試資料庫連線
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] TestConnection failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Notification Settings Operations

        /// <summary>
        /// 從資料庫載入通知設定
        /// </summary>
        /// <returns>設定字典 (key => value)</returns>
        public Dictionary<string, string> LoadNotificationSettings()
        {
            var settings = new Dictionary<string, string>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT setting_key, setting_value FROM notification_settings";

                    using (var command = new MySqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string key = reader.GetString("setting_key");
                            string value = reader.IsDBNull(reader.GetOrdinal("setting_value")) 
                                ? string.Empty 
                                : reader.GetString("setting_value");

                            settings[key] = value;
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] Loaded {settings.Count} notification settings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] LoadNotificationSettings failed: {ex.Message}");
            }

            return settings;
        }

        /// <summary>
        /// 儲存單一通知設定
        /// </summary>
        public void SaveNotificationSetting(string key, string value, string description = null)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        INSERT INTO notification_settings (setting_key, setting_value, description)
                        VALUES (@key, @value, @description)
                        ON DUPLICATE KEY UPDATE 
                            setting_value = @value,
                            description = COALESCE(@description, description),
                            updated_at = CURRENT_TIMESTAMP";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@key", key);
                        command.Parameters.AddWithValue("@value", value ?? string.Empty);
                        command.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] SaveNotificationSetting: {key} = {value}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] SaveNotificationSetting failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 批次儲存通知設定
        /// </summary>
        public void SaveNotificationSettings(Dictionary<string, string> settings)
        {
            foreach (var kvp in settings)
            {
                SaveNotificationSetting(kvp.Key, kvp.Value);
            }
        }

        #endregion

        #region Site Config Operations

        /// <summary>
        /// 載入場域配置
        /// </summary>
        public SiteConfig LoadSiteConfig(string siteId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] LoadSiteConfig called with siteId: '{siteId}'");

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        SELECT site_id, site_name, current_mode_id, config_data, 
                               config_version, last_updated_by, updated_at
                        FROM site_config
                        WHERE site_id = @siteId";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@siteId", siteId);

                        System.Diagnostics.Debug.WriteLine(
                            $"[DeviceDatabase] Executing SQL: {sql.Replace(Environment.NewLine, " ")} " +
                            $"with siteId='{siteId}'");

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var config = new SiteConfig
                                {
                                    SiteId = reader.GetString("site_id"),
                                    SiteName = reader.GetString("site_name"),
                                    CurrentModeId = reader.IsDBNull(reader.GetOrdinal("current_mode_id")) 
                                        ? (int?)null 
                                        : reader.GetInt32("current_mode_id"),
                                    ConfigData = reader.IsDBNull(reader.GetOrdinal("config_data"))
                                        ? null
                                        : reader.GetString("config_data"),
                                    ConfigVersion = reader.GetInt32("config_version"),
                                    LastUpdatedBy = reader.IsDBNull(reader.GetOrdinal("last_updated_by"))
                                        ? null
                                        : reader.GetString("last_updated_by"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                };

                                System.Diagnostics.Debug.WriteLine(
                                    $"[DeviceDatabase] Loaded site config: SiteId={config.SiteId}, " +
                                    $"Mode={config.CurrentModeId}, Version={config.ConfigVersion}");

                                return config;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    $"[DeviceDatabase] No site config found for siteId: '{siteId}'");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] LoadSiteConfig failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] Stack trace: {ex.StackTrace}");
            }

            return null;
        }

        /// <summary>
        /// 載入所有可用場域
        /// </summary>
        public Dictionary<string, string> LoadAvailableSites()
        {
            var sites = new Dictionary<string, string>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT site_id, site_name FROM site_config ORDER BY id";

                    using (var command = new MySqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sites[reader.GetString("site_id")] = reader.GetString("site_name");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] Loaded {sites.Count} available sites");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] LoadAvailableSites failed: {ex.Message}");
            }

            return sites;
        }

        /// <summary>
        /// 更新場域的排程模式
        /// </summary>
        public void UpdateSiteMode(string siteId, int? modeId, string updatedBy)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[DeviceDatabase] *** UpdateSiteMode called *** siteId='{siteId}', modeId={modeId}, updatedBy='{updatedBy}'");

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        UPDATE site_config 
                        SET current_mode_id = @modeId,
                            config_version = config_version + 1,
                            last_updated_by = @updatedBy,
                            updated_at = CURRENT_TIMESTAMP
                        WHERE site_id = @siteId";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@siteId", siteId);
                        command.Parameters.AddWithValue("@modeId", modeId.HasValue ? (object)modeId.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@updatedBy", updatedBy ?? "UNKNOWN");

                        int affected = command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine(
                            $"[DeviceDatabase] *** UpdateSiteMode executed *** " +
                            $"site='{siteId}', mode={modeId}, rows_affected={affected}");

                        if (affected == 0)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"[DeviceDatabase] WARNING: No rows affected! Site '{siteId}' might not exist.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[DeviceDatabase] UpdateSiteMode failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// 更新場域配置資料
        /// </summary>
        public void UpdateSiteConfigData(string siteId, string configData, string updatedBy)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        UPDATE site_config 
                        SET config_data = @configData,
                            config_version = config_version + 1,
                            last_updated_by = @updatedBy,
                            updated_at = CURRENT_TIMESTAMP
                        WHERE site_id = @siteId";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@siteId", siteId);
                        command.Parameters.AddWithValue("@configData", configData ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@updatedBy", updatedBy ?? "UNKNOWN");

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceDatabase] UpdateSiteConfigData failed: {ex.Message}");
                throw;
            }
        }

        #endregion
    }

    #region Site Config Class

    /// <summary>
    /// 場域配置類別
    /// </summary>
    public class SiteConfig
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public int? CurrentModeId { get; set; }
        public string ConfigData { get; set; }
        public int ConfigVersion { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    #endregion
}
