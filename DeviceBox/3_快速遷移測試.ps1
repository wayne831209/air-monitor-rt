# ========================================
# 設備配置遷移測試腳本
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "設備配置 XML → MySQL 遷移工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 檢查 config.xml 是否存在
if (!(Test-Path "config.xml")) {
	Write-Host "❌ 錯誤: 找不到 config.xml 檔案!" -ForegroundColor Red
	Write-Host "請確認您在程式目錄中執行此腳本" -ForegroundColor Yellow
	pause
	exit 1
}

Write-Host "✅ 找到 config.xml 檔案" -ForegroundColor Green

# 檢查資料庫 SQL 檔案
$sqlFile = "Database\1_執行資料庫建表.sql"
if (!(Test-Path $sqlFile)) {
	Write-Host "❌ 錯誤: 找不到 SQL 檔案 $sqlFile" -ForegroundColor Red
	pause
	exit 1
}

Write-Host "✅ 找到資料庫建表 SQL 檔案" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "遷移步驟:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "步驟 1:" -ForegroundColor Yellow
Write-Host "請先在 MySQL 中執行 SQL 腳本建立資料庫表"
Write-Host "檔案位置: $sqlFile"
Write-Host ""
Write-Host "可使用以下方式執行:"
Write-Host "  - MySQL Workbench (推薦)" -ForegroundColor Green
Write-Host "  - 命令列: mysql -h 192.168.102.182 -u Client -p ycm_energy < $sqlFile" -ForegroundColor Gray
Write-Host ""
$continue = Read-Host "是否已完成步驟 1? (Y/N)"

if ($continue -ne "Y" -and $continue -ne "y") {
	Write-Host "請先完成步驟 1,然後重新執行此腳本" -ForegroundColor Yellow
	pause
	exit 0
}

Write-Host ""
Write-Host "步驟 2:" -ForegroundColor Yellow
Write-Host "準備執行資料遷移..." -ForegroundColor Yellow
Write-Host ""

# 檢查執行檔
if (Test-Path "bin\Debug\DeviceBox.exe") {
	$exePath = "bin\Debug\DeviceBox.exe"
} elseif (Test-Path "bin\Release\DeviceBox.exe") {
	$exePath = "bin\Release\DeviceBox.exe"
} elseif (Test-Path "DeviceBox.exe") {
	$exePath = "DeviceBox.exe"
} else {
	Write-Host "❌ 錯誤: 找不到 DeviceBox.exe" -ForegroundColor Red
	Write-Host "請先建置專案" -ForegroundColor Yellow
	pause
	exit 1
}

Write-Host "✅ 找到執行檔: $exePath" -ForegroundColor Green
Write-Host ""

# 備份 config.xml
$backupFile = "config.xml.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item "config.xml" $backupFile
Write-Host "✅ 已備份 config.xml → $backupFile" -ForegroundColor Green
Write-Host ""

# 執行遷移工具
Write-Host "正在啟動遷移工具..." -ForegroundColor Cyan
Write-Host "請在遷移工具視窗中點擊「2. 執行資料遷移」" -ForegroundColor Yellow
Write-Host ""

Start-Process -FilePath $exePath -ArgumentList "--migrate" -Wait

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "遷移完成!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "下一步:" -ForegroundColor Yellow
Write-Host "1. 正常啟動程式測試: $exePath" -ForegroundColor White
Write-Host "2. 檢查資料庫中的資料是否正確" -ForegroundColor White
Write-Host "3. 參考文件: 遷移指南_XML轉MySQL.md" -ForegroundColor White
Write-Host ""

pause
