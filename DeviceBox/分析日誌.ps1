# 自動分析場域同步問題的腳本
# 此腳本會：
# 1. 找到最新的 debug 日誌檔案
# 2. 分析關鍵訊息
# 3. 顯示問題診斷結果

param(
	[string]$LogPath = "DeviceBox\bin\Debug"
)

Write-Host "=== 場域同步日誌分析工具 ===" -ForegroundColor Cyan
Write-Host ""

# 尋找日誌檔案
$logFiles = Get-ChildItem -Path $LogPath -Filter "debug_*.log" -ErrorAction SilentlyContinue | 
			Sort-Object LastWriteTime -Descending

if ($logFiles.Count -eq 0) {
	Write-Host "❌ 找不到日誌檔案！" -ForegroundColor Red
	Write-Host ""
	Write-Host "請先執行 DeviceBox.exe 進行測試，日誌會自動產生在：" -ForegroundColor Yellow
	Write-Host "  $LogPath\debug_<PID>_<時間>.log" -ForegroundColor Gray
	Write-Host ""
	Write-Host "然後再執行此腳本分析日誌。" -ForegroundColor Yellow
	exit 1
}

Write-Host "找到 $($logFiles.Count) 個日誌檔案：" -ForegroundColor Green
foreach ($file in $logFiles) {
	Write-Host "  - $($file.Name) ($([int]($file.Length/1KB)) KB, $($file.LastWriteTime))" -ForegroundColor Gray
}
Write-Host ""

# 分析每個日誌檔案
$issuesFound = @()

