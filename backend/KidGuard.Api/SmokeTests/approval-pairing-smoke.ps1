param(
    [string]$BaseUrl = "http://127.0.0.1:5133",
    [switch]$UseRunningServer
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

    for ($attempt = 0; $attempt -lt 30; $attempt++) {
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

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
$outLog = Join-Path $env:TEMP "kidguard-approval-pairing-smoke-out.log"
$errLog = Join-Path $env:TEMP "kidguard-approval-pairing-smoke-err.log"
$process = $null

if (-not $env:Jwt__Secret) {
    $env:Jwt__Secret = "integration-test-secret-32-characters-long"
}

if (-not $env:SetupToken__Token) {
    $env:SetupToken__Token = "demo-setup-token"
}

if (-not $env:ASPNETCORE_ENVIRONMENT) {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
}

try {
    if (-not $UseRunningServer) {
        Remove-Item $outLog, $errLog -ErrorAction SilentlyContinue
        $process = Start-Process `
            -FilePath "dotnet" `
            -ArgumentList @("run", "--project", "backend\KidGuard.Api\KidGuard.Api.csproj", "--urls", $BaseUrl) `
            -WorkingDirectory $repoRoot `
            -RedirectStandardOutput $outLog `
            -RedirectStandardError $errLog `
            -WindowStyle Hidden `
            -PassThru
    }

    Wait-ForBackend -HealthUrl "$BaseUrl/health"

    $stamp = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
    $email = "approval-smoke-$stamp@example.com"
    $password = "Password123!"

    $register = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/auth/register" -Body @{
        fullName = "Approval Smoke Parent"
        email = $email
        password = $password
        phoneNumber = "0900000000"
    }
    if (-not $register.success) { throw "Register failed." }

    $login = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/auth/login" -Body @{
        email = $email
        password = $password
    }
    $jwt = $login.data.accessToken
    if ([string]::IsNullOrWhiteSpace($jwt)) { throw "Login did not return access token." }
    $parentHeaders = @{ Authorization = "Bearer $jwt" }

    $childCode = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/pairing/child/connection-code" -Body @{
        deviceName = "Approval Smoke Child"
        computerName = "APPROVAL-SMOKE-$stamp"
        agentVersion = "1.0.1"
    }
    $connectionCode = $childCode.data.connectionCode
    if ([string]::IsNullOrWhiteSpace($connectionCode)) { throw "Connection code missing." }

    $parentRequest = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/pairing/requests" -Headers $parentHeaders -Body @{
        connectionCode = $connectionCode
    }
    $pairingRequestId = $parentRequest.data.pairingRequestId
    if ($parentRequest.data.status -ne "pending") { throw "Expected pending pairing request." }

    $pending = Invoke-KidGuardJson -Method Get -Uri "$BaseUrl/pairing/child/pending?connectionCode=$connectionCode"
    if ($pending.data.pairingRequestId -ne $pairingRequestId) { throw "Pending poll did not return expected request." }

    $approved = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/pairing/child/approve" -Body @{
        connectionCode = $connectionCode
        pairingRequestId = $pairingRequestId
    }
    $deviceToken = $approved.data.deviceToken
    $deviceId = $approved.data.deviceId
    if ($approved.data.status -ne "approved") { throw "Approve did not return approved status." }
    if ([string]::IsNullOrWhiteSpace($deviceToken)) { throw "Approve did not return device token." }

    $status = Invoke-KidGuardJson -Method Get -Uri "$BaseUrl/pairing/requests/$pairingRequestId/status" -Headers $parentHeaders
    if ($status.data.status -ne "approved") { throw "Pairing status is not approved." }

    $devices = Invoke-KidGuardJson -Method Get -Uri "$BaseUrl/devices" -Headers $parentHeaders
    $approvedDevice = @($devices.data.items | Where-Object { $_.deviceId -eq $deviceId })
    if ($approvedDevice.Count -ne 1) { throw "Approved device was not returned by device list." }

    $modeUpdate = Invoke-KidGuardJson -Method Put -Uri "$BaseUrl/devices/$deviceId/mode" -Headers $parentHeaders -Body @{
        mode = "study"
    }
    if ($modeUpdate.data.mode -ne "study") { throw "Mode update did not persist study mode." }

    $deviceHeaders = @{ Authorization = "Bearer $deviceToken" }
    $modeSync = Invoke-KidGuardJson -Method Get -Uri "$BaseUrl/devices/$deviceId/mode" -Headers $deviceHeaders
    if ($modeSync.data.mode -ne "study") { throw "Mode sync did not return study mode." }

    $heartbeat = Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/devices/$deviceId/heartbeat" -Headers $deviceHeaders -Body @{
        status = "online"
        agentVersion = "1.0.1"
    }
    if ($heartbeat.data.nextHeartbeat -lt 1) { throw "Heartbeat did not return next interval." }

    Invoke-KidGuardJson -Method Post -Uri "$BaseUrl/devices/$deviceId/logs" -Headers $deviceHeaders -Body @{
        processName = "notepad.exe"
        action = "blocked"
        mode = "study"
        message = "Approval smoke test log."
    } | Out-Null

    $logs = Invoke-KidGuardJson -Method Get -Uri "$BaseUrl/devices/$deviceId/logs" -Headers $parentHeaders
    if ($logs.data.total -lt 1) { throw "Log view did not return uploaded log." }

    Write-Output "APPROVAL_PAIRING_SMOKE_OK requestId=$pairingRequestId deviceId=$deviceId code=$connectionCode"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
