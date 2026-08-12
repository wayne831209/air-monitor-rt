using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySQL;

namespace DeviceBox
{
    public partial class MainForm : Form
    {
        // Color Constants
        private static readonly Color StatusRunning = Color.FromArgb(0, 200, 0);      // Green - Running
        private static readonly Color StatusStopped = Color.FromArgb(128, 128, 128);  // Gray - Stopped
        private static readonly Color StatusAlarm = Color.FromArgb(255, 140, 0);      // Orange - Alarm
        private static readonly Color StatusFault = Color.FromArgb(220, 50, 50);      // Red - Fault
        private static readonly Color StatusReady = Color.FromArgb(0, 180, 255);      // Blue - Ready
        private static readonly Color StatusNotReady = Color.FromArgb(100, 100, 100); // Dark Gray - Not Ready
        private static readonly Color StatusDisabled = Color.FromArgb(60, 60, 60);    // Dark Gray - Disabled
        private static readonly Color TextNormal = Color.White;                        // Normal Text
        private static readonly Color ScheduleActive = Color.FromArgb(0, 150, 0);     // Green - Schedule Active
        private static readonly Color ScheduleInactive = Color.FromArgb(150, 0, 0);   // Red - No Schedule
        private static readonly Color StatusOverLimit = Color.FromArgb(255, 50, 50);   // Red - Over Limit

        // View Mode
        private enum ViewMode { OtherFactories, CastingFactory }
        private ViewMode currentViewMode = ViewMode.OtherFactories;
        private int[] currentDisplayIndices = { 0, 1, 2, 3, 4 };  // Factory indices to display
        private const int CASTING_FACTORY_ID = 6;  // Casting Factory ID in config

        // ===== Log 檔案設定 =====
        // 是否啟用 log 檔案記錄 (true=啟用, false=停用)
        private const bool ENABLE_FILE_LOGGING = false;

        // 自動清理舊 log 檔案的天數 (0 = 不自動清理)
        private const int LOG_RETENTION_DAYS = 7;  // 保留最近 7 天的 log
        // ======================

        private Timer updateTimer;
        private List<ModBus_List> modbusList;
        private Config config;
        private ScheduleMode currentMode;  // 當前選擇的模式
        private bool isManualMode = false;    // 是否為手動模式

        // 記錄每個設備上次寫入的 DO 狀態，避免重複寫入
        // Key: "FactoryId_MachineNo", Value: last written DO value (1=on, 0=off)
        private Dictionary<string, ushort> lastDOStates = new Dictionary<string, ushort>();

        // 手動模式下，記錄每個設備的手動 DO 狀態
        // Key: "FactoryId_MachineNo", Value: manual DO value (1=on, 0=off)
        private Dictionary<string, ushort> manualDOStates = new Dictionary<string, ushort>();

        // 記錄每個設備的上次警報/故障狀態，避免重複推播
        // Key: "DeviceName", Value: 狀態 ("正常", "警報", "故障")
        private Dictionary<string, string> lastDeviceAlertStates = new Dictionary<string, string>();

        // 記錄異常的全局開始時間，用於延遲推播
        // 只要有任何設備異常（空壓/溫度超限），就開始計時
        // 所有設備恢復正常後，計時歸零
        private DateTime? globalAbnormalStartTime = null;

        // 記錄當前異常的設備列表（用於追蹤）
        private HashSet<string> currentAbnormalDevices = new HashSet<string>();

        // Teams 通知服務
        private TeamsNotificationService teamsNotificationService;

        // 場域配置同步服務
        private ConfigSyncService syncService;