foreach ($logFile in $logFiles) {
	Write-Host "=================================================" -ForegroundColor Cyan
	Write-Host "分析: $($logFile.Name)" -ForegroundColor Cyan
	Write-Host "=================================================" -ForegroundColor Cyan

	$content = Get-Content $logFile.FullName -Raw -Encoding UTF8
	$lines = $content -split "`n"

	# 提取關鍵資訊
	$pid = $null
	$siteId = $null
	$siteName = $null

	# 1. 找到 PID
	if ($logFile.Name -match 'debug_(\d+)_') {
		$pid = $matches[1]
		Write-Host "[PID] $pid" -ForegroundColor White
	}

	# 2. 找到場域
	foreach ($line in $lines) {
		if ($line -match 'SiteManager.*Loaded site: (\w+) - ([^"]+)') {
			$siteId = $matches[1]
			$siteName = $matches[2]
			Write-Host "[場域] $siteId - $siteName" -ForegroundColor White
			break
		}
	}

	# 3. 找到同步服務啟動
	$syncStarted = $lines | Where-Object { $_ -match 'Config sync service started for site: (\w+)' }
	if ($syncStarted) {
		$syncSiteId = $matches[1]
		Write-Host "[同步] 監聽場域: $syncSiteId" -ForegroundColor White

		if ($syncSiteId -ne $siteId) {
			$issue = "❌ 錯誤！監聽場域 ($syncSiteId) 與當前場域 ($siteId) 不一致"
			Write-Host $issue -ForegroundColor Red
			$issuesFound += $issue
		}
	}

	# 4. 檢查是否有 SAVING MODE
	$savingMode = $lines | Where-Object { $_ -match '\*\*\* SAVING MODE \*\*\* Site: (\w+).*Mode: ([^(]+)' }
	if ($savingMode) {
		foreach ($save in $savingMode) {
			if ($save -match 'Site: (\w+).*Mode: ([^(]+)') {
				Write-Host "[儲存] 場域 $($matches[1]) 切換到 $($matches[2].Trim())" -ForegroundColor Yellow
			}
		}
	}

	# 5. 檢查是否有 SYNC EVENT RECEIVED
	$syncEvents = $lines | Where-Object { $_ -match '\*\*\* SYNC EVENT RECEIVED \*\*\*.*Event SiteId: (\w+), Current SiteId: (\w+)' }
	if ($syncEvents) {
		Write-Host "" -ForegroundColor White
		Write-Host "收到同步事件：" -ForegroundColor Cyan
		foreach ($event in $syncEvents) {
			if ($event -match 'Event SiteId: (\w+), Current SiteId: (\w+).*Mode: (\d+)') {
				$eventSite = $matches[1]
				$currentSite = $matches[2]
				$mode = $matches[3]

				if ($eventSite -eq $currentSite) {
					Write-Host "  ✓ 同場域更新 ($eventSite) → 應該套用" -ForegroundColor Green
				} else {
					Write-Host "  ⚠ 跨場域更新 (Event: $eventSite, Current: $currentSite)" -ForegroundColor Yellow
				}
			}
		}
	}

	# 6. 檢查是否有 IGNORING
	$ignoring = $lines | Where-Object { $_ -match '\*\*\* IGNORING \*\*\*' }
	if ($ignoring) {
		Write-Host "" -ForegroundColor White
		Write-Host "忽略的更新：" -ForegroundColor Cyan
		foreach ($ignore in $ignoring) {
			if ($ignore -match 'Current: (\w+).*Event: (\w+)') {
				Write-Host "  ✓ 正確忽略 $($matches[2]) 的更新 (當前: $($matches[1]))" -ForegroundColor Green
			}
		}
	}

	# 7. 檢查是否有 APPLYING SYNC
	$applying = $lines | Where-Object { $_ -match '\*\*\* APPLYING SYNC \*\*\*.*Site: (\w+).*Mode: (\d+)' }
	if ($applying) {
		Write-Host "" -ForegroundColor White
		Write-Host "套用的同步：" -ForegroundColor Cyan
		foreach ($apply in $applying) {
			if ($apply -match 'Site: (\w+).*Mode: (\d+)') {
				$applySite = $matches[1]
				$applyMode = $matches[2]

				if ($applySite -eq $siteId) {
					Write-Host "  ✓ 套用本場域更新 ($applySite) 模式 $applyMode" -ForegroundColor Green
				} else {
					$issue = "  ❌ 錯誤！套用了其他場域 ($applySite) 的更新，當前場域: $siteId"
					Write-Host $issue -ForegroundColor Red
					$issuesFound += $issue
				}
			}
		}
	}

	# 8. 檢查版本變更
	$versionChanges = $lines | Where-Object { $_ -match 'VERSION CHANGE DETECTED.*Site: (\w+)' }
	if ($versionChanges) {
		Write-Host "" -ForegroundColor White
		Write-Host "版本變更：" -ForegroundColor Cyan
		foreach ($change in $versionChanges) {
			if ($change -match 'Site: (\w+)') {
				Write-Host "  場域 $($matches[1]) 有變更" -ForegroundColor Yellow
			}
		}
	}

	Write-Host ""
}

# 總結
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "診斷總結" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

if ($issuesFound.Count -eq 0) {
	Write-Host "✅ 沒有發現明顯的場域同步問題！" -ForegroundColor Green
	Write-Host ""
	Write-Host "如果仍然有跨場域同步的問題，請檢查：" -ForegroundColor Yellow
	Write-Host "1. 資料庫中 site_config 表的內容" -ForegroundColor White
	Write-Host "2. 是否有程式碼在其他地方修改了場域配置" -ForegroundColor White
	Write-Host "3. 兩個軟體是否真的選擇了不同場域" -ForegroundColor White
} else {
	Write-Host "❌ 發現以下問題：" -ForegroundColor Red
	Write-Host ""
	foreach ($issue in $issuesFound) {
		Write-Host "  $issue" -ForegroundColor Red
	}
	Write-Host ""
	Write-Host "建議：" -ForegroundColor Yellow
	Write-Host "1. 檢查 StartConfigSync() 是否傳遞了正確的 siteId" -ForegroundColor White
	Write-Host "2. 檢查 SiteManager.Instance.CurrentSiteId 是否被意外修改" -ForegroundColor White
	Write-Host "3. 檢查資料庫查詢是否正確過濾場域" -ForegroundColor White
}

Write-Host ""
Write-Host "完整日誌檔案位置：" -ForegroundColor Cyan
foreach ($file in $logFiles) {
	Write-Host "  $($file.FullName)" -ForegroundColor Gray
}
