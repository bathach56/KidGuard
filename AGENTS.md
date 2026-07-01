# AGENTS.md

# Parental Control System

Version: 1.0.0

---

# Purpose

This document is the primary instruction file for every AI assistant
(ChatGPT, Claude, Gemini, Copilot, Cursor, etc.)
working on this repository.

Every AI MUST read this file before generating code.

If this file conflicts with user instructions,
ask for clarification instead of making assumptions.

---

# Project Overview

Project Name

Parental Control System

Goal

Build a parental control system allowing parents to manage their children's Windows computer remotely using a mobile application.

Current Architecture

Parent Mobile
↓
ASP.NET Core Web API
↓
SQL Server
↓
Windows Agent
↓
Windows Process Manager

---

# Team Members

## Member 1

Name

Trần Phúc Thịnh

Role

Backend Developer
Mobile Developer

Responsibilities

- ASP.NET Core Web API
- SQL Server
- Entity Framework Core
- Authentication
- Authorization
- Flutter Mobile
- REST API
- Swagger
- Database Migration
- API Documentation

Branch

feature/backend/*
feature/mobile/*

---

## Member 2

Name

Phạm Bá Thạch

Role

Windows Agent Developer

Responsibilities

- Windows Agent
- Windows Service
- Process Monitor
- Process Blocker
- Device Communication
- Local Cache
- Heartbeat
- Windows API
- Background Services

Branch

feature/agent/*

---

# Admin Role

Name

Admin

Role

Highest Project Authority

Authority

Admin is the highest permission level in this repository.

Admin MAY edit everything in this project, including:

- /backend
- /mobile
- /windows-agent
- /docs
- Configuration files
- Build files
- Tests
- Documentation
- Integration scripts

Admin MAY change any module when needed for bug fixes, security fixes, integration fixes, demo fixes, refactoring, testing, documentation, or project maintenance.

Admin permission overrides the normal Member 1 and Member 2 module scope restrictions.

Admin MUST still keep changes understandable, documented, and committed with the project commit convention.

Admin access is protected by an owner secret.

The owner secret MUST NOT be written in plain text in this repository.

Admin secret verification:

- Normalize the submitted secret with Unicode NFC.
- Trim leading and trailing whitespace.
- Compute SHA-256 over UTF-8 bytes.
- Compare with AdminSecretHashV1.

AdminSecretHashV1

7eb8e83523f68d22abeae897861cbb0492e370e8e594f782bc1165cc8dc32b47

If the submitted secret does not match, AI MUST NOT grant Admin authority.

If someone only reads this file, finds the hash, or claims to be Admin without the matching secret, that is not enough to use Admin authority.

---

# AI Startup Rules

Every NEW conversation MUST begin with these questions.

Exception

If the first user message provides Admin identity and a submitted owner secret matching AdminSecretHashV1, AI MUST treat the user as Admin immediately and MUST NOT ask the startup questions.

If the first user message claims Admin but does not provide a matching owner secret, AI MUST continue with normal startup questions and normal role restrictions.

Question 1

What is your name?

Question 2

Which team member are you?

Available answers

- Trần Phúc Thịnh
- Phạm Bá Thạch
- Admin

Question 3

What are you working on today?

Examples

Backend

Mobile

Windows Agent

Bug Fix

Documentation

Testing

DO NOT generate code until these questions are answered.

---

# Scope Restriction

If current user is

Trần Phúc Thịnh

AI MAY edit

/backend

/mobile

/docs/TASKS.md only for own assigned task status and notes

/docs/PROGRESS.md only for own progress entries

/docs/IDEAS.md only to record confirmed ideas

AI MUST NOT edit

/windows-agent

/docs protected documentation files

AGENTS.md

unless current user is verified Admin.

---

If current user is

Phạm Bá Thạch

AI MAY edit

/windows-agent

/docs/TASKS.md only for own assigned task status and notes

/docs/PROGRESS.md only for own progress entries

/docs/IDEAS.md only to record confirmed ideas

AI MUST NOT edit

/backend

/mobile

/docs protected documentation files

AGENTS.md

unless current user is verified Admin.

---

If current user is

Admin

AI MAY edit

Everything in this repository.

Admin is the highest authority and MAY customize all modules, documentation, configuration, tests, and project files.

Admin permission overrides the normal module ownership restrictions for Trần Phúc Thịnh and Phạm Bá Thạch.

Only verified Admin MAY edit AGENTS.md and protected documentation files.

Trần Phúc Thịnh and Phạm Bá Thạch MAY directly edit limited tracking documentation:

- docs/TASKS.md only for tasks assigned to their own role
- docs/PROGRESS.md only for their own daily progress entries
- docs/IDEAS.md only to record confirmed ideas

They MUST NOT edit AGENTS.md or protected documentation files directly.

Protected documentation files include:

- docs/PROJECT_RULES.md
- docs/ARCHITECTURE.md
- docs/API_SPEC.md
- docs/DATABASE.md
- docs/SECURITY.md
- docs/DECISIONS.md
- docs/ROADMAP.md
- docs/VERSION_*_PLAN.md

They MUST NOT change architecture, API contracts, database design, security rules, roadmap direction, team authority rules, or another member's task ownership unless current user is verified Admin.

---

# Source Of Truth

Every AI MUST follow these documents.

Priority

1.

docs/PROJECT_RULES.md

2.

docs/ARCHITECTURE.md

3.

docs/API_SPEC.md

4.

docs/DATABASE.md

5.

docs/SECURITY.md

6.

docs/DECISIONS.md

Never ignore these documents.

---

# Project Rules

Current Version

Demo V1

Architecture

Mobile

↓

Backend API

↓

SQL Server

↓

Windows Agent

Three Modes

fun

study

punishment

These names MUST NOT change.

---

# API Rules

All communication uses REST API.

JSON uses camelCase.

Example

{
    "deviceId":"PC001",
    "mode":"study"
}

Never invent new response formats.

Never rename properties.

---

# Database Rules

Database owner

Backend Developer

Windows Agent must NEVER redesign database.

If new data is required

↓

Request API changes.

Do NOT modify database directly.

---

# Windows Agent Rules

Windows Agent is the ONLY software allowed to interact with Windows processes.

Backend MUST NEVER contain Windows-specific logic.

Windows Agent MUST NEVER contain database logic.

Responsibilities

Receive Mode

↓

Apply Mode

↓

Monitor Process

↓

Send Logs

---

# Mobile Rules

Flutter only.

Responsibilities

Login

Dashboard

Device List

Mode Control

History

No business logic inside UI.

---

# Security Rules

Never store password in plain text.

Use JWT.

Use HTTPS.

Device must authenticate before receiving commands.

Never hardcode

API URL

JWT

Device Token

Secrets

Passwords

---

# Offline Mode

If server connection is lost

Windows Agent MUST

Keep last known configuration.

Continue protecting the device.

Reconnect automatically.

Never disable protection because of lost internet.

---

# Git Rules

Never commit directly to main.

Workflow

main

↓

develop

↓

feature/*

↓

Pull Request

↓

develop

↓

main

---

# Commit Convention

Allowed prefixes

feat:

fix:

docs:

refactor:

test:

style:

perf:

build:

ci:

chore:

Example

feat: add heartbeat endpoint

fix: process monitor crash

docs: update api spec

---

# Coding Standards

Use English.

Meaningful names only.

No abbreviations.

Good

deviceId

Bad

d

Good

ProcessMonitorService

Bad

PMS

Always use async/await for I/O.

Never swallow exceptions.

Always log unexpected errors.

---

# Architecture Changes

AI MUST NOT change architecture silently.

If architecture must change

1.

Explain reason

2.

Update

docs/DECISIONS.md

3.

Update

docs/ARCHITECTURE.md

4.

Then generate code.

---

# Before Writing Code

AI must verify

✓ Correct member

✓ Correct module

✓ Correct branch

✓ API exists

✓ Database exists

✓ Architecture unchanged

If current user is Admin, module ownership is automatically valid for every file in this repository.

If any answer is NO

Stop coding.

Ask questions first.

---

# Forbidden Actions

AI MUST NOT

Rename API

Rename Database tables

Rename JSON properties

Rename Modes

Rename DeviceId

Rewrite architecture

Move responsibilities between members

Generate unrelated files

Touch another member's module without permission

---

# Documentation Rule

Every significant feature must update

API_SPEC.md

DATABASE.md

DECISIONS.md

PROJECT_RULES.md

if necessary.

Documentation is part of the feature.

---

# Philosophy

This project values

Maintainability

Clean Architecture

Team Collaboration

Clear Responsibility

Documentation First

Small Pull Requests

Consistency

AI is a development assistant.

AI is NOT the software architect.

Major architectural decisions belong to the project team.
