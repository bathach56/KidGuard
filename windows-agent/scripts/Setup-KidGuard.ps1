param(
    [string]$ApiUrl = "http://127.0.0.1:5123/",
    [string]$SetupToken = "demo-setup-token",
    [string]$InstallDir = "C:\KidGuard"
)

$ErrorActionPreference = "Stop"

# 1. Check for Administrator rights
$isAdministrator = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdministrator) {
    Write-Error "Please run this script from an Administrator PowerShell session."
    Exit 1
}

Write-Host "=== KidGuard Installer ===" -ForegroundColor Green
Write-Host "Target API URL: $ApiUrl"
Write-Host "Setup Token   : $SetupToken"
Write-Host "Install Path  : $InstallDir"
Write-Host "=========================="

# 2. Stop and remove existing service if present
$ServiceName = "KidGuardAgent"
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Stopping and removing existing service '$ServiceName'..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -ErrorAction SilentlyContinue
    # Give it a moment to stop
    Start-Sleep -Seconds 2
    # Remove service using sc.exe for compatibility or Remove-Service if supported
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 1
}

# 3. Create install directories
if (Test-Path -Path $InstallDir) {
    Write-Host "Cleaning existing installation directory..." -ForegroundColor Yellow
    Remove-Item -Path $InstallDir -Recurse -Force -ErrorAction SilentlyContinue
}
New-Item -Path "$InstallDir\Agent" -ItemType Directory -Force | Out-Null
New-Item -Path "$InstallDir\Client" -ItemType Directory -Force | Out-Null

# 4. Get script path and project paths
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$agentRoot = Split-Path -Parent $scriptDirectory

$agentProject = Join-Path $agentRoot "src\KidGuard.Agent\KidGuard.Agent.csproj"
$clientProject = Join-Path $agentRoot "src\KidGuard.Client\KidGuard.Client.csproj"

# 5. Build and Publish Agent (Service)
Write-Host "Building and publishing KidGuard Agent..." -ForegroundColor Cyan
dotnet publish $agentProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    --output "$InstallDir\Agent"

# 6. Build and Publish Client (GUI)
Write-Host "Building and publishing KidGuard Client UI..." -ForegroundColor Cyan
dotnet publish $clientProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    --output "$InstallDir\Client"

# 7. Configure appsettings.json in Agent directory
$appSettingsPath = Join-Path "$InstallDir\Agent" "appsettings.json"
if (Test-Path -Path $appSettingsPath) {
    Write-Host "Configuring appsettings.json..." -ForegroundColor Cyan
    $json = Get-Content -Raw -Path $appSettingsPath | ConvertFrom-Json
    $json.Agent.ApiBaseUrl = $ApiUrl
    $json.Agent.SetupToken = $SetupToken
    $json | ConvertTo-Json -Depth 10 | Set-Content -Path $appSettingsPath
}

# 8. Set Machine-level Environment Variables (persistent)
Write-Host "Setting system environment variables..." -ForegroundColor Cyan
[Environment]::SetEnvironmentVariable("KIDGUARD_API_BASE_URL", $ApiUrl, [System.EnvironmentVariableTarget]::Machine)
[Environment]::SetEnvironmentVariable("KIDGUARD_SETUP_TOKEN", $SetupToken, [System.EnvironmentVariableTarget]::Machine)
[Environment]::SetEnvironmentVariable("Agent__ApiBaseUrl", $ApiUrl, [System.EnvironmentVariableTarget]::Machine)
[Environment]::SetEnvironmentVariable("Agent__SetupToken", $SetupToken, [System.EnvironmentVariableTarget]::Machine)

# 9. Register and Start Windows Service
Write-Host "Installing Windows Service '$ServiceName'..." -ForegroundColor Cyan
$executablePath = Join-Path "$InstallDir\Agent" "KidGuard.Agent.exe"
New-Service `
    -Name $ServiceName `
    -DisplayName "KidGuard Agent Service" `
    -BinaryPathName "`"$executablePath`"" `
    -StartupType Automatic `
    -Description "KidGuard background worker service for monitoring and blocking applications."

Write-Host "Starting Windows Service..." -ForegroundColor Cyan
Start-Service -Name $ServiceName

# 10. Create Desktop Shortcut for KidGuard Client (UI)
Write-Host "Creating Desktop shortcut..." -ForegroundColor Cyan
$desktopPath = [System.IO.Path]::Combine($env:USERPROFILE, "Desktop")
$shortcutPath = Join-Path $desktopPath "KidGuard Client.lnk"
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($shortcutPath)
$Shortcut.TargetPath = "$InstallDir\Client\KidGuard.Client.exe"
$Shortcut.WorkingDirectory = "$InstallDir\Client"
$Shortcut.Description = "KidGuard Child Interface"
$Shortcut.Save()

Write-Host "`n=======================================================" -ForegroundColor Green
Write-Host "SUCCESS: KidGuard has been successfully installed!" -ForegroundColor Green
Write-Host "Installation Folder : $InstallDir" -ForegroundColor Green
Write-Host "Desktop Shortcut    : $shortcutPath" -ForegroundColor Green
Write-Host "Windows Service     : $ServiceName (Running)" -ForegroundColor Green
Write-Host "=======================================================" -ForegroundColor Green
