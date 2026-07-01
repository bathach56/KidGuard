# TASKS.md

# Parental Control System

Version: 1.0.1

---

# Active Target

Version

1.0.1

Goal

Build a Windows-first Parent/Child KidGuard application with approval-based pairing.

Plan

docs/VERSION_1_0_1_PLAN.md

Priority

Version 1.0.1 tasks have priority over the old 1.0.0 Demo V1 checklist.

---

# Version 1.0.1 Task Assignment

## Admin Tasks

Owner

Admin

Purpose

Keep Version 1.0.1 aligned across architecture, backend, Windows client, Windows service, mobile, and documentation.

Admin is the only role allowed to edit documentation files and AGENTS.md.

### Admin - Planning And Documentation

- [x] Create Version 1.0.1 redesign plan
  Priority: High
  Output: docs/VERSION_1_0_1_PLAN.md

- [x] Update ROADMAP.md to make Version 1.0.1 the active direction
  Priority: High
  Output: docs/ROADMAP.md

- [x] Add official architecture decision for Version 1.0.1
  Priority: High
  Output: docs/DECISIONS.md

- [x] Update TASKS.md with Version 1.0.1 task assignments
  Priority: High
  Output: docs/TASKS.md

- [ ] Update ARCHITECTURE.md for Windows-first Parent/Child flow
  Priority: High
  Output: updated architecture diagrams and module boundaries

- [ ] Update API_SPEC.md for approval-based pairing APIs
  Priority: High
  Output: request/response contracts for register, code creation, pairing request, approve, reject, and status

- [ ] Update DATABASE.md for pairing request state
  Priority: High
  Output: table/entity design for connection codes and pairing requests

- [ ] Update SECURITY.md for child approval, token storage, and service activation
  Priority: High
  Output: security rules for explicit child consent and protected credentials

- [ ] Review cross-module changes before implementation starts
  Priority: High
  Output: confirmed implementation order and no conflicting contracts

### Admin - Integration And Release

- [ ] Coordinate Backend and Windows integration checkpoints
  Priority: High
  Output: agreed API behavior before Windows client implementation

- [ ] Maintain full end-to-end demo checklist
  Priority: High
  Output: demo-ready checklist with pass/fail status

- [ ] Verify final Version 1.0.1 build before release
  Priority: High
  Output: Backend build, Windows Client build, Windows Service build, and smoke test result

---

## Trần Phúc Thịnh Tasks

Owner

Trần Phúc Thịnh

Primary Responsibility

Backend, Database, API, Swagger, and Mobile adaptation after Windows is stable.

Restrictions

- Must not edit protected documentation files.
- Must not edit AGENTS.md.
- Must not edit Windows Agent implementation files.
- May update TASKS.md only for own assigned task status and notes.
- May update PROGRESS.md only for own progress entries.
- May update IDEAS.md only to record confirmed ideas.

### Thịnh - Backend And Database

- [x] Add Parent Register API
  Priority: High
  Details: Parent can create an account with email, password, full name, and optional phone number.

- [x] Keep and verify Parent Login API
  Priority: High
  Details: Login must return JWT and work for both Windows Parent Client and future Mobile Parent App.

- [x] Design PairingRequests database model
  Priority: High
  Details: Store requester parent, child device/session, code, status, expiration, approval time, rejection time, and created time.

- [ ] Add EF migration for Version 1.0.1 pairing flow
  Priority: High
  Details: Add new table or update PairCodes without breaking existing device/mode/log tables.

- [ ] Add Child Connection Code API
  Priority: High
  Details: Child client requests a temporary code without parent login.

- [ ] Add Parent Pairing Request API
  Priority: High
  Details: Parent enters child code and creates a pending request.

- [ ] Add Child Pending Request Poll API
  Priority: High
  Details: Child client polls Backend and receives pending parent request information.

- [ ] Add Child Approve Pairing API
  Priority: High
  Details: Child approves request, Backend binds device to parent and issues device credentials.

- [ ] Add Child Reject Pairing API
  Priority: High
  Details: Child rejects request and Backend marks request as rejected.

- [ ] Add Pairing Status API
  Priority: High
  Details: Parent can see pending, approved, rejected, or expired status.

