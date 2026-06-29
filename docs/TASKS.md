# TASKS.md

# Parental Control System

Version: 1.0.0

---

# Purpose

This document tracks all project tasks.

Every completed task must be checked.

Only work on tasks assigned to your role unless discussed with the team.

---

# Sprint Overview

Sprint Duration

2 Weeks

Goal

Deliver Demo V1.

Definition

Parent can control a Windows computer remotely using a mobile application.

---

# MVP Priority

The following tasks are required for Demo V1. Do these first.

Backend

- [x] Login API
- [x] Pair Code API
- [x] Device Pair API
- [x] Device List API
- [x] Mode Update API
- [ ] Mode Sync API
- [ ] Heartbeat API
- [ ] Log Upload API

Mobile

- [ ] Login Screen
- [ ] Device List
- [ ] Device Detail
- [ ] Mode Switch
- [ ] Basic Log View

Windows Agent

- [ ] Create Windows Agent project
- [ ] Create pair code request
- [ ] Store Device Token securely
- [ ] Send heartbeat
- [ ] Sync current mode
- [ ] Cache last known mode
- [ ] Monitor processes
- [ ] Block demo process
- [ ] Upload logs
- [ ] Continue protection while offline

Anything outside this list must not delay Demo V1.

---

# Sprint 1

## Goal

Build project foundation.

---

## Member 1

Name

Tráº§n PhÃºc Thá»‹nh

Role

Backend + Mobile

---

## Backend Tasks

- [x] Create ASP.NET Core Solution
  Priority: High

- [x] Configure SQL Server
  Priority: High

- [x] Create Entity Framework Core setup
  Priority: High

- [x] Create Database Migration
  Priority: High

- [x] Create Authentication with JWT
  Priority: High

- [x] Create Login API
  Priority: High

- [x] Create Pair Code API
  Priority: High

- [x] Create Device API
  Priority: High

- [x] Create Pair API
  Priority: High

- [x] Create Mode API
  Priority: High

- [ ] Create Heartbeat API
  Priority: High

- [ ] Create Logs API
  Priority: High

- [ ] Create Swagger Documentation
  Priority: Medium

---

## Mobile Tasks

- [ ] Create Flutter Project
  Priority: High

- [ ] Create Login Screen
  Priority: High

- [ ] Create Dashboard
  Priority: Medium

- [ ] Create Device List
  Priority: High

- [ ] Create Device Detail
  Priority: High

- [ ] Create Mode Switch
  Priority: High

- [ ] Create API Integration
  Priority: High

- [ ] Create Basic Log View
  Priority: Medium

---

## Member 1 Deliverables

- Backend running
- Swagger working
- Database connected
- Flutter can login
- Flutter can pair a device
- Flutter can change mode

---

# Member 2

Name

Pháº¡m BÃ¡ Tháº¡ch

Role

Windows Agent

---

## Windows Agent Tasks

- [ ] Create Solution
  Priority: High

- [ ] Create Worker Service
  Priority: High

- [ ] Configure Windows Service hosting
  Priority: High

- [ ] Implement Device Registration / Pair Code request
  Priority: High

- [ ] Store Device Token securely
  Priority: High

- [ ] Implement Heartbeat Service
  Priority: High

- [ ] Implement Mode Synchronization
  Priority: High

- [ ] Implement Local Cache
  Priority: High

- [ ] Implement Retry Mechanism
  Priority: Medium

- [ ] Implement Process Monitor
  Priority: High

- [ ] Implement Process Blocker
  Priority: High

- [ ] Add protected system process list
  Priority: High

- [ ] Implement Local Logging
  Priority: Medium

- [ ] Implement Log Upload
  Priority: High

- [ ] Implement Offline Protection
  Priority: High

---

## Member 2 Deliverables

- Agent installed
- Pair code created
- Device Token stored
- Heartbeat working
- Mode received
- Demo process blocked
- Offline cache working
- Logs uploaded after reconnect

---

# Integration Tasks

Owner

Both

- [ ] Mobile Login -> Backend
- [ ] Backend -> Database
- [ ] Agent -> Pair Code -> Backend
- [ ] Mobile -> Pair Device -> Backend
- [ ] Agent -> Heartbeat -> Backend
- [ ] Agent -> Receive Mode -> Apply Mode
- [ ] Agent -> Upload Logs -> Backend
- [ ] Mobile -> View Device Status -> Backend

---

# Sprint 2

Goal

Complete Demo.

Backend

- [ ] Improve Validation
- [ ] Improve Error Handling
- [ ] Optimize Database

Mobile

- [ ] Device Status
- [ ] Log Screen
- [ ] Loading State
- [ ] Error Dialog

Windows Agent

- [ ] Improve Retry
- [ ] Better Logging
- [ ] Performance Optimization

Integration

- [ ] Full End-to-End Test
- [ ] Bug Fixes

---

# Dependencies

Backend must finish before

- Mobile API Integration
- Agent API Integration

Agent depends on

- API_SPEC.md
- Backend implementation
- AGENT_DESIGN.md

Mobile depends on

- Backend API
- Authentication
- API_SPEC.md
- BACKEND_MOBILE_DESIGN.md

---

# Definition of Done

A task is complete only if

- [ ] Code compiles
- [ ] No warnings
- [ ] Feature tested
- [ ] Documentation updated
- [ ] Committed to Git
- [ ] Pull Request created
- [ ] Reviewed
- [ ] Merged into develop

---

# Daily Checklist

Every developer should

- [ ] Pull latest develop
- [ ] Create feature branch
- [ ] Update task status
- [ ] Update PROGRESS.md
- [ ] Commit frequently
- [ ] Push before ending work

---

# AI Instructions

When a developer asks for help, AI must:

1. Identify developer.
2. Read AGENTS.md.
3. Read PROJECT_RULES.md.
4. Stay inside assigned module.
5. Never modify another module without explicit permission.

---

# Demo V1 Success

Demo is complete when

- [ ] Parent Login
- [ ] Device Pair
- [ ] Device Online
- [ ] Heartbeat Working
- [ ] Change Mode
- [ ] Agent Receives Mode
- [ ] Agent Applies Mode
- [ ] Process Blocking Works
- [ ] Logs Uploaded
- [ ] Mobile Displays Device Status
- [ ] Agent Continues Protection Offline

---

# Future Tasks

These features are not part of Demo V1 and must not delay Demo V1.

- Website Blocking
- Screen Time Schedule
- Notifications
- Reports
- Child Profile
- Multi-language
- Auto Update
- AI Recommendation
- Remote Desktop
- Multi-platform Agent






