param(
    [string]$ServiceName = "KidGuardAgent"
)

$ErrorActionPreference = "Stop"

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $service) {
    throw "Service '$ServiceName' was not found. Install it first."
}

Start-Service -Name $ServiceName
Write-Host "Started service '$ServiceName'."
