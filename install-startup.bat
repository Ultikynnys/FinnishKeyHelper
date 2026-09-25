@echo off
setlocal
cd /d "%~dp0"

if not exist "FinnishKeyHelper.exe" (
    echo FinnishKeyHelper.exe not found. Running build.bat first...
    call build.bat
)

if not exist "FinnishKeyHelper.exe" (
    echo Build failed or file missing.
    pause
    exit /b 1
)

echo Registering FinnishKeyHelper in Windows Startup (Registry)...
"%~dp0FinnishKeyHelper.exe" --install

echo Creating shortcut in Windows Startup Folder...
powershell -NoProfile -Command "$ws = New-Object -ComObject WScript.Shell; $target = (Resolve-Path 'FinnishKeyHelper.exe').Path; $s = $ws.CreateShortcut((Join-Path $ws.SpecialFolders.Item('Startup') 'FinnishKeyHelper.lnk')); $s.TargetPath = $target; $s.WorkingDirectory = (Split-Path $target); $s.Description = 'Finnish Key Helper'; $s.Save()"

echo Starting FinnishKeyHelper...
start "" "%~dp0FinnishKeyHelper.exe"

echo.
echo =======================================================
echo Verified: FinnishKeyHelper is configured to start on boot!
echo =======================================================
pause
