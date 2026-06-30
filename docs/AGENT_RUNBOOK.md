# AGENT_RUNBOOK.md

# KidGuard Windows Agent Runbook

Version: 1.0.0

---

# Purpose

This document explains how to build, configure, run, and test the Windows Agent for Demo V1.

Owner

Phạm Bá Thạch

---

# Prerequisites

- Windows computer
- .NET 9 Runtime x64
- PowerShell
- Administrator permission for service install/uninstall
- Backend API running and reachable

---

# Build

From the repository root:

```powershell
dotnet build windows-agent\KidGuard.WindowsAgent.sln
```

Expected result:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

---

# Configure

Set local development values in:

```text
windows-agent\src\KidGuard.Agent\appsettings.Development.json
```

Required values before pairing:

```json
{
  "Agent": {
    "ApiBaseUrl": "https://localhost:5001/api/v1/",
    "SetupToken": "SETUP_TOKEN_FROM_BACKEND",
    "DeviceName": "Study PC"
  }
}
```

Never commit real Setup Token, Device Token, JWT, or secrets.

---

# Pairing Flow

1. Start the Agent before it is paired.
2. The Agent calls `POST /pair-code`.
3. Copy the pair code from the Agent log.
4. Parent enters the pair code in the mobile app.
5. Backend returns `deviceId` and `deviceToken` after pairing.
6. Save the credentials locally:

```powershell
dotnet run --project windows-agent\src\KidGuard.Agent\KidGuard.Agent.csproj -- --save-credentials <deviceId> <deviceToken>
```

The credentials are stored with Windows protected local storage.

---

# Publish

From the repository root:

```powershell
.\windows-agent\scripts\Publish-KidGuardAgent.ps1
```

Default output:

```text
windows-agent\artifacts\publish\KidGuard.Agent
```

---

# Install As Windows Service

Open PowerShell as Administrator.

From the repository root:

```powershell
.\windows-agent\scripts\Install-KidGuardAgentService.ps1
```

The default service name is:

```text
KidGuardAgent
```

---

# Start Service

Open PowerShell as Administrator.

```powershell
.\windows-agent\scripts\Start-KidGuardAgentService.ps1
```

---

# Stop Service

Open PowerShell as Administrator.

```powershell
.\windows-agent\scripts\Stop-KidGuardAgentService.ps1
```

---

# Uninstall Service

Open PowerShell as Administrator.

```powershell
.\windows-agent\scripts\Uninstall-KidGuardAgentService.ps1
```

---

# Demo Test Checklist

- [ ] Agent creates pair code.
- [ ] Parent pairs device.
- [ ] Agent stores credentials.
- [ ] Agent sends heartbeat.
- [ ] Agent syncs mode.
- [ ] Change mode to `study`.
- [ ] Open `notepad.exe`.
- [ ] Agent blocks `notepad.exe`.
- [ ] Agent stores local log.
- [ ] Agent uploads log to Backend.
- [ ] Stop Backend temporarily.
- [ ] Agent continues using cached mode.
- [ ] Start Backend again.
- [ ] Agent uploads pending logs.

---

# Troubleshooting

## Missing .NET Runtime

If the Agent cannot start because `Microsoft.NETCore.App 9.0` is missing, install .NET 9 Runtime x64.

## Service Already Exists

Uninstall the old service first:

```powershell
.\windows-agent\scripts\Uninstall-KidGuardAgentService.ps1
```

Then install again.

## Access Denied

Run PowerShell as Administrator.

## Not Blocking Process

Check:

- Agent is running as Administrator or Windows Service.
- Current mode is `study` or `punishment`.
- Process name matches the rule list.
- Process is not in the protected system process list.
