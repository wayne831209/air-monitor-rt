# 視圖同步快速測試腳本
# 用於驗證新的視圖基礎同步機制

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "視圖同步測試工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$exePath = ".\bin\Debug\DeviceBox.exe"
$logPath = ".\bin\Debug"

# 檢查程式是否存在
if (-not (Test-Path $exePath)) {
	Write-Host "錯誤: 找不到 DeviceBox.exe" -ForegroundColor Red
	Write-Host "請確認路徑: $exePath" -ForegroundColor Yellow
	pause
	exit
}

Write-Host "新的同步機制說明：" -ForegroundColor Yellow
Write-Host "  - 不再需要啟動時選擇場域" -ForegroundColor White
Write-Host "  - 同步完全基於當前顯示的視圖" -ForegroundColor White
Write-Host "  - 其它廠域視圖 → 同步 'other' 場域" -ForegroundColor White
Write-Host "  - 鑄造廠域視圖 → 同步 'foundry' 場域" -ForegroundColor White
Write-Host ""

Write-Host "測試步驟：" -ForegroundColor Green
Write-Host "1. 啟動兩個 DeviceBox 實例" -ForegroundColor White
Write-Host "2. 兩個實例預設都會顯示「其它廠域」" -ForegroundColor White
Write-Host "3. 在實例 A 切換模式，觀察實例 B 是否同步（應該會）" -ForegroundColor White
Write-Host "4. 在實例 B 點擊左上角切換到「鑄造廠域」" -ForegroundColor White
Write-Host "5. 在實例 A 切換模式，觀察實例 B 是否同步（應該不會）" -ForegroundColor White
Write-Host "6. 在實例 B 切換回「其它廠域」" -ForegroundColor White
Write-Host "7. 觀察實例 B 是否同步到實例 A 的最新模式（應該會）" -ForegroundColor White
Write-Host ""

Write-Host "按任意鍵啟動第一個實例..." -ForegroundColor Yellow
pause

Write-Host "啟動實例 1..." -ForegroundColor Cyan
Start-Process $exePath
Start-Sleep -Seconds 2

Write-Host "按任意鍵啟動第二個實例..." -ForegroundColor Yellow
pause

Write-Host "啟動實例 2..." -ForegroundColor Cyan
Start-Process $exePath
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "兩個實例已啟動" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

Write-Host "請執行上述測試步驟" -ForegroundColor Yellow
Write-Host ""
Write-Host "完成測試後，按任意鍵分析日誌..." -ForegroundColor Yellow
pause

Write-Host ""
Write-Host "正在分析日誌..." -ForegroundColor Cyan
Write-Host ""

# 取得最新的兩個 debug 日誌檔
$debugLogs = Get-ChildItem "$logPath\debug_*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 2

if ($debugLogs.Count -lt 2) {
	Write-Host "警告: 找不到足夠的日誌檔案" -ForegroundColor Yellow
	Write-Host "請確認程式是否正常啟動" -ForegroundColor Yellow
	pause
	exit
}

Write-Host "找到日誌檔案：" -ForegroundColor Green
$debugLogs | ForEach-Object {
	Write-Host "  - $($_.Name) (PID: $($_.Name -replace 'debug_(\d+)_.*', '$1'))" -ForegroundColor White
}
Write-Host ""

# 分析每個日誌檔
foreach ($log in $debugLogs) {
	$pid = $log.Name -replace 'debug_(\d+)_.*', '$1'
	Write-Host "========================================" -ForegroundColor Cyan
	Write-Host "分析 PID: $pid" -ForegroundColor Cyan
	Write-Host "========================================" -ForegroundColor Cyan

	$content = Get-Content $log.FullName -Encoding UTF8

	# 初始化場域
	$initLine = $content | Where-Object { $_ -match "View-based sync initialized for:" } | Select-Object -First 1
	if ($initLine) {
		Write-Host "初始化: " -NoNewline -ForegroundColor Yellow
		Write-Host ($initLine -replace '.*View-based sync initialized for: (.*)$', '$1') -ForegroundColor White
	}

	# 視圖切換
	$switchLines = $content | Where-Object { $_ -match "Switching sync site to:" }
	if ($switchLines) {
		Write-Host "視圖切換:" -ForegroundColor Yellow
		$switchLines | ForEach-Object {
			Write-Host "  → " -NoNewline -ForegroundColor White
			Write-Host ($_ -replace '.*Switching sync site to: (.*)$', '$1') -ForegroundColor Cyan
		}
	}

	# 模式儲存
	$saveLines = $content | Where-Object { $_ -match "\*\*\* SAVING MODE \*\*\*" }
	if ($saveLines) {
		Write-Host "模式變更:" -ForegroundColor Yellow
		$saveLines | ForEach-Object {
			Write-Host "  → " -NoNewline -ForegroundColor White
			Write-Host ($_ -replace '.*Site: ([^,]+).*Mode: (.*)$', '場域 $1: $2') -ForegroundColor Green
		}
	}

	# 同步事件
	$syncReceived = $content | Where-Object { $_ -match "\*\*\* SYNC EVENT RECEIVED \*\*\*" }
	if ($syncReceived) {
		Write-Host "接收同步事件:" -ForegroundColor Yellow
		$syncReceived | ForEach-Object {
			if ($_ -match "Event SiteId: ([^,]+).*Current View Site: ([^,]+)") {
				$eventSite = $matches[1]
				$currentSite = $matches[2]
				if ($eventSite -eq $currentSite) {
					Write-Host "  ✓ 接受: 事件場域 $eventSite = 當前視圖 $currentSite" -ForegroundColor Green
				} else {
					Write-Host "  ✗ 忽略: 事件場域 $eventSite ≠ 當前視圖 $currentSite" -ForegroundColor DarkGray
				}
			}
		}
	}

	# 同步套用
	$syncApplied = $content | Where-Object { $_ -match "\*\*\* APPLYING SYNC \*\*\*" }
	if ($syncApplied) {
		Write-Host "套用同步:" -ForegroundColor Yellow
		$syncApplied | ForEach-Object {
			Write-Host "  → " -NoNewline -ForegroundColor White
			Write-Host ($_ -replace '.*Site: ([^,]+).*Version: (\d+).*Mode: (\d+|None).*', '場域 $1, 版本 $2, 模式 $3') -ForegroundColor Green
		}
	}

	# 忽略訊息
	$ignored = $content | Where-Object { $_ -match "\*\*\* IGNORING \*\*\*" }
	if ($ignored) {
		Write-Host "忽略的更新:" -ForegroundColor DarkGray
		$ignored | ForEach-Object {
			Write-Host "  ✗ " -NoNewline -ForegroundColor DarkGray
			Write-Host ($_ -replace '.*Current view: ([^,]+).*Event: (.*)$', '當前視圖 $1, 事件來自 $2') -ForegroundColor DarkGray
		}
	}

	Write-Host ""
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "分析完成" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

Write-Host "驗證重點：" -ForegroundColor Yellow
Write-Host "  ✓ 初始化時應顯示 'other (OtherFactories)'" -ForegroundColor White
Write-Host "  ✓ 切換視圖時應看到 'Switching sync site to' 訊息" -ForegroundColor White
Write-Host "  ✓ 同視圖的實例應接受並套用同步（綠色勾號）" -ForegroundColor White
Write-Host "  ✓ 不同視圖的實例應忽略同步（灰色叉號）" -ForegroundColor White
Write-Host ""

Write-Host "按任意鍵退出..." -ForegroundColor Yellow
pause
