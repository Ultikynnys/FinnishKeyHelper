# Powershell script to register FinnishKeyHelper as a Windows background task running on logon with highest privileges.
# This provides service-grade background execution inside the user's interactive session.

$ErrorActionPreference = "Stop"

$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = Join-Path $dir "FinnishKeyHelper.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "FinnishKeyHelper.exe not found. Please run build.bat first." -ForegroundColor Red
    exit 1
}

$taskName = "FinnishKeyHelper"
$action = New-ScheduledTaskAction -Execute $exePath -Argument "--silent"
$trigger = New-ScheduledTaskTrigger -AtLogOn -User $env:USERNAME
$principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit 0

# Unregister if already present
Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue

# Register new scheduled task
Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings

# Start the task now
Start-ScheduledTask -TaskName $taskName

Write-Host "Successfully registered and started Scheduled Task '$taskName'!" -ForegroundColor Green
Write-Host "It will run silently in the background at every user logon." -ForegroundColor Green
