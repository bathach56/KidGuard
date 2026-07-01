# DECISIONS.md

# Parental Control System

Version: 1.0.0

---

# Purpose

This document records all important technical and architectural decisions.

It is the single source of truth for "WHY" a decision was made.

Do NOT delete old decisions.

If a decision changes, add a new record instead of modifying history.

---

# Decision Format

## Decision ID

DEC-XXXX

## Date

YYYY-MM-DD

## Status

- Proposed
- Accepted
- Deprecated
- Replaced

## Category

- Architecture
- Backend
- Mobile
- Windows Agent
- Database
- Security
- API
- Git

## Decision

...

## Reason

...

## Impact

...

## Alternatives Considered

...

---

# Decisions

---

## DEC-0001

Date

2026-06-28

Status

Accepted

Category

Architecture

Decision

The project will consist of four modules:

- Mobile App
- Backend API
- SQL Server
- Windows Agent

Reason

Clear separation of responsibilities and easier maintenance.

Impact

Every new feature must fit this architecture.

---

## DEC-0002

Date

2026-06-28

Status

Accepted

Category

Windows Agent

Decision

The child device will use a Windows Agent instead of a browser extension.

Reason

Browser extensions cannot control Windows processes.

Impact

All Windows-related control belongs to Windows Agent.

---

## DEC-0003

Date

2026-06-28

Status

Accepted

Category

Communication

Decision

All communication goes through Backend API.

Reason

Security and centralized business logic.

Impact

Mobile never communicates directly with Windows Agent.

Windows Agent never communicates directly with Mobile.

---

## DEC-0004

Date

2026-06-28

Status

Accepted

Category

Database

Decision

Only Backend may access SQL Server.

Reason

Prevent duplicated business logic.

Impact

Windows Agent communicates through REST API only.

---

## DEC-0005

Date

2026-06-28

Status

Accepted

Category

Authentication

Decision

Parent uses JWT.

Windows Agent uses Device Token.

Reason

Separate authentication models for users and devices.

Impact

API authorization differs between Parent and Agent endpoints.

---

## DEC-0006

Date

2026-06-28

Status

Accepted

Category

Modes

Decision

System supports exactly three modes in Demo V1.

- fun
- study
- punishment

Reason

Keep MVP simple.

Impact

No additional modes without updating documentation.

---

## DEC-0007

Date

2026-06-28

Status

Accepted

Category

Offline

Decision

If internet connection is lost,

Windows Agent continues using the last synchronized configuration.

Reason

Protection should continue even without internet.

Impact

Agent must implement local cache.

---

## DEC-0008

Date

2026-06-28

Status

Accepted

Category

Team

Decision

Module ownership is fixed.

Owner

Backend + Mobile

Trần Phúc Thịnh

Owner

Windows Agent

Phạm Bá Thạch

Reason

Reduce merge conflicts and overlapping work.

Impact

Cross-module changes require discussion.

---

## DEC-0009

Date

2026-06-28

Status

Accepted

Category

Git

Decision

Git Flow will be used.

main

↓

develop

↓

