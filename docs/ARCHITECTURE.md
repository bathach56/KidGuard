# ARCHITECTURE.md

# Parental Control System Architecture

Version: 1.0.0

---

# 1. Purpose

This document defines the overall system architecture.

Every module must follow this architecture.

No module is allowed to communicate outside this architecture unless the project lead approves the change.

---

# 2. High-Level Architecture

```text
                  Parent
              Flutter Mobile
                     │
               HTTPS / JWT
                     │
                     ▼
        ASP.NET Core Web API
                     │
         Entity Framework Core
                     │
                     ▼
              SQL Server Database
                     ▲
                     │
        HTTPS + Device Token
                     │
                     ▼
          Windows Agent Service
                     │
          Windows Process Manager
                     │
                     ▼
           Windows Operating System
```

---

# 3. Components

## Parent Mobile

Responsibilities

- Login
- Pair Device
- View Device List
- Change Mode
- View Logs
- Receive Notifications (Future)

Technology

- Flutter

Never

- Talk directly to Windows Agent
- Store business logic
- Access SQL Server

---

## Backend API

Responsibilities

- Authentication
- Authorization
- Device Management
- Pair Device
- Store Logs
- Store Device Mode
- Device Communication
- Business Logic

Technology

- ASP.NET Core Web API
- Entity Framework Core

Never

- Kill Windows Processes
- Execute Windows Commands

---

## SQL Server

Responsibilities

- Store Users
- Store Devices
- Store Modes
- Store Logs
- Store Tokens

Never

- Execute business logic

---

## Windows Agent

Responsibilities

- Authenticate Device
- Heartbeat
- Download Current Mode
- Monitor Processes
- Block Applications
- Upload Logs
- Cache Data Offline

Technology

- .NET Worker Service / Windows Service

Never

- Modify Database
- Authenticate Parent
- Implement Business Logic

---

# 4. Communication Flow

## Parent Login

```text
Parent Mobile
      │
POST /auth/login
      │
      ▼
Backend
      │
Return JWT
      ▼
Mobile
```

---

## Pair Device

```text
Windows Agent
        │
Generate Pair Code
        ▼

Backend
        │
Store Pair Code
        ▼

Parent Mobile
        │
Enter Pair Code
        ▼

Backend
        │
Bind Parent + Device
        ▼

Return Device Token
```

---

## Change Mode

```text
Parent
      │
Change Mode
      ▼
Backend
      │
Save Database
      ▼
Windows Agent
      │
Sync
      ▼
Apply New Mode
```

---

## Heartbeat

Every 30 seconds

```text
Windows Agent
      │
Heartbeat
      ▼
Backend
      │
Update Last Seen
      ▼
Database
```

---

## Upload Logs

```text
Agent

↓

Backend

↓

Database

↓

Parent can view later
```

---

# 5. Module Responsibilities

## Mobile

Allowed

- UI
- API Calls
- Token Storage

Forbidden

- SQL
- Windows Logic
- Device Rules

---

## Backend

Allowed

- Business Logic
- Authentication
- Database

Forbidden

- Windows Process Control

---

## Windows Agent

Allowed

- Windows API
- Process Monitoring
- Cache

Forbidden

- Database Access
- Parent Authentication

---

# 6. Process Monitoring Flow

```text
Windows Service

↓

Read Current Mode

↓

Read Allowed Rules

↓

Scan Running Processes

↓

Compare

↓

If blocked

↓

Terminate Process

↓

Write Local Log

↓

Send Log to Backend
```

---

# 7. Offline Flow

```text
Internet Lost

↓

Windows Agent

↓

Use Cached Rules

↓

Continue Protection

↓

Retry Connection

↓

Internet Restored

↓

Upload Cached Logs

↓

Download Latest Configuration
```

---

# 8. Authentication

## Parent

JWT

```text
Login

↓

JWT

↓

Call API
```

---

## Device

Device Token

```text
Pair

↓

Receive Device Token

↓

Heartbeat

↓

Authenticated
```

---

# 9. Data Ownership

| Module | Owns |
|---------|------|
| Mobile | UI |
| Backend | Business Logic |
| SQL Server | Data |
| Windows Agent | Windows Control |

---

# 10. Current Modes

Only

```text
fun

study

punishment
```

These values MUST NOT change.

---

# 11. Future Architecture

Future components

```text
Notification Service

AI Recommendation

Web Dashboard

Website Filter

Schedule Engine

Statistics Engine
```

Not implemented in Demo V1.

---

# 12. Error Flow

```text
Agent Error

↓

Retry

↓

Log Local

↓

Upload Later

↓

Backend

↓

Database
```

---

# 13. Integration Rules

Backend exposes API.

Agent consumes API.

Mobile consumes API.

Neither Mobile nor Agent communicates directly with SQL Server.

---

# 14. Performance Goals

Parent action

↓

Backend

↓

Agent

Target

< 5 seconds

Heartbeat

30 seconds

Offline Cache

Unlimited until reconnect

---

# 15. Security Boundary

```text
Internet
──────────────

HTTPS

──────────────

Backend

──────────────

Database

──────────────

Windows Agent
```

Only Backend is publicly accessible.

Database is private.

Windows Agent never exposes ports.

---

# 16. Architecture Principles

- Single Responsibility
- Separation of Concerns
- API First
- Documentation First
- Backend is the Single Source of Truth
- Agent never modifies business data
- Mobile never contains business logic

---

# 17. Demo V1 Success Criteria

✅ Parent can login

✅ Parent can pair a device

✅ Parent can switch between

- fun
- study
- punishment

✅ Agent receives mode

✅ Agent applies mode

✅ Agent uploads heartbeat

✅ Agent uploads logs

System considered complete only when the full flow works end-to-end.