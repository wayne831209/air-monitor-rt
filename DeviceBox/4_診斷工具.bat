@echo off
chcp 65001 >nul
cls
echo ========================================
echo Data Loading Diagnostic Tool
echo ========================================
echo.
echo This tool will help diagnose why the program cannot see device data
echo.
echo The diagnostic tool will:
echo 1. Test database connection
echo 2. Check if there is data in the database
echo 3. Test if Config class can load correctly
echo 4. Display detailed diagnostic information
echo.
pause

echo.
echo Starting diagnostic tool...
echo.

REM Try different paths to find DeviceBox.exe
if exist "DeviceBox.exe" (
    DeviceBox.exe --diagnostic
) else if exist "bin\Debug\DeviceBox.exe" (
    bin\Debug\DeviceBox.exe --diagnostic
) else if exist "bin\Release\DeviceBox.exe" (
    bin\Release\DeviceBox.exe --diagnostic
) else (
    echo ERROR: Cannot find DeviceBox.exe
    echo Please build the project first in Visual Studio
    echo.
    pause
    exit /b 1
)

pause