- [ ] Update Device List API for approved devices
  Priority: Medium
  Details: Parent should only see approved and owned devices.

- [ ] Keep Mode Update API compatible with approved devices
  Priority: High
  Details: Parent can change mode only for approved owned device.

- [ ] Keep Mode Sync, Heartbeat, and Log Upload APIs compatible with Windows Service
  Priority: High
  Details: Windows Service continues to use Device Token after approval.

- [ ] Update Swagger for all Version 1.0.1 endpoints
  Priority: High
  Details: Include auth type, request body, response body, and error cases.

- [ ] Add backend smoke test for approval-based pairing
  Priority: High
  Details: Register, login, child code, parent request, child approve, heartbeat, mode, log.

### Thịnh - Mobile

- [ ] Freeze current Mobile app during Windows-first implementation
  Priority: High
  Details: Do not expand Mobile until Windows 1.0.1 flow is stable.

- [ ] Keep Mobile API client ready for future Parent APIs
  Priority: Low
  Details: Later reuse register, login, pairing request, device list, mode, and logs.

- [ ] Plan Mobile adaptation after Windows Parent Client works
  Priority: Low
  Details: Mobile becomes a second Parent client, not the first target for 1.0.1.

---

## Phạm Bá Thạch Tasks

Owner

Phạm Bá Thạch

Primary Responsibility

Windows Parent Client, Windows Child Client, Windows Service, process protection, local cache, credential storage, and real Windows demo testing.

Restrictions

- Must not edit protected documentation files.
- Must not edit AGENTS.md.
- Must not edit Backend or Mobile implementation files.
- May update TASKS.md only for own assigned task status and notes.
- May update PROGRESS.md only for own progress entries.
- May update IDEAS.md only to record confirmed ideas.

### Thạch - Windows Client

- [x] Decide Windows client technology
  Priority: High
  Details: Chosen WPF for the Windows-first demo because it works with the existing .NET Windows stack without extra UI dependencies.

- [x] Create Windows client project
  Priority: High
  Details: Added KidGuard.Client as a WPF UI app separate from the Windows Service.

- [x] Add role selection screen
  Priority: High
  Details: User can choose Parent or Child when opening KidGuard.

- [x] Add Parent login screen
  Priority: High
  Details: Parent login screen calls Backend POST /auth/login and stores the returned JWT in memory for the current client session.

- [ ] Add Parent register screen
  Priority: High
  Details: Parent can create account from Windows app.

- [ ] Add Parent dashboard
  Priority: High
  Details: Basic approved device list and status are implemented through GET /devices after login. Pending requests, mode controls, and log entry points still need the Version 1.0.1 flow.

- [ ] Add Parent enter child code screen
  Priority: High
  Details: Bridge implemented with Demo V1 POST /devices/pair so Parent Windows Client can pair using a child code after login. Final Version 1.0.1 behavior still needs the pending pairing request API.

- [x] Add Parent pairing status view
  Priority: Medium
  Details: Shows not started, waiting, paired, and failed states for the current Demo V1 bridge, including paired device summary and one-time Device Token copy action. Version 1.0.1 pending, approved, rejected, and expired states still depend on Backend pairing status APIs.

- [x] Add Child code display screen
  Priority: High
  Details: Child can create and display a temporary code through the existing POST /pair-code endpoint while waiting for the Version 1.0.1 child connection API.

- [ ] Add Child pending request polling
  Priority: High
  Details: Child client asks Backend for pending parent request.

- [ ] Add Child approval request dialog
  Priority: High
  Details: Child sees parent request and can approve or reject.

- [ ] Save approved device credentials securely
  Priority: High
  Details: Reuse protected credential storage; never store Device Token in plain text.

- [ ] Add Windows client error states
  Priority: Medium
  Details: Show Backend offline, invalid code, expired code, rejected request, and unauthorized states.

### Thạch - Windows Service

- [ ] Keep Windows Service separate from Windows UI
  Priority: High
  Details: UI handles interaction; service handles protection.

- [ ] Connect approved credentials from Windows Client to Windows Service
  Priority: High
  Details: Service starts normal heartbeat/mode/log flow only after approval.

- [ ] Reuse ProcessMonitorService
  Priority: High
  Details: Continue scanning running processes.

