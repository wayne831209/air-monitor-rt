# 測試自動載入場域配置
# 用於驗證啟動和切換時自動載入資料庫配置的功能

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "自動載入場域配置測試工具" -ForegroundColor Cyan
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

Write-Host "功能說明：" -ForegroundColor Yellow
Write-Host "  1. 啟動時自動載入場域配置" -ForegroundColor White
Write-Host "  2. 切換場域時自動載入配置" -ForegroundColor White
Write-Host "  3. 確保多人協作時看到最新設定" -ForegroundColor White
Write-Host ""

# 提供測試選項
Write-Host "請選擇測試情境：" -ForegroundColor Green
Write-Host "  1. 測試啟動時自動載入" -ForegroundColor White
Write-Host "  2. 測試切換場域時自動載入" -ForegroundColor White
Write-Host "  3. 測試多人協作情境" -ForegroundColor White
Write-Host "  4. 查看資料庫當前配置" -ForegroundColor White
Write-Host ""

$choice = Read-Host "請輸入選項 (1-4)"

switch ($choice) {
	"1" {
		Write-Host ""
		Write-Host "========================================" -ForegroundColor Cyan
		Write-Host "測試 1：啟動時自動載入" -ForegroundColor Cyan
		Write-Host "========================================" -ForegroundColor Cyan
		Write-Host ""

		Write-Host "測試步驟：" -ForegroundColor Yellow
		Write-Host "1. 請先用 MySQL 設定 site_config 的 current_mode_id" -ForegroundColor White
		Write-Host "   例如：UPDATE site_config SET current_mode_id = 15 WHERE site_id = 'other';" -ForegroundColor DarkGray
		Write-Host "2. 啟動軟體" -ForegroundColor White
		Write-Host "3. 觀察 label3 是否顯示對應的模式名稱" -ForegroundColor White
		Write-Host ""

		Write-Host "按任意鍵啟動軟體..." -ForegroundColor Yellow
		pause

		Write-Host "啟動軟體..." -ForegroundColor Cyan
		Start-Process $exePath
		Start-Sleep -Seconds 3

		Write-Host ""
		Write-Host "請觀察軟體介面，按任意鍵查看日誌..." -ForegroundColor Yellow
		pause
	}

	"2" {
		Write-Host ""
		Write-Host "========================================" -ForegroundColor Cyan
		Write-Host "測試 2：切換場域時自動載入" -ForegroundColor Cyan
		Write-Host "========================================" -ForegroundColor Cyan
		Write-Host ""

		Write-Host "測試步驟：" -ForegroundColor Yellow
		Write-Host "1. 請先用 MySQL 設定兩個場域的 current_mode_id：" -ForegroundColor White
		Write-Host "   UPDATE site_config SET current_mode_id = 15 WHERE site_id = 'other';" -ForegroundColor DarkGray
		Write-Host "   UPDATE site_config SET current_mode_id = 20 WHERE site_id = 'foundry';" -ForegroundColor DarkGray
		Write-Host "2. 啟動軟體（應顯示其他廠域的模式）" -ForegroundColor White
		Write-Host "3. 點擊左上角切換到鑄造廠（應自動載入鑄造廠的模式）" -ForegroundColor White
		Write-Host "4. 再次點擊切換回其他廠域（應載入其他廠域的模式）" -ForegroundColor White
		Write-Host ""

		Write-Host "按任意鍵啟動軟體..." -ForegroundColor Yellow
		pause

		Write-Host "啟動軟體..." -ForegroundColor Cyan
		Start-Process $exePath
		Start-Sleep -Seconds 3

		Write-Host ""
		Write-Host "請執行測試步驟，完成後按任意鍵查看日誌..." -ForegroundColor Yellow
		pause
	}

	"3" {
		Write-Host ""
		Write-Host "========================================" -ForegroundColor Cyan
		Write-Host "測試 3：多人協作情境" -ForegroundColor Cyan
		Write-Host "========================================" -ForegroundColor Cyan
		Write-Host ""

		Write-Host "模擬情境：" -ForegroundColor Yellow
		Write-Host "  A 人員上午開啟軟體，設定為「模式一」後關閉" -ForegroundColor White
		Write-Host "  B 人員下午開啟軟體，應自動看到「模式一」" -ForegroundColor White
		Write-Host ""

		Write-Host "測試步驟：" -ForegroundColor Yellow
		Write-Host "1. 模擬 A 人員：啟動第一個實例" -ForegroundColor White
		Write-Host "2. 在實例 A 中切換模式" -ForegroundColor White
		Write-Host "3. 關閉實例 A" -ForegroundColor White
		Write-Host "4. 模擬 B 人員：啟動第二個實例" -ForegroundColor White
		Write-Host "5. 觀察實例 B 是否自動顯示 A 設定的模式" -ForegroundColor White
		Write-Host ""

		Write-Host "按任意鍵啟動實例 A (模擬 A 人員)..." -ForegroundColor Yellow
		pause

		Write-Host "啟動實例 A..." -ForegroundColor Cyan
		$processA = Start-Process $exePath -PassThru
		Start-Sleep -Seconds 3

		Write-Host ""
		Write-Host "請在實例 A 中切換模式，然後關閉軟體" -ForegroundColor Yellow
		Write-Host "關閉後按任意鍵繼續..." -ForegroundColor Yellow
		pause

		Write-Host ""
		Write-Host "按任意鍵啟動實例 B (模擬 B 人員)..." -ForegroundColor Yellow
		pause

		Write-Host "啟動實例 B..." -ForegroundColor Cyan
		Start-Process $exePath
		Start-Sleep -Seconds 3

		Write-Host ""
		Write-Host "請觀察實例 B 是否自動顯示 A 設定的模式" -ForegroundColor Yellow
		Write-Host "完成後按任意鍵查看日誌..." -ForegroundColor Yellow
		pause
	}

	"4" {
		Write-Host ""
		Write-Host "請在 MySQL 中執行以下查詢：" -ForegroundColor Yellow
		Write-Host ""
		Write-Host "SELECT " -ForegroundColor Cyan
		Write-Host "    site_id as '場域ID'," -ForegroundColor Cyan
		Write-Host "    site_name as '場域名稱'," -ForegroundColor Cyan
		Write-Host "    current_mode_id as '當前模式ID'," -ForegroundColor Cyan
		Write-Host "    config_version as '配置版本'," -ForegroundColor Cyan
		Write-Host "    last_updated_by as '最後更新者'," -ForegroundColor Cyan
		Write-Host "    updated_at as '更新時間'" -ForegroundColor Cyan
		Write-Host "FROM site_config" -ForegroundColor Cyan
		Write-Host "ORDER BY site_id;" -ForegroundColor Cyan
		Write-Host ""
		pause
		exit
	}

	default {
		Write-Host "無效的選項" -ForegroundColor Red
		pause
		exit
	}
}

