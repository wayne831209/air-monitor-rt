# 簡易資料庫檢查腳本
# ========================================

Write-Host "========================================"
Write-Host "資料庫狀態檢查" -ForegroundColor Cyan
Write-Host "========================================"
Write-Host ""

# 讀取 config.xml 取得資料庫設定
$configPath = "config.xml"

if (!(Test-Path $configPath)) {
	Write-Host "❌ 錯誤: 找不到 config.xml" -ForegroundColor Red
	exit 1
}

Write-Host "讀取 config.xml..." -ForegroundColor Yellow
[xml]$config = Get-Content $configPath

$dbIP = $config.Setting.Database.IP
$dbName = $config.Setting.Database.DB
$dbUser = $config.Setting.Database.USER
$dbPassword = $config.Setting.Database.Password

Write-Host "資料庫設定:" -ForegroundColor Green
Write-Host "  IP: $dbIP" -ForegroundColor White
Write-Host "  DB: $dbName" -ForegroundColor White
Write-Host "  User: $dbUser" -ForegroundColor White
Write-Host ""

# 測試 MySQL 連線
Write-Host "測試 MySQL 連線..." -ForegroundColor Yellow
try {
	$result = & mysql -h $dbIP -u $dbUser -p$dbPassword -e "SELECT 1" 2>&1

	if ($LASTEXITCODE -eq 0) {
		Write-Host "✅ MySQL 連線成功!" -ForegroundColor Green
	} else {
		Write-Host "❌ MySQL 連線失敗" -ForegroundColor Red
		Write-Host "錯誤: $result" -ForegroundColor Red
		Write-Host ""
		Write-Host "可能原因:" -ForegroundColor Yellow
		Write-Host "  1. MySQL 未安裝或未啟動" -ForegroundColor Gray
		Write-Host "  2. 帳號密碼錯誤" -ForegroundColor Gray
		Write-Host "  3. IP 位址錯誤" -ForegroundColor Gray
		exit 1
	}
} catch {
	Write-Host "❌ 無法執行 mysql 命令" -ForegroundColor Red
	Write-Host "請確認已安裝 MySQL 客戶端工具" -ForegroundColor Yellow
	Write-Host ""
}

Write-Host ""
Write-Host "檢查資料庫表..." -ForegroundColor Yellow

# 檢查表是否存在
$tables = & mysql -h $dbIP -u $dbUser -p$dbPassword $dbName -e "SHOW TABLES LIKE 'factories'" 2>&1

if ($tables -match "factories") {
	Write-Host "✅ 找到 factories 表" -ForegroundColor Green

	# 查詢工廠數量
	$factoryCount = & mysql -h $dbIP -u $dbUser -p$dbPassword $dbName -e "SELECT COUNT(*) FROM factories" -N 2>&1
	Write-Host "  工廠數量: $factoryCount" -ForegroundColor White

	if ($factoryCount -eq "0") {
		Write-Host ""
		Write-Host "⚠️  警告: 資料庫中沒有工廠資料!" -ForegroundColor Yellow
		Write-Host "請執行資料遷移工具:" -ForegroundColor Yellow
		Write-Host "  .\bin\Debug\DeviceBox.exe --migrate" -ForegroundColor Gray
	} else {
		# 查詢設備數量
		$deviceCount = & mysql -h $dbIP -u $dbUser -p$dbPassword $dbName -e "SELECT COUNT(*) FROM device_config" -N 2>&1
		Write-Host "  設備數量: $deviceCount" -ForegroundColor White

		Write-Host ""
		Write-Host "✅ 資料庫狀態正常!" -ForegroundColor Green
		Write-Host ""
		Write-Host "如果程式無法載入資料,請檢查:" -ForegroundColor Yellow
		Write-Host "  1. Config.cs 的 LoadFactoriesFromDatabase 方法" -ForegroundColor Gray
		Write-Host "  2. Visual Studio 輸出視窗的錯誤訊息" -ForegroundColor Gray
	}
} else {
	Write-Host "❌ 找不到 factories 表" -ForegroundColor Red
	Write-Host ""
	Write-Host "請先建立資料庫表:" -ForegroundColor Yellow
	Write-Host "  在 MySQL 中執行: Database\1_執行資料庫建表.sql" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================"
Read-Host "按 Enter 結束"
