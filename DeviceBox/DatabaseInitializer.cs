using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DeviceBox
{
    /// <summary>
    /// 資料庫初始化工具
    /// 自動建立排程管理所需的資料表
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string server, string database, string user, string password)
        {
            _connectionString = $"server={server};database={database};uid={user};pwd={password};Connect Timeout=10;CharSet=utf8mb4;";
        }

        /// <summary>
        /// 建立所有排程管理表
        /// </summary>
        public bool CreateScheduleTables()
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // 1. 建立 schedule_modes 表
                    string createModesTable = @"
CREATE TABLE IF NOT EXISTS `schedule_modes` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '模式ID',
  `name` VARCHAR(100) NOT NULL COMMENT '模式名稱',
  `description` VARCHAR(500) NULL COMMENT '模式說明',
  `is_default` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否為預設模式',
  `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否啟用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新時間',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='排程模式表';";

                    ExecuteNonQuery(conn, createModesTable, "schedule_modes");

                    // 2. 建立 schedule_items 表
                    string createItemsTable = @"
CREATE TABLE IF NOT EXISTS `schedule_items` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '排程ID',
  `factory_id` INT NOT NULL COMMENT '廠區ID',
  `factory_name` VARCHAR(100) NOT NULL COMMENT '廠區名稱',
  `device_name` VARCHAR(100) NOT NULL COMMENT '設備名稱',
  `machine_no` INT NOT NULL COMMENT '機台編號',
  `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否啟用',
  `is_span_mode` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '排程模式',
  `start_day` TINYINT NOT NULL DEFAULT 1 COMMENT '開始星期',
  `start_time` TIME NOT NULL COMMENT '開始時間',
  `end_day` TINYINT NOT NULL DEFAULT 5 COMMENT '結束星期',
  `end_time` TIME NOT NULL COMMENT '結束時間',
  `repeat_days` VARCHAR(50) NULL COMMENT '重複日期',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新時間',
  PRIMARY KEY (`id`),
  KEY `idx_factory_device` (`factory_id`, `device_name`, `machine_no`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='排程項目表';";

                    ExecuteNonQuery(conn, createItemsTable, "schedule_items");

                    // 3. 建立 mode_schedule_mapping 表
                    string createMappingTable = @"
CREATE TABLE IF NOT EXISTS `mode_schedule_mapping` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '關聯ID',
  `mode_id` INT NOT NULL COMMENT '模式ID',
  `schedule_id` INT NOT NULL COMMENT '排程ID',
  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序順序',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '建立時間',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_mode_schedule` (`mode_id`, `schedule_id`),
  KEY `idx_mode_id` (`mode_id`),
  KEY `idx_schedule_id` (`schedule_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='模式排程關聯表';";

                    ExecuteNonQuery(conn, createMappingTable, "mode_schedule_mapping");

                    // 4. 建立外鍵約束（如果表已存在則略過）
                    try
                    {
                        string addForeignKeys = @"
ALTER TABLE `mode_schedule_mapping`
ADD CONSTRAINT `fk_mapping_mode` FOREIGN KEY (`mode_id`) REFERENCES `schedule_modes` (`id`) ON DELETE CASCADE,
ADD CONSTRAINT `fk_mapping_schedule` FOREIGN KEY (`schedule_id`) REFERENCES `schedule_items` (`id`) ON DELETE CASCADE;";

                        ExecuteNonQuery(conn, addForeignKeys, "foreign keys");
                    }
                    catch
                    {
                        // 外鍵可能已存在，忽略錯誤
                        System.Diagnostics.Debug.WriteLine("[DatabaseInitializer] Foreign keys might already exist");
                    }

                    // 5. 插入預設模式
                    string insertDefaultMode = @"
INSERT INTO `schedule_modes` (`name`, `description`, `is_default`, `enabled`) 
VALUES ('一般模式', '標準作業排程', 1, 1)
ON DUPLICATE KEY UPDATE `description` = VALUES(`description`);";

                    ExecuteNonQuery(conn, insertDefaultMode, "default mode");

                    System.Diagnostics.Debug.WriteLine("[DatabaseInitializer] All tables created successfully");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DatabaseInitializer] CreateScheduleTables failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 執行 SQL 指令
        /// </summary>
        private void ExecuteNonQuery(MySqlConnection conn, string sql, string description)
        {
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 60;
                cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"[DatabaseInitializer] Created: {description}");
            }
        }

        /// <summary>
        /// 檢查表是否存在
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
                System.Diagnostics.Debug.WriteLine($"[DatabaseInitializer] CheckTablesExist failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 顯示資料庫初始化對話框
        /// </summary>
        public static void ShowInitializeDialog()
        {
            try
            {
                // 載入 config
                var config = new Config();
                if (!config.LoadConfig())
                {
                    MessageBox.Show("無法載入 config.xml", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 建立初始化工具
                var initializer = new DatabaseInitializer(config.IP, config.DB, config.USER, config.Password);

                // 檢查表是否已存在
                if (initializer.CheckTablesExist())
                {
                    var result = MessageBox.Show(
                        "資料庫表已存在！\n\n是否要重新建立？（這不會刪除現有資料）",
                        "資料庫表已存在",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                        return;
                }
                else
                {
                    var result = MessageBox.Show(
                        "即將在資料庫中建立排程管理表：\n\n" +
                        "- schedule_modes (模式表)\n" +
                        "- schedule_items (排程項目表)\n" +
                        "- mode_schedule_mapping (關聯表)\n\n" +
                        $"資料庫：{config.DB}\n" +
                        $"伺服器：{config.IP}\n\n" +
                        "確定要繼續嗎？",
                        "建立資料庫表",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result != DialogResult.Yes)
                        return;
                }

                // 執行建立
                if (initializer.CreateScheduleTables())
                {
                    MessageBox.Show(
                        "資料庫表建立成功！\n\n" +
                        "已建立以下表：\n" +
                        "✓ schedule_modes\n" +
                        "✓ schedule_items\n" +
                        "✓ mode_schedule_mapping\n\n" +
                        "並已建立預設模式「一般模式」",
                        "成功",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "資料庫表建立失敗！\n\n請檢查：\n" +
                        "1. 資料庫連線設定是否正確\n" +
                        "2. 使用者是否有建表權限\n" +
                        "3. Debug 輸出視窗的詳細錯誤訊息",
                        "錯誤",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
