@echo off
chcp 65001 >nul
cls
echo ========================================
echo Device Configuration Migration Tool
echo ========================================
echo.
echo This tool will:
echo 1. Import factory data from config.xml to MySQL
echo 2. Import device configuration from config.xml to MySQL
echo 3. Import alarm limits from config.xml to MySQL
echo.
echo Please confirm:
echo - MySQL database tables have been created
echo - config.xml exists in the program directory
echo.
pause

echo.
echo Starting migration tool...
echo.

REM Try different paths to find DeviceBox.exe
if exist "DeviceBox.exe" (
    DeviceBox.exe --migrate
) else if exist "bin\Debug\DeviceBox.exe" (
    bin\Debug\DeviceBox.exe --migrate
) else if exist "bin\Release\DeviceBox.exe" (
    bin\Release\DeviceBox.exe --migrate
) else (
    echo ERROR: Cannot find DeviceBox.exe
    echo Please build the project first in Visual Studio
    echo.
    pause
    exit /b 1
)

echo.
echo Migration completed!
pause