# 分析日誌
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "分析日誌" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 取得最新的日誌檔
$debugLogs = Get-ChildItem "$logPath\debug_*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 2

if ($debugLogs.Count -eq 0) {
	Write-Host "警告: 找不到日誌檔案" -ForegroundColor Yellow
	pause
	exit
}

Write-Host "找到日誌檔案：" -ForegroundColor Green
$debugLogs | ForEach-Object {
	$pid = $_.Name -replace 'debug_(\d+)_.*', '$1'
	Write-Host "  - $($_.Name) (PID: $pid)" -ForegroundColor White
}
Write-Host ""

# 分析每個日誌檔
foreach ($log in $debugLogs) {
	$pid = $log.Name -replace 'debug_(\d+)_.*', '$1'
	Write-Host "========================================" -ForegroundColor Cyan
	Write-Host "PID: $pid" -ForegroundColor Cyan
	Write-Host "========================================" -ForegroundColor Cyan

	$content = Get-Content $log.FullName -Encoding UTF8

	# 啟動時載入配置
	Write-Host ""
	Write-Host "【啟動時載入】" -ForegroundColor Yellow
	$initLoadLines = $content | Where-Object { 
		$_ -match "Loading site config from database" -or
		$_ -match "Loaded site config - Site:" -or
		$_ -match "Applying mode:" -or
		$_ -match "Site config applied successfully" -or
		$_ -match "No site config found" -or
		$_ -match "No mode set for site"
	} | Select-Object -First 10

	if ($initLoadLines) {
		$initLoadLines | ForEach-Object {
			if ($_ -match "Loading site config") {
				Write-Host "  📂 " -NoNewline -ForegroundColor Cyan
				Write-Host ($_ -replace '.*\[MainForm\] ', '') -ForegroundColor White
			}
			elseif ($_ -match "Loaded site config") {
				Write-Host "  ✓ " -NoNewline -ForegroundColor Green
				Write-Host ($_ -replace '.*\[MainForm\] ', '') -ForegroundColor White
			}
			elseif ($_ -match "Applying mode") {
				Write-Host "  ⚙ " -NoNewline -ForegroundColor Yellow
				Write-Host ($_ -replace '.*\[MainForm\] ', '') -ForegroundColor White
			}
			elseif ($_ -match "applied successfully") {
				Write-Host "  ✓ " -NoNewline -ForegroundColor Green
				Write-Host ($_ -replace '.*\[MainForm\] ', '') -ForegroundColor Green
			}
			elseif ($_ -match "No site config found|No mode set") {
				Write-Host "  ⚠ " -NoNewline -ForegroundColor Yellow
				Write-Host ($_ -replace '.*\[MainForm\] ', '') -ForegroundColor Yellow
			}
		}
	} else {
		Write-Host "  (未找到載入記錄)" -ForegroundColor DarkGray
	}

	# 切換場域時載入
	Write-Host ""
	Write-Host "【切換場域時載入】" -ForegroundColor Yellow
	$switchLines = $content | Where-Object { 
		$_ -match "Switching sync site to:" -or
		($_ -match "Loading site config from database" -and $_ -notmatch "View-based sync initialized")
	}

	if ($switchLines -and $switchLines.Count -gt 1) {
		# 跳過第一次(啟動時)
		$switchLines | Select-Object -Skip 1 | ForEach-Object {
			if ($_ -match "Switching sync site") {
				Write-Host "  🔄 " -NoNewline -ForegroundColor Cyan
				Write-Host ($_ -replace '.*\[MainForm\] ', '') -ForegroundColor Cyan
			}
			elseif ($_ -match "Loading site config") {
				Write-Host "  📂 " -NoNewline -ForegroundColor White
				Write-Host ($_ -replace '.*\[MainForm\] ', '') -ForegroundColor White
			}
		}

		# 顯示套用的模式
		$applyAfterSwitch = $content | Where-Object { $_ -match "Applying mode:" } | Select-Object -Skip 1
		$applyAfterSwitch | ForEach-Object {
			Write-Host "  ⚙ " -NoNewline -ForegroundColor Yellow
			Write-Host ($_ -replace '.*\[MainForm\] ', '') -ForegroundColor Yellow
		}
	} else {
		Write-Host "  (未偵測到場域切換)" -ForegroundColor DarkGray
	}

	# 資料庫查詢記錄
	Write-Host ""
	Write-Host "【資料庫查詢】" -ForegroundColor Yellow
	$dbLoadLines = $content | Where-Object { $_ -match "\[DeviceDatabase\] LoadSiteConfig called" }
	if ($dbLoadLines) {
		$dbLoadLines | ForEach-Object {
			Write-Host "  🗄 " -NoNewline -ForegroundColor Cyan
			Write-Host ($_ -replace '.*\[DeviceDatabase\] ', '') -ForegroundColor Gray
		}
	} else {
		Write-Host "  (未找到資料庫查詢記錄)" -ForegroundColor DarkGray
	}

	Write-Host ""
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "分析完成" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

Write-Host "驗證重點：" -ForegroundColor Yellow
Write-Host "  ✓ 啟動時應看到 'Loading site config from database'" -ForegroundColor White
Write-Host "  ✓ 應看到 'Loaded site config - Site: xxx, Mode ID: xxx'" -ForegroundColor White
Write-Host "  ✓ 應看到 'Applying mode: xxx (ID: xxx)'" -ForegroundColor White
Write-Host "  ✓ 應看到 'Site config applied successfully'" -ForegroundColor White
Write-Host "  ✓ 切換場域時應重複上述流程" -ForegroundColor White
Write-Host ""

Write-Host "按任意鍵退出..." -ForegroundColor Yellow
pause
