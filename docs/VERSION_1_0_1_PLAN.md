# VERSION_1_0_1_PLAN.md

# KidGuard Version 1.0.1 Redesign Plan

Version: 1.0.1

Status: Proposed Active Plan

Owner: Admin

Date: 2026-07-01

---

# 1. Purpose

Version 1.0.1 changes the Demo V1 direction from a mobile-first parental control flow into a Windows-first dual-role application flow.

The new target experience is similar to remote support pairing tools:

- A user opens KidGuard.
- The user chooses Parent or Child.
- Parent signs in and enters the child's connection code.
- Child does not sign in.
- Child sees a code and receives an approval request.
- After Child approves, Parent can manage the child's device.
- Windows Service protection is enabled only after the approved pairing flow.

Mobile remains part of the future product, but Version 1.0.1 prioritizes the Windows experience first.

---

# 2. Product Direction

## Parent Mode

Parent Mode is used by the parent on Windows first.

Parent can:

- Register an account.
- Login.
- Enter a child connection code.
- Send a connection request to the child device.
- See pending, approved, and rejected pairing states.
- Manage approved devices.
- Change mode.
- View device status.
- View activity logs.

Future parent clients:

- Mobile Parent App.
- Web dashboard if needed.

---

## Child Mode

Child Mode is used on the child's Windows computer.

Child does not need an account.

Child can:

- Open KidGuard.
- Choose Child Mode.
- See a temporary connection code and password/PIN.
- Receive a visible request when a parent tries to connect.
- Approve or reject the request.
- Allow KidGuard Windows Service to start protection after approval.

After approval, the child device:

- Stores device credentials securely.
- Sends heartbeat.
- Syncs current mode.
- Applies process rules.
- Blocks disallowed applications.
- Uploads logs.
- Keeps protecting while offline.

---

# 3. Version 1.0.1 Scope

Version 1.0.1 focuses on a complete Windows demo.

In scope:

- Windows Parent Mode.
- Windows Child Mode.
- Parent register and login.
- Child connection code generation.
- Parent enters child code.
- Child receives approval request.
- Child approves or rejects pairing.
- Backend stores pairing state.
- Windows Service starts or continues protection after approval.
- Parent can change mode after approval.
- Agent receives mode and blocks demo process.
- Logs upload and display.
- Local end-to-end demo.

Out of scope for 1.0.1:

- Full Mobile rewrite.
- Android child control.
- iOS child control.
- Website blocking.
- Remote desktop.
- Screen streaming.
- AI recommendations.
- Production installer polish.

---

# 4. Target Architecture

```text
Parent Windows Client
        |
        | JWT
        v
ASP.NET Core Backend API
        |
        | SQL Server
        v
Child Windows Client
        |
        | Local approval + protected credentials
        v
KidGuard Windows Service
        |
        v
Windows Process Manager
```

Future:

```text
Parent Mobile Client
        |
        | JWT
        v
ASP.NET Core Backend API
```

Rules:

- Parent client never talks directly to Child client.
- Child client never talks directly to Parent client.
- All remote communication goes through Backend API.
- Windows Service is the only component allowed to control Windows processes.
- Backend must not contain Windows process control logic.

---

# 5. Main User Flow

## Flow A - Parent Register/Login

1. Parent opens KidGuard Windows app.
2. Parent chooses Parent Mode.
3. Parent registers an account or logs in.
4. Backend returns JWT.
5. Parent dashboard is shown.

---

## Flow B - Child Creates Connection Code

1. Child opens KidGuard Windows app.
2. Child chooses Child Mode.
3. Child app requests a temporary connection code from Backend.
4. Backend creates a pending child device/session.
5. Child app displays:
   - Connection code
   - Password/PIN if required
   - Expiration time

---

## Flow C - Parent Requests Pairing

1. Parent enters child's connection code.
2. Backend validates the code.
3. Backend creates a pending pairing request.
4. Child app polls Backend and receives the pending request.
5. Child app shows a visible approve/reject dialog.

---

## Flow D - Child Approves

1. Child clicks Approve.
2. Backend marks pairing request as approved.
3. Backend binds the child device to the parent.
4. Backend issues device credentials.
5. Child app stores credentials securely.
6. Windows Service protection is enabled.
7. Parent can manage the device.

---

## Flow E - Protection

1. Parent changes mode.
2. Backend stores mode.
3. Windows Service syncs mode.
4. Windows Service applies process rules.
5. Blocked processes are logged locally.
6. Logs are uploaded to Backend.
7. Parent views logs.

---

# 6. Backend Changes

Required API work:

- Add Parent Register API.
- Keep Parent Login API.
- Add Child Connection Code API.
- Add Parent Pairing Request API.
- Add Child Pending Request Poll API.
- Add Child Approve Pairing API.
- Add Child Reject Pairing API.
- Adjust Device Pairing API around approved request state.
- Keep Mode Update API.
- Keep Mode Sync API.
- Keep Heartbeat API.
- Keep Log Upload API.
- Keep Device List and Log View APIs.

Backend must own:

- Account authentication.
- Pairing state.
- Device ownership.
- Device token generation.
- Mode storage.
- Logs and heartbeat storage.

---

# 7. Database Changes

Likely changes:

