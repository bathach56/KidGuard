param(
    [string]$ServiceName = "KidGuardAgent"
)

$ErrorActionPreference = "Stop"

$isAdministrator = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdministrator) {
    throw "Please run PowerShell as Administrator before uninstalling the Windows service."
}

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $service) {
    Write-Host "Service '$ServiceName' is not installed."
    return
}

if ($service.Status -ne "Stopped") {
    Stop-Service -Name $ServiceName
}

sc.exe delete $ServiceName | Out-Null
Write-Host "Uninstalled service '$ServiceName'."
