# AGENT_DESIGN.md

# Windows Agent Design

Version: 1.0.0

---

# Purpose

This document defines the Windows Agent design for Demo V1.

The Windows Agent is owned by Phạm Bá Thạch.

---

# Agent Responsibilities

Windows Agent must:

- Create pair code before pairing.
- Store Device Token securely after pairing.
- Send heartbeat to Backend.
- Download current mode from Backend.
- Cache last known configuration.
- Monitor running processes.
- Block disallowed processes.
- Write local logs.
- Upload logs to Backend.
- Continue protection while offline.

Windows Agent must never:

- Connect directly to SQL Server.
- Authenticate parent accounts.
- Define API behavior.
- Change database schema.
- Disable protection because the internet is lost.

---

# Recommended Internal Services

## AgentHost

Runs the worker service lifecycle.

Responsibilities:

- Start background services.
- Stop services gracefully.
- Log startup and shutdown.

---

## PairingService

Handles first-time device pairing.

Responsibilities:

- Request pair code from Backend.
- Show or log pair code for parent entry.
- Receive and store Device Token after pairing flow is complete.

Demo V1 Note

If automatic token delivery is not ready, the team may manually copy the Device Token into local secure configuration during early development. This shortcut is only allowed for local testing and must not be used as the final production flow.

Local Demo Credential Command

During Demo V1 local testing, after the parent pairs a device and receives the Device Token, the Agent can save credentials into protected local storage with:

```text
KidGuard.Agent --save-credentials <deviceId> <deviceToken>
```

This command stores the credentials using Windows protected storage. Do not commit Device Token values into configuration files.

---

## HeartbeatService

Reports device status.

Responsibilities:

- Send heartbeat every 30 seconds.
- Include agent version.
- Retry on temporary failures.
- Update local connection status.

---

## ModeSyncService

Downloads current mode.

Responsibilities:

- Call `GET /devices/{deviceId}/mode`.
- Save successful response to local cache.
- Provide current mode to ProcessMonitorService.

Recommended Demo V1 Strategy

Use polling every 5 to 10 seconds.

Reason

Polling is simpler than SignalR or WebSocket and is enough for Demo V1.

---

## LocalCacheService

Stores last known safe configuration.

Responsibilities:

- Store current mode.
- Store last successful sync time.
- Store pending logs.
- Read cache when Backend is unavailable.

Cache Rule

If Backend is unavailable, use the last known mode and continue protection.

---

## ProcessMonitorService

Scans running processes.

Responsibilities:

- Read current mode from ModeSyncService or LocalCacheService.
- Enumerate running processes.
- Compare processes against current rules.
- Send blocked process request to ProcessBlockerService.

Recommended scan interval for Demo V1

2 to 5 seconds.

---

## ProcessBlockerService

Blocks disallowed processes.

Responsibilities:

- Terminate disallowed user applications.
- Never terminate protected system processes.
- Return result to LoggingService.

Demo V1 Test Process

Use `notepad.exe` as the first blocked process.

---

## LoggingService

Writes local logs.

Responsibilities:

- Log blocked process events.
- Log unexpected errors.
- Log offline and reconnect events.
- Avoid logging secrets.

Never log:

- Password
- JWT
- Device Token
- Connection String

---

## LogUploadService

Uploads logs to Backend.

Responsibilities:

- Upload logs when online.
- Keep logs locally when offline.
- Retry upload after reconnecting.
- Mark logs as uploaded after success.

---

# Mode Rules For Demo V1

## fun

Behavior

- Do not block applications.
- Continue heartbeat.
- Continue logging important events.

---

## study

Behavior

- Block distracting applications.
- Demo V1 can start with `notepad.exe` as a test blocked process.
- Rules should be configurable later through Backend.

---

## punishment

Behavior

- Allow only approved applications.
- Never terminate Windows system processes.
- For Demo V1, keep this mode simple and safe.

---

# Protected System Processes

Agent must not terminate critical Windows processes.

Minimum protected examples:

- System
- Idle
- wininit.exe
- winlogon.exe
- csrss.exe
- smss.exe
- services.exe
- lsass.exe
- explorer.exe
- svchost.exe
- dwm.exe

This list should be expanded during testing.

---

# Configuration Rules

Never hardcode:

- API URL
- Setup Token
- Device Token
- Device ID
- Retry interval
- Scan interval

Use configuration files or environment variables.

---

# Offline Rules

When Backend is unavailable:

1. Keep last known mode.
2. Continue process monitoring.
3. Continue process blocking.
4. Store logs locally.
5. Retry Backend connection.
6. Upload cached logs after reconnecting.

Protection must never stop only because the server is unavailable.

---

# Agent Definition Of Done

Agent work is complete for Demo V1 when:

- [x] Agent can request pair code.
- [x] Agent can store Device Token.
- [x] Agent sends heartbeat every 30 seconds.
- [x] Agent syncs current mode.
- [x] Agent caches last known mode.
- [x] Agent blocks demo process.
- [x] Agent avoids protected system processes.
- [x] Agent writes local logs.
- [x] Agent uploads logs.
- [x] Agent continues protection offline.
- [x] Agent uploads cached logs after reconnect.
