using System;
using System.Timers;

namespace DeviceBox
{
    /// <summary>
    /// 配置同步服務 - 定期檢查並同步場域配置
    /// </summary>
    public class ConfigSyncService : IDisposable
    {
        private Timer syncTimer;
        private readonly DeviceDatabase database;
        private readonly string siteId;
        private int lastKnownVersion = 0;
        private DateTime lastSyncTime = DateTime.MinValue;
        private bool isDisposed = false;

        /// <summary>
        /// 配置更新事件
        /// </summary>
        public event EventHandler<ConfigUpdatedEventArgs> ConfigUpdated;

        /// <summary>
        /// 同步間隔(毫秒),預設 5 秒
        /// </summary>
        public int SyncIntervalMs { get; set; } = 5000;

        public ConfigSyncService(DeviceDatabase db, string currentSiteId)
        {
            database = db ?? throw new ArgumentNullException(nameof(db));
            siteId = currentSiteId ?? throw new ArgumentNullException(nameof(currentSiteId));

            // 讀取初始版本
            var initialConfig = database.LoadSiteConfig(siteId);
            if (initialConfig != null)
            {
                lastKnownVersion = initialConfig.ConfigVersion;
            }

            System.Diagnostics.Debug.WriteLine($"[ConfigSyncService] Initialized for site: {siteId}, version: {lastKnownVersion}");
        }

        /// <summary>
        /// 啟動同步服務
        /// </summary>
        public void Start()
        {
            if (syncTimer != null)
            {
                System.Diagnostics.Debug.WriteLine("[ConfigSyncService] Already running");
                return;
            }

            syncTimer = new Timer(SyncIntervalMs);
            syncTimer.Elapsed += SyncTimer_Elapsed;
            syncTimer.AutoReset = true;
            syncTimer.Start();

            System.Diagnostics.Debug.WriteLine($"[ConfigSyncService] Started with interval: {SyncIntervalMs}ms");
        }

        /// <summary>
        /// 停止同步服務
        /// </summary>
        public void Stop()
        {
            if (syncTimer != null)
            {
                syncTimer.Stop();
                syncTimer.Elapsed -= SyncTimer_Elapsed;
                syncTimer.Dispose();
                syncTimer = null;

                System.Diagnostics.Debug.WriteLine("[ConfigSyncService] Stopped");
            }
        }

        private void SyncTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                CheckForUpdates();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConfigSyncService] Error during sync: {ex.Message}");
            }
        }

        /// <summary>
        /// 檢查配置更新
        /// </summary>
        private void CheckForUpdates()
        {
            try
            {
                var currentConfig = database.LoadSiteConfig(siteId);

                if (currentConfig == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ConfigSyncService] No config found for site: {siteId}");
                    return;
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[ConfigSyncService] Checking site '{siteId}': current_version={currentConfig.ConfigVersion}, " +
                    $"last_known={lastKnownVersion}, mode={currentConfig.CurrentModeId}");

                // 檢查版本是否有變更
                if (currentConfig.ConfigVersion > lastKnownVersion)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[ConfigSyncService] *** VERSION CHANGE DETECTED *** " +
                        $"Site: {siteId}, Version: {lastKnownVersion} -> {currentConfig.ConfigVersion}");
                    System.Diagnostics.Debug.WriteLine(
                        $"[ConfigSyncService] Updated by: {currentConfig.LastUpdatedBy} at {currentConfig.UpdatedAt}");

                    lastKnownVersion = currentConfig.ConfigVersion;
                    lastSyncTime = DateTime.Now;

                    // 觸發更新事件
                    OnConfigUpdated(new ConfigUpdatedEventArgs
                    {
                        SiteId = currentConfig.SiteId,
                        SiteName = currentConfig.SiteName,
                        CurrentModeId = currentConfig.CurrentModeId,
                        ConfigVersion = currentConfig.ConfigVersion,
                        UpdatedBy = currentConfig.LastUpdatedBy,
                        UpdatedAt = currentConfig.UpdatedAt
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConfigSyncService] CheckForUpdates error: {ex.Message}");
            }
        }

        /// <summary>
        /// 手動觸發同步檢查
        /// </summary>
        public void ForceSync()
        {
            System.Diagnostics.Debug.WriteLine("[ConfigSyncService] Force sync triggered");
            CheckForUpdates();
        }

        /// <summary>
        /// 觸發配置更新事件
        /// </summary>
        protected virtual void OnConfigUpdated(ConfigUpdatedEventArgs e)
        {
            ConfigUpdated?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                Stop();
                isDisposed = true;
                System.Diagnostics.Debug.WriteLine("[ConfigSyncService] Disposed");
            }
        }
    }

    /// <summary>
    /// 配置更新事件參數
    /// </summary>
    public class ConfigUpdatedEventArgs : EventArgs
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public int? CurrentModeId { get; set; }
        public int ConfigVersion { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