        public MainForm()
        {
            // 啟用檔案日誌記錄
            EnableFileLogging();

            System.Diagnostics.Debug.WriteLine("=================================================");
            System.Diagnostics.Debug.WriteLine($"[MainForm] *** NEW INSTANCE STARTING *** PID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
            System.Diagnostics.Debug.WriteLine($"[MainForm] Start time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine("=================================================");

            InitializeComponent();
            InitializeConfig();
            InitializeModbus();
            InitializeTimer();
            InitializeFactoryHeaders();
            InitializeCompressorNames();
            InitializeDefaultMode();
            InitializeTeamsNotification();
            InitializeViewBasedSync();  // 新增：根據視圖的動態同步
        }

        /// <summary>
        /// 啟用檔案日誌記錄（用於診斷）
        /// </summary>
        private void EnableFileLogging()
        {
            try
            {
                // 檢查是否啟用 log 功能
                if (!ENABLE_FILE_LOGGING)
                {
                    System.Diagnostics.Debug.WriteLine("[MainForm] File logging is disabled");
                    return;
                }

                // 自動清理舊的 log 檔案
                if (LOG_RETENTION_DAYS > 0)
                {
                    CleanOldLogFiles();
                }

                var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                var logPath = System.IO.Path.Combine(
                    Application.StartupPath,
                    $"debug_{pid}_{DateTime.Now:yyyyMMdd_HHmmss}.log");

                var fileListener = new System.Diagnostics.TextWriterTraceListener(logPath);
                System.Diagnostics.Debug.Listeners.Add(fileListener);
                System.Diagnostics.Debug.AutoFlush = true;

                System.Diagnostics.Debug.WriteLine($"[MainForm] Log file created: {logPath}");
            }
            catch (Exception ex)
            {
                // 如果日誌建立失敗也不影響程式運行
                System.Diagnostics.Debug.WriteLine($"[MainForm] Failed to create log file: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理超過保留天數的舊 log 檔案
        /// </summary>
        private void CleanOldLogFiles()
        {
            try
            {
                var logDirectory = Application.StartupPath;
                var logFiles = System.IO.Directory.GetFiles(logDirectory, "debug_*.log");
                var cutoffDate = DateTime.Now.AddDays(-LOG_RETENTION_DAYS);
                int deletedCount = 0;

                foreach (var logFile in logFiles)
                {
                    var fileInfo = new System.IO.FileInfo(logFile);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        try
                        {
                            System.IO.File.Delete(logFile);
                            deletedCount++;
                        }
                        catch
                        {
                            // 忽略無法刪除的檔案(可能被其他程序使用)
                        }
                    }
                }

                if (deletedCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainForm] Cleaned up {deletedCount} old log file(s) older than {LOG_RETENTION_DAYS} days");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainForm] Failed to clean old log files: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize Config
        /// </summary>
        private void InitializeConfig()
        {
            config = new Config();
            if (!config.LoadConfig())
            {
                MessageBox.Show("Failed to load config.xml", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Initialize Modbus Connections
        /// </summary>
        private void InitializeModbus()
        {
            modbusList = new List<ModBus_List>();

            try
            {
                foreach (var factory in config.Factories)
                {
                    modbusList.Add(new ModBus_List(factory.ModbusIp, factory.ModbusPort, factory.Name));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Modbus init failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Initialize Factory Headers
        /// </summary>
        private void InitializeFactoryHeaders()
        {
            // Set initial display indices (first 5 factories excluding casting factory)
            var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
            for (int i = 0; i < 5; i++)
            {
                if (i < otherFactories.Count)
                    currentDisplayIndices[i] = config.Factories.IndexOf(otherFactories[i]);
                else
                    currentDisplayIndices[i] = -1;
            }
            
            RefreshFactoryDisplay();
        }

        /// <summary>
        /// Initialize Compressor Names (Row1)
        /// </summary>
        private void InitializeCompressorNames()
        {
            // Row1: Device Names
            Label[] deviceNameLabels = { device_col1, device_col2, device_col3, device_col4, device_col5 };
            // Row2: Schedule
            Label[] scheduleLabels = { schedule_col1, schedule_col2, schedule_col3, schedule_col4, schedule_col5 };

            for (int i = 0; i < Math.Min(config.Factories.Count, 5); i++)
            {
                var factory = config.Factories[i];
                var compressors = factory.GetDevicesByType(DeviceType.Compressor);

                if (compressors.Count > 0)
                {
                    // Build device name string
                    string deviceNames = BuildCompressorNameString(factory, compressors);
                    UpdateLabel(deviceNameLabels[i], deviceNames, TextNormal);

                    // Update Schedule (Row2)
                    UpdateScheduleLabel(scheduleLabels[i], compressors);
                }
                else
                {
                    UpdateLabel(deviceNameLabels[i], "--", StatusDisabled);
                    UpdateLabelWithBackground(scheduleLabels[i], "No Schedule", TextNormal, ScheduleInactive);
                }
            }
        }

        /// <summary>
        /// Build Compressor Name String
        /// Format: "FactoryName:CO-38" or "CO-38\nCO-37" for multiple
        /// </summary>
        private string BuildCompressorNameString(FactoryConfig factory, List<DeviceConfig> compressors)
        {
            if (compressors.Count == 1)
            {
                // Single compressor: just show name
                return compressors[0].Name;
            }
            else
            {
                // Multiple compressors: show factory:name format
                var names = compressors
                    .OrderBy(c => c.MachineNo)
                    .Select(c => factory.Name + ":" + c.Name);
                return string.Join("\n", names);
            }
        }

        private void InitializeTimer()
        {
            updateTimer = new Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        /// <summary>
        /// Initialize Default Mode - 啟動時載入預設模式並自動套用排程
        /// </summary>
        private void InitializeDefaultMode()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MainForm] InitializeDefaultMode() called");

                // 從資料庫載入預設模式（IsDefault=true）
                var defaultMode = ModeSelectForm.GetDefaultModeFromDatabase();
                if (defaultMode != null)
                {
                    currentMode = defaultMode;
                    label3.Text = defaultMode.Name;
                    if (!string.IsNullOrEmpty(defaultMode.Description))
                    {
                        label4.Text = defaultMode.Description;
                    }

                    System.Diagnostics.Debug.WriteLine($"[MainForm] Default mode loaded: {defaultMode.Name} with {defaultMode.Schedules.Count} schedules");

                    // 自動套用預設模式的排程到設備
                    ModeSelectForm.ApplyModeSchedulesToConfig(defaultMode);

                    // 重新載入設定以反映套用的排程
                    config.LoadConfig();
                    RefreshFactoryDisplay();

                    System.Diagnostics.Debug.WriteLine("[MainForm] Default mode applied successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[MainForm] WARNING: No default mode found!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainForm] Load default mode failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[MainForm] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 初始化 Teams 通知服務
        /// </summary>
        private void InitializeTeamsNotification()
        {
            try
            {
                if (config.TeamsNotificationEnabled && !string.IsNullOrEmpty(config.TeamsWebhookUrl))
                {
                    teamsNotificationService = new TeamsNotificationService(
                        config.TeamsWebhookUrl, 
                        config.TeamsNotificationEmail,
                        config.NotificationCooldownMinutes);
                    System.Diagnostics.Debug.WriteLine("[MainForm] Teams 通知服務已啟用");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[MainForm] Teams 通知服務未啟用");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainForm] Teams 通知服務初始化失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化基於視圖的動態同步
        /// </summary>
        private void InitializeViewBasedSync()
        {
            try
            {
                var database = new DeviceDatabase(config.IP, config.DB, config.USER, config.Password);

                // 根據當前視圖決定場域
                string currentSiteId = GetCurrentSiteId();

                // 載入場域的最新配置
                LoadAndApplySiteConfig(currentSiteId);

                syncService = new ConfigSyncService(database, currentSiteId);
                syncService.ConfigUpdated += SyncService_ConfigUpdated;
                syncService.Start();

                System.Diagnostics.Debug.WriteLine($"[MainForm] View-based sync initialized for: {currentSiteId} ({currentViewMode})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainForm] Failed to init view-based sync: {ex.Message}");
            }
        }

        /// <summary>
        /// 根據當前視圖模式獲取場域 ID
        /// </summary>
        private string GetCurrentSiteId()
        {
            return currentViewMode == ViewMode.CastingFactory ? "foundry" : "other";
        }

        /// <summary>
        /// 切換同步場域（視圖改變時調用）
        /// </summary>
        private void SwitchSyncSite()
        {
            try
            {
                string newSiteId = GetCurrentSiteId();

                System.Diagnostics.Debug.WriteLine(
                    $"[MainForm] Switching sync site to: {newSiteId} ({currentViewMode})");

                // 停止舊的同步服務
                if (syncService != null)
                {
                    syncService.ConfigUpdated -= SyncService_ConfigUpdated;
                    syncService.Stop();
                }

                // 載入新場域的最新配置
                LoadAndApplySiteConfig(newSiteId);

                // 啟動新的同步服務
                var database = new DeviceDatabase(config.IP, config.DB, config.USER, config.Password);
                syncService = new ConfigSyncService(database, newSiteId);
                syncService.ConfigUpdated += SyncService_ConfigUpdated;
                syncService.Start();

                System.Diagnostics.Debug.WriteLine($"[MainForm] Sync site switched successfully to: {newSiteId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainForm] Failed to switch sync site: {ex.Message}");
            }
        }

        /// <summary>
        /// 載入並套用場域配置(從資料庫讀取最新設定)
        /// </summary>
        private void LoadAndApplySiteConfig(string siteId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MainForm] Loading site config from database for: {siteId}");

                var database = new DeviceDatabase(config.IP, config.DB, config.USER, config.Password);
                var siteConfig = database.LoadSiteConfig(siteId);

                if (siteConfig == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainForm] No site config found for: {siteId}");
                    return;
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[MainForm] Loaded site config - Site: {siteConfig.SiteName}, " +
                    $"Mode ID: {siteConfig.CurrentModeId}, Version: {siteConfig.ConfigVersion}");

                // 如果有設定的模式，套用它
                if (siteConfig.CurrentModeId.HasValue)
                {
                    var mode = ModeSelectForm.GetModeById(siteConfig.CurrentModeId.Value);
                    if (mode != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MainForm] Applying mode: {mode.Name} (ID: {mode.Id})");

                        currentMode = mode;
                        label3.Text = mode.Name;

                        if (!string.IsNullOrEmpty(mode.Description))
                        {
                            label4.Text = mode.Description;
                        }

                        // 判斷是否為手動模式
                        isManualMode = mode.Name.Contains("手動");

                        // 套用模式的排程到配置
                        ModeSelectForm.ApplyModeSchedulesToConfig(mode);
                        config.LoadConfig();
                        RefreshFactoryDisplay();

                        System.Diagnostics.Debug.WriteLine($"[MainForm] Site config applied successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[MainForm] Mode not found with ID: {siteConfig.CurrentModeId.Value}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MainForm] No mode set for site: {siteId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainForm] Failed to load site config: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[MainForm] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 配置同步更新事件處理
        /// </summary>
        private void SyncService_ConfigUpdated(object sender, ConfigUpdatedEventArgs e)
        {
            try
            {
                // 在 UI 執行緒上執行
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => SyncService_ConfigUpdated(sender, e)));
                    return;
                }

                // 驗證場域匹配 - 根據當前視圖判斷
                string currentSiteId = GetCurrentSiteId();

                System.Diagnostics.Debug.WriteLine(
                    $"[MainForm] *** SYNC EVENT RECEIVED *** " +
                    $"Event SiteId: {e.SiteId}, Current View Site: {currentSiteId} ({currentViewMode}), " +
                    $"Mode: {e.CurrentModeId}, Version: {e.ConfigVersion}");

                if (e.SiteId != currentSiteId)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MainForm] *** IGNORING *** Config update from different site. " +
                        $"Current view: {currentSiteId} ({currentViewMode}), Event: {e.SiteId}");
                    return;
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[MainForm] *** APPLYING SYNC *** Site: {e.SiteId}, Version: {e.ConfigVersion}, " +
                    $"Mode: {e.CurrentModeId}, Updated by: {e.UpdatedBy}");

                // 重新載入模式
                if (e.CurrentModeId.HasValue)
                {
                    var updatedMode = ModeSelectForm.GetModeById(e.CurrentModeId.Value);
                    if (updatedMode != null)
                    {
                        currentMode = updatedMode;
                        label3.Text = updatedMode.Name;
                        if (!string.IsNullOrEmpty(updatedMode.Description))
                        {
                            label4.Text = updatedMode.Description;
                        }

                        // 判斷是否為手動模式
                        isManualMode = updatedMode.Name.Contains("手動");

                        ModeSelectForm.ApplyModeSchedulesToConfig(updatedMode);
                        config.LoadConfig();
                        RefreshFactoryDisplay();

                        System.Diagnostics.Debug.WriteLine($"[MainForm] *** SYNC COMPLETE *** Mode: {updatedMode.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainForm] Config sync error: {ex.Message}");
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (modbusList == null || modbusList.Count == 0)
                return;

            try
            {
                UpdateAllFactories();
                ExecuteScheduleControl();

                // 背景監控所有設備狀態（不受當前視圖限制）
                MonitorAllDevicesInBackground();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Update failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Update all factories based on current view mode
        /// </summary>
        private void UpdateAllFactories()
        {
            Label[] deviceNameLabels = { device_col1, device_col2, device_col3, device_col4, device_col5 };
            Label[] scheduleLabels = { schedule_col1, schedule_col2, schedule_col3, schedule_col4, schedule_col5 };
            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            Label[] readyRemoteLabels = { ready_remote_col1, ready_remote_col2, ready_remote_col3, ready_remote_col4, ready_remote_col5 };
            Label[] precoolerLabels = { precooler_col1, precooler_col2, precooler_col3, precooler_col4, precooler_col5 };
            Label[] dryerLabels = { dryer_col1, dryer_col2, dryer_col3, dryer_col4, dryer_col5 };
            Label[] fanLabels = { fan_col1, fan_col2, fan_col3, fan_col4, fan_col5 };
            Label[] pressureLabels = { pressure_col1, pressure_col2, pressure_col3, pressure_col4, pressure_col5 };
            Label[] tempLabels = { temp_col1, temp_col2, temp_col3, temp_col4, temp_col5 };
            Label[] powerLabels = { power_col1, power_col2, power_col3, power_col4, power_col5 };
            Label[] compressedTempLabels = { CompressedTemp_col1, CompressedTemp_col2, CompressedTemp_col3, CompressedTemp_col4, CompressedTemp_col5 };

            if (currentViewMode == ViewMode.CastingFactory)
            {
                // Casting Factory Mode - Each compressor in separate column
                var castingFactory = config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
                if (castingFactory == null) return;

                int modbusIndex = config.Factories.IndexOf(castingFactory);
                if (modbusIndex >= modbusList.Count) return;

                var modbus = modbusList[modbusIndex];
                if (modbus.address_val == null) return;

                var compressors = castingFactory.GetDevicesByType(DeviceType.Compressor).OrderBy(c => c.MachineNo).ToList();

                // Update device name label color based on ConnectState
                for (int colIndex = 0; colIndex < Math.Min(compressors.Count, 5); colIndex++)
                {
                    UpdateLabel(deviceNameLabels[colIndex], deviceNameLabels[colIndex].Text, modbus.ConnectState ? Color.Green : Color.Red);
                }

                // Common devices (shared across all columns)
                var precoolerStatus = GetDeviceStatusByConfig(modbus, castingFactory, DeviceType.Precooler);
                var dryerStatus = GetDeviceStatusByConfig(modbus, castingFactory, DeviceType.Dryer);
                var fanStatus = GetDeviceStatusByConfig(modbus, castingFactory, DeviceType.Fan);
                var pressure = GetPressureValue(modbus);
                var temp = GetTempValue(modbus);
                var presuretemp = GetPresureTempValue(modbus);
                var compressedTemp = GetCompressedTempValue(modbus);

                // Update each compressor in separate column
                for (int colIndex = 0; colIndex < Math.Min(compressors.Count, 5); colIndex++)
                {
                    var compressor = compressors[colIndex];

                    // Get individual compressor status
                    bool isRunning = GetDIValue(modbus, compressor.IO.RunDI);
                    bool isAlarm = GetDIValue(modbus, compressor.IO.AlarmDI);
                    bool isFault = GetDIValue(modbus, compressor.IO.FaultDI);

                    // 檢查並發送設備狀態通知
                    CheckAndNotifyDeviceStatus(compressor.Name, isAlarm, isFault);

                    DeviceStatus status;
                    if (isFault)
                        status = new DeviceStatus("故障", StatusFault);
                    else if (isAlarm)
                        status = new DeviceStatus("警報", StatusAlarm);
                    else if (isRunning)
                        status = new DeviceStatus("運轉", StatusRunning);
                    else
                        status = new DeviceStatus("停止", StatusStopped);

                    UpdateLabel(statusLabels[colIndex], status.Text, status.Color);
                    UpdateScheduleLabel(scheduleLabels[colIndex], new List<DeviceConfig> { compressor });

                    // 備妥 / 遠端 (從 config IO 設定讀取)
                    bool isReady = !GetDIValue(modbus, 15 + compressor.IO.IsReadyDI);
                    bool isRemote = !GetDIValue(modbus, 15 + compressor.IO.IsRemoteDI);
                    string readyText = isReady ? "ON" : "OFF";
                    string remoteText = isRemote ? "ON" : "OFF";
                    Color readyColor = isReady ? StatusRunning : StatusStopped;
                    Color remoteColor = isRemote ? StatusRunning : StatusStopped;
                    UpdateLabel(readyRemoteLabels[colIndex], "備妥:" + readyText + "\n遠端:" + remoteText, isReady && isRemote ? StatusRunning : (isReady || isRemote ? Color.Yellow : StatusStopped));

                    // Common devices - show same values in all compressor columns
                    UpdateLabel(precoolerLabels[colIndex], precoolerStatus.Text, precoolerStatus.Color);
                    UpdateLabel(dryerLabels[colIndex], dryerStatus.Text, dryerStatus.Color);
                    UpdateLabel(fanLabels[colIndex], fanStatus.Text, fanStatus.Color);
                    UpdatePressureLabelWithLimitCheck(pressureLabels[colIndex], pressure, castingFactory.AlarmLimits, compressor.Name);
                    UpdateTempLabelWithLimitCheck(tempLabels[colIndex], temp, castingFactory.AlarmLimits, compressor.Name);
                    UpdateCompressedTempLabelWithLimitCheck(compressedTempLabels[colIndex], compressedTemp, castingFactory.AlarmLimits, compressor.Name);

                    // Power value from DB
                    string powerValue = GetPowerValueFromDB(compressor.Name);
                    UpdateLabel(powerLabels[colIndex], powerValue, StatusRunning);
                }
            }
            else
            {
                // Other Factories Mode
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();

                for (int colIndex = 0; colIndex < 5; colIndex++)
                {
                    if (colIndex < otherFactories.Count)
                    {
                        var factory = otherFactories[colIndex];
                        int modbusIndex = config.Factories.IndexOf(factory);

                        if (modbusIndex >= modbusList.Count) continue;

                        var modbus = modbusList[modbusIndex];

                        // Update device name label color based on ConnectState
                        UpdateLabel(deviceNameLabels[colIndex], deviceNameLabels[colIndex].Text, modbus.ConnectState ? Color.Green : Color.Red);

                        if (modbus.address_val == null) continue;

                        var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                        var compressorStatuses = GetCompressorStatuses(modbus, factory);
                        var precoolerStatus = GetDeviceStatusByConfig(modbus, factory, DeviceType.Precooler);
                        var dryerStatus = GetDeviceStatusByConfig(modbus, factory, DeviceType.Dryer);
                        var fanStatus = GetDeviceStatusByConfig(modbus, factory, DeviceType.Fan);
                        var pressure = GetPressureValue(modbus);
                        var temp = GetTempValue(modbus);
                        var presuretemp = GetPresureTempValue(modbus);
                        var compressedTemp = GetCompressedTempValue(modbus);

                        // Update compressor status
                        if (compressorStatuses.Count > 0)
                        {
                            string statusText = BuildCompressorStatusString(factory, compressorStatuses);
                            Color statusColor = GetOverallStatusColor(compressorStatuses);
                            UpdateLabel(statusLabels[colIndex], statusText, statusColor);
                        }
                        else
                        {
                            UpdateLabel(statusLabels[colIndex], "--", StatusDisabled);
                        }

                        UpdateScheduleLabel(scheduleLabels[colIndex], compressors);

                        // 備妥 / 遠端 (從 config IO 設定讀取)
                        var firstCompressor = compressors.FirstOrDefault();
                        int readyDI = firstCompressor != null && firstCompressor.IO.IsReadyDI >= 0 ? 15 + firstCompressor.IO.IsReadyDI : -1;
                        int remoteDI = firstCompressor != null && firstCompressor.IO.IsRemoteDI >= 0 ? 15 + firstCompressor.IO.IsRemoteDI : -1;
                        bool isReady = !GetDIValue(modbus, readyDI);
                        bool isRemote = !GetDIValue(modbus, remoteDI);
                        string readyText = isReady ? "ON" : "OFF";
                        string remoteText = isRemote ? "ON" : "OFF";
                        UpdateLabel(readyRemoteLabels[colIndex], "備妥:" + readyText + "\n遠端:" + remoteText, isReady && isRemote ? StatusRunning : (isReady || isRemote ? Color.Yellow : StatusStopped));

                        UpdateLabel(precoolerLabels[colIndex], precoolerStatus.Text, precoolerStatus.Color);
                        UpdateLabel(dryerLabels[colIndex], dryerStatus.Text, dryerStatus.Color);
                        UpdateLabel(fanLabels[colIndex], fanStatus.Text, fanStatus.Color);

                        // 建立設備名稱字串，用於推播通知
                        string deviceNames = compressors.Count > 0 
                            ? string.Join(", ", compressors.Select(c => c.Name)) 
                            : factory.Name;

                        UpdatePressureLabelWithLimitCheck(pressureLabels[colIndex], pressure, factory.AlarmLimits, deviceNames);
                        UpdateTempLabelWithLimitCheck(tempLabels[colIndex], temp, factory.AlarmLimits, deviceNames);
                        UpdateCompressedTempLabelWithLimitCheck(compressedTempLabels[colIndex], compressedTemp, factory.AlarmLimits, deviceNames);

                        // Power value from DB - build combined power text for all compressors
                        if (compressors.Count == 1)
                        {
                            string powerValue = GetPowerValueFromDB(compressors[0].Name);
                            UpdateLabel(powerLabels[colIndex], powerValue, StatusRunning);
                        }
                        else if (compressors.Count > 1)
                        {
                            var powerTexts = compressors
                                .OrderBy(c => c.MachineNo)
                                .Select(c => c.Name + ":" + GetPowerValueFromDB(c.Name));
                            UpdateLabel(powerLabels[colIndex], string.Join("\n", powerTexts), StatusRunning);
                        }
                        else
                        {
                            UpdateLabel(powerLabels[colIndex], "--", StatusDisabled);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all compressor statuses for a factory
        /// </summary>
        private List<CompressorStatus> GetCompressorStatuses(ModBus_List modbus, FactoryConfig factory)
        {
            var statuses = new List<CompressorStatus>();
            var compressors = factory.GetDevicesByType(DeviceType.Compressor);

            //System.Diagnostics.Debug.WriteLine($"[{factory.Name}] Found {compressors.Count} compressors");

            foreach (var compressor in compressors.OrderBy(c => c.MachineNo))
            {
                bool isRunning = GetDIValue(modbus, compressor.IO.RunDI);
                bool isAlarm = GetDIValue(modbus, compressor.IO.AlarmDI);
                bool isFault = GetDIValue(modbus, compressor.IO.FaultDI);

                //System.Diagnostics.Debug.WriteLine($"  [{compressor.Name}] MachineNo={compressor.MachineNo}, RunDI={compressor.IO.RunDI}, isRunning={isRunning}");

                // 檢查並發送設備狀態通知
                CheckAndNotifyDeviceStatus(compressor.Name, isAlarm, isFault);

                DeviceStatus status;
                if (isFault)
                    status = new DeviceStatus("故障", StatusFault);
                else if (isAlarm)
                    status = new DeviceStatus("警報", StatusAlarm);
                else if (isRunning)
                    status = new DeviceStatus("運轉", StatusRunning);
                else
                    status = new DeviceStatus("停止", StatusStopped);

                statuses.Add(new CompressorStatus
                {
                    Name = compressor.Name,
                    MachineNo = compressor.MachineNo,
                    Status = status
                });
            }

            return statuses;
        }

        /// <summary>
        /// Get device status by config
        /// </summary>
        private DeviceStatus GetDeviceStatusByConfig(ModBus_List modbus, FactoryConfig factory, DeviceType deviceType)
        {
            var device = factory.GetDevice(deviceType, 1);
            
            if (device == null || !device.Enabled)
            {
                return new DeviceStatus("--", StatusDisabled);
            }

            bool isOn = GetDIValue(modbus, device.IO.OnDI);
            bool isOff = GetDIValue(modbus, device.IO.OffDI);
            bool isFault = GetDIValue(modbus, device.IO.FaultDI);

            if (isOff && isFault) return new DeviceStatus("故障", StatusFault);
            if (isOn) return new DeviceStatus("啟動", StatusRunning);
            if (isOff) return new DeviceStatus("停止" +
                "", StatusStopped);
            return new DeviceStatus("--", StatusStopped);
        }

        /// <summary>
        /// Get DI Value by number
        /// </summary>
        private bool GetDIValue(ModBus_List modbus, int diNumber)
        {
            if (diNumber < 0) return false;

            switch (diNumber)
            {
                case 0: return modbus.address_val.Address_4051_DI_0 == "1";
                case 1: return modbus.address_val.Address_4051_DI_1 == "1";
                case 2: return modbus.address_val.Address_4051_DI_2 == "1";
                case 3: return modbus.address_val.Address_4051_DI_3 == "1";
                case 4: return modbus.address_val.Address_4051_DI_4 == "1";
                case 5: return modbus.address_val.Address_4051_DI_5 == "1";
                case 6: return modbus.address_val.Address_4051_DI_6 == "1";
                case 7: return modbus.address_val.Address_4051_DI_7 == "1";
                case 8: return modbus.address_val.Address_4051_DI_8 == "1";
                case 9: return modbus.address_val.Address_4051_DI_9 == "1";
                case 10: return modbus.address_val.Address_4051_DI_10 == "1";
                case 11: return modbus.address_val.Address_4051_DI_11 == "1";
                case 12: return modbus.address_val.Address_4051_DI_12 == "1";
                case 13: return modbus.address_val.Address_4051_DI_13 == "1";
                case 14: return modbus.address_val.Address_4051_DI_14 == "1";
                case 15: return modbus.address_val.Address_4051_DI_15 == "1";
                case 16: return modbus.address_val.Address_4050_DI_1 == "1";
                case 17: return modbus.address_val.Address_4050_DI_2 == "1";
                case 18: return modbus.address_val.Address_4050_DI_3 == "1";
                case 19: return modbus.address_val.Address_4050_DI_4 == "1";
                case 20: return modbus.address_val.Address_4050_DI_5 == "1";
                case 21: return modbus.address_val.Address_4050_DI_6 == "1";
                default: return false;
            }
        }

        /// <summary>
        /// Get Pressure Value
        /// </summary>
        private string GetPressureValue(ModBus_List modbus)
        {
            try
            {
                double pressureValue = Convert.ToDouble(modbus.address_val.Address_Air_Sensor_Pressure_Value);
                double decimalPlaces = Convert.ToDouble(modbus.address_val.Address_Air_Sensor_Decimal);
                double pressure = pressureValue / Math.Pow(10, decimalPlaces);
                return pressure.ToString("F2");
            }
            catch
            {
                return "--";
            }
        }

        /// <summary>
        /// Get Temp Value
        /// </summary>
        private string GetTempValue(ModBus_List modbus)
        {
            try
            {
                double tempValue = Convert.ToDouble(modbus.address_val.Address_E5CC_1_PV);
                double temperature = tempValue;
                if (temperature == 0)
                    return "--";
                return temperature.ToString();
            }
            catch
            {
                return "--";
            }
        }

        /// <summary>
        /// Get PresureTemp Value
        /// </summary>
        private string GetPresureTempValue(ModBus_List modbus)
        {
            try
            {
                double tempValue = Convert.ToDouble(modbus.address_val.Address_E5CC_1_PV);
                double temperature = tempValue;
                if (temperature == 0)
                    return "--";
                return temperature.ToString();
            }
            catch
            {
                return "--";
            }
        }

        /// <summary>
        /// Get Compressed Temp Value (空壓溫度)
        /// </summary>
        private string GetCompressedTempValue(ModBus_List modbus)
        {
            try
            {
                double tempValue = Convert.ToDouble(modbus.address_val.Address_CompressedTemp);
                double temperature = tempValue;
                if (temperature == 0)
                    return "--";
                return temperature.ToString();
            }
            catch
            {
                return "--";
            }
        }

        /// <summary>
        /// 從資料庫讀取設備的功率值 (P 欄位)
        /// </summary>
        /// <param name="deviceName">設備名稱</param>
        /// <returns>功率值字串，讀取失敗時回傳 "--"</returns>
        private string GetPowerValueFromDB(string deviceName)
        {
            try
            {
                if (string.IsNullOrEmpty(config.IP) || string.IsNullOrEmpty(config.DB) || string.IsNullOrEmpty(config.machinery_factory_realtime_table1))
                    return "--";

                var db = new MYSQL(config.IP, config.DB, config.USER, config.Password);
                db.selectdata("SELECT `P` FROM `" + config.machinery_factory_realtime_table1 + "` WHERE `Meter_Name`='" + deviceName + "' ORDER BY `Time` DESC LIMIT 1");

                if (db.readdata != null && db.readdata.Count > 0 && !string.IsNullOrEmpty(db.readdata[0]))
                {
                    return db.readdata[0];
                }
                return "--";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetPowerValueFromDB] {deviceName} failed: {ex.Message}");
                return "--";
            }
        }


        /// <summary>
        /// Build Compressor Status String for Row3
        /// </summary>
        private string BuildCompressorStatusString(FactoryConfig factory, List<CompressorStatus> statuses)
        {
            if (statuses.Count == 1)
            {
                return statuses[0].Status.Text;
            }
            else
            {
                // Multiple compressors: show each status
                var statusStrings = statuses.Select(s => s.Name + ":" + s.Status.Text);
                return string.Join("\n", statusStrings);
            }
        }

        /// <summary>
        /// Get Overall Status Color (priority: Fault > Alarm > Running > Stopped)
        /// </summary>
        private Color GetOverallStatusColor(List<CompressorStatus> statuses)
        {
            if (statuses.Any(s => s.Status.Color == StatusFault))
                return StatusFault;
            if (statuses.Any(s => s.Status.Color == StatusAlarm))
                return StatusAlarm;
            if (statuses.Any(s => s.Status.Color == StatusRunning))
                return StatusRunning;
            return StatusStopped;
        }

        /// <summary>
        /// Update Label
        /// </summary>
        private void UpdateLabel(Label label, string text, Color foreColor)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() =>
                {
                    label.Text = text;
                    label.ForeColor = foreColor;
                }));
            }
            else
            {
                label.Text = text;
                label.ForeColor = foreColor;
            }
        }

        /// <summary>
        /// Update Schedule Label with background color
        /// </summary>
        private void UpdateScheduleLabel(Label label, List<DeviceConfig> compressors)
        {
            // 手動模式下顯示「手動模式」
            if (isManualMode)
            {
                UpdateLabelWithBackground(label, "手動模式", TextNormal, Color.FromArgb(0, 122, 204));
                return;
            }

            // 從 currentMode 中獲取排程資訊
            bool hasActiveSchedule = false;
            bool hasAnySchedule = false;
            List<string> scheduleTexts = new List<string>();

            if (currentMode != null && currentMode.Schedules != null)
            {
                foreach (var compressor in compressors)
                {
                    var schedules = GetDeviceSchedules(compressor);
                    if (schedules != null && schedules.Count > 0)
                    {
                        foreach (var schedule in schedules)
                        {
                            if (schedule.Enabled)
                            {
                                hasAnySchedule = true;
                                if (IsInSchedule(schedule))
                                {
                                    hasActiveSchedule = true;
                                }
                                scheduleTexts.Add(schedule.GetTimeDisplayText());
                            }
                        }
                    }
                }
            }

            if (hasAnySchedule)
            {
                string scheduleText = string.Join("\n", scheduleTexts.Distinct());

                if (hasActiveSchedule)
                {
                    UpdateLabelWithBackground(label, "時間排程", TextNormal, ScheduleActive);
                }
                else
                {
                    UpdateLabelWithBackground(label, "時間排程", TextNormal, ScheduleInactive);
                }
            }
            else
            {
                UpdateLabelWithBackground(label, "時間排程", TextNormal, ScheduleInactive);
            }
        }

        /// <summary>
        /// Update Label with text, foreground and background color
        /// </summary>
        private void UpdateLabelWithBackground(Label label, string text, Color foreColor, Color backColor)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() =>
                {
                    label.Text = text;
                    label.ForeColor = foreColor;
                    label.BackColor = backColor;
                }));
            }
            else
            {
                label.Text = text;
                label.ForeColor = foreColor;
                label.BackColor = backColor;
            }
        }

        /// <summary>
        /// 排程控制 - 根據排程時間自動控制 DO 輸出
        /// 當目前時間在排程內 → DO=1 (啟動)
        /// 當目前時間不在排程內 → DO=0 (停止)
        /// 手動模式下不執行自動排程控制
        /// </summary>
        private void ExecuteScheduleControl()
        {
            // 手動模式下不執行自動排程控制
            if (isManualMode) return;

            System.Diagnostics.Debug.WriteLine($"[排程控制] ========== 執行排程控制 ========== 當前時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ({DateTime.Now.DayOfWeek})");

            for (int i = 0; i < config.Factories.Count; i++)
            {
                var factory = config.Factories[i];
                if (i >= modbusList.Count) continue;

                var modbus = modbusList[i];
                if (!modbus.ConnectState || modbus.address_val == null) continue;

                var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                foreach (var compressor in compressors)
                {
                    // 只處理有設定 controlDO 的設備
                    if (compressor.IO.ControlDO < 0)
                        continue;

                    // 從 currentMode 中獲取該設備的所有排程
                    var schedules = GetDeviceSchedules(compressor);
                    if (schedules == null || schedules.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[排程控制] {compressor.Name} - 沒有排程設定");
                        continue;
                    }

                    System.Diagnostics.Debug.WriteLine($"[排程控制] {compressor.Name} - 找到 {schedules.Count} 個排程");

                    // 檢查是否有任一個啟用的排程在當前時間範圍內
                    bool isInSchedule = false;
                    bool hasEnabledSchedule = false;
                    foreach (var schedule in schedules)
                    {
                        if (schedule.Enabled)
                        {
                            hasEnabledSchedule = true;
                            bool inRange = IsInSchedule(schedule);

                            System.Diagnostics.Debug.WriteLine($"[排程控制]   - 排程: {schedule.DeviceName} " +
                                $"IsSpanMode={schedule.IsSpanMode}, " +
                                $"StartDay={schedule.StartDay}, StartTime={schedule.StartTime:hh\\:mm}, " +
                                $"EndDay={schedule.EndDay}, EndTime={schedule.EndTime:hh\\:mm}, " +
                                $"RepeatDays={string.Join(",", schedule.RepeatDays ?? new List<DayOfWeek>())}, " +
                                $"InRange={inRange}");

                            if (inRange)
                            {
                                isInSchedule = true;
                                break; // 只要有一個在範圍內就可以了
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[排程控制]   - 排程: {schedule.DeviceName} (未啟用)");
                        }
                    }

                    // 如果沒有啟用的排程，跳過
                    if (!hasEnabledSchedule)
                    {
                        System.Diagnostics.Debug.WriteLine($"[排程控制] {compressor.Name} - 沒有啟用的排程，跳過");
                        continue;
                    }

                    ushort targetValue = isInSchedule ? (ushort)1 : (ushort)0;

                    System.Diagnostics.Debug.WriteLine($"[排程控制] {compressor.Name} - 判斷結果: isInSchedule={isInSchedule}, targetValue={targetValue}");

                    // 用 FactoryId_MachineNo 作為 key 來追蹤狀態
                    string key = factory.Id + "_" + compressor.MachineNo;

                    // 只在狀態變化時才寫入，避免每秒重複寫入
                    ushort lastValue;
                    if (!lastDOStates.TryGetValue(key, out lastValue) || lastValue != targetValue)
                    {
                        bool success = modbus.WriteDO(compressor.IO.ControlDO, targetValue);
                        if (success)
                        {
                            lastDOStates[key] = targetValue;
                            System.Diagnostics.Debug.WriteLine(
                                $"[排程控制] ✓ {factory.Name} {compressor.Name} (MachineNo={compressor.MachineNo}) " +
                                $"DO_{compressor.IO.ControlDO} = {targetValue} ({(isInSchedule ? "啟動" : "停止")})");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"[排程控制] ✗ {factory.Name} {compressor.Name} 寫入失敗");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 從當前模式中獲取指定設備的排程（可能有多個時間區間）
        /// </summary>
        private List<ModeScheduleItem> GetDeviceSchedules(DeviceConfig device)
        {
            if (currentMode == null || currentMode.Schedules == null)
                return new List<ModeScheduleItem>();

            // 找出該設備所屬的廠區
            var factory = config.Factories.FirstOrDefault(f => f.Devices.Contains(device));
            if (factory == null)
                return new List<ModeScheduleItem>();

            // 在當前模式中查找該設備的所有排程（同一設備可能有多個時間區間）
            return currentMode.Schedules.Where(s =>
                s.FactoryId == factory.Id &&
                s.MachineNo == device.MachineNo &&
                s.DeviceName == device.Name).ToList();
        }

        /// <summary>
        /// 檢查指定設備是否在任一排程時間範圍內
        /// </summary>
        private bool IsDeviceInSchedule(DeviceConfig device)
        {
            var schedules = GetDeviceSchedules(device);
            if (schedules == null || schedules.Count == 0)
                return false;

            // 只要有任一個排程在當前時間範圍內且啟用，就返回 true
            foreach (var schedule in schedules)
            {
                if (schedule.Enabled && IsInSchedule(schedule))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 檢查指定排程是否在當前時間範圍內
        /// </summary>
        private bool IsInSchedule(ModeScheduleItem schedule)
        {
            if (schedule == null || !schedule.Enabled)
                return false;

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            var currentDay = now.DayOfWeek;

            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] ========== 開始檢查 ==========");
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] 當前時間: {now:yyyy-MM-dd HH:mm:ss} ({currentDay})");
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] IsSpanMode: {schedule.IsSpanMode}");

            // 重複模式：檢查當天是否在 RepeatDays 中，且時間在 StartTime~EndTime 之間
            if (!schedule.IsSpanMode)
            {
                System.Diagnostics.Debug.WriteLine($"[IsInSchedule] === 重複模式 ===");
                System.Diagnostics.Debug.WriteLine($"[IsInSchedule] StartTime: {schedule.StartTime:hh\\:mm}, EndTime: {schedule.EndTime:hh\\:mm}");
                System.Diagnostics.Debug.WriteLine($"[IsInSchedule] RepeatDays: {string.Join(",", schedule.RepeatDays ?? new List<DayOfWeek>())}");

                if (schedule.RepeatDays != null && schedule.RepeatDays.Count > 0 && !schedule.RepeatDays.Contains(currentDay))
                {
                    System.Diagnostics.Debug.WriteLine($"[IsInSchedule] ✗ 當天 ({currentDay}) 不在 RepeatDays 中");
                    return false;
                }

                TimeSpan effectiveEnd = schedule.EndTime;
                if (schedule.EndTime.Minutes == 59)
                    effectiveEnd = schedule.EndTime.Add(TimeSpan.FromSeconds(59));

                bool resultRepeat;
                if (schedule.StartTime <= schedule.EndTime)
                {
                    resultRepeat = currentTime >= schedule.StartTime && currentTime <= effectiveEnd;
                    System.Diagnostics.Debug.WriteLine($"[IsInSchedule] 時間檢查: {currentTime:hh\\:mm} >= {schedule.StartTime:hh\\:mm} && <= {effectiveEnd:hh\\:mm} = {resultRepeat}");
                }
                else
                {
                    resultRepeat = currentTime >= schedule.StartTime || currentTime <= effectiveEnd;
                    System.Diagnostics.Debug.WriteLine($"[IsInSchedule] 時間檢查(跨日): {currentTime:hh\\:mm} >= {schedule.StartTime:hh\\:mm} || <= {effectiveEnd:hh\\:mm} = {resultRepeat}");
                }

                return resultRepeat;
            }

            // 跨日模式：使用週分鐘連續區間
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] === 跨日模式 ===");
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] StartDay: {schedule.StartDay}, StartTime: {schedule.StartTime:hh\\:mm}");
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] EndDay: {schedule.EndDay}, EndTime: {schedule.EndTime:hh\\:mm}");
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] RepeatDays: {string.Join(",", schedule.RepeatDays ?? new List<DayOfWeek>())}");

            // 將 DayOfWeek 轉換為週分鐘
            // 注意：Sunday = 0，但在一週中應該是最後一天，所以轉換為 7
            int ToWeeklyMinutes(DayOfWeek day, TimeSpan time)
            {
                int dayValue = (int)day;
                if (dayValue == 0) // Sunday
                    dayValue = 7;
                return dayValue * 1440 + (int)time.TotalMinutes;
            }

            int current = ToWeeklyMinutes(currentDay, currentTime);
            int start = ToWeeklyMinutes(schedule.StartDay, schedule.StartTime);
            int end = ToWeeklyMinutes(schedule.EndDay, schedule.EndTime);

            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] 週分鐘計算:");
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule]   current = {currentDay}({(currentDay == DayOfWeek.Sunday ? 7 : (int)currentDay)}) * 1440 + {(int)currentTime.TotalMinutes} = {current}");
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule]   start   = {schedule.StartDay}({(schedule.StartDay == DayOfWeek.Sunday ? 7 : (int)schedule.StartDay)}) * 1440 + {(int)schedule.StartTime.TotalMinutes} = {start}");
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule]   end     = {schedule.EndDay}({(schedule.EndDay == DayOfWeek.Sunday ? 7 : (int)schedule.EndDay)}) * 1440 + {(int)schedule.EndTime.TotalMinutes} = {end}");

            bool resultSpan;
            if (start <= end)
            {
                // 不跨週：例如 週一 08:00 ~ 週五 17:00，或 週一 00:00 ~ 週日 23:59
                resultSpan = current >= start && current <= end;
                System.Diagnostics.Debug.WriteLine($"[IsInSchedule] 不跨週檢查: {current} >= {start} && {current} <= {end} = {resultSpan}");
            }
            else
            {
                // 跨週：例如 週六 20:00 ~ 週一 08:00
                // 需要處理週末跨到下週一的情況
                resultSpan = current >= start || current <= end;
                System.Diagnostics.Debug.WriteLine($"[IsInSchedule] 跨週檢查: {current} >= {start} || {current} <= {end} = {resultSpan}");
            }

            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] 最終結果: {resultSpan}");
            System.Diagnostics.Debug.WriteLine($"[IsInSchedule] ========== 檢查結束 ==========");

            return resultSpan;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("=================================================");
            System.Diagnostics.Debug.WriteLine($"[MainForm] *** INSTANCE CLOSING *** PID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
            System.Diagnostics.Debug.WriteLine($"[MainForm] Close time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine("=================================================");

            updateTimer?.Stop();
            updateTimer?.Dispose();

            // 停止配置同步服務
            if (syncService != null)
            {
                syncService.ConfigUpdated -= SyncService_ConfigUpdated;
                syncService.Stop();
                syncService = null;
            }

            // 清理場域配置檔案
            try
            {
                SiteManager.Instance.Cleanup();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainForm] SiteManager cleanup error: {ex.Message}");
            }

            // 刷新並關閉日誌
            System.Diagnostics.Debug.Flush();

            base.OnFormClosing(e);
        }

        private void Factory_Click(object sender, EventArgs e)
        {
            ViewMode previousViewMode = currentViewMode;

            if (currentViewMode == ViewMode.OtherFactories)
            {
                // Switch to Casting Factory view
                currentViewMode = ViewMode.CastingFactory;
                Factory.Text = "鑄造廠域_空壓系統即時狀態";

                // Find casting factory index
                var castingFactory = config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
                if (castingFactory != null)
                {
                    int castingIndex = config.Factories.IndexOf(castingFactory);
                    // Display casting factory in all 5 columns (for 3 compressors)
                    currentDisplayIndices = new int[] { castingIndex, -1, -1, -1, -1 };
                }
            }
            else
            {
                // Switch to Other Factories view
                currentViewMode = ViewMode.OtherFactories;
                Factory.Text = "其它廠域_空壓系統即時狀態";

                // Display first 5 factories (excluding casting factory)
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
                currentDisplayIndices = new int[5];
                for (int i = 0; i < 5; i++)
                {
                    if (i < otherFactories.Count)
                        currentDisplayIndices[i] = config.Factories.IndexOf(otherFactories[i]);
                    else
                        currentDisplayIndices[i] = -1;
                }
            }

            // 視圖改變時切換同步場域
            if (previousViewMode != currentViewMode)
            {
                SwitchSyncSite();
            }

            // Refresh display
            RefreshFactoryDisplay();
        }

        /// <summary>
        /// Refresh all factory displays based on current view mode
        /// </summary>
        private void RefreshFactoryDisplay()
        {
            Label[] factoryHeaders = { factory_col1, factory_col2, factory_col3, factory_col4, factory_col5 };
            Label[] deviceNameLabels = { device_col1, device_col2, device_col3, device_col4, device_col5 };
            Label[] scheduleLabels = { schedule_col1, schedule_col2, schedule_col3, schedule_col4, schedule_col5 };
            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            Label[] readyRemoteLabels = { ready_remote_col1, ready_remote_col2, ready_remote_col3, ready_remote_col4, ready_remote_col5 };
            Label[] precoolerLabels = { precooler_col1, precooler_col2, precooler_col3, precooler_col4, precooler_col5 };
            Label[] dryerLabels = { dryer_col1, dryer_col2, dryer_col3, dryer_col4, dryer_col5 };
            Label[] fanLabels = { fan_col1, fan_col2, fan_col3, fan_col4, fan_col5 };
            Label[] pressureLabels = { pressure_col1, pressure_col2, pressure_col3, pressure_col4, pressure_col5 };
            Label[] tempLabels = { temp_col1, temp_col2, temp_col3, temp_col4, temp_col5 };
            Label[] powerLabels = { power_col1, power_col2, power_col3, power_col4, power_col5 };
            Label[] compressedTempLabels = { CompressedTemp_col1, CompressedTemp_col2, CompressedTemp_col3, CompressedTemp_col4, CompressedTemp_col5 };


            if (currentViewMode == ViewMode.CastingFactory)
            {
                // Casting Factory Mode - Each compressor in separate column
                var castingFactory = config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
                if (castingFactory != null)
                {
                    var compressors = castingFactory.GetDevicesByType(DeviceType.Compressor).OrderBy(c => c.MachineNo).ToList();
                    
                    for (int i = 0; i < 5; i++)
                    {
                        if (i < compressors.Count)
                        {
                            var compressor = compressors[i];
                            // Header: Factory name
                            UpdateLabel(factoryHeaders[i], castingFactory.Name, TextNormal);
                            // Device name: Compressor name
                            UpdateLabel(deviceNameLabels[i], compressor.Name, TextNormal);
                            // Schedule
                            UpdateScheduleLabel(scheduleLabels[i], new List<DeviceConfig> { compressor });
                            // Status will be updated in timer
                            UpdateLabel(statusLabels[i], "--", StatusDisabled);
                            // Power will be updated in timer
                            UpdateLabel(powerLabels[i], "--", StatusDisabled);
                            // Common devices will be updated in timer (show in all columns)
                        }
                        else
                        {
                            // Hide unused columns
                            UpdateLabel(factoryHeaders[i], "--", StatusDisabled);
                            UpdateLabel(deviceNameLabels[i], "--", StatusDisabled);
                            UpdateLabelWithBackground(scheduleLabels[i], "--", TextNormal, StatusDisabled);
                            UpdateLabel(statusLabels[i], "--", StatusDisabled);
                            UpdateLabel(readyRemoteLabels[i], "--", StatusDisabled);
                            UpdateLabel(precoolerLabels[i], "--", StatusDisabled);
                            UpdateLabel(dryerLabels[i], "--", StatusDisabled);
                            UpdateLabel(fanLabels[i], "--", StatusDisabled);
                            UpdateLabel(pressureLabels[i], "--", StatusDisabled);
                            UpdateLabel(tempLabels[i], "--", StatusDisabled);
                            UpdateLabel(powerLabels[i], "--", StatusDisabled);
                            UpdateLabel(compressedTempLabels[i], "--", StatusDisabled);
                        }
                    }
                }
            }
            else
            {
                // Other Factories Mode
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
                
                for (int i = 0; i < 5; i++)
                {
                    if (i < otherFactories.Count)
                    {
                        var factory = otherFactories[i];
                        var compressors = factory.GetDevicesByType(DeviceType.Compressor);
                        
                        UpdateLabel(factoryHeaders[i], factory.Name, TextNormal);
                        
                        if (compressors.Count > 0)
                        {
                            string deviceNames = BuildCompressorNameString(factory, compressors);
                            UpdateLabel(deviceNameLabels[i], deviceNames, TextNormal);
                            UpdateScheduleLabel(scheduleLabels[i], compressors);
                        }
                        else
                        {
                            UpdateLabel(deviceNameLabels[i], "--", StatusDisabled);
                            UpdateLabelWithBackground(scheduleLabels[i], "No Schedule", TextNormal, ScheduleInactive);
                        }
                    }
                    else
                    {
                        UpdateLabel(factoryHeaders[i], "--", StatusDisabled);
                        UpdateLabel(deviceNameLabels[i], "--", StatusDisabled);
                        UpdateLabelWithBackground(scheduleLabels[i], "--", TextNormal, StatusDisabled);
                        UpdateLabel(statusLabels[i], "--", StatusDisabled);
                        UpdateLabel(precoolerLabels[i], "--", StatusDisabled);
                        UpdateLabel(dryerLabels[i], "--", StatusDisabled);
                        UpdateLabel(fanLabels[i], "--", StatusDisabled);
                        UpdateLabel(pressureLabels[i], "--", StatusDisabled);
                        UpdateLabel(tempLabels[i], "--", StatusDisabled);
                        UpdateLabel(powerLabels[i], "--", StatusDisabled);
                    }
                }
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            // 取得當前模式名稱，傳入排程設定表單
            string modeName = currentMode != null ? currentMode.Name : label3.Text;
            int modeId = currentMode != null ? currentMode.Id : 0;
            
            // 依目前廠域篩選工廠清單
            var factories = GetCurrentViewFactories();
            var factoryIds = new HashSet<int>(factories.Select(f => f.Id));

            ScheduleSettingForm scheduleForm = new ScheduleSettingForm(modeId, modeName, factoryIds);
            scheduleForm.ShowDialog();

            // 重新載入設定
            if (scheduleForm.DialogResult == DialogResult.OK)
            {
                // 重新從資料庫載入當前模式的排程資料
                if (currentMode != null)
                {
                    var updatedMode = ModeSelectForm.GetModeById(currentMode.Id);
                    if (updatedMode != null)
                    {
                        currentMode = updatedMode;
                        ModeSelectForm.ApplyModeSchedulesToConfig(updatedMode);
                    }
                }

                config.LoadConfig();
                lastDOStates.Clear();
                RefreshFactoryDisplay();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            using (var modeSelectForm = new ModeSelectForm())
            {
                if (modeSelectForm.ShowDialog() == DialogResult.OK && modeSelectForm.SelectedMode != null)
                {
                    var selectedMode = modeSelectForm.SelectedMode;
                    
                    // 儲存當前選擇的模式
                    currentMode = selectedMode;
                    
                    // 判斷是否為手動模式（模式名稱包含「手動」）
                    bool wasManual = isManualMode;
                    isManualMode = selectedMode.Name.Contains("手動");
                    
                    // 切換到手動模式時，清除手動 DO 狀態
                    if (isManualMode && !wasManual)
                    {
                        manualDOStates.Clear();
                        lastDOStates.Clear();
                    }
                    
                    // 更新 label3 顯示模式名稱
                    label3.Text = selectedMode.Name;
                    
                    // 更新 label4 顯示模式描述（如果有的話）
                    if (!string.IsNullOrEmpty(selectedMode.Description))
                    {
                        label4.Text = selectedMode.Description;
                    }
                    
                    // 重新載入設定（因為模式切換時已套用排程到設備）
                    config.LoadConfig();
                    RefreshFactoryDisplay();
                    UpdateStatusLabelCursors();

                    // 儲存到場域配置
                    SaveModeToSiteConfig(selectedMode);
                }
            }
        }

        /// <summary>
        /// 儲存模式到場域配置
        /// </summary>
        private void SaveModeToSiteConfig(ScheduleMode mode)
        {
            try
            {
                string currentSiteId = GetCurrentSiteId();
                var database = new DeviceDatabase(config.IP, config.DB, config.USER, config.Password);

                System.Diagnostics.Debug.WriteLine(
                    $"[MainForm] *** SAVING MODE *** Site: {currentSiteId} ({currentViewMode}), " +
                    $"Mode: {mode.Name} (ID:{mode.Id})");

                database.UpdateSiteMode(
                    currentSiteId,
                    mode.Id,
                    Environment.MachineName);

                System.Diagnostics.Debug.WriteLine(
                    $"[MainForm] Mode saved successfully to site '{currentSiteId}'");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[MainForm] Failed to save mode to site config: {ex.Message}");
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            var factories = GetCurrentViewFactories();
            var factoryIds = new HashSet<int>(factories.Select(f => f.Id));

            // 傳遞當前模式給 TrendChart，確保顯示正確的排程資料
            TrendChart trendChart = new TrendChart(currentMode, factoryIds);
            trendChart.Show();
        }

        /// <summary>
        /// device_col1 ~ device_col5 點選事件 - 開啟壓力/溫度曲線圖
        /// </summary>
        private void DeviceCol_Click(object sender, EventArgs e)
        {
            Label clickedLabel = sender as Label;
            if (clickedLabel == null) return;

            // 判斷點選的是哪一欄
            Label[] deviceLabels = { device_col1, device_col2, device_col3, device_col4, device_col5 };
            Label[] factoryLabels = { factory_col1, factory_col2, factory_col3, factory_col4, factory_col5 };

            string factoryName = null;
            for (int i = 0; i < deviceLabels.Length; i++)
            {
                if (clickedLabel == deviceLabels[i])
                {
                    factoryName = factoryLabels[i].Text;
                    break;
                }
            }

            DeviceTrendChartForm chartForm = new DeviceTrendChartForm(factoryName);
            chartForm.Show();
        }

        /// <summary>
        /// pressure_col1 ~ pressure_col5 點選事件 - 設定空壓上下限（依目前廠域）
        /// </summary>
        private void PressureCol_Click(object sender, EventArgs e)
        {
            var factories = GetCurrentViewFactories();
            var database = config.GetDeviceDatabase();
            using (var form = new AlarmLimitSettingForm(factories, "Pressure", database))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var kvp in form.ResultLimitsMap)
                    {
                        config.SaveAlarmLimits(kvp.Key, kvp.Value);
                    }

                    // 重新載入推播設定（因為可能被修改）
                    config.LoadTeamsNotificationSettingsFromDatabase();
                    InitializeTeamsNotification();
                }
            }
        }

        /// <summary>
        /// temp_col1 ~ temp_col5 點選事件 - 設定溫度上下限（依目前廠域）
        /// </summary>
        private void TempCol_Click(object sender, EventArgs e)
        {
            var factories = GetCurrentViewFactories();
            var database = config.GetDeviceDatabase();
            using (var form = new AlarmLimitSettingForm(factories, "Temp", database))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var kvp in form.ResultLimitsMap)
                    {
                        config.SaveAlarmLimits(kvp.Key, kvp.Value);
                    }

                    // 重新載入推播設定（因為可能被修改）
                    config.LoadTeamsNotificationSettingsFromDatabase();
                    InitializeTeamsNotification();
                }
            }
        }

        /// <summary>
        /// CompressedTemp_col1 ~ CompressedTemp_col5 點選事件 - 設定空壓溫度上下限(依目前廠域)
        /// </summary>
        private void CompressedTempCol_Click(object sender, EventArgs e)
        {
            var factories = GetCurrentViewFactories();
            var database = config.GetDeviceDatabase();
            using (var form = new AlarmLimitSettingForm(factories, "CompressedTemp", database))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var kvp in form.ResultLimitsMap)
                    {
                        config.SaveAlarmLimits(kvp.Key, kvp.Value);
                    }

                    // 重新載入推播設定(因為可能被修改)
                    config.LoadTeamsNotificationSettingsFromDatabase();
                    InitializeTeamsNotification();
                }
            }
        }

        /// <summary>
        /// 根據目前的 ViewMode 取得對應的工廠清單
        /// </summary>
        private List<FactoryConfig> GetCurrentViewFactories()
        {
            if (currentViewMode == ViewMode.CastingFactory)
            {
                return config.Factories.Where(f => f.Id == CASTING_FACTORY_ID).ToList();
            }
            else
            {
                return config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).ToList();
            }
        }

        /// <summary>
        /// 根據目前顯示欄位索引取得對應的 FactoryConfig
        /// </summary>
        private FactoryConfig GetFactoryByColumnIndex(int colIndex)
        {
            if (currentViewMode == ViewMode.CastingFactory)
            {
                return config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
            }
            else
            {
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
                if (colIndex < otherFactories.Count)
                    return otherFactories[colIndex];
                return null;
            }
        }

        /// <summary>
        /// 更新空壓 Label 並檢查是否超過上下限，超過則變色
        /// 推播通知由背景監控統一處理
        /// </summary>
        private void UpdatePressureLabelWithLimitCheck(Label label, string valueText, AlarmLimitsConfig limits, string deviceName = "")
        {
            double value;
            if (double.TryParse(valueText, out value))
            {
                bool overLimit = (limits.PressureUpperLimit != double.MaxValue && value > limits.PressureUpperLimit)
                              || (limits.PressureLowerLimit != double.MinValue && value < limits.PressureLowerLimit);
                if (overLimit)
                {
                    UpdateLabel(label, valueText, StatusOverLimit);
                    // 推播通知由 MonitorAllDevicesInBackground() 統一處理
                }
                else
                {
                    UpdateLabel(label, valueText, StatusRunning);
                }
            }
            else
            {
                UpdateLabel(label, valueText, StatusRunning);
            }
        }

        /// <summary>
        /// 更新溫度 Label 並檢查是否超過上下限，超過則變色
        /// 推播通知由背景監控統一處理
        /// </summary>
        private void UpdateTempLabelWithLimitCheck(Label label, string valueText, AlarmLimitsConfig limits, string deviceName = "")
        {
            double value;
            if (double.TryParse(valueText, out value))
            {
                bool overLimit = (limits.TempUpperLimit != double.MaxValue && value > limits.TempUpperLimit)
                              || (limits.TempLowerLimit != double.MinValue && value < limits.TempLowerLimit);
                if (overLimit)
                {
                    UpdateLabel(label, valueText, StatusOverLimit);
                    // 推播通知由 MonitorAllDevicesInBackground() 統一處理
                }
                else
                {
                    UpdateLabel(label, valueText, StatusRunning);
                }
            }
            else
            {
                UpdateLabel(label, valueText, StatusRunning);
            }
        }

        /// <summary>
        /// 更新溫度 Label 並檢查是否超過上下限，超過則變色
        /// 推播通知由背景監控統一處理
        /// </summary>
        private void UpdatePresureTempLabelWithLimitCheck(Label label, string valueText, AlarmLimitsConfig limits, string deviceName = "")
        {
            double value;
            if (double.TryParse(valueText, out value))
            {
                // 這個方法目前沒有上下限，保留原功能
                UpdateLabel(label, valueText, StatusRunning);
            }
            else
            {
                UpdateLabel(label, valueText, StatusRunning);
            }
        }

        /// <summary>
        /// 更新空壓溫度 Label 並檢查是否超過上下限，超過則變色
        /// 推播通知由背景監控統一處理
        /// </summary>
        private void UpdateCompressedTempLabelWithLimitCheck(Label label, string valueText, AlarmLimitsConfig limits, string deviceName = "")
        {
            double value;
            if (double.TryParse(valueText, out value))
            {
                bool overLimit = (limits.CompressedTempUpperLimit != double.MaxValue && value > limits.CompressedTempUpperLimit)
                              || (limits.CompressedTempLowerLimit != double.MinValue && value < limits.CompressedTempLowerLimit);
                if (overLimit)
                {
                    UpdateLabel(label, valueText, StatusOverLimit);
                    // 推播通知由 MonitorAllDevicesInBackground() 統一處理
                }
                else
                {
                    UpdateLabel(label, valueText, StatusRunning);
                }
            }
            else
            {
                UpdateLabel(label, valueText, StatusRunning);
            }
        }


        // ========================================
        // 背景監控與推播（新版）
        // ========================================

        /// <summary>
        /// 取得工廠中當前有排程運行的空壓機 ID 集合
        /// </summary>
        private HashSet<int> GetScheduledCompressorIds(FactoryConfig factory)
        {
            var scheduledIds = new HashSet<int>();

            if (currentMode == null || currentMode.Schedules == null)
                return scheduledIds;

            // 找出該工廠當前時間內有排程的所有空壓機
            var activeSchedules = currentMode.Schedules.Where(s =>
                s.FactoryId == factory.Id &&
                s.Enabled &&
                IsInSchedule(s)).ToList();

            foreach (var schedule in activeSchedules)
            {
                scheduledIds.Add(schedule.MachineNo);
            }

            System.Diagnostics.Debug.WriteLine($"[排程推播] {factory.Name} 當前有 {scheduledIds.Count} 台空壓機在排程時間內");

            return scheduledIds;
        }

        /// <summary>
        /// 背景監控所有設備狀態（不受當前視圖限制）
        /// 檢查所有工廠的空壓、溫度、警報和故障狀態
        /// 只推播有排程運行的設備
        /// </summary>
        private void MonitorAllDevicesInBackground()
        {
            if (config == null || config.Factories == null || modbusList == null)
            {
                //System.Diagnostics.Debug.WriteLine($"[背景監控] 監控未啟動: config={config != null}, Factories={config?.Factories != null}, modbusList={modbusList != null}");
                return;
            }

            try
            {
                // 收集所有異常（設備名稱 + 數值）
                var pressureAbnormalDevices = new Dictionary<string, string>(); // 設備名稱 → 壓力值
                var tempAbnormalDevices = new Dictionary<string, string>();     // 設備名稱 → 溫度值
                var compressedTempAbnormalDevices = new Dictionary<string, string>(); // 設備名稱 → 空壓溫度值
                var alarmDevices = new List<string>();
                var faultDevices = new List<string>();

                //System.Diagnostics.Debug.WriteLine($"[背景監控] 開始監控，共 {config.Factories.Count} 個工廠");

                // 遍歷所有工廠
                for (int factoryIndex = 0; factoryIndex < config.Factories.Count; factoryIndex++)
                {
                    var factory = config.Factories[factoryIndex];

                    // 檢查 modbus 連線是否存在
                    if (factoryIndex >= modbusList.Count)
                    {
                        //System.Diagnostics.Debug.WriteLine($"[背景監控] {factory.Name} - modbus 索引超出範圍");
                        continue;
                    }

                    var modbus = modbusList[factoryIndex];

                    // 檢查連線狀態
                    if (modbus == null || modbus.address_val == null || !modbus.ConnectState)
                    {
                        //System.Diagnostics.Debug.WriteLine($"[背景監控] {factory.Name} - modbus 未連線 (modbus={modbus != null}, address_val={modbus?.address_val != null}, ConnectState={modbus?.ConnectState})");
                        continue;
                    }

                    // 取得該工廠當前有排程的空壓機 ID 集合
                    var scheduledCompressorIds = GetScheduledCompressorIds(factory);

                    // 如果沒有任何空壓機有排程，跳過此工廠
                    if (scheduledCompressorIds.Count == 0)
                    {
                        //System.Diagnostics.Debug.WriteLine($"[排程推播] {factory.Name} 沒有空壓機在排程時間內，跳過監控");
                        continue;
                    }

                    //System.Diagnostics.Debug.WriteLine($"[背景監控] {factory.Name} - 有 {scheduledCompressorIds.Count} 台空壓機在排程中");

                    // 取得該工廠的空壓和溫度值
                    string pressure = GetPressureValue(modbus);
                    string temp = GetTempValue(modbus);
                    string compressedTemp = GetCompressedTempValue(modbus);

                    //System.Diagnostics.Debug.WriteLine($"[背景監控] {factory.Name} - 空壓={pressure}, 溫度={temp}, 空壓溫度={compressedTemp}, 上限=[空壓:{factory.AlarmLimits.PressureUpperLimit}, 溫度:{factory.AlarmLimits.TempUpperLimit}, 空壓溫度:{factory.AlarmLimits.CompressedTempUpperLimit}], 下限=[空壓:{factory.AlarmLimits.PressureLowerLimit}, 溫度:{factory.AlarmLimits.TempLowerLimit}, 空壓溫度:{factory.AlarmLimits.CompressedTempLowerLimit}]");

                    // 取得該工廠的所有空壓機
                    var compressors = factory.GetDevicesByType(DeviceType.Compressor);

                    // 只保留有排程的空壓機
                    var scheduledCompressors = compressors.Where(c => scheduledCompressorIds.Contains(c.MachineNo)).ToList();

                    // 檢查空壓和溫度是否超限（只針對有排程的空壓機）
                    if (scheduledCompressors.Count > 0)
                    {
                        // 檢查空壓超限
                        double pressureValue;
                        if (double.TryParse(pressure, out pressureValue))
                        {
                            bool pressureOverLimit = (factory.AlarmLimits.PressureUpperLimit != double.MaxValue && pressureValue > factory.AlarmLimits.PressureUpperLimit)
                                                  || (factory.AlarmLimits.PressureLowerLimit != double.MinValue && pressureValue < factory.AlarmLimits.PressureLowerLimit);

                            if (pressureOverLimit)
                            {
                                //System.Diagnostics.Debug.WriteLine($"[背景監控] {factory.Name} - 空壓超限！數值={pressureValue}, 上限={factory.AlarmLimits.PressureUpperLimit}, 下限={factory.AlarmLimits.PressureLowerLimit}");
                                foreach (var compressor in scheduledCompressors)
                                {
                                    pressureAbnormalDevices[compressor.Name] = pressure + "|" + GetPowerValueFromDB(compressor.Name); // 儲存設備名稱和壓力值
                                }
                            }
                        }

                        // 檢查溫度超限
                        double tempValue;
                        if (double.TryParse(temp, out tempValue))
                        {
                            bool tempOverLimit = (factory.AlarmLimits.TempUpperLimit != double.MaxValue && tempValue > factory.AlarmLimits.TempUpperLimit)
                                              || (factory.AlarmLimits.TempLowerLimit != double.MinValue && tempValue < factory.AlarmLimits.TempLowerLimit);

                            if (tempOverLimit)
                            {
                                //System.Diagnostics.Debug.WriteLine($"[背景監控] {factory.Name} - 溫度超限！數值={tempValue}, 上限={factory.AlarmLimits.TempUpperLimit}, 下限={factory.AlarmLimits.TempLowerLimit}");
                                foreach (var compressor in scheduledCompressors)
                                {
                                    tempAbnormalDevices[compressor.Name] = temp + "|" + GetPowerValueFromDB(compressor.Name); // 儲存設備名稱和溫度值
                                }
                            }
                        }

                        // 檢查空壓溫度超限
                        double compressedTempValue;
                        if (double.TryParse(compressedTemp, out compressedTempValue))
                        {
                            bool compressedTempOverLimit = (factory.AlarmLimits.CompressedTempUpperLimit != double.MaxValue && compressedTempValue > factory.AlarmLimits.CompressedTempUpperLimit)
                                                         || (factory.AlarmLimits.CompressedTempLowerLimit != double.MinValue && compressedTempValue < factory.AlarmLimits.CompressedTempLowerLimit);

                            if (compressedTempOverLimit)
                            {
                                //System.Diagnostics.Debug.WriteLine($"[背景監控] {factory.Name} - 空壓溫度超限！數值={compressedTempValue}, 上限={factory.AlarmLimits.CompressedTempUpperLimit}, 下限={factory.AlarmLimits.CompressedTempLowerLimit}");
                                foreach (var compressor in scheduledCompressors)
                                {
                                    compressedTempAbnormalDevices[compressor.Name] = compressedTemp + "|" + GetPowerValueFromDB(compressor.Name); // 儲存設備名稱和空壓溫度值
                                }
                            }
                        }
                    }

                    // 檢查每台有排程的空壓機的警報和故障狀態
                    foreach (var compressor in scheduledCompressors)
                    {
                        bool isAlarm = GetDIValue(modbus, compressor.IO.AlarmDI);
                        bool isFault = GetDIValue(modbus, compressor.IO.FaultDI);

                        // 收集警報設備
                        if (isAlarm && !isFault)
                        {
                            if (!alarmDevices.Contains(compressor.Name))
                                alarmDevices.Add(compressor.Name);
                        }

                        // 收集故障設備
                        if (isFault)
                        {
                            if (!faultDevices.Contains(compressor.Name))
                                faultDevices.Add(compressor.Name);
                        }

                        // 更新設備狀態記錄（用於追蹤狀態變更）
                        string currentStatus = isFault ? "故障" : (isAlarm ? "警報" : "正常");
                        if (!lastDeviceAlertStates.ContainsKey(compressor.Name))
                        {
                            lastDeviceAlertStates[compressor.Name] = "正常";
                        }
                    }
                }

                // 統一發送推播通知
                SendCombinedAbnormalNotification(pressureAbnormalDevices, tempAbnormalDevices, compressedTempAbnormalDevices, alarmDevices, faultDevices);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[背景監控] 監控失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 發送合併的異常通知
        /// 全局計時器邏輯：
        /// - 只要有任何設備異常（空壓/溫度/空壓溫度超限）→ 開始計時
        /// - 所有設備恢復正常 → 計時歸零
        /// - 達到延遲時間 → 推播所有當前異常
        /// - 警報/故障 → 立即推播（不延遲）
        /// </summary>
        private void SendCombinedAbnormalNotification(Dictionary<string, string> pressureAbnormal, Dictionary<string, string> tempAbnormal, Dictionary<string, string> compressedTempAbnormal, List<string> alarmDevices, List<string> faultDevices)
        {
            //System.Diagnostics.Debug.WriteLine($"[推播檢查] 進入推播檢查，空壓異常={pressureAbnormal.Count}, 溫度異常={tempAbnormal.Count}, 警報={alarmDevices.Count}, 故障={faultDevices.Count}");
            //System.Diagnostics.Debug.WriteLine($"[推播檢查] 推播設定: Enabled={config.TeamsNotificationEnabled}, AlarmDelayMinutes={config.AlarmDelayMinutes}, CooldownMinutes={config.NotificationCooldownMinutes}");

            // 移除重複設備名稱
            alarmDevices = alarmDevices.Distinct().ToList();
            faultDevices = faultDevices.Distinct().ToList();

            // ========================================
            // 步驟 1: 檢查是否有空壓/溫度/空壓溫度異常
            // ========================================
            bool hasPressureTempAbnormal = pressureAbnormal.Count > 0 || tempAbnormal.Count > 0 || compressedTempAbnormal.Count > 0;

            // 更新當前異常設備集合
            currentAbnormalDevices.Clear();
            currentAbnormalDevices.UnionWith(pressureAbnormal.Keys);
            currentAbnormalDevices.UnionWith(tempAbnormal.Keys);
            currentAbnormalDevices.UnionWith(compressedTempAbnormal.Keys);

            // ========================================
            // 步驟 2: 管理全局計時器
            // ========================================
            if (hasPressureTempAbnormal)
            {
                // 有異常：如果計時器還沒啟動，就啟動它
                if (!globalAbnormalStartTime.HasValue)
                {
                    globalAbnormalStartTime = DateTime.Now;
                    //System.Diagnostics.Debug.WriteLine($"[推播延遲] 全局計時器啟動，異常設備: {string.Join(", ", currentAbnormalDevices)}");
                }
                else
                {
                    // 計時器已在運行，顯示當前進度
                    TimeSpan duration = DateTime.Now - globalAbnormalStartTime.Value;
                    //System.Diagnostics.Debug.WriteLine($"[推播延遲] 全局計時器運行中，已持續 {duration.TotalMinutes:F1} 分鐘，異常設備: {string.Join(", ", currentAbnormalDevices)}");
                }
            }
            else
            {
                // 沒有異常：重置計時器
                if (globalAbnormalStartTime.HasValue)
                {
                    //System.Diagnostics.Debug.WriteLine($"[推播延遲] 所有設備恢復正常，計時器歸零");
                    globalAbnormalStartTime = null;
                }
            }

            // ========================================
            // 步驟 3: 建立推播訊息（新格式）
            // ========================================
            bool shouldSend = false;
            var tempAbnormalList = new Dictionary<string, string>();
            var pressureAbnormalList = new Dictionary<string, string>();
            var compressedTempAbnormalList = new Dictionary<string, string>();

            // 3.1 檢查空壓/溫度是否達到推播條件
            if (hasPressureTempAbnormal && globalAbnormalStartTime.HasValue)
            {
                TimeSpan duration = DateTime.Now - globalAbnormalStartTime.Value;

                if (duration.TotalMinutes >= config.AlarmDelayMinutes)
                {
                    // 達到延遲時間，準備推播
                    shouldSend = true;
                    tempAbnormalList = new Dictionary<string, string>(tempAbnormal);
                    pressureAbnormalList = new Dictionary<string, string>(pressureAbnormal);
                    compressedTempAbnormalList = new Dictionary<string, string>(compressedTempAbnormal);
                    //System.Diagnostics.Debug.WriteLine($"[推播延遲] 異常已持續 {duration.TotalMinutes:F1} 分鐘，符合推播條件");
                }
                else
                {
                    // 尚未達到延遲時間
                    //System.Diagnostics.Debug.WriteLine($"[推播延遲] 異常已持續 {duration.TotalMinutes:F1} 分鐘，尚未達到推播條件 ({config.AlarmDelayMinutes} 分鐘)");
                }
            }

            // 3.2 設備警報：立即推播（不延遲）
            if (alarmDevices.Count > 0)
            {
                shouldSend = true;
                System.Diagnostics.Debug.WriteLine($"[推播] 設備警報偵測到，立即推播: {string.Join(", ", alarmDevices)}");
            }

            // 3.3 設備故障：立即推播（不延遲）
            if (faultDevices.Count > 0)
            {
                shouldSend = true;
                System.Diagnostics.Debug.WriteLine($"[推播] 設備故障偵測到，立即推播: {string.Join(", ", faultDevices)}");
            }

            // ========================================
            // 步驟 4: 發送推播
            // ========================================
            if (!shouldSend)
            {
                System.Diagnostics.Debug.WriteLine($"[推播] 沒有符合推播條件的異常");
                return;
            }

            // 發送 Teams 通知
            if (teamsNotificationService != null && config.TeamsNotificationEnabled)
            {
                try
                {
                    Task.Run(async () =>
                    {
                        await teamsNotificationService.SendCombinedAbnormalAlertAsync(tempAbnormalList, pressureAbnormalList, compressedTempAbnormalList, alarmDevices, faultDevices);
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[推播] 發送合併通知失敗: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 檢查並處理設備狀態變更（警報/故障）
        /// </summary>
        private void CheckAndNotifyDeviceStatus(string deviceName, bool isAlarm, bool isFault)
        {
            string currentStatus = isFault ? "故障" : (isAlarm ? "警報" : "正常");

            // 檢查是否有狀態變更
            if (!lastDeviceAlertStates.ContainsKey(deviceName))
            {
                lastDeviceAlertStates[deviceName] = "正常"; // 初始化為正常
            }

            string lastStatus = lastDeviceAlertStates[deviceName];

            // 只在狀態變更且變成警報或故障時發送通知
            if (currentStatus != lastStatus && (isAlarm || isFault))
            {
                System.Diagnostics.Debug.WriteLine($"[推播] 設備狀態變更! 設備={deviceName}, 上次狀態={lastStatus}, 當前狀態={currentStatus}");

                // 更新記錄的狀態
                lastDeviceAlertStates[deviceName] = currentStatus;

                // 如果 Teams 通知服務已啟用，發送通知
                if (teamsNotificationService != null && config.TeamsNotificationEnabled)
                {
                    try
                    {
                        // 使用 Task.Run 避免阻塞 UI 執行緒
                        Task.Run(async () =>
                        {
                            if (isFault)
                            {
                                await teamsNotificationService.SendDeviceFaultAsync(deviceName, currentStatus);
                            }
                            else if (isAlarm)
                            {
                                await teamsNotificationService.SendDeviceAlarmAsync(deviceName, currentStatus);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[推播] 發送設備狀態通知失敗: {ex.Message}");
                    }
                }
            }
            else if (currentStatus == "正常" && lastStatus != "正常")
            {
                // 狀態恢復正常時，也更新記錄
                System.Diagnostics.Debug.WriteLine($"[推播] 設備狀態恢復正常! 設備={deviceName}, 上次狀態={lastStatus}");
                lastDeviceAlertStates[deviceName] = currentStatus;
            }
        }

        /// <summary>
        /// 更新 status_col 的游標樣式（手動模式下顯示手型游標）
        /// </summary>
        private void UpdateStatusLabelCursors()
        {
            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            foreach (var label in statusLabels)
            {
                label.Cursor = isManualMode ? Cursors.Hand : Cursors.Default;
            }
        }

        /// <summary>
        /// status_col1 ~ status_col5 點選事件 - 手動模式下切換空壓機啟動/停止
        /// </summary>
        private void StatusCol_Click(object sender, EventArgs e)
        {
            if (!isManualMode) return;

            Label clickedLabel = sender as Label;
            if (clickedLabel == null) return;

            Label[] statusLabels = { status_col1, status_col2, status_col3, status_col4, status_col5 };
            int colIndex = -1;
            for (int i = 0; i < statusLabels.Length; i++)
            {
                if (clickedLabel == statusLabels[i])
                {
                    colIndex = i;
                    break;
                }
            }
            if (colIndex < 0) return;

            if (currentViewMode == ViewMode.CastingFactory)
            {
                // 鑄造廠模式：每欄一台壓縮機
                var castingFactory = config.Factories.FirstOrDefault(f => f.Id == CASTING_FACTORY_ID);
                if (castingFactory == null) return;

                int modbusIndex = config.Factories.IndexOf(castingFactory);
                if (modbusIndex >= modbusList.Count) return;

                var modbus = modbusList[modbusIndex];
                var compressors = castingFactory.GetDevicesByType(DeviceType.Compressor).OrderBy(c => c.MachineNo).ToList();
                if (colIndex >= compressors.Count) return;

                var compressor = compressors[colIndex];
                ToggleCompressorManual(castingFactory, compressor, modbus);
            }
            else
            {
                // 其它廠域模式：每欄一個工廠
                var otherFactories = config.Factories.Where(f => f.Id != CASTING_FACTORY_ID).Take(5).ToList();
                if (colIndex >= otherFactories.Count) return;

                var factory = otherFactories[colIndex];
                int modbusIndex = config.Factories.IndexOf(factory);
                if (modbusIndex >= modbusList.Count) return;

                var modbus = modbusList[modbusIndex];
                var compressors = factory.GetDevicesByType(DeviceType.Compressor);

                if (compressors.Count == 1)
                {
                    ToggleCompressorManual(factory, compressors[0], modbus);
                }
                else if (compressors.Count > 1)
                {
                    // 多台壓縮機時，彈出選擇視窗
                    using (var selectForm = new ManualCompressorSelectForm(factory, compressors, manualDOStates))
                    {
                        if (selectForm.ShowDialog() == DialogResult.OK && selectForm.SelectedCompressor != null)
                        {
                            ToggleCompressorManual(factory, selectForm.SelectedCompressor, modbus);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 手動切換壓縮機的啟動/停止
        /// </summary>
        private void ToggleCompressorManual(FactoryConfig factory, DeviceConfig compressor, ModBus_List modbus)
        {
            if (compressor.IO.ControlDO < 0)
            {
                MessageBox.Show($"{compressor.Name} 未設定控制 DO，無法手動控制", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!modbus.ConnectState)
            {
                MessageBox.Show($"{factory.Name} 通訊中斷，無法控制", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string key = factory.Id + "_" + compressor.MachineNo;

            // 取得目前手動狀態，預設為停止(0)
            ushort currentState;
            if (!manualDOStates.TryGetValue(key, out currentState))
                currentState = 0;

            // 切換狀態
            ushort newState = currentState == 1 ? (ushort)0 : (ushort)1;
            string actionText = newState == 1 ? "啟動" : "停止";

            var result = MessageBox.Show(
                $"確定要{actionText} {factory.Name} - {compressor.Name} 嗎？",
                "手動控制確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bool success = modbus.WriteDO(compressor.IO.ControlDO, newState);
                if (success)
                {
                    manualDOStates[key] = newState;
                    lastDOStates[key] = newState;
                    System.Diagnostics.Debug.WriteLine(
                        $"[手動控制] {factory.Name} {compressor.Name} DO_{compressor.IO.ControlDO} = {newState} ({actionText})");
                }
                else
                {
                    MessageBox.Show($"控制指令發送失敗", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// Device Status Structure
    /// </summary>
    public struct DeviceStatus
    {
        public string Text { get; }
        public Color Color { get; }

        public DeviceStatus(string text, Color color)
        {
            Text = text;
            Color = color;
        }
    }

    /// <summary>
    /// Compressor Status Structure
    /// </summary>
    public struct CompressorStatus
    {
        public string Name { get; set; }
        public int MachineNo { get; set; }
        public DeviceStatus Status { get; set; }
    }
}