feature/*

Reason

Safer collaboration.

Impact

No direct commits to main.

---

## DEC-0010

Date

2026-06-28

Status

Accepted

Category

Documentation

Decision

Documentation must be updated before implementing breaking changes.

Reason

Documentation is the project's source of truth.

Impact

Architecture, API and Database changes require document updates first.

---

## DEC-0011

Date

2026-06-28

Status

Accepted

Category

Windows Agent

Decision

Demo V1 will use polling for Windows Agent mode synchronization.

Reason

Polling is simpler to implement, easier to debug, and sufficient for the first project demo.

Impact

Windows Agent will call the mode API every 5 to 10 seconds instead of using SignalR or WebSocket.

Alternatives Considered

- SignalR
- WebSocket
- Push notification from Backend to Agent

These alternatives may be reconsidered after Demo V1 is stable.

---

## DEC-0012

Date

2026-06-28

Status

Accepted

Category

Security

Decision

Windows Agent will use a Setup Token only for creating a pair code before the device is paired.

Reason

The Agent does not have a Device Token before pairing, but Backend still needs to prevent unauthorized pair-code creation.

Impact

The Setup Token can only call `POST /pair-code`.

It must not access device mode, heartbeat, logs, parent data, or database data.

After pairing, Windows Agent must use Device Token for all normal communication.

Alternatives Considered

- Anonymous pair-code creation
- Hardcoded Device Token
- Manual database registration

These alternatives are rejected for Demo V1 because they weaken security or break project architecture.

---

## DEC-0013

Date

2026-07-01

Status

Accepted

Category

Team

Decision

The project adds an Admin role as the highest permission level in the repository.

Admin may edit every module and every project file when needed.

Admin permission overrides the normal module ownership restrictions for:

- Trần Phúc Thịnh
- Phạm Bá Thạch

Reason

The team needs a highest-level project role that can quickly fix bugs, security issues, integration problems, demo blockers, documentation issues, and cross-module problems without waiting for separate module permissions.

Impact

When the current developer is Admin, AI may assist across the entire repository:

- Backend
- Database
- Mobile
- Windows Agent
- Documentation
- Configuration
- Tests
- Integration scripts

Admin changes must still be understandable, documented when needed, tested when reasonable, and committed with the project commit convention.

Alternatives Considered

- Keep only separate module owners.
- Require explicit permission every time a cross-module bug appears.

These alternatives were rejected because they slow down urgent integration and demo fixes.

---

## DEC-0014

Date

2026-07-01

Status

Accepted

Category

Architecture

Decision

Version 1.0.1 changes the active product direction to a Windows-first dual-role application.

The Windows application will allow the user to choose:

- Parent
- Child

Parent can register, login, enter a child connection code, and manage approved devices.

Child does not need to login. Child receives a connection code and approves or rejects parent pairing requests directly on the child Windows computer.

Reason

The previous 1.0.0 flow was too passive and mobile-first. The intended product experience is closer to an approval-based connection flow where the child device visibly confirms the pairing before Windows Service protection is enabled.

Impact

Version 1.0.1 becomes the active target.

Required updates:

- Backend pairing APIs must support pending, approved, rejected, and expired states.
- Database must store pairing request state.
- Windows must have a client UI in addition to the background service.
- Mobile work is deferred until the Windows flow is stable.
- Existing 1.0.0 Backend, Mobile, and Agent code remains useful foundation code.

Alternatives Considered

- Continue the mobile-first 1.0.0 direction.
- Only patch the existing pair-code flow.

These alternatives were rejected because they do not match the intended Parent/Child approval experience.

---

## DEC-0015

Date

2026-07-01

Status

Accepted

Category

Security

Decision

Admin authority requires an owner secret.

The owner secret is not stored in plain text in repository files.

Repository documents store only AdminSecretHashV1.

Admin secret verification uses:

- Unicode NFC normalization
- Trimmed input
- SHA-256 over UTF-8 bytes
- Comparison with AdminSecretHashV1

Reason

Admin is the highest project authority and can change every part of the repository. The team needs a way for trusted users to activate Admin authority while preventing ordinary readers of the documentation from using it.

Impact

AI may skip normal startup questions and treat the user as Admin only when the user provides Admin identity and a matching owner secret.

If the Admin secret is missing or invalid, AI must not grant Admin authority.

Reading the documentation or copying the hash is not enough to use Admin authority.

Alternatives Considered

- Let anyone type Admin.
- Store the owner secret in plain text.
- Keep asking module-scope questions for Admin users.

These alternatives were rejected because they either weaken Admin control or slow down trusted emergency work.

---

## DEC-0016

Date

2026-07-01

Status

Accepted

Category

Team

Decision

Only verified Admin may edit documentation files and AGENTS.md.

Member 1 and Member 2 may edit only their assigned implementation modules.

Member 1:

- May edit Backend and Mobile implementation files.
- Must not edit Windows Agent implementation files.
- Must not edit documentation files.
- Must not edit AGENTS.md.

Member 2:

- May edit Windows Agent implementation files.
- Must not edit Backend or Mobile implementation files.
- Must not edit documentation files.
- Must not edit AGENTS.md.

Verified Admin:

- May edit all modules.
- May edit documentation files.
- May edit AGENTS.md.
- Does not need separate Member 1 or Member 2 permission.

Reason

Documentation and AGENTS.md define project authority, architecture, task ownership, and AI behavior. These files must be controlled by the highest authority only to prevent accidental or unauthorized governance changes.

Impact

AI must refuse documentation and AGENTS.md edits unless the current user is verified Admin.

Normal team members can still request implementation help inside their assigned modules.

Alternatives Considered

- Let every member edit documentation.
- Let members edit docs only for their module.

These alternatives were rejected because Version 1.0.1 needs a strict authority model and consistent project rules.

---

## DEC-0017

Date

2026-07-01

Status

Accepted

Category

Team

Decision

Member 1 and Member 2 may edit limited tracking documentation without verified Admin authority.

Allowed tracking documentation:

- TASKS.md for tasks assigned to their own role.
- PROGRESS.md for their own daily progress entries.
- IDEAS.md for confirmed idea records.

Protected documentation remains restricted to verified Admin:

- AGENTS.md
- PROJECT_RULES.md
- ARCHITECTURE.md
- API_SPEC.md
- DATABASE.md
- SECURITY.md
- DECISIONS.md
- ROADMAP.md
- VERSION_*_PLAN.md

Reason

Team members need to record what they completed, update their own task status, and capture useful ideas during normal development.

Locking every documentation file behind Admin makes progress tracking slower and makes the task board less accurate.

Impact

AI may help a normal team member update only their own task/progress/idea records.

AI must still refuse protected documentation edits unless the current user is verified Admin.

AI must not let a team member change another member's task ownership, source-of-truth contracts, project authority rules, architecture, API, database, security, roadmap direction, or release plans.

Alternatives Considered

- Keep all documentation restricted to Admin.
- Let all members edit every documentation file.

The first alternative was rejected because it blocks normal progress tracking.

The second alternative was rejected because source-of-truth files still need strict control.

---

# Future Decisions

When a major change is proposed, create a new decision.

Example

DEC-0013

Implement SignalR instead of Polling

Status

Proposed

---

# Decision Lifecycle

Idea

↓

Discussion

↓

Decision

↓

Documentation

↓

Implementation

↓

Review

---

# Rules

Never delete an old decision.

If a decision changes:

- Mark the old one as "Deprecated" or "Replaced".
- Create a new decision.
- Reference the previous decision.

---

# AI Rules

If an AI suggests:

- Changing architecture
- Renaming APIs
- Changing database schema
- Changing authentication
- Moving responsibilities

The AI must:

1. Create a Proposed Decision.
2. Explain the reason.
3. Wait for developer approval.
4. Update related documentation.
5. Then implement the change.

AI must never make architectural decisions automatically.

---

# Decision Log

| ID | Status | Category | Summary |
|----|--------|----------|---------|
| DEC-0001 | Accepted | Architecture | 4-module architecture |
| DEC-0002 | Accepted | Windows Agent | Use Windows Agent instead of browser extension |
| DEC-0003 | Accepted | Communication | Backend is the communication hub |
| DEC-0004 | Accepted | Database | Backend exclusively owns database access |
| DEC-0005 | Accepted | Security | JWT + Device Token authentication |
| DEC-0006 | Accepted | Modes | Three modes for Demo V1 |
| DEC-0007 | Accepted | Offline | Agent uses cached rules |
| DEC-0008 | Accepted | Team | Fixed module ownership |
| DEC-0009 | Accepted | Git | Git Flow strategy |
| DEC-0010 | Accepted | Documentation | Documentation-first approach |
| DEC-0011 | Accepted | Windows Agent | Agent uses polling for mode synchronization in Demo V1 |
| DEC-0012 | Accepted | Security | Setup Token is used only for pair-code creation |
| DEC-0013 | Accepted | Team | Admin is the highest repository permission level |
| DEC-0014 | Accepted | Architecture | Version 1.0.1 uses Windows-first Parent/Child approval pairing |
| DEC-0015 | Accepted | Security | Admin authority requires owner secret hash verification |
| DEC-0016 | Accepted | Team | Documentation and AGENTS.md edits require verified Admin |
| DEC-0017 | Accepted | Team | Team members may edit limited tracking documentation |

---

# Final Rule

A technical decision is not official until it is recorded in this document.

This file is considered part of the project's architecture.
