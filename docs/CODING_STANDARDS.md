# CODING_STANDARDS.md

# Parental Control System

Version: 1.0.0

---

# Purpose

This document defines coding conventions for the entire project.

Every contributor must follow these rules.

Consistency is more important than personal preference.

---

# Languages

Backend

- C#

Mobile

- Dart

Windows Agent

- C#

Documentation

- Markdown

Database

- SQL Server

---

# General Principles

Follow

- SOLID
- DRY (Don't Repeat Yourself)
- KISS (Keep It Simple)
- YAGNI (You Aren't Gonna Need It)

Always prefer readability over clever code.

---

# Naming Convention

## Classes

PascalCase

Example

DeviceService

HeartbeatWorker

LoginController

---

## Interfaces

Prefix

I

Example

IDeviceService

ILogService

IAuthenticationService

---

## Methods

Verb + Object

Example

GetDevice()

CreatePairCode()

UpdateMode()

SendHeartbeat()

BlockProcess()

Bad

Run()

Do()

Data()

---

## Variables

camelCase

Example

deviceId

deviceToken

currentMode

blockedProcesses

---

## Constants

PascalCase

Example

HeartbeatInterval

MaxRetryCount

---

## Private Fields

Prefix

_

Example

_deviceRepository

_logger

_httpClient

---

# Folder Structure

## Backend

```text
backend/

Controllers/

Services/

Repositories/

Entities/

DTOs/

Configurations/

Middleware/

Authentication/

Extensions/

Migrations/

Models/

Interfaces/

Utilities/
```

---

## Windows Agent

```text
windows-agent/

Services/

Workers/

Communication/

Process/

Cache/

Configuration/

Models/

Utilities/

Logging/
```

---

## Mobile

```text
mobile/

lib/

screens/

widgets/

services/

models/

providers/

utils/

config/
```

---

# File Naming

PascalCase

Good

DeviceController.cs

HeartbeatWorker.cs

LoginScreen.dart

Bad

devicecontroller.cs

login.dart

---

# Method Rules

Maximum

50 lines

If method exceeds 50 lines

↓

Split into smaller methods.

---

# Class Rules

One class

↓

One responsibility

Never create

God Classes.

---

# Comments

Only explain

WHY

Never explain

WHAT

Bad

// increment i

Good

// Retry because heartbeat may fail due to temporary network loss.

---

# Async Rules

Always use

async / await

Never block threads with

.Result

.Wait()

unless absolutely necessary.

---

# Exception Handling

Bad

```csharp
catch
{
}
```

Good

```csharp
catch(Exception ex)
{
    _logger.LogError(ex);
    throw;
}
```

Never swallow exceptions.

---

# Logging

Log

- Startup
- Shutdown
- Login
- Pair
- Heartbeat
- Blocked Process
- Unexpected Errors

Never log

- Password
- JWT
- Device Token
- Connection String

---

# DTO Rules

Always use DTOs.

Never expose Entity directly.

Good

DeviceResponse

ModeResponse

LoginRequest

Bad

Return Device Entity

---

# Controller Rules

Controllers

Only

- Validate
- Call Service
- Return Response

No business logic.

---

# Service Rules

Business Logic

belongs here.

Controllers must remain thin.

---

# Repository Rules

Repositories

Only communicate with database.

Never contain business logic.

---

# Dependency Injection

Always use Dependency Injection.

Never instantiate dependencies manually.

Good

```csharp
public DeviceService(IDeviceRepository repository)
```

Bad

```csharp
new DeviceRepository();
```

---

# API Response

Every API follows

```json
{
    "success": true,
    "message": "Success",
    "data": {}
}
```

Never invent new response structures.

---

# JSON

camelCase only.

Good

deviceId

currentMode

Bad

DeviceId

CurrentMode

---

# Nullable

Enable Nullable Reference Types.

Never ignore compiler warnings.

---

# Magic Values

Never write

```csharp
if(mode=="study")
```

Instead

```csharp
Mode.Study
```

or

```csharp
ModeConstants.Study
```

---

# Configuration

Never hardcode

API URL

Timeout

Connection String

Secret

JWT

Use configuration files.

---

# Clean Code Checklist

Before Commit

[ ] No duplicated code

[ ] No TODO left

[ ] No hardcoded values

[ ] No commented-out code

[ ] Build successful

[ ] Documentation updated

---

# Git Commit Quality

Every commit should compile.

Do not commit broken code.

Small commits are preferred.

---

# Pull Request Checklist

Before PR

[ ] Build passes

[ ] Feature tested

[ ] No warnings

[ ] Documentation updated

[ ] Branch synchronized

---

# AI Generated Code

AI-generated code must be reviewed.

Never merge AI code without understanding it.

Developers are responsible for all committed code.

---

# Definition of Done

A task is complete only if

✓ Code compiles

✓ Feature works

✓ No critical warnings

✓ Documentation updated

✓ Code reviewed

✓ Ready for merge

---

# Final Rule

Readable code is more valuable than clever code.

Future teammates should understand your code without asking you.