using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DeviceBox
{
    /// <summary>
    /// 資料遷移工具
    /// 將 config.xml 中的 Mode 資料匯入資料庫
    /// </summary>
    public class MigrationTool
    {
        private ScheduleDatabase _scheduleDb;
        private string _configPath;

        public MigrationTool(ScheduleDatabase scheduleDb, string configPath = null)
        {
            _scheduleDb = scheduleDb;
            _configPath = configPath ?? Path.Combine(Application.StartupPath, "config.xml");
        }

        /// <summary>
        /// 執行遷移：從 config.xml 匯入 Modes 到資料庫
        /// </summary>
        public bool MigrateModesFromXml()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MigrationTool] Starting migration from config.xml");

                if (!File.Exists(_configPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[MigrationTool] Config file not found: {_configPath}");
                    return false;
                }

                // 檢查資料庫連線
                if (!_scheduleDb.TestConnection())
                {
                    System.Diagnostics.Debug.WriteLine("[MigrationTool] Database connection failed");
                    return false;
                }

                // 檢查資料庫表是否存在
                if (!_scheduleDb.CheckTablesExist())
                {
                    System.Diagnostics.Debug.WriteLine("[MigrationTool] Database tables not found. Please run create_schedule_tables.sql first");
                    return false;
                }

                // 載入 config.xml
                XDocument doc = XDocument.Load(_configPath);
                var modesElement = doc.Root?.Element("Modes");

                if (modesElement == null)
                {
                    System.Diagnostics.Debug.WriteLine("[MigrationTool] No Modes section found in config.xml");
                    return false;
                }

                // 先載入現有模式一次，避免重複查詢
                var existingModes = _scheduleDb.LoadModesFromDatabase();
                System.Diagnostics.Debug.WriteLine($"[MigrationTool] Existing modes in database: {existingModes.Count}");
                foreach (var existing in existingModes)
                {
                    System.Diagnostics.Debug.WriteLine($"[MigrationTool]   - {existing.Name} (ID={existing.Id})");
                }

                int successCount = 0;
                int failCount = 0;
                int skippedCount = 0;

                // 解析每個 Mode
                var modeElements = modesElement.Elements("Mode").ToList();
                System.Diagnostics.Debug.WriteLine($"[MigrationTool] Found {modeElements.Count} modes in config.xml");

                foreach (var modeElement in modeElements)
                {
                    try
                    {
                        var mode = ParseModeFromXml(modeElement);
                        if (mode != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[MigrationTool] Processing mode: {mode.Name} with {mode.Schedules.Count} schedules");

                            // 檢查是否已存在同名模式
                            if (existingModes.Any(m => m.Name == mode.Name))
                            {
                                System.Diagnostics.Debug.WriteLine($"[MigrationTool] Mode '{mode.Name}' already exists, skipping");
                                skippedCount++;
                                continue;
                            }

                            // 儲存到資料庫
                            if (_scheduleDb.SaveMode(mode))
                            {
                                successCount++;
                                System.Diagnostics.Debug.WriteLine($"[MigrationTool] ✓ Migrated mode: {mode.Name} (ID={mode.Id})");
                            }
                            else
                            {
                                failCount++;
                                System.Diagnostics.Debug.WriteLine($"[MigrationTool] ✗ Failed to migrate mode: {mode.Name}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[MigrationTool] ✗ Failed to parse mode (returned null)");
                            failCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        System.Diagnostics.Debug.WriteLine($"[MigrationTool] ✗ Error parsing mode: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[MigrationTool] Migration complete: {successCount} succeeded, {failCount} failed, {skippedCount} skipped");
                return successCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MigrationTool] Migration failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 從 XML 元素解析 Mode
        /// </summary>
        private ScheduleMode ParseModeFromXml(XElement modeElement)
        {
            var mode = new ScheduleMode
            {
                Id = 0, // 新模式
                Name = modeElement.Attribute("name")?.Value ?? "",
                Description = modeElement.Attribute("description")?.Value ?? "",
                IsDefault = bool.Parse(modeElement.Attribute("isDefault")?.Value ?? "false"),
                Schedules = new List<ModeScheduleItem>()
            };

            if (string.IsNullOrEmpty(mode.Name))
            {
                return null;
            }

            // 解析排程
            foreach (var scheduleElement in modeElement.Elements("Schedule"))
            {
                try
                {
                    var schedule = ParseScheduleFromXml(scheduleElement);
                    if (schedule != null)
                    {
                        mode.Schedules.Add(schedule);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MigrationTool] Error parsing schedule: {ex.Message}");
                }
            }

            return mode;
        }

        /// <summary>
        /// 從 XML 元素解析 Schedule
        /// </summary>
        private ModeScheduleItem ParseScheduleFromXml(XElement scheduleElement)
        {
            var schedule = new ModeScheduleItem
            {
                FactoryId = int.Parse(scheduleElement.Attribute("factoryId")?.Value ?? "0"),
                FactoryName = scheduleElement.Attribute("factoryName")?.Value ?? "",
                DeviceName = scheduleElement.Attribute("deviceName")?.Value ?? "",
                MachineNo = int.Parse(scheduleElement.Attribute("machineNo")?.Value ?? "1"),
                Enabled = bool.Parse(scheduleElement.Attribute("enabled")?.Value ?? "true"),
                IsSpanMode = bool.Parse(scheduleElement.Attribute("isSpanMode")?.Value ?? "true")
            };

            // 解析開始日期和時間
            if (scheduleElement.Attribute("startDay") != null)
            {
                DayOfWeek sd;
                if (Enum.TryParse(scheduleElement.Attribute("startDay").Value, out sd))
                    schedule.StartDay = sd;
            }

            if (scheduleElement.Attribute("start") != null)
            {
                schedule.StartTime = TimeSpan.Parse(scheduleElement.Attribute("start").Value);
            }

            // 解析結束日期和時間
            if (scheduleElement.Attribute("endDay") != null)
            {
                DayOfWeek ed;
                if (Enum.TryParse(scheduleElement.Attribute("endDay").Value, out ed))
                    schedule.EndDay = ed;
            }

            if (scheduleElement.Attribute("end") != null)
            {
                schedule.EndTime = TimeSpan.Parse(scheduleElement.Attribute("end").Value);
            }

            // 解析重複日期（可能用 days 或 repeatDays 屬性）
            string daysStr = scheduleElement.Attribute("days")?.Value ?? scheduleElement.Attribute("repeatDays")?.Value;
            if (!string.IsNullOrEmpty(daysStr))
            {
                schedule.RepeatDays = daysStr.Split(',')
                    .Select(d => (DayOfWeek)int.Parse(d.Trim()))
                    .ToList();
            }

            return schedule;
        }

        /// <summary>
        /// 顯示遷移對話框（帶 UI）
        /// </summary>
        public static void ShowMigrationDialog()
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

                // 建立資料庫連線
                var scheduleDb = new ScheduleDatabase(config.IP, config.DB, config.USER, config.Password);

                // 測試連線
                if (!scheduleDb.TestConnection())
                {
                    MessageBox.Show("無法連接到資料庫，請檢查連線設定", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 檢查資料庫表
                if (!scheduleDb.CheckTablesExist())
                {
                    var result = MessageBox.Show(
                        "資料庫表結構不存在，是否要開啟 SQL 建表腳本資料夾？\n\n請先執行 create_schedule_tables.sql 建立資料表", 
                        "資料庫表不存在", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        string sqlPath = Path.Combine(Application.StartupPath, "Database");
                        if (Directory.Exists(sqlPath))
                        {
                            System.Diagnostics.Process.Start(sqlPath);
                        }
                    }
                    return;
                }

                // 確認遷移
                var confirmResult = MessageBox.Show(
                    "即將將 config.xml 中的 Modes 資料匯入資料庫。\n\n此操作不會刪除資料庫中現有的資料，但會略過同名的模式。\n\n確定要繼續嗎？",
                    "確認資料遷移",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                    return;

                // 執行遷移
                var migrationTool = new MigrationTool(scheduleDb);
                if (migrationTool.MigrateModesFromXml())
                {
                    MessageBox.Show("資料遷移完成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("資料遷移失敗，請檢查 Debug 輸出視窗的詳細訊息", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"資料遷移發生錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
