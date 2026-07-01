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

---

# Final Rule

A technical decision is not official until it is recorded in this document.

This file is considered part of the project's architecture.
