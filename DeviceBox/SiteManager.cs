using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DeviceBox
{
    /// <summary>
    /// 場域管理器 - 負責場域識別和本機配置儲存
    /// </summary>
    public class SiteManager
    {
        // 使用 Process ID 區分不同軟體實例的配置檔案
        private static readonly string SiteConfigPath = Path.Combine(
            Application.StartupPath, 
            $"site_{System.Diagnostics.Process.GetCurrentProcess().Id}.config");

        private static SiteManager _instance;
        private static readonly object _lock = new object();

        public string CurrentSiteId { get; private set; }
        public string CurrentSiteName { get; private set; }

        private SiteManager()
        {
            LoadSiteConfig();
            System.Diagnostics.Debug.WriteLine(
                $"[SiteManager] Instance created for PID {System.Diagnostics.Process.GetCurrentProcess().Id}, " +
                $"config file: {SiteConfigPath}");
        }

        /// <summary>
        /// 取得單例實例
        /// </summary>
        public static SiteManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SiteManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 載入本機場域配置
        /// </summary>
        private void LoadSiteConfig()
        {
            try
            {
                if (File.Exists(SiteConfigPath))
                {
                    XDocument doc = XDocument.Load(SiteConfigPath);
                    CurrentSiteId = doc.Root?.Element("SiteId")?.Value;
                    CurrentSiteName = doc.Root?.Element("SiteName")?.Value;

                    System.Diagnostics.Debug.WriteLine($"[SiteManager] Loaded site: {CurrentSiteId} - {CurrentSiteName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SiteManager] No site.config found, will prompt user to select");
                    CurrentSiteId = null;
                    CurrentSiteName = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SiteManager] Error loading site config: {ex.Message}");
                CurrentSiteId = null;
                CurrentSiteName = null;
            }
        }

        /// <summary>
        /// 儲存場域配置到本機
        /// </summary>
        public void SaveSiteConfig(string siteId, string siteName)
        {
            try
            {
                var doc = new XDocument(
                    new XElement("SiteConfig",
                        new XElement("SiteId", siteId),
                        new XElement("SiteName", siteName),
                        new XElement("UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    )
                );

                doc.Save(SiteConfigPath);
                CurrentSiteId = siteId;
                CurrentSiteName = siteName;

                System.Diagnostics.Debug.WriteLine($"[SiteManager] Saved site config: {siteId} - {siteName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SiteManager] Error saving site config: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 檢查是否已設定場域
        /// </summary>
        public bool IsSiteConfigured()
        {
            return !string.IsNullOrEmpty(CurrentSiteId);
        }

        /// <summary>
        /// 重設場域配置
        /// </summary>
        public void ResetSiteConfig()
        {
            try
            {
                if (File.Exists(SiteConfigPath))
                {
                    File.Delete(SiteConfigPath);
                }
                CurrentSiteId = null;
                CurrentSiteName = null;
                System.Diagnostics.Debug.WriteLine("[SiteManager] Site config reset");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SiteManager] Error resetting site config: {ex.Message}");
            }
        }

        /// <summary>
        /// 取得電腦名稱(用於記錄更新來源)
        /// </summary>
        public static string GetComputerIdentifier()
        {
            try
            {
                return Environment.MachineName;
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        /// <summary>
        /// 清理當前實例的配置檔案 (在應用程式關閉時呼叫)
        /// </summary>
        public void Cleanup()
        {
            try
            {
                if (File.Exists(SiteConfigPath))
                {
                    File.Delete(SiteConfigPath);
                    System.Diagnostics.Debug.WriteLine($"[SiteManager] Cleaned up config file: {SiteConfigPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SiteManager] Cleanup error: {ex.Message}");
            }
        }
    }
}
