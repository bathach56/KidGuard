param(
    [string]$ServiceName = "KidGuardAgent"
)

$ErrorActionPreference = "Stop"

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $service) {
    throw "Service '$ServiceName' was not found."
}

if ($service.Status -eq "Stopped") {
    Write-Host "Service '$ServiceName' is already stopped."
    return
}

Stop-Service -Name $ServiceName
Write-Host "Stopped service '$ServiceName'."