- [ ] Reuse ProcessBlockerService
  Priority: High
  Details: Continue blocking demo process such as notepad.exe.

- [ ] Reuse LocalCacheService
  Priority: High
  Details: Continue using last known mode offline.

- [ ] Reuse BackendApiClient where possible
  Priority: Medium
  Details: Extend client only where new 1.0.1 endpoints are required.

- [ ] Improve local logging for demo troubleshooting
  Priority: Medium
  Details: Make pairing, approval, heartbeat, mode sync, block, and upload events easy to inspect.

- [ ] Verify service runs with Administrator permission
  Priority: High
  Details: Process blocking must work in real Windows test.

- [ ] Verify offline protection
  Priority: High
  Details: Stop Backend and confirm protection continues with cached mode.

### Thạch - Real Windows Testing

- [ ] Run Child Mode on Windows
  Priority: High
  Details: Confirm code appears.

- [ ] Run Parent Mode on Windows
  Priority: High
  Details: Confirm register/login and code entry work.

- [ ] Approve pairing on Child machine
  Priority: High
  Details: Confirm Parent receives approved state.

- [ ] Change mode from Parent Windows Client
  Priority: High
  Details: Confirm Backend stores mode and service syncs it.

- [ ] Block notepad.exe in study mode
  Priority: High
  Details: Confirm actual process is blocked.

- [ ] Confirm logs upload and display
  Priority: High
  Details: Parent can see blocked process log.

- [ ] Run full offline/reconnect test
  Priority: Medium
  Details: Backend offline should not disable protection; pending logs upload after reconnect.

---

## Shared Version 1.0.1 Milestones

- [ ] Milestone 1: Documentation and contracts approved
- [ ] Milestone 2: Backend 1.0.1 APIs compile and pass smoke test
- [ ] Milestone 3: Windows Client role selection, Parent login/register, and Child code screen work
- [ ] Milestone 4: Approval-based pairing works end-to-end
- [ ] Milestone 5: Windows Service protects only after approval
- [ ] Milestone 6: Full local demo passes
- [ ] Milestone 7: Mobile adaptation can begin

---

# Purpose

This document tracks all project tasks.

Every completed task must be checked.

Only work on tasks assigned to your role unless discussed with the team.

---

# Sprint Overview

Sprint Duration

2 Weeks

Goal

Deliver Demo V1.

Definition

Parent can control a Windows computer remotely using a mobile application.

---

# MVP Priority

The following tasks are required for Demo V1. Do these first.

Backend

- [x] Login API
- [x] Pair Code API
- [x] Device Pair API
- [x] Device List API
- [x] Mode Update API
- [x] Mode Sync API
- [x] Heartbeat API
- [x] Log Upload API

Mobile

- [x] Login Screen
- [x] Device List
- [x] Device Detail
- [x] Mode Switch
- [x] Basic Log View

Windows Agent

- [x] Create Windows Agent project
- [x] Create pair code request
- [x] Store Device Token securely
- [x] Send heartbeat
- [x] Sync current mode
- [x] Cache last known mode
- [x] Monitor processes
- [x] Block demo process
- [x] Upload logs
- [x] Continue protection while offline

Anything outside this list must not delay Demo V1.

---

# Sprint 1

## Goal

Build project foundation.

---

## Member 1

Name

Tráº§n PhÃºc Thá»‹nh

Role

Backend + Mobile

---

## Backend Tasks

- [x] Create ASP.NET Core Solution
  Priority: High

- [x] Configure SQL Server
  Priority: High

- [x] Create Entity Framework Core setup
  Priority: High

- [x] Create Database Migration
  Priority: High

- [x] Create Authentication with JWT
  Priority: High

- [x] Create Login API
  Priority: High

- [x] Create Pair Code API
  Priority: High

- [x] Create Device API
  Priority: High

- [x] Create Pair API
  Priority: High

- [x] Create Mode API
  Priority: High

- [x] Create Heartbeat API
  Priority: High

- [x] Create Logs API
  Priority: High

- [x] Create Swagger Documentation
  Priority: Medium

---

## Mobile Tasks

- [x] Create Flutter Project
  Priority: High

- [x] Create Login Screen
  Priority: High

- [x] Create Dashboard
  Priority: Medium

- [x] Create Device List
  Priority: High

