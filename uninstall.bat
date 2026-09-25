@echo off
setlocal
cd /d "%~dp0"

echo Stopping any running FinnishKeyHelper processes...
taskkill /f /im FinnishKeyHelper.exe >nul 2>&1

echo Removing from Windows startup registry...
if exist "FinnishKeyHelper.exe" (
    "%~dp0FinnishKeyHelper.exe" --uninstall
)

echo Removing shortcut from Windows Startup folder...
powershell -NoProfile -Command "$ws = New-Object -ComObject WScript.Shell; $path = Join-Path $ws.SpecialFolders.Item('Startup') 'FinnishKeyHelper.lnk'; if (Test-Path $path) { Remove-Item $path -Force }"

echo.
echo Uninstallation complete. FinnishKeyHelper stopped and removed from startup.
pause
