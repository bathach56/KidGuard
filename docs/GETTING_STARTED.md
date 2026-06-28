# GETTING_STARTED.md

# Parental Control System Getting Started

Version: 1.0.0

---

# Purpose

This document helps team members start the project quickly and consistently.

Read this file before creating code for Demo V1.

---

# Required Tools

Backend

- .NET SDK
- SQL Server 2022
- Entity Framework Core tools
- Visual Studio or Visual Studio Code

Mobile

- Flutter SDK
- Android Studio or emulator
- A physical Android device is optional

Windows Agent

- .NET SDK
- Visual Studio or Visual Studio Code
- Windows machine
- Administrator permission for Windows Service testing

---

# Read First

Before implementation, read these documents in order:

1. AGENTS.md
2. PROJECT_RULES.md
3. ARCHITECTURE.md
4. API_SPEC.md
5. DATABASE.md
6. SECURITY.md
7. DECISIONS.md
8. TASKS.md
9. PROGRESS.md

Windows Agent developers should also read:

- AGENT_DESIGN.md

Backend and Mobile developers should also read:

- BACKEND_MOBILE_DESIGN.md

---

# Recommended Build Order

Do not build everything at once. Follow this order.

1. Backend project foundation
2. Database schema and migrations
3. Login API
4. Pair Code API
5. Device Pair API
6. Mode API
7. Heartbeat API
8. Log Upload API
9. Windows Agent pairing
10. Windows Agent heartbeat
11. Windows Agent mode sync
12. Windows Agent process blocking
13. Mobile login
14. Mobile device list
15. Mobile mode switch
16. End-to-end demo

---

# Local Development Flow

Start Backend first.

Backend owns:

- API behavior
- Database access
- Authentication
- Device ownership validation

Start Windows Agent second.

Windows Agent consumes:

- Pair Code API
- Mode API
- Heartbeat API
- Logs API

Start Mobile third.

Mobile consumes:

- Login API
- Device API
- Pair API
- Mode API
- Logs API

---

# Demo V1 Rule

Demo V1 should prove the full flow, not every future feature.

The minimum successful demo is:

1. Parent logs in.
2. Agent creates pair code.
3. Parent pairs device.
4. Agent sends heartbeat.
5. Parent changes mode.
6. Agent receives mode.
7. Agent blocks a demo process.
8. Agent uploads log.
9. Mobile displays device status or log.
10. Agent keeps protecting the device while offline.

---

# Demo Test Process

Use a simple process for the first demo.

Recommended:

- notepad.exe

Reason:

- Available on Windows by default
- Easy to open
- Easy to verify when blocked
- Safer than testing with large third-party applications

Do not use system-critical processes for testing.

---

# Common Mistakes To Avoid

- Do not let Mobile talk directly to Windows Agent.
- Do not let Windows Agent connect directly to SQL Server.
- Do not hardcode API URLs, JWT, Device Token, or secrets.
- Do not add new modes outside `fun`, `study`, and `punishment`.
- Do not build future features before Demo V1 is stable.
- Do not change API response format without updating API_SPEC.md.

---

# Before Asking For Code Help

Prepare these answers:

- Which team member are you?
- Which module are you working on?
- Which task in TASKS.md are you doing?
- Which branch are you on?
- Which API endpoint or file is involved?

This keeps AI assistance and teammate reviews focused.
