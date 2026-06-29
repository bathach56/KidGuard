param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$agentRoot = Split-Path -Parent $scriptDirectory
$projectPath = Join-Path $agentRoot "src\KidGuard.Agent\KidGuard.Agent.csproj"

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $agentRoot "artifacts\publish\KidGuard.Agent"
}

dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained false `
    --output $OutputDirectory

Write-Host "KidGuard Agent published to: $OutputDirectory"
