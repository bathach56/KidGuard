param(
    [string]$BaseUrl = "http://127.0.0.1:5133"
)

$ErrorActionPreference = "Stop"

function Invoke-KidGuardJson {
    param(
        [string]$Method,
        [string]$Uri,
        [hashtable]$Headers = @{},
        [object]$Body = $null
    )

    $parameters = @{
        Method = $Method
        Uri = $Uri
        Headers = $Headers
        TimeoutSec = 15
    }

    if ($null -ne $Body) {
        $parameters.ContentType = "application/json"
        $parameters.Body = ($Body | ConvertTo-Json -Depth 8)
    }

    Invoke-RestMethod @parameters
}

function Wait-ForBackend {
    param([string]$HealthUrl)

    for ($attempt = 0; $attempt -lt 45; $attempt++) {
        try {
            $health = Invoke-KidGuardJson -Method Get -Uri $HealthUrl
            if ($health.success) {
                return
            }
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    throw "Backend did not become ready at $HealthUrl."
}

function Backup-FileIfExists {
    param(
        [string]$Path,
        [string]$BackupPath
    )

    if (Test-Path $Path) {
        Copy-Item -LiteralPath $Path -Destination $BackupPath -Force
        return $true
    }

    return $false
}

function Restore-FileBackup {
    param(
        [string]$Path,
        [string]$BackupPath,
        [bool]$HadOriginal
    )

    if ($HadOriginal) {
        Copy-Item -LiteralPath $BackupPath -Destination $Path -Force
    }
    elseif (Test-Path $Path) {
        Remove-Item -LiteralPath $Path -Force
    }

    if (Test-Path $BackupPath) {
        Remove-Item -LiteralPath $BackupPath -Force
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$agentProject = Join-Path $repoRoot "windows-agent\src\KidGuard.Agent\KidGuard.Agent.csproj"
$credentialDirectory = Join-Path $env:ProgramData "KidGuard\Agent"
$credentialPath = Join-Path $credentialDirectory "device-credentials.dat"
$cachePath = Join-Path $credentialDirectory "agent-cache.json"
$credentialBackupPath = Join-Path $env:TEMP "kidguard-device-credentials.backup"
$cacheBackupPath = Join-Path $env:TEMP "kidguard-agent-cache.backup"
$backendOutLog = Join-Path $env:TEMP "kidguard-agent-approval-backend-out.log"
$backendErrLog = Join-Path $env:TEMP "kidguard-agent-approval-backend-err.log"
$agentOutLog = Join-Path $env:TEMP "kidguard-agent-approval-agent-out.log"
$agentErrLog = Join-Path $env:TEMP "kidguard-agent-approval-agent-err.log"
$backendProcess = $null
$agentProcess = $null
$notepadProcess = $null
$hadCredential = Backup-FileIfExists -Path $credentialPath -BackupPath $credentialBackupPath
$hadCache = Backup-FileIfExists -Path $cachePath -BackupPath $cacheBackupPath

if (-not $env:Jwt__Secret) {
    $env:Jwt__Secret = "integration-test-secret-32-characters-long"
}

if (-not $env:ASPNETCORE_ENVIRONMENT) {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
}

try {
    Remove-Item $backendOutLog, $backendErrLog, $agentOutLog, $agentErrLog -ErrorAction SilentlyContinue

    $backendProcess = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList @("run", "--project", "backend\KidGuard.Api\KidGuard.Api.csproj", "--urls", $BaseUrl) `
        -WorkingDirectory $repoRoot `
        -RedirectStandardOutput $backendOutLog `
        -RedirectStandardError $backendErrLog `
        -WindowStyle Hidden `
        -PassThru

    Wait-ForBackend -HealthUrl "$BaseUrl/health"

    $stamp = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
    $email = "agent-smoke-$stamp@example.com"
    $password = "Password123!"

    Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/auth/register" -Body @{
        fullName = "Agent Smoke Parent"
        email = $email
        password = $password
        phoneNumber = "0900000000"
    } | Out-Null

    $login = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/auth/login" -Body @{
        email = $email
        password = $password
    }
    $parentHeaders = @{ Authorization = "Bearer $($login.data.accessToken)" }

    $childCode = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/pairing/child/connection-code" -Body @{
        deviceName = "Agent Smoke Child"
        computerName = "AGENT-SMOKE-$stamp"
        agentVersion = "1.0.1"
    }

    $connectionCode = $childCode.data.connectionCode
    $parentRequest = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/pairing/requests" -Headers $parentHeaders -Body @{
        connectionCode = $connectionCode
    }

    $approved = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/pairing/child/approve" -Body @{
        connectionCode = $connectionCode
        pairingRequestId = $parentRequest.data.pairingRequestId
    }

    $deviceId = $approved.data.deviceId
    $deviceToken = $approved.data.deviceToken

    Invoke-KidGuardJson -Method Put -Uri "$BaseUrl/devices/$deviceId/mode" -Headers $parentHeaders -Body @{
        mode = "study"
    } | Out-Null

    & dotnet run --project $agentProject -- --save-credentials $deviceId $deviceToken
    if ($LASTEXITCODE -ne 0) {
        throw "Agent credential save command failed with exit code $LASTEXITCODE."
    }

    New-Item -ItemType Directory -Path $credentialDirectory -Force | Out-Null
    @{
        CurrentMode = 1
        LastSuccessfulSyncAt = $null
        PendingLogs = @(
            @{
                ProcessName = "notepad.exe"
                Action = "blocked"
                Mode = 1
                Message = "Agent approval smoke seeded pending log."
                CreatedAt = [DateTimeOffset]::UtcNow.ToString("O")
            }
        )
    } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $cachePath -Encoding UTF8

    $env:Agent__ApiBaseUrl = $BaseUrl
    $env:Agent__HeartbeatIntervalSeconds = "5"
    $env:Agent__ModeSyncIntervalSeconds = "2"
    $env:Agent__ProcessScanIntervalSeconds = "1"
    $env:Agent__AgentVersion = "1.0.1"

    $agentProcess = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList "run --project `"$agentProject`" --no-launch-profile" `
        -WorkingDirectory $repoRoot `
        -RedirectStandardOutput $agentOutLog `
        -RedirectStandardError $agentErrLog `
        -WindowStyle Hidden `
        -PassThru

    Start-Sleep -Seconds 5
    try {
        $notepadProcess = Start-Process -FilePath "notepad.exe" -PassThru
    }
    catch {
        Write-Warning "Could not start notepad.exe for interactive block check: $($_.Exception.Message)"
    }
    Start-Sleep -Seconds 10

    $logs = Invoke-KidGuardJson -Method Get -Uri "$BaseUrl/devices/$deviceId/logs" -Headers $parentHeaders
    $blockedNotepad = @($logs.data.items | Where-Object {
        $_.processName -eq "notepad.exe" -and $_.action -eq "blocked" -and $_.mode -eq "study"
    })

    if ($blockedNotepad.Count -lt 1) {
        throw "Agent did not upload a study-mode pending log."
    }

    Write-Output "AGENT_APPROVAL_FLOW_SMOKE_OK requestId=$($parentRequest.data.pairingRequestId) deviceId=$deviceId"
}
finally {
    if ($notepadProcess -and -not $notepadProcess.HasExited) {
        Stop-Process -Id $notepadProcess.Id -Force -ErrorAction SilentlyContinue
    }

    if ($agentProcess -and -not $agentProcess.HasExited) {
        Stop-Process -Id $agentProcess.Id -Force
    }

    if ($backendProcess -and -not $backendProcess.HasExited) {
        Stop-Process -Id $backendProcess.Id -Force
    }

    Restore-FileBackup -Path $credentialPath -BackupPath $credentialBackupPath -HadOriginal $hadCredential
    Restore-FileBackup -Path $cachePath -BackupPath $cacheBackupPath -HadOriginal $hadCache

    Remove-Item Env:\Agent__ApiBaseUrl -ErrorAction SilentlyContinue
    Remove-Item Env:\Agent__HeartbeatIntervalSeconds -ErrorAction SilentlyContinue
    Remove-Item Env:\Agent__ModeSyncIntervalSeconds -ErrorAction SilentlyContinue
    Remove-Item Env:\Agent__ProcessScanIntervalSeconds -ErrorAction SilentlyContinue
    Remove-Item Env:\Agent__AgentVersion -ErrorAction SilentlyContinue
}
