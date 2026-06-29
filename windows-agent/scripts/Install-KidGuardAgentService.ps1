param(
    [string]$ServiceName = "KidGuardAgent",
    [string]$DisplayName = "KidGuard Agent",
    [string]$PublishDirectory = ""
)

$ErrorActionPreference = "Stop"

$isAdministrator = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdministrator) {
    throw "Please run PowerShell as Administrator before installing the Windows service."
}

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$agentRoot = Split-Path -Parent $scriptDirectory

if ([string]::IsNullOrWhiteSpace($PublishDirectory)) {
    $PublishDirectory = Join-Path $agentRoot "artifacts\publish\KidGuard.Agent"
}

$executablePath = Join-Path $PublishDirectory "KidGuard.Agent.exe"
if (-not (Test-Path -LiteralPath $executablePath)) {
    throw "KidGuard.Agent.exe was not found. Run Publish-KidGuardAgent.ps1 first."
}

$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    throw "Service '$ServiceName' already exists. Uninstall it first or choose another service name."
}

New-Service `
    -Name $ServiceName `
    -DisplayName $DisplayName `
    -BinaryPathName "`"$executablePath`"" `
    -StartupType Automatic `
    -Description "KidGuard Windows Agent for Demo V1."

Write-Host "Installed service '$ServiceName'."
Write-Host "Start it with: .\Start-KidGuardAgentService.ps1"