- Add parent registration support if missing.
- Extend or replace PairCodes with a clearer connection session model.
- Store connection code.
- Store optional connection password/PIN.
- Store pairing request status:
  - pending
  - approved
  - rejected
  - expired
- Store requester parent ID.
- Store child device ID.
- Store approval timestamp.

Existing tables to keep if possible:

- Users
- Devices
- Modes
- Heartbeats
- DeviceLogs

Tables to review:

- PairCodes

Possible new table:

- PairingRequests

---

# 8. Windows Changes

Current Windows Agent is a background Worker Service.

Version 1.0.1 needs a Windows Client plus Windows Service split.

## Windows Client

Responsibilities:

- Show role selection.
- Parent Mode UI.
- Child Mode UI.
- Show child connection code.
- Show child approval request.
- Save credentials after approval.
- Start or communicate with Windows Service.

Recommended approach:

- Create a new Windows desktop client project under `windows-agent` or a new `windows-client` folder.
- Keep the service code for process monitoring and blocking.
- Avoid putting UI inside the Windows Service.

## Windows Service

Responsibilities:

- Read approved credentials.
- Send heartbeat.
- Sync mode.
- Monitor processes.
- Block processes.
- Cache last known mode.
- Upload logs.
- Continue protection offline.

---

# 9. Mobile Changes

Mobile is not the first implementation target for 1.0.1.

For now:

- Keep existing Flutter app as a reference.
- Do not expand Mobile until Windows Parent Mode is stable.
- Later, update Mobile to use the same Parent APIs as Windows Parent Mode.

Future Mobile scope:

- Parent login/register.
- Enter child code.
- Manage approved devices.
- Change mode.
- View logs.

---

# 10. Task Plan

## Phase 1 - Documentation and Design

- [ ] Update ARCHITECTURE.md for Windows-first dual-role flow.
- [ ] Update API_SPEC.md for register and approval-based pairing.
- [ ] Update DATABASE.md for pairing request state.
- [ ] Update SECURITY.md for child approval and credential storage.
- [ ] Update TASKS.md to make 1.0.1 the active target.
- [ ] Add DECISION entry for Version 1.0.1 direction.

---

## Phase 2 - Backend API

- [ ] Add Register API.
- [ ] Add connection code creation API.
- [ ] Add parent pairing request API.
- [ ] Add child pending request API.
- [ ] Add approve pairing API.
- [ ] Add reject pairing API.
- [ ] Add pairing status API.
- [ ] Add or update EF migration.
- [ ] Update Swagger documentation.
- [ ] Add backend smoke test flow.

---

## Phase 3 - Windows Client

- [ ] Create Windows client project.
- [ ] Add role selection screen.
- [ ] Add Parent login screen.
- [ ] Add Parent register screen.
- [ ] Add Parent dashboard.
- [ ] Add Parent enter child code screen.
- [ ] Add Child code display screen.
- [ ] Add Child approval request screen.
- [ ] Store approved credentials securely.
- [ ] Connect client to Windows Service configuration.

---

## Phase 4 - Windows Service Integration

- [ ] Reuse existing process monitor.
- [ ] Reuse existing process blocker.
- [ ] Reuse existing local cache.
- [ ] Reuse existing heartbeat.
- [ ] Reuse existing mode sync.
- [ ] Reuse existing log upload.
- [ ] Start protection only after approval.
- [ ] Verify offline protection.

---

## Phase 5 - Full Windows End-to-End Demo

- [ ] Parent registers.
- [ ] Parent logs in.
- [ ] Child creates code.
- [ ] Parent enters code.
- [ ] Child receives request.
- [ ] Child approves.
- [ ] Parent sees device online.
- [ ] Parent changes mode to study.
- [ ] Child service blocks notepad.exe.
- [ ] Log uploads.
- [ ] Parent views log.
- [ ] Backend restart does not disable local protection.
- [ ] Pending logs upload after reconnect.

---

# 11. Completion Criteria

Version 1.0.1 is complete when:

- Windows Parent Mode works.
- Windows Child Mode works.
- Parent can register and login.
- Child can show a connection code.
- Parent can send pairing request.
- Child can approve or reject.
- Approved child device becomes manageable.
- Windows Service blocks demo process after approved pairing.
- Logs are visible to Parent.
- Offline protection still works.
- Backend, Windows Client, and Windows Service build successfully.
- Documentation matches the implemented behavior.

---

# 12. Implementation Order

Recommended order:

1. Documentation and API/database design.
2. Backend register and pairing request state.
3. Windows Child Mode code display and approval.
4. Windows Parent Mode login/register and pairing.
5. Windows Service credential handoff.
6. Mode control and process blocking.
7. Logs and offline protection.
8. Full end-to-end demo.
9. Mobile adaptation after Windows is stable.

---

# 13. Risk Notes

Important risks:

- UI and Windows Service should not be mixed into one process.
- Child approval must be explicit and visible.
- Device credentials must not be stored in plain text.
- Backend must not control Windows processes directly.
- Mobile should not be rewritten before Windows 1.0.1 is stable.
- Pairing state must handle expired and rejected requests cleanly.

---

# 14. Decision

Version 1.0.1 replaces the old Demo V1 execution priority.

The old 1.0.0 implementation remains useful as backend, mobile, and agent foundation code, but the active product direction is now:

Windows-first Parent/Child role selection with approval-based pairing.
