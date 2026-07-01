# 快速診斷 - 一鍵執行
# ========================================

$ErrorActionPreference = "Stop"

Write-Host "========================================"
Write-Host "快速診斷工具" -ForegroundColor Cyan
Write-Host "========================================"
Write-Host ""

# 檢查是否在正確的目錄
if (!(Test-Path "DeviceBox.csproj")) {
	Write-Host "警告: 您不在專案根目錄" -ForegroundColor Yellow
	Write-Host "正在切換到專案目錄..." -ForegroundColor Yellow

	if (Test-Path "..\DeviceBox.csproj") {
		Set-Location ..
	} elseif (Test-Path "DeviceBox\DeviceBox.csproj") {
		Set-Location DeviceBox
	} else {
		Write-Host "錯誤: 找不到專案檔" -ForegroundColor Red
		Read-Host "按 Enter 結束"
		exit 1
	}
}

Write-Host "目前目錄: $PWD" -ForegroundColor Green
Write-Host ""

# 尋找執行檔
Write-Host "尋找 DeviceBox.exe..." -ForegroundColor Yellow

$exePath = $null
$searchPaths = @(
	"bin\Debug\DeviceBox.exe",
	"bin\Release\DeviceBox.exe",
	"DeviceBox.exe"
)

foreach ($path in $searchPaths) {
	if (Test-Path $path) {
		$exePath = Resolve-Path $path
		Write-Host "✅ 找到: $exePath" -ForegroundColor Green
		break
	}
}

if ($null -eq $exePath) {
	Write-Host ""
	Write-Host "❌ 找不到 DeviceBox.exe" -ForegroundColor Red
	Write-Host ""
	Write-Host "請先編譯專案:" -ForegroundColor Yellow
	Write-Host ""
	Write-Host "方法 1 - 使用 Visual Studio:" -ForegroundColor White
	Write-Host "  1. 開啟 DeviceBox.sln" -ForegroundColor Gray
	Write-Host "  2. 按 F6 或 建置 → 建置方案" -ForegroundColor Gray
	Write-Host ""
	Write-Host "方法 2 - 使用命令列:" -ForegroundColor White
	Write-Host "  msbuild DeviceBox.sln /p:Configuration=Debug" -ForegroundColor Gray
	Write-Host ""

	$build = Read-Host "是否要現在嘗試編譯? (Y/N)"

	if ($build -eq "Y" -or $build -eq "y") {
		Write-Host ""
		Write-Host "正在編譯..." -ForegroundColor Cyan

		try {
			# 嘗試使用 msbuild
			$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.exe"

			if (!(Test-Path $msbuild)) {
				# 嘗試其他可能的路徑
				$msbuild = "msbuild"
			}

			& $msbuild "DeviceBox.csproj" /p:Configuration=Debug /p:Platform=AnyCPU /v:minimal

			Write-Host ""
			Write-Host "✅ 編譯成功!" -ForegroundColor Green

			# 重新尋找執行檔
			$exePath = Resolve-Path "bin\Debug\DeviceBox.exe"
		}
		catch {
			Write-Host ""
			Write-Host "❌ 編譯失敗" -ForegroundColor Red
			Write-Host $_.Exception.Message -ForegroundColor Red
			Read-Host "按 Enter 結束"
			exit 1
		}
	}
	else {
		Read-Host "按 Enter 結束"
		exit 1
	}
}

Write-Host ""
Write-Host "========================================"
Write-Host "啟動診斷工具" -ForegroundColor Cyan
Write-Host "========================================"
Write-Host ""

try {
	& $exePath --diagnostic
}
catch {
	Write-Host ""
	Write-Host "❌ 錯誤: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "診斷完成" -ForegroundColor Green
Read-Host "按 Enter 結束"
