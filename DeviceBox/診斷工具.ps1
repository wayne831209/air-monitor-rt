# 快速診斷腳本
# 執行此腳本來收集完整的診斷資訊

Write-Host "=== 場域同步診斷工具 ===" -ForegroundColor Cyan
Write-Host ""

$binPath = "DeviceBox\bin\Debug"
$exePath = Join-Path $binPath "DeviceBox.exe"

# 1. 檢查 exe 是否存在
Write-Host "[1/5] 檢查 DeviceBox.exe..." -ForegroundColor Yellow
if (Test-Path $exePath) {
	Write-Host "  ✓ 找到執行檔: $exePath" -ForegroundColor Green
} else {
	Write-Host "  ✗ 找不到執行檔: $exePath" -ForegroundColor Red
	Write-Host "  請先建置專案 (Ctrl+Shift+B)" -ForegroundColor Red
	exit 1
}

# 2. 檢查舊的配置檔案
Write-Host ""
Write-Host "[2/5] 檢查配置檔案..." -ForegroundColor Yellow
$oldConfig = Join-Path $binPath "site.config"
$siteConfigs = Get-ChildItem -Path $binPath -Filter "site_*.config" -ErrorAction SilentlyContinue

if (Test-Path $oldConfig) {
	Write-Host "  ⚠ 發現舊的 site.config，建議刪除" -ForegroundColor Yellow
	Write-Host "    路徑: $oldConfig" -ForegroundColor Gray
}

if ($siteConfigs.Count -gt 0) {
	Write-Host "  發現 $($siteConfigs.Count) 個場域配置檔案:" -ForegroundColor Cyan
	foreach ($config in $siteConfigs) {
		Write-Host "    - $($config.Name)" -ForegroundColor Gray
		# 顯示內容
		$content = Get-Content $config.FullName -Raw
		if ($content -match '<SiteId>([^<]+)</SiteId>') {
			Write-Host "      場域: $($matches[1])" -ForegroundColor DarkGray
		}
	}
}

# 3. 檢查是否有 DeviceBox 正在執行
Write-Host ""
Write-Host "[3/5] 檢查執行中的 DeviceBox 實例..." -ForegroundColor Yellow
$processes = Get-Process -Name "DeviceBox" -ErrorAction SilentlyContinue
if ($processes) {
	Write-Host "  發現 $($processes.Count) 個執行中的實例:" -ForegroundColor Cyan
	foreach ($proc in $processes) {
		Write-Host "    - PID: $($proc.Id), 啟動時間: $($proc.StartTime)" -ForegroundColor Gray
		$configFile = Join-Path $binPath "site_$($proc.Id).config"
		if (Test-Path $configFile) {
			$content = Get-Content $configFile -Raw
			if ($content -match '<SiteId>([^<]+)</SiteId>.*<SiteName>([^<]+)</SiteName>') {
				Write-Host "      → 場域: $($matches[1]) ($($matches[2]))" -ForegroundColor DarkGray
			}
		}
	}
	Write-Host ""
	Write-Host "  建議：關閉所有實例後重新測試" -ForegroundColor Yellow
} else {
	Write-Host "  ✓ 沒有執行中的實例" -ForegroundColor Green
}

# 4. 清理殘留的配置檔案
Write-Host ""
Write-Host "[4/5] 清理殘留的配置檔案..." -ForegroundColor Yellow
$cleaned = 0
foreach ($config in $siteConfigs) {
	$pid = $config.Name -replace 'site_(\d+)\.config', '$1'
	$procExists = Get-Process -Id $pid -ErrorAction SilentlyContinue
	if (-not $procExists) {
		Write-Host "  刪除殘留配置: $($config.Name) (PID $pid 已不存在)" -ForegroundColor Gray
		Remove-Item $config.FullName -Force
		$cleaned++
	}
}
if ($cleaned -eq 0) {
	Write-Host "  ✓ 沒有需要清理的檔案" -ForegroundColor Green
} else {
	Write-Host "  已清理 $cleaned 個殘留檔案" -ForegroundColor Cyan
}

# 5. 測試資料庫連線
Write-Host ""
Write-Host "[5/5] 檢查資料庫連線..." -ForegroundColor Yellow
Write-Host "  請手動在 MySQL Workbench 執行以下 SQL：" -ForegroundColor Cyan
Write-Host ""
Write-Host "  USE ycm_energy;" -ForegroundColor White
Write-Host "  SELECT site_id, site_name, current_mode_id, config_version FROM site_config;" -ForegroundColor White
Write-Host ""

# 總結
Write-Host ""
Write-Host "=== 診斷完成 ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "接下來的測試步驟：" -ForegroundColor Yellow
Write-Host "1. 啟動第一個 DeviceBox.exe，選擇「其他場域」" -ForegroundColor White
Write-Host "2. 啟動第二個 DeviceBox.exe，選擇「鑄造廠」" -ForegroundColor White
Write-Host "3. 在 Visual Studio 開啟 Output 視窗 (Ctrl+Alt+O)" -ForegroundColor White
Write-Host "4. 在第二個軟體切換模式" -ForegroundColor White
Write-Host "5. 觀察 Output 視窗的日誌" -ForegroundColor White
Write-Host ""
Write-Host "重點關注的日誌訊息：" -ForegroundColor Yellow
Write-Host "  - [SiteManager] Instance created for PID XXX" -ForegroundColor Gray
Write-Host "  - [MainForm] Starting sync service for site: XXX" -ForegroundColor Gray
Write-Host "  - [ConfigSyncService] Checking site 'XXX'" -ForegroundColor Gray
Write-Host "  - [MainForm] *** SAVING MODE *** Site: XXX" -ForegroundColor Gray
Write-Host "  - [MainForm] *** SYNC EVENT RECEIVED ***" -ForegroundColor Gray
Write-Host "  - [MainForm] *** IGNORING *** (如果看到這個，表示過濾有效)" -ForegroundColor Gray
Write-Host ""
Write-Host "如果問題依舊，請提供完整的 Output 日誌！" -ForegroundColor Cyan
