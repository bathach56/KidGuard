# DEMO_FLOW.md

# Parental Control System Demo Flow

Version: 1.0.0

---

# Purpose

This document defines the expected Project 1 demo flow.

Use this checklist to decide whether Demo V1 is ready.

---

# Demo Goal

Show that a parent can remotely control a Windows computer from a mobile application through the Backend API.

---

# Demo Preparation

Prepare these before presenting:

- Backend running
- SQL Server connected
- Mobile app installed or running in emulator
- Windows Agent running on a Windows machine
- Demo process available, recommended: `notepad.exe`
- Test parent account created
- API base URL configured through environment or config file

---

# Demo Steps

## Step 1 - Parent Login

Action

Parent logs in from Mobile.

Expected Result

- Backend returns JWT.
- Mobile stores JWT securely.
- Mobile navigates to device list or dashboard.

---

## Step 2 - Agent Creates Pair Code

Action

Windows Agent calls `POST /pair-code`.

Expected Result

- Backend returns an 8-character pair code.
- Pair code expires after 10 minutes.
- Pair code can only be used once.

---

## Step 3 - Parent Pairs Device

Action

Parent enters pair code in Mobile.

Expected Result

- Backend binds device to parent.
- Backend returns device information.
- Device becomes visible in Mobile.
- Device Token is issued for Agent communication.

---

## Step 4 - Agent Sends Heartbeat

Action

Windows Agent calls `POST /devices/{deviceId}/heartbeat`.

Expected Result

- Backend updates `lastSeen`.
- Device status becomes online.
- Mobile can display device online status.

---

## Step 5 - Parent Changes Mode

Action

Parent changes mode from Mobile.

Recommended Demo Mode

`study`

Expected Result

- Backend saves the new mode.
- API returns updated mode.
- Mobile displays the selected mode.

---

## Step 6 - Agent Syncs Mode

Action

Windows Agent calls `GET /devices/{deviceId}/mode`.

Expected Result

- Agent receives the current mode.
- Agent saves mode to local cache.
- Agent applies mode rules.

---

## Step 7 - Agent Blocks Demo Process

Action

Open `notepad.exe` while the mode requires blocking.

Expected Result

- Agent detects the process.
- Agent blocks or terminates the process.
- Agent writes a local log.

---

## Step 8 - Agent Uploads Log

Action

Windows Agent calls `POST /devices/{deviceId}/logs`.

Expected Result

- Backend stores the log.
- Parent can view the log from Mobile or API.

---

## Step 9 - Offline Protection Test

Action

Disconnect the Windows machine from the internet.

Expected Result

- Agent keeps last known mode.
- Agent continues blocking according to cached rules.
- Agent stores logs locally.
- Agent does not disable protection.

---

## Step 10 - Reconnect Test

Action

Reconnect the Windows machine to the internet.

Expected Result

- Agent reconnects automatically.
- Agent uploads cached logs.
- Agent downloads latest configuration.
- Backend receives new heartbeat.

---

# Demo Success Checklist

- [ ] Parent can login.
- [ ] Agent can create pair code.
- [ ] Parent can pair device.
- [ ] Device appears in Mobile.
- [ ] Agent sends heartbeat.
- [ ] Parent can change mode.
- [ ] Agent receives mode.
- [ ] Agent applies mode.
- [ ] Agent blocks demo process.
- [ ] Agent uploads logs.
- [ ] Offline protection works.
- [ ] Cached logs upload after reconnect.

---

# Demo Failure Rules

If one step fails:

1. Stop at the failed step.
2. Record expected result.
3. Record actual result.
4. Check the related document:
   - API_SPEC.md for API mismatch
   - DATABASE.md for schema mismatch
   - AGENT_DESIGN.md for Agent behavior
   - SECURITY.md for authentication issues
5. Fix the smallest possible issue first.

Do not skip failed steps during final testing.
