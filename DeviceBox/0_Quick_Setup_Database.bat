@echo off
chcp 65001 >nul
echo ========================================
echo Quick Database Setup
echo ========================================
echo.

REM Database connection info
set DB_HOST=192.168.102.182
set DB_NAME=ycm_energy
set DB_USER=Client
set DB_PASS=root

echo Connecting to database: %DB_HOST%/%DB_NAME%
echo.

REM Check if mysql command exists
where mysql >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
	echo [ERROR] mysql command not found
	echo.
	echo Please use one of the following methods:
	echo   1. Open MySQL Workbench and execute:
	echo      DeviceBox\Database\完整建表並匯入範例資料.sql
	echo.
	echo   2. Run migration tool:
	echo      DeviceBox\bin\Debug\DeviceBox.exe --migrate
	echo.
	pause
	exit /b 1
)

echo Executing SQL script...
echo.

mysql -h %DB_HOST% -u %DB_USER% -p%DB_PASS% %DB_NAME% < "DeviceBox\Database\完整建表並匯入範例資料.sql"

if %ERRORLEVEL% EQU 0 (
	echo.
	echo ========================================
	echo Success!
	echo ========================================
	echo.
	echo Tables created and notification settings imported!
	echo.
	echo Next step: Run migration tool to import factories and devices
	echo   DeviceBox\bin\Debug\DeviceBox.exe --migrate
	echo.
) else (
	echo.
	echo [ERROR] SQL execution failed
	echo.
)

pause