- [x] Create Device Detail
  Priority: High

- [x] Create Mode Switch
  Priority: High

- [x] Create API Integration
  Priority: High

- [x] Create Basic Log View
  Priority: Medium

---

## Member 1 Deliverables

- Backend running
- Swagger working
- Database connected
- Flutter can login
- Flutter can pair a device
- Flutter can change mode

---

# Member 2

Name

Pháº¡m BÃ¡ Tháº¡ch

Role

Windows Agent

---

## Windows Agent Tasks

- [x] Create Solution
  Priority: High

- [x] Create Worker Service
  Priority: High

- [x] Configure Windows Service hosting
  Priority: High

- [x] Implement Device Registration / Pair Code request
  Priority: High

- [x] Store Device Token securely
  Priority: High

- [x] Implement Heartbeat Service
  Priority: High

- [x] Implement Mode Synchronization
  Priority: High

- [x] Implement Local Cache
  Priority: High

- [x] Implement Retry Mechanism
  Priority: Medium

- [x] Implement Process Monitor
  Priority: High

- [x] Implement Process Blocker
  Priority: High

- [x] Add protected system process list
  Priority: High

- [x] Implement Local Logging
  Priority: Medium

- [x] Implement Log Upload
  Priority: High

- [x] Implement Offline Protection
  Priority: High

---

## Member 2 Deliverables

- Agent installed
- Pair code created
- Device Token stored
- Heartbeat working
- Mode received
- Demo process blocked
- Offline cache working
- Logs uploaded after reconnect

---

# Integration Tasks

Owner

Both

- [ ] Mobile Login -> Backend
- [ ] Backend -> Database
- [ ] Agent -> Pair Code -> Backend
- [x] Mobile -> Pair Device -> Backend
- [ ] Agent -> Heartbeat -> Backend
- [ ] Agent -> Receive Mode -> Apply Mode
- [ ] Agent -> Upload Logs -> Backend
- [ ] Mobile -> View Device Status -> Backend

---

# Sprint 2

Goal

Complete Demo.

Backend

- [ ] Improve Validation
- [ ] Improve Error Handling
- [ ] Optimize Database

Mobile

- [ ] Device Status
- [ ] Log Screen
- [ ] Loading State
- [ ] Error Dialog

Windows Agent

- [ ] Improve Retry
- [ ] Better Logging
- [ ] Performance Optimization

Integration

- [ ] Full End-to-End Test
- [ ] Bug Fixes

---

# Dependencies

Backend must finish before

- Mobile API Integration
- Agent API Integration

Agent depends on

- API_SPEC.md
- Backend implementation
- AGENT_DESIGN.md

Mobile depends on

- Backend API
- Authentication
- API_SPEC.md
- BACKEND_MOBILE_DESIGN.md

---

# Definition of Done

A task is complete only if

- [ ] Code compiles
- [ ] No warnings
- [ ] Feature tested
- [ ] Documentation updated
- [ ] Committed to Git
- [ ] Pull Request created
- [ ] Reviewed
- [ ] Merged into develop

---

# Daily Checklist

Every developer should

- [ ] Pull latest develop
- [ ] Create feature branch
- [ ] Update task status
- [ ] Update PROGRESS.md
- [ ] Commit frequently
- [ ] Push before ending work

---

# AI Instructions

When a developer asks for help, AI must:

1. Identify developer.
2. Read AGENTS.md.
3. Read PROJECT_RULES.md.
4. Stay inside assigned module.
5. Never modify another module without explicit permission.

---

# Demo V1 Success

Demo is complete when

- [ ] Parent Login
- [ ] Device Pair
- [ ] Device Online
- [ ] Heartbeat Working
- [ ] Change Mode
- [ ] Agent Receives Mode
- [ ] Agent Applies Mode
- [ ] Process Blocking Works
- [ ] Logs Uploaded
- [ ] Mobile Displays Device Status
- [ ] Agent Continues Protection Offline

---

# Future Tasks

These features are not part of Demo V1 and must not delay Demo V1.

- Website Blocking
- Screen Time Schedule
- Notifications
- Reports
- Child Profile
- Multi-language
- Auto Update
- AI Recommendation
- Remote Desktop
- Multi-platform Agent
