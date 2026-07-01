# ========================================
# 直接測試 - 不需要診斷工具
# ========================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "直接檢查資料庫和程式載入狀態" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 檢查 config.xml
Write-Host "1. 檢查 config.xml..." -ForegroundColor Yellow

if (!(Test-Path "config.xml")) {
	Write-Host "   ❌ 找不到 config.xml" -ForegroundColor Red
	exit 1
}

Write-Host "   ✅ config.xml 存在" -ForegroundColor Green

# 讀取資料庫設定
[xml]$config = Get-Content "config.xml"
$dbIP = $config.Setting.Database.IP
$dbName = $config.Setting.Database.DB
$dbUser = $config.Setting.Database.USER
$dbPass = $config.Setting.Database.Password

Write-Host "   資料庫: $dbIP / $dbName" -ForegroundColor Gray
Write-Host ""

# 2. 檢查是否有 MySQL 命令列工具
Write-Host "2. 檢查 MySQL 工具..." -ForegroundColor Yellow

$mysqlPath = Get-Command mysql -ErrorAction SilentlyContinue

if ($null -eq $mysqlPath) {
	Write-Host "   ⚠️  找不到 mysql 命令" -ForegroundColor Yellow
	Write-Host "   將嘗試使用程式直接測試" -ForegroundColor Gray
} else {
	Write-Host "   ✅ MySQL 命令列工具可用" -ForegroundColor Green

	# 測試連線
	Write-Host ""
	Write-Host "3. 測試資料庫連線..." -ForegroundColor Yellow

	$testQuery = "SELECT COUNT(*) FROM factories"
	$result = & mysql -h $dbIP -u $dbUser -p$dbPass $dbName -e $testQuery -N 2>&1

	if ($LASTEXITCODE -eq 0) {
		Write-Host "   ✅ 資料庫連線成功" -ForegroundColor Green
		Write-Host "   工廠數量: $result" -ForegroundColor White

		if ($result -eq 0) {
			Write-Host ""
			Write-Host "   ⚠️  資料庫是空的!" -ForegroundColor Yellow
			Write-Host "   需要執行資料遷移" -ForegroundColor Yellow
			Write-Host ""
			Write-Host "解決方法:" -ForegroundColor Cyan
			Write-Host "   1. 確認已在 MySQL 中建立表 (執行 1_執行資料庫建表.sql)" -ForegroundColor White
			Write-Host "   2. 執行遷移: .\bin\Debug\DeviceBox.exe --migrate" -ForegroundColor White
		} else {
			Write-Host ""
			Write-Host "   ✅ 資料庫中有資料" -ForegroundColor Green

			# 查詢設備數量
			$deviceCount = & mysql -h $dbIP -u $dbUser -p$dbPass $dbName -e "SELECT COUNT(*) FROM device_config" -N 2>&1
			Write-Host "   設備數量: $deviceCount" -ForegroundColor White
		}
	} else {
		Write-Host "   ❌ 資料庫連線失敗" -ForegroundColor Red
		Write-Host "   錯誤: $result" -ForegroundColor Red
	}
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "測試程式是否能載入資料" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 使用反射載入程式並測試
Write-Host "載入 DeviceBox.dll..." -ForegroundColor Yellow

$dllPath = ".\bin\Debug\DeviceBox.exe"

if (!(Test-Path $dllPath)) {
	Write-Host "❌ 找不到: $dllPath" -ForegroundColor Red
	exit 1
}

try {
	Add-Type -Path $dllPath

	# 建立 Config 物件
	$configObj = New-Object DeviceBox.Config

	Write-Host "呼叫 Config.LoadConfig()..." -ForegroundColor Yellow
	$loadResult = $configObj.LoadConfig()

	if ($loadResult) {
		Write-Host "✅ Config.LoadConfig() 成功" -ForegroundColor Green
		Write-Host "   Factories 數量: $($configObj.Factories.Count)" -ForegroundColor White

		if ($configObj.Factories.Count -eq 0) {
			Write-Host ""
			Write-Host "❌ 問題找到了!" -ForegroundColor Red
			Write-Host "   Config.LoadConfig() 成功,但 Factories 是空的" -ForegroundColor Yellow
			Write-Host ""
			Write-Host "這表示 LoadFactoriesFromDatabase() 沒有正確載入資料" -ForegroundColor Yellow
			Write-Host ""
			Write-Host "可能原因:" -ForegroundColor Cyan
			Write-Host "   1. DeviceDatabase 連線失敗 (沒有錯誤訊息)" -ForegroundColor White
			Write-Host "   2. LoadFactories() 返回空列表" -ForegroundColor White
			Write-Host "   3. 資料庫查詢有問題" -ForegroundColor White
			Write-Host ""
			Write-Host "建議:" -ForegroundColor Cyan
			Write-Host "   在 Config.cs 的 LoadFactoriesFromDatabase() 方法中加入更多 Debug 輸出" -ForegroundColor White
			Write-Host "   或查看 Visual Studio 的輸出視窗" -ForegroundColor White
		} else {
			Write-Host "✅ 成功載入工廠!" -ForegroundColor Green
			foreach ($factory in $configObj.Factories) {
				Write-Host "   - $($factory.Name): $($factory.Devices.Count) 個設備" -ForegroundColor White
			}
			Write-Host ""
			Write-Host "✅ 資料載入正常,問題可能在 MainForm 的顯示部分" -ForegroundColor Green
		}
	} else {
		Write-Host "❌ Config.LoadConfig() 返回 false" -ForegroundColor Red
	}

} catch {
	Write-Host "❌ 錯誤: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Read-Host "按 Enter 結束"
