# ========================================
# 自動建立資料庫表並匯入通知設定
# ========================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "自動建立資料庫表" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 讀取 config.xml 取得資料庫設定
Write-Host "1. 讀取資料庫設定..." -ForegroundColor Yellow

if (!(Test-Path "DeviceBox\config.xml")) {
	Write-Host "   ❌ 找不到 config.xml" -ForegroundColor Red
	Read-Host "按 Enter 結束"
	exit 1
}

[xml]$config = Get-Content "DeviceBox\config.xml"
$dbIP = $config.Setting.Database.IP
$dbName = $config.Setting.Database.DB
$dbUser = $config.Setting.Database.USER
$dbPass = $config.Setting.Database.Password

Write-Host "   資料庫: $dbIP / $dbName" -ForegroundColor Green
Write-Host ""

# 2. 檢查 MySQL 命令列工具
Write-Host "2. 檢查 MySQL 工具..." -ForegroundColor Yellow

$mysqlPath = Get-Command mysql -ErrorAction SilentlyContinue

if ($null -eq $mysqlPath) {
	Write-Host "   ❌ 找不到 mysql 命令列工具" -ForegroundColor Red
	Write-Host ""
	Write-Host "請選擇執行方式:" -ForegroundColor Cyan
	Write-Host "   1. 手動使用 MySQL Workbench 執行 SQL 腳本" -ForegroundColor White
	Write-Host "      檔案位置: DeviceBox\Database\完整建表並匯入範例資料.sql" -ForegroundColor Gray
	Write-Host ""
	Write-Host "   2. 使用應用程式的遷移工具" -ForegroundColor White
	Write-Host "      執行: DeviceBox\bin\Debug\DeviceBox.exe --migrate" -ForegroundColor Gray
	Write-Host ""
	Read-Host "按 Enter 結束"
	exit 0
}

Write-Host "   ✅ MySQL 命令列工具可用: $($mysqlPath.Source)" -ForegroundColor Green
Write-Host ""

# 3. 執行建表 SQL
Write-Host "3. 執行建表 SQL..." -ForegroundColor Yellow

$sqlFile = "DeviceBox\Database\完整建表並匯入範例資料.sql"

if (!(Test-Path $sqlFile)) {
	Write-Host "   ❌ 找不到 SQL 檔案: $sqlFile" -ForegroundColor Red
	Read-Host "按 Enter 結束"
	exit 1
}

Write-Host "   執行 SQL 腳本..." -ForegroundColor Gray

try {
	# 執行 SQL
	$result = Get-Content $sqlFile | & mysql -h $dbIP -u $dbUser -p$dbPass $dbName 2>&1

	if ($LASTEXITCODE -eq 0) {
		Write-Host "   ✅ SQL 執行成功!" -ForegroundColor Green
		Write-Host ""

		# 顯示結果
		if ($result) {
			Write-Host "執行結果:" -ForegroundColor Cyan
			$result | ForEach-Object { Write-Host "   $_" -ForegroundColor White }
		}
	} else {
		Write-Host "   ❌ SQL 執行失敗" -ForegroundColor Red
		Write-Host "   錯誤: $result" -ForegroundColor Red
		Read-Host "按 Enter 結束"
		exit 1
	}
} catch {
	Write-Host "   ❌ 執行過程發生錯誤" -ForegroundColor Red
	Write-Host "   錯誤: $($_.Exception.Message)" -ForegroundColor Red
	Read-Host "按 Enter 結束"
	exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "驗證建立結果" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 4. 驗證表是否建立成功
Write-Host "4. 驗證表..." -ForegroundColor Yellow

$tables = @("factories", "device_config", "alarm_limits", "notification_settings")

foreach ($table in $tables) {
	$checkResult = & mysql -h $dbIP -u $dbUser -p$dbPass $dbName -e "SHOW TABLES LIKE '$table'" -N 2>&1

	if ($checkResult -like "*$table*") {
		Write-Host "   ✅ $table" -ForegroundColor Green
	} else {
		Write-Host "   ❌ $table (未建立)" -ForegroundColor Red
	}
}

Write-Host ""

# 5. 顯示通知設定
Write-Host "5. 通知設定..." -ForegroundColor Yellow

$notifSettings = & mysql -h $dbIP -u $dbUser -p$dbPass $dbName -e "SELECT setting_key, LEFT(setting_value, 50) as value FROM notification_settings" -t 2>&1

if ($LASTEXITCODE -eq 0) {
	Write-Host "$notifSettings" -ForegroundColor White
} else {
	Write-Host "   ⚠️  無法讀取通知設定" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "下一步驟" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "資料庫表已建立完成!" -ForegroundColor Green
Write-Host "通知設定已匯入!" -ForegroundColor Green
Write-Host ""
Write-Host "接下來請執行遷移工具來匯入工廠和設備資料:" -ForegroundColor Cyan
Write-Host "   .\DeviceBox\bin\Debug\DeviceBox.exe --migrate" -ForegroundColor White
Write-Host ""
Write-Host "或執行:" -ForegroundColor Cyan
Write-Host "   .\2_執行資料遷移.bat" -ForegroundColor White
Write-Host ""

Read-Host "按 Enter 結束"
