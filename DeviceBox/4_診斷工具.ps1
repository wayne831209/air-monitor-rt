# ========================================
# 資料載入診斷工具 (PowerShell版)
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "資料載入診斷工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "此工具會幫助您診斷為什麼程式看不到設備資料" -ForegroundColor Yellow
Write-Host ""
Write-Host "診斷工具將會:" -ForegroundColor White
Write-Host "  1. 測試資料庫連線" -ForegroundColor Gray
Write-Host "  2. 檢查資料庫中是否有資料" -ForegroundColor Gray
Write-Host "  3. 測試 Config 類別是否能正確載入" -ForegroundColor Gray
Write-Host "  4. 顯示詳細的診斷資訊" -ForegroundColor Gray
Write-Host ""

$continue = Read-Host "按 Enter 繼續..."

Write-Host ""
Write-Host "正在尋找 DeviceBox.exe..." -ForegroundColor Yellow

# 尋找執行檔
$exePath = $null
$searchPaths = @(
	"DeviceBox.exe",
	"bin\Debug\DeviceBox.exe",
	"bin\Release\DeviceBox.exe",
	"..\bin\Debug\DeviceBox.exe",
	"..\bin\Release\DeviceBox.exe"
)

foreach ($path in $searchPaths) {
	if (Test-Path $path) {
		$exePath = $path
		Write-Host "✅ 找到執行檔: $exePath" -ForegroundColor Green
		break
	}
}

if ($null -eq $exePath) {
	Write-Host "❌ 錯誤: 找不到 DeviceBox.exe" -ForegroundColor Red
	Write-Host ""
	Write-Host "請先在 Visual Studio 中編譯專案:" -ForegroundColor Yellow
	Write-Host "  1. 開啟專案" -ForegroundColor White
	Write-Host "  2. 建置 → 建置方案 (F6)" -ForegroundColor White
	Write-Host "  3. 等待編譯完成" -ForegroundColor White
	Write-Host ""
	Read-Host "按 Enter 結束..."
	exit 1
}

Write-Host ""
Write-Host "正在啟動診斷工具..." -ForegroundColor Cyan
Write-Host ""

# 啟動診斷工具
try {
	& $exePath --diagnostic
	Write-Host ""
	Write-Host "診斷工具已關閉" -ForegroundColor Green
}
catch {
	Write-Host "❌ 錯誤: 無法啟動診斷工具" -ForegroundColor Red
	Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Read-Host "按 Enter 結束..."
