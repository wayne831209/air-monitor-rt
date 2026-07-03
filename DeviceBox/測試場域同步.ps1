# ========================================
# 場域同步測試腳本
# ========================================
# 此腳本會：
# 1. 清理舊的場域配置檔案
# 2. 引導你正確測試場域隔離功能

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "        場域同步功能測試腳本" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

$binPath = "DeviceBox\bin\Debug"

# 1. 清理舊配置
Write-Host "[步驟 1/4] 清理舊的場域配置檔案..." -ForegroundColor Yellow
$oldConfigs = Get-ChildItem -Path $binPath -Filter "site_*.config" -ErrorAction SilentlyContinue
if ($oldConfigs) {
	foreach ($file in $oldConfigs) {
		Remove-Item $file.FullName -Force
		Write-Host "  已刪除: $($file.Name)" -ForegroundColor Gray
	}
	Write-Host "  ✓ 清理完成" -ForegroundColor Green
} else {
	Write-Host "  ✓ 沒有需要清理的檔案" -ForegroundColor Green
}

Write-Host ""

# 2. 啟動測試
Write-Host "[步驟 2/4] 準備啟動測試..." -ForegroundColor Yellow
Write-Host ""
Write-Host "請按照以下步驟操作：" -ForegroundColor Cyan
Write-Host ""

Write-Host "【A 軟體 - 其他場域】" -ForegroundColor Green
Write-Host "  1. 雙擊執行 DeviceBox.exe" -ForegroundColor White
Write-Host "  2. 在場域選擇對話框中，選擇：其他場域" -ForegroundColor Yellow
Write-Host "  3. 確認後，切換到：模式一" -ForegroundColor White
Write-Host ""

Write-Host "【B 軟體 - 鑄造廠】" -ForegroundColor Green  
Write-Host "  1. 再次雙擊執行 DeviceBox.exe（不要關閉 A）" -ForegroundColor White
Write-Host "  2. ⚠ 在場域選擇對話框中，選擇：鑄造廠 ⚠" -ForegroundColor Red
Write-Host "  3. 確認後，切換到：手動模式" -ForegroundColor White
Write-Host ""

Write-Host "【預期結果】" -ForegroundColor Cyan
Write-Host "  ✓ A 軟體保持在「模式一」" -ForegroundColor White
Write-Host "  ✓ B 軟體切換到「手動模式」" -ForegroundColor White
Write-Host "  ✗ A 軟體「不應該」跟著變成手動模式！" -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "準備好了嗎？按 Enter 繼續，或輸入 N 取消"
if ($confirm -eq "N" -or $confirm -eq "n") {
	Write-Host "測試已取消" -ForegroundColor Gray
	exit
}

Write-Host ""
Write-Host "[步驟 3/4] 開啟執行檔位置..." -ForegroundColor Yellow
Start-Process explorer.exe -ArgumentList $binPath
Write-Host "  ✓ 已開啟資料夾，請雙擊 DeviceBox.exe 開始測試" -ForegroundColor Green
Write-Host ""

Write-Host "[步驟 4/4] 等待測試完成後，按任意鍵分析日誌..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "正在分析日誌..." -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# 執行日誌分析
if (Test-Path ".\分析日誌.ps1") {
	& ".\分析日誌.ps1"
} else {
	Write-Host "找不到分析腳本，請手動檢查日誌檔案：" -ForegroundColor Yellow
	$logs = Get-ChildItem -Path "DeviceBox\Log" -Filter "debug_*.log" -ErrorAction SilentlyContinue
	foreach ($log in $logs) {
		Write-Host "  $($log.FullName)" -ForegroundColor Gray
	}
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "SQL 驗證查詢（請在 MySQL Workbench 執行）：" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "USE ycm_energy;" -ForegroundColor White
Write-Host "SELECT site_id AS '場域', current_mode_id AS '模式', config_version AS '版本', last_updated_by AS '更新者' FROM site_config;" -ForegroundColor White
Write-Host ""
Write-Host "應該看到：" -ForegroundColor Yellow
Write-Host "  - other   | 15 | xx | PC12326  (模式一)" -ForegroundColor Gray
Write-Host "  - foundry | 17 | xx | PC12326  (手動模式)" -ForegroundColor Gray
