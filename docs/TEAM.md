# TEAM.md

# Parental Control System

Version: 1.0.0

Last Updated

2026-06-28

---

# Purpose

This document defines team members, responsibilities, ownership, communication rules, and collaboration guidelines.

Every contributor and every AI assistant must follow this document.

---

# Team Members

## Member 1

Name

Trần Phúc Thịnh

Role

Backend Developer
Mobile Developer

Primary Responsibilities

- ASP.NET Core Web API
- Authentication
- Authorization
- SQL Server
- Entity Framework Core
- Flutter Mobile
- REST API
- Swagger
- Database Migration
- API Documentation

Repository Ownership

/backend

/mobile

Database Ownership

Yes

API Ownership

Yes

Security Ownership

Shared

Documentation Ownership

Shared

---

## Member 2

Name

Phạm Bá Thạch

Role

Windows Agent Developer

Primary Responsibilities

- Windows Agent
- Windows Service
- Process Monitor
- Process Blocker
- Heartbeat
- Local Cache
- Device Communication
- Windows Integration
- Offline Protection

Repository Ownership

/windows-agent

Windows Ownership

Yes

Database Ownership

No

API Ownership

Consumer Only

Security Ownership

Shared

Documentation Ownership

Shared

---

# Ownership Matrix

| Module | Owner | Contributor |
|---------|-------|------------|
| Backend API | Trần Phúc Thịnh | Phạm Bá Thạch (API Consumer) |
| Mobile | Trần Phúc Thịnh | None |
| Windows Agent | Phạm Bá Thạch | None |
| Database | Trần Phúc Thịnh | None |
| API Specification | Trần Phúc Thịnh | Phạm Bá Thạch |
| Security | Shared | Shared |
| Documentation | Shared | Shared |
| Git Workflow | Shared | Shared |

---

# Decision Ownership

## Backend Decisions

Owner

Trần Phúc Thịnh

Examples

- Database Schema
- API Endpoint
- JWT
- Entity Framework
- Authentication
- API Validation

---

## Windows Agent Decisions

Owner

Phạm Bá Thạch

Examples

- Process Monitoring
- Windows Service
- Local Cache
- Retry Strategy
- Offline Protection
- Process Blocking

---

## Shared Decisions

Require agreement from both members.

Examples

- Architecture
- API Contract
- Security Model
- Folder Structure
- Git Workflow
- New Modules
- Breaking Changes

---

# Communication Rules

Before changing

Database

↓

Notify Backend Owner.

---

Before changing

API Contract

↓

Notify both members.

---

Before changing

Windows Agent communication

↓

Notify Windows Agent Owner.

---

Before changing

Architecture

↓

Update

ARCHITECTURE.md

DECISIONS.md

PROJECT_RULES.md

before implementation.

---

# Pull Request Rules

Every Pull Request must include

Purpose

Affected Module

Documentation Updated

Testing Result

Screenshots (if UI)

---

# Code Review

Backend

Reviewed by

Phạm Bá Thạch

---

Windows Agent

Reviewed by

Trần Phúc Thịnh

---

Shared Files

Reviewed together.

---

# AI Collaboration Rules

Before generating code,

AI MUST ask

1.

What is your name?

2.

Which team member are you?

Allowed answers

- Trần Phúc Thịnh
- Phạm Bá Thạch

3.

Which module are you working on?

Examples

Backend

Mobile

Windows Agent

Documentation

Testing

---

# AI Scope Rules

If current developer is

Trần Phúc Thịnh

AI may edit

/backend

/mobile

/docs

AI must NOT modify

/windows-agent

unless explicitly requested.

---

If current developer is

Phạm Bá Thạch

AI may edit

/windows-agent

/docs

AI must NOT modify

/backend

/mobile

unless explicitly requested.

---

# Integration Rules

Backend exposes API.

Windows Agent consumes API.

Mobile consumes API.

No module communicates directly with SQL Server except Backend.

---

# Escalation Rules

If a requested change impacts another module

↓

Do NOT implement immediately.

↓

Discuss with module owner.

↓

Update documentation.

↓

Implement after approval.

---

# New Idea Agreement Rules

If a new idea appears during development, it is not automatically approved.

AI must ask for confirmation before editing any documentation file.

For ideas affecting only one module:

- The module owner may approve recording the idea.
- The module owner may approve implementation if it does not change shared contracts.

For ideas affecting shared contracts:

- Both members must agree before implementation.
- Shared contracts include API, database, architecture, security, modes, pairing flow, heartbeat flow, and log format.

Valid approval phrases include:

- "We agree."
- "Both members agree."
- "Thinh agrees."
- "Thach agrees."
- "Add this to Demo V1."
- "Keep this as a future idea."
- "Reject this idea."

If approval is not explicit, AI must only discuss the idea and must not edit TASKS.md or implementation-related documents.

---

# Branch Ownership

Trần Phúc Thịnh

feature/backend/*

feature/mobile/*

fix/backend/*

fix/mobile/*

---

Phạm Bá Thạch

feature/agent/*

fix/agent/*

---

# Shared Branches

develop

release/*

---

# Shared Files

The following files require discussion before modification.

AGENTS.md

PROJECT_RULES.md

ARCHITECTURE.md

DATABASE.md

API_SPEC.md

SECURITY.md

DECISIONS.md

---

# Working Agreement

We agree to

- Keep documentation updated.
- Respect module ownership.
- Communicate breaking changes early.
- Review each other's Pull Requests.
- Keep commits small and meaningful.
- Never bypass agreed architecture.

---

# Definition of Collaboration

Good collaboration means

- Clear ownership
- Small Pull Requests
- Updated documentation
- Predictable API
- Stable architecture

No contributor, including AI assistants, should introduce changes that affect another module without coordination.

---

# Final Rule

The goal of this project is not only to build software,
but to build software that can be maintained, extended,
and understood by any future contributor.
