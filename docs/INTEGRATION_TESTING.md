# INTEGRATION_TESTING.md

# KidGuard Integration Testing

Version: 1.0.0

---

# Purpose

This document records the local integration smoke test used by the `test/integration-all-projects` branch.

The smoke test verifies the Demo V1 backend contract used by Mobile and Windows Agent:

- Parent login
- Agent pair code creation
- Parent device pairing
- Agent heartbeat
- Agent mode sync
- Agent log upload
- Parent log view

---

# Required Local Configuration

Do not commit real secrets.

Set these values only in your local shell or local user secrets:

```powershell
$env:Jwt__Secret = '<local-jwt-secret-at-least-32-characters>'
$env:SetupToken__Token = '<local-setup-token>'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
```

Backend default local database:

```text
Server=(localdb)\MSSQLLocalDB;Database=KidGuardDb;Trusted_Connection=True;TrustServerCertificate=True;
```

---

# Backend Smoke Test Flow

Start Backend locally:

```powershell
dotnet run --no-build --project backend\KidGuard.Api\KidGuard.Api.csproj --urls http://127.0.0.1:5123
```

Then verify these steps:

1. `GET /health`
2. `POST /auth/login` with parent account
3. `POST /pair-code` with Setup Token
4. `POST /devices/pair` with parent JWT
5. `POST /devices/{deviceId}/heartbeat` with Device Token
6. `GET /devices/{deviceId}/mode` with Device Token
7. `POST /devices/{deviceId}/logs` with Device Token
8. `GET /devices/{deviceId}/logs` with parent JWT

---

# Latest Local Result

Date: 2026-06-30

Branch: `test/integration-all-projects`

Result: Passed

Verified:

- Login returned parent JWT.
- Pair code was created.
- Device pairing returned Device ID and one-time Device Token.
- Heartbeat endpoint accepted Device Token.
- Mode sync returned current mode.
- Log upload succeeded.
- Log view returned uploaded log.

---

# Mobile Verification

Mobile verification commands:

```powershell
flutter analyze
flutter test
flutter build windows
```

Expected result:

- Analyze: no issues
- Tests: all pass
- Windows build: `build\windows\x64\runner\Release\kidguard_mobile.exe`

---

# Agent Verification

Windows Agent verification command:

```powershell
dotnet build windows-agent\KidGuard.WindowsAgent.sln
```

Expected result:

- Build succeeds with 0 warnings and 0 errors.

---

# Remaining Full Demo Work

The API smoke test proves the backend contract. The remaining manual demo work is to run the actual Windows Agent process and Mobile app against the same local Backend instance:

1. Start Backend.
2. Start Windows Agent with local API URL and Setup Token.
3. Read pair code from Agent logs.
4. Pair from Mobile.
5. Save returned Device Token into Agent credentials.
6. Verify heartbeat, mode sync, process block, and uploaded logs from Mobile.
