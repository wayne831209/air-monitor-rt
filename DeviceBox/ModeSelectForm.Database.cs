using System;
using System.Linq;
using System.Windows.Forms;

namespace DeviceBox
{
    /// <summary>
    /// ModeSelectForm 的部分類別擴充 - 資料庫版本的靜態方法
    /// </summary>
    public partial class ModeSelectForm
    {
        /// <summary>
        /// 從資料庫獲取預設模式 (IsDefault=true)
        /// 如果沒有預設模式，返回第一個模式
        /// </summary>
        public static ScheduleMode GetDefaultModeFromDatabase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[ModeSelectForm] GetDefaultModeFromDatabase() called");

                var config = new Config();
                if (config.LoadConfig())
                {
                    var scheduleDb = config.GetScheduleDatabase();
                    if (scheduleDb != null)
                    {
                        // 嘗試獲取標記為預設的模式
                        var defaultMode = scheduleDb.GetDefaultMode();
                        if (defaultMode != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] Found default mode from DB: {defaultMode.Name} (ID={defaultMode.Id}, Schedules={defaultMode.Schedules.Count})");
                            return defaultMode;
                        }

                        System.Diagnostics.Debug.WriteLine("[ModeSelectForm] No default mode found, loading all modes");

                        // 如果沒有預設模式，返回第一個模式
                        var modes = scheduleDb.LoadModesFromDatabase();
                        if (modes.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] Using first mode as default: {modes[0].Name} (ID={modes[0].Id})");
                            return modes[0];
                        }

                        System.Diagnostics.Debug.WriteLine("[ModeSelectForm] No modes found in database");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[ModeSelectForm] ScheduleDatabase is null");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ModeSelectForm] Config.LoadConfig() failed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] GetDefaultModeFromDatabase error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] Stack trace: {ex.StackTrace}");
            }

            System.Diagnostics.Debug.WriteLine("[ModeSelectForm] GetDefaultModeFromDatabase returning null");
            return null;
        }

        /// <summary>
        /// 儲存模式的排程到資料庫
        /// </summary>
        public static bool SaveModeSchedulesToDatabase(ScheduleMode mode)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] SaveModeSchedulesToDatabase called");
                System.Diagnostics.Debug.WriteLine($"[ModeSelectForm]   Mode: {mode.Name} (ID={mode.Id})");
                System.Diagnostics.Debug.WriteLine($"[ModeSelectForm]   Schedules: {mode.Schedules.Count}");

                var config = new Config();
                if (config.LoadConfig())
                {
                    var scheduleDb = config.GetScheduleDatabase();
                    if (scheduleDb != null)
                    {
                        bool success = scheduleDb.SaveMode(mode);
                        System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] Save result: {success}");

                        if (success)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] Mode '{mode.Name}' saved to database successfully");
                            return true;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] Failed to save mode to database");
                            MessageBox.Show("儲存排程到資料庫失敗", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] ScheduleDatabase is null");
                        MessageBox.Show("無法連接資料庫", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] Config.LoadConfig() failed");
                    MessageBox.Show("載入設定失敗", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] SaveModeSchedulesToDatabase exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ModeSelectForm] Stack trace: {ex.StackTrace}");
                MessageBox.Show($"儲存排程失敗:{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
