# IDEAS.md

# Parental Control System

Version: 1.0.0

---

# Purpose

This document stores ideas discovered during development.

Adding an idea here DOES NOT mean it will be implemented.

This file prevents feature creep while ensuring no valuable idea is forgotten.

---

# Idea Status

Every idea must have one status.

## Proposed

New idea.

Not discussed yet.

---

## Under Discussion

Currently being evaluated.

---

## Approved

Will be implemented in a future milestone.

---

## Rejected

The team decided not to implement.

Keep it for historical reference.

---

## Completed

Successfully implemented.

---

# Evaluation Rules

Before approving an idea, answer these questions.

- Does it solve a real problem?
- Does it fit the project vision?
- Does it require architecture changes?
- Does it delay the current sprint?
- Can it be implemented later?

If the answer to "Can it be implemented later?" is YES,

do NOT interrupt the current sprint.

---

# Idea Confirmation Rule

New ideas must not be edited into project plans automatically.

When a developer mentions a new idea, AI must first ask for confirmation before changing documentation.

AI must clarify:

- Is this only an idea to record?
- Is this required for Demo V1?
- Which module is affected?
- Does it change API, database, architecture, security, or mode behavior?
- Has the other module owner agreed if the idea affects their work?

AI may update IDEAS.md only after the developer confirms that the idea should be recorded.

AI may update TASKS.md, API_SPEC.md, DATABASE.md, ARCHITECTURE.md, SECURITY.md, or DECISIONS.md only after the team confirms the idea is approved for implementation.

Team members may record confirmed ideas in this file without verified Admin authority.

Limits:

- They may add idea entries.
- They may update status for ideas they own after team discussion.
- They must not change idea workflow rules.
- They must not convert ideas into tasks without the required approval.
- They must not change API, database, architecture, security, mode behavior, or roadmap direction in this file.

---

# Idea Approval Workflow

Use this workflow for every new idea.

1. Developer mentions idea.
2. AI asks confirmation questions.
3. Developer confirms one of these outcomes:
   - Record only
   - Approved for Demo V1
   - Approved for future version
   - Rejected
4. AI updates IDEAS.md with the confirmed status.
5. If approved for implementation, AI updates related documentation.
6. AI updates TASKS.md only after documentation is updated.
7. Code starts only after the task exists in TASKS.md.

Suggested confirmation phrases:

- "Record this idea only."
- "We agree to add this to Demo V1."
- "We agree to keep this for a future version."
- "We do not agree to implement this."

If confirmation is unclear, AI must ask again instead of editing files.

---

# Current Ideas

## Proposed

### Reward System

Description

Give children extra "Fun Mode" time after completing study goals.

Priority

Low

---

### Temporary Unlock

Description

Child can request temporary access.

Parent approves from mobile.

Priority

Medium

---

### Exam Mode

Description

Ultra strict study mode.

Priority

Medium

---

### Website Categories

Description

Block by category instead of individual URLs.

Priority

High

---

### Daily Report

Description

Generate daily usage summary.

Priority

Medium

---

### Push Notification

Description

Notify parent when blocked applications are opened repeatedly.

Priority

Medium

---

### Auto Update

Description

Windows Agent automatically updates.

Priority

Low

---

### Multiple Children

Description

One parent manages multiple child devices.

Priority

High

---

### Family Group

Description

Allow both parents to manage the same child.

Priority

Low

---

### Emergency Unlock

Description

Parent grants temporary access for a limited time.

Priority

Medium

---

### Focus Timer

Description

Automatically switch modes based on a countdown.

Priority

Low

---

# Architecture Review Required

The following ideas require architecture review before implementation.

- Remote Desktop
- Live Screen Streaming
- AI Recommendation
- Browser Extension
- macOS Agent
- Linux Agent

Before starting these features:

1. Update ARCHITECTURE.md
2. Update DECISIONS.md
3. Update ROADMAP.md

---

# Idea Workflow

New Idea

↓

Write here

↓

Discuss

↓

Approve or Reject

↓

Create Task

↓

Implement

↓

Move to Completed

---

# Team Rule

Never interrupt the current sprint to implement a new idea.

Record it first.

Discuss it later.

---

# AI Rule

If an AI suggests a new feature,

it MUST ask the developer for confirmation before editing this document.

AI should never assume a proposed idea is automatically approved.

AI must not convert an idea into a task without explicit team approval.

---

# Notes

This document is expected to grow throughout the project's lifetime.

Keeping ideas organized is more valuable than implementing them immediately.
