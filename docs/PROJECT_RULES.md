# PROJECT_RULES.md

# Parental Control System

Version: 1.0.0

---

# 1. Project Vision

## Goal

Build a parental control system that allows parents to manage and monitor
their children's Windows computers remotely through a mobile application.

The first version (Demo V1) focuses on simplicity, stability and a clean architecture.

---

# 2. Current Scope

Current modules

- Backend API
- Mobile Application (Parent)
- Windows Agent (Child)
- SQL Server Database

Out of scope

- Browser Extension
- AI Recommendation
- Remote Desktop
- Screen Streaming
- Linux Support
- macOS Support

---

# 3. Core Features

### Parent

- Login
- Pair Device
- View Device
- Change Mode
- View Logs

### Child

- Receive Mode
- Monitor Running Processes
- Block Applications
- Send Heartbeat
- Send Activity Logs
- Cache Rules Offline

---

# 4. Architecture

```
Parent Mobile
        │
        │ HTTPS
        ▼
ASP.NET Core API
        │
        ▼
SQL Server
        │
        ▼
Windows Agent
        │
        ▼
Windows Process Manager
```

No module may bypass the Backend API.

Mobile NEVER communicates directly with Windows Agent.

---

# 5. Team Responsibility

## Member 1

Name

Trần Phúc Thịnh

Responsible

- Backend
- Database
- Mobile
- Authentication
- API
- Swagger
- Entity Framework

Cannot modify

- Windows Agent logic
- Protected documentation files
- AGENTS.md

Can modify limited tracking documentation

- TASKS.md only for own assigned task status and notes
- PROGRESS.md only for own progress entries
- IDEAS.md only to record confirmed ideas

---

## Member 2

Name

Phạm Bá Thạch

Responsible

- Windows Agent
- Windows Service
- Process Monitor
- Process Blocker
- Local Cache
- Windows Communication

Cannot modify

- Backend Architecture
- Database Design
- Mobile UI
- Protected documentation files
- AGENTS.md

Can modify limited tracking documentation

- TASKS.md only for own assigned task status and notes
- PROGRESS.md only for own progress entries
- IDEAS.md only to record confirmed ideas

---

## Admin

Name

Admin

Responsible

- Entire repository
- Backend
- Database
- Mobile
- Windows Agent
- Documentation
- Configuration
- Testing
- Integration
- Emergency fixes
- Security fixes
- Project maintenance

Authority

Admin is the highest permission level in this repository.

Admin may customize every part of the project when needed.

Admin may edit all modules and all project files.

Admin permission overrides normal Member 1 and Member 2 module ownership restrictions.

Only verified Admin may edit AGENTS.md and protected documentation files.

Protected documentation files include:

- PROJECT_RULES.md
- ARCHITECTURE.md
- API_SPEC.md
- DATABASE.md
- SECURITY.md
- DECISIONS.md
- ROADMAP.md
- VERSION_*_PLAN.md

Member 1 and Member 2 may directly edit limited tracking documentation:

- TASKS.md only for tasks assigned to their own role
- PROGRESS.md only for their own daily progress entries
- IDEAS.md only to record confirmed ideas

Member 1 and Member 2 must not directly edit protected documentation files or AGENTS.md.

Cannot modify

- None, inside this repository.

Admin still must keep changes understandable, documented, tested when reasonable, and committed with the project commit convention.

Admin Access Control

Admin access requires an owner secret.

The owner secret must never be stored in plain text in repository files.

Admin secret verification:

- Normalize the submitted secret with Unicode NFC.
- Trim leading and trailing whitespace.
- Compute SHA-256 over UTF-8 bytes.
- Compare with AdminSecretHashV1.

AdminSecretHashV1

7eb8e83523f68d22abeae897861cbb0492e370e8e594f782bc1165cc8dc32b47

Knowing that Admin exists is not enough.

Reading this file or copying the hash is not enough.

Only a user who provides a matching owner secret may use Admin authority.

---

# 6. Development Principles

Always

- Build small features
- Commit frequently
- Test before merge
- Update documentation

Never

- Push directly to main
- Skip code review
- Change API without updating documentation
- Break another member's work

---

# 7. Development Workflow

Idea

↓

Discussion

↓

Documentation

↓

Implementation

↓

Testing

↓

Pull Request

↓

Merge

Documentation always comes before implementation.

---

# 8. Coding Principles

Follow SOLID whenever reasonable.

Prefer composition over inheritance.

Avoid duplicated code.

Avoid hardcoded values.

Use Dependency Injection.

Separate Business Logic from UI.

Never put business logic inside Flutter Widgets.

Never put Windows logic inside Backend.

---

# 9. Naming Convention

Classes

PascalCase

Example

DeviceService

Variables

camelCase

Example

deviceId

Methods

Verb + Object

Example

GetDevice()

UpdateMode()

BlockProcess()

