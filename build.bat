@echo off
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist %CSC% set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
echo Compiling FinnishKeyHelper.exe using %CSC%...
%CSC% /nologo /target:winexe /optimize+ /out:"%~dp0FinnishKeyHelper.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll "%~dp0Program.cs"
if %ERRORLEVEL% equ 0 (
    echo Successfully built FinnishKeyHelper.exe!
) else (
    echo Build failed.
)
