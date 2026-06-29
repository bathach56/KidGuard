# GIT_WORKFLOW.md

# Parental Control System

Version: 1.0.0

---

# Purpose

This document defines the Git workflow for the project.

Every contributor must follow these rules.

---

# Repository Structure

Main Branches

main

develop

Working Branches

feature/*

fix/*

hotfix/*

release/*

---

# Branch Strategy

main

Production-ready code only.

Never commit directly.

---

develop

Integration branch.

Every completed feature merges here first.

---

feature/*

New features.

Examples

feature/backend/auth

feature/backend/device-api

feature/mobile/login

feature/mobile/dashboard

feature/agent/heartbeat

feature/agent/process-monitor

feature/agent/process-blocker

---

fix/*

Bug fixes.

Example

fix/login-validation

---

hotfix/*

Emergency production fixes.

Only when absolutely necessary.

---

release/*

Prepare production release.

---

# Responsibilities

## Trần Phúc Thịnh

Allowed Branches

feature/backend/*

feature/mobile/*

fix/backend/*

fix/mobile/*

Never modify

windows-agent/

without discussion.

---

## Phạm Bá Thạch

Allowed Branches

feature/agent/*

fix/agent/*

Never modify

backend/

mobile/

without discussion.

---

# Daily Workflow

Step 1

Update develop

git checkout develop

git pull origin develop

---

Step 2

Create feature branch

git checkout -b feature/backend/auth

---

Step 3

Develop

Commit frequently.

---

Step 4

Push

git push origin feature/backend/auth

---

Step 5

Create Pull Request

Target

develop

---

Step 6

Review

Another member reviews.

---

Step 7

Merge

Merge into develop.

---

# Commit Convention

Format

<type>: <message>

Examples

feat: add login api

feat: add process monitor

feat: add heartbeat endpoint

fix: resolve jwt validation

fix: prevent duplicate pair code

docs: update api specification

docs: update architecture

refactor: simplify heartbeat service

test: add login integration tests

style: format project

chore: update packages

---

# Allowed Types

feat

fix

docs

style

refactor

test

perf

build

ci

chore

---

# Commit Rules

Each commit should

Compile successfully.

Contain only one logical change.

Be easy to revert.

---

# Pull Request Rules

Every Pull Request must include

Title

Description

Checklist

Affected Modules

Related Issue (optional)

---

Example

Title

feat: implement heartbeat api

Description

Implemented heartbeat endpoint
Added validation
Added unit tests

---

# Pull Request Checklist

Before merge

[ ] Build passes

[ ] Feature tested

[ ] Documentation updated

[ ] No merge conflicts

[ ] Branch up to date

[ ] No secrets committed

[ ] Reviewer approved

---

# Merge Strategy

Preferred

Squash Merge

Reason

Cleaner history.

One feature

↓

One commit

---

# Forbidden

Never

Force Push main

Force Push develop

Delete shared branches

Commit generated binaries

Commit passwords

Commit JWT secret

Commit SQL credentials

Commit Device Tokens

---

# .gitignore

Always ignore

bin/

obj/

.vs/

.idea/

publish/

TestResults/

*.user

*.suo

appsettings.Development.json

.env

.env.local

---

# Conflict Resolution

If merge conflict occurs

1.

Pull latest develop

2.

Resolve conflicts

3.

Build project

4.

Run tests

5.

Commit resolved changes

Never guess during conflict resolution.

Ask teammate if unsure.

---

# Documentation Rule

If feature changes

API

↓

Update API_SPEC.md

Database

↓

Update DATABASE.md

Architecture

↓

Update ARCHITECTURE.md

Security

↓

Update SECURITY.md

Workflow

↓

Update GIT_WORKFLOW.md

---

# AI Workflow

Before generating code

AI must identify

Current developer

Current branch

Current module

AI must NOT create code for another developer's module.

---

# Branch Ownership

Backend

Owner

Trần Phúc Thịnh

Mobile

Owner

Trần Phúc Thịnh

Windows Agent

Owner

Phạm Bá Thạch

Documentation

Shared

---

# Release Workflow

feature/*

↓

develop

↓

release/*

↓

Testing

↓

main

↓

Tag Release

---

# Version Tag

Examples

v1.0.0

v1.1.0

v2.0.0

---

# Definition of Ready

Before starting work

✓ Task assigned

✓ API confirmed

✓ Database confirmed

✓ Documentation updated

✓ Branch created

---

# Definition of Done

Task is complete only when

✓ Code compiles

✓ Feature works

✓ Tests pass

✓ Documentation updated

✓ Pull Request approved

✓ Merged into develop

---

# Final Rule

Git history is part of the project documentation.

Write commits so another developer can understand the project history months later.