Files

PascalCase

Database Tables

PascalCase

Users

Devices

Logs

Columns

camelCase

userId

deviceId

createdAt

---

# 10. Date & Time

Use UTC only.

Never save local time into database.

Frontend converts UTC to local timezone.

---

# 11. Authentication

Parent

JWT

Windows Agent

Device Token

Every request must be authenticated.

---

# 12. Device Lifecycle

Create Device

↓

Generate Pair Code

↓

Parent Pair Device

↓

Generate Device Token

↓

Windows Agent Connected

↓

Heartbeat

↓

Receive Commands

---

# 13. Modes

Only three modes exist.

fun

study

punishment

These values are immutable.

Do not translate them.

Do not rename them.

Database must store these exact values.

API must return these exact values.

---

# 14. Mode Behavior

## fun

Purpose

Relaxation

Rules

No application blocking.

Logs are still recorded.

---

## study

Purpose

Study only.

Rules

Block distracting applications.

Examples

- steam.exe
- discord.exe
- riotclientservices.exe

Allowed applications are configurable.

---

## punishment

Purpose

Strict restriction.

Rules

Allow only approved applications.

Default examples

- chrome.exe
- browser.exe (Cốc Cốc)
- explorer.exe

Never terminate Windows system processes.

---

# 15. Offline Behavior

If internet connection is lost

Windows Agent

- Keep last synced mode
- Continue protection
- Retry connection automatically
- Upload cached logs after reconnecting

Protection must never stop because the server is unavailable.

---

# 16. Logging

Every important event should be logged.

Examples

Login

Device Pair

Mode Changed

Heartbeat Lost

Blocked Process

Unexpected Exception

---

# 17. Error Handling

Never ignore exceptions.

Return meaningful error messages.

Write server logs.

Write Windows Agent logs.

---

# 18. Documentation Policy

Every feature affecting

API

Database

Architecture

Security

must update its corresponding documentation before merge.

---

# 19. Git Policy

Branches

main

develop

feature/backend/*

feature/mobile/*

feature/agent/*

fix/*

hotfix/*

No direct commits to main.

---

# 20. Pull Request Checklist

Before merging

- Code compiles
- No warnings
- Feature tested
- Documentation updated
- Branch up to date
- No merge conflicts

---

# 21. AI Collaboration Rules

AI must identify the developer first.

Allowed developers

- Trần Phúc Thịnh
- Phạm Bá Thạch
- Admin

AI only assists within that developer's responsibility.

If the developer is Admin, the responsibility is the entire repository.

AI may treat a user as Admin only after the user provides a submitted owner secret matching AdminSecretHashV1.

If the Admin secret is missing or invalid, AI must not grant Admin authority.

If the developer is Trần Phúc Thịnh or Phạm Bá Thạch, AI may edit only limited tracking documentation:

- TASKS.md for that developer's assigned task status and notes
- PROGRESS.md for that developer's own progress entries
- IDEAS.md for confirmed idea records

Protected documentation and AGENTS.md edits require verified Admin authority.

If an answer requires changing another module

↓

Explain the impact

↓

Do not generate code outside the current developer's scope.

This restriction does not apply when the current developer is Admin.

---

# 22. Documentation Priority

The following files are the project's source of truth.

1. AGENTS.md
2. PROJECT_RULES.md
3. ARCHITECTURE.md
4. API_SPEC.md
5. DATABASE.md
6. SECURITY.md
7. DECISIONS.md

If there is any conflict,

these documents override assumptions.

---

# 23. Long-term Goal

Future versions may include

- Website Blocking
- Time Scheduling
- Notifications
- Usage Statistics
- AI Suggestions
- Cross-platform Support

These features are NOT part of Demo V1.

Focus on delivering a stable MVP first.

---

# 24. Getting Started Documents

Before implementation, developers should use these practical documents:

- GETTING_STARTED.md
- DEMO_FLOW.md
- TASKS.md
- PROGRESS.md

Windows Agent development should also follow:

- AGENT_DESIGN.md

Backend and Mobile development should also follow:

- BACKEND_MOBILE_DESIGN.md

These documents do not replace the source-of-truth documents. They explain how to execute Demo V1 in a clear order.

PROGRESS.md should be updated at the end of each working day to record actual progress.

---

# 25. New Idea Confirmation

When a developer mentions a new idea, AI must not immediately change project plans.

AI must first ask whether the idea should be:

- Recorded only
- Added to Demo V1
- Saved for a future version
- Rejected

If the idea affects API, database, architecture, security, modes, pairing flow, heartbeat flow, or log format, both team members must explicitly agree before AI updates implementation documents or TASKS.md.

AI may update IDEAS.md only after the developer confirms that the idea should be recorded.

AI may help implement the idea only after:

1. Team approval is confirmed.
2. Related documentation is updated.
3. TASKS.md contains the approved task.
