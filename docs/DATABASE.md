# DATABASE.md

# Parental Control System Database Design

Version: 1.0.0

Database Engine

SQL Server 2022

ORM

Entity Framework Core

---

# 1. Database Principles

Backend is the ONLY module allowed to access the database.

Windows Agent MUST NEVER connect directly to SQL Server.

Mobile MUST NEVER connect directly to SQL Server.

All communication goes through Backend API.

---

# 2. Entity Relationship

```text
Users
 │
 │ 1 - N
 ▼
Devices
 │
 │ 1 - N
 ▼
DeviceLogs

Devices
 │
 │ N - 1
 ▼
Modes

Devices
 │
 │ 1 - N
 ▼
Heartbeats

Devices
 │
 │ 1 - 1
 ▼
PairCodes
```

---

# 3. Table : Users

Purpose

Store parent accounts.

Columns

| Column | Type | Required | Description |
|---------|------|----------|-------------|
| id | uniqueidentifier | Yes | Primary Key |
| fullName | nvarchar(100) | Yes | Parent Name |
| email | nvarchar(255) | Yes | Login Email |
| passwordHash | nvarchar(max) | Yes | BCrypt Hash |
| phoneNumber | nvarchar(20) | No | Phone |
| createdAt | datetime2 | Yes | UTC |
| updatedAt | datetime2 | Yes | UTC |

Indexes

- email UNIQUE

---

# 4. Table : Devices

Purpose

Store every Windows computer.

Pairing Rule

Before pairing, a device may exist as a pending device created from `POST /pair-code`.

In this state:

- userId may be null.
- currentMode defaults to fun.
- isOnline defaults to false.
- deviceToken is generated only after successful pairing.

Columns

| Column | Type |
|---------|------|
| id | uniqueidentifier |
| userId | uniqueidentifier |
| deviceName | nvarchar(100) |
| computerName | nvarchar(100) |
| deviceToken | nvarchar(max) |
| currentMode | nvarchar(20) |
| isOnline | bit |
| lastSeen | datetime2 |
| createdAt | datetime2 |
| updatedAt | datetime2 |

Relationships

User

1

↓

Many Devices

Indexes

deviceToken UNIQUE

userId

currentMode

---

# 5. Table : Modes

Purpose

Reference table.

Columns

| Column | Type |
|---------|------|
| id | int |
| name | nvarchar(30) |
| description | nvarchar(255) |

Seed Data

| id | name |
|----|------|
|1|fun|
|2|study|
|3|punishment|

These values MUST NEVER change.

---

# 6. Table : PairCodes

Purpose

Temporary pairing.

Columns

| Column | Type |
|---------|------|
| id | uniqueidentifier |
| deviceId | uniqueidentifier |
| pairCode | nvarchar(20) |
| expiresAt | datetime2 |
| isUsed | bit |
| createdAt | datetime2 |

Rules

Pair Code

Expires after

10 minutes

Pair Code is one-time use only.

Pair Code must be linked to a pending device.

---

# 7. Table : Heartbeats

Purpose

Track device status.

Columns

| Column | Type |
|---------|------|
| id | uniqueidentifier |
| deviceId | uniqueidentifier |
| status | nvarchar(20) |
| agentVersion | nvarchar(20) |
| createdAt | datetime2 |

Example

online

offline

Indexes

deviceId

createdAt

---

# 8. Table : DeviceLogs

Purpose

Store Windows Agent logs.

Columns

| Column | Type |
|---------|------|
| id | uniqueidentifier |
| deviceId | uniqueidentifier |
| processName | nvarchar(255) |
| action | nvarchar(100) |
| mode | nvarchar(20) |
| message | nvarchar(max) |
| createdAt | datetime2 |

Example

steam.exe

blocked

study

Indexes

deviceId

createdAt

---

# 9. Future Tables

These tables are NOT part of Demo V1.

AppRules

WebsiteRules

Schedules

Notifications

Children

Permissions

---

# 10. Relationships

Users

1

↓

N

Devices

Devices

1

↓

N

Logs

Devices

1

↓

N

Heartbeats

Devices

1

↓

1

PairCodes

Modes

1

↓

N

Devices

---

# 11. Naming Convention

Tables

PascalCase

Users

Devices

DeviceLogs

Columns

camelCase

deviceId

createdAt

updatedAt

Foreign Keys

tableNameId

Examples

userId

deviceId

---

# 12. Date Policy

All dates stored in UTC.

Frontend converts to local timezone.

---

# 13. Delete Policy

Users

Soft Delete (Future)

Devices

Soft Delete (Future)

Logs

Never Delete Automatically

Heartbeats

Can be archived later

---

# 14. Database Rules

Never store

Plain Password

JWT

Refresh Token

Temporary Cache

Always store

Password Hash

Device Token

Created Time

Updated Time

---

# 15. Seed Data

Modes

fun

study

punishment

No additional modes.

---

# 16. Database Owner

Backend Developer

Current Owner

Trần Phúc Thịnh

Any schema changes require updating

DATABASE.md

API_SPEC.md

DECISIONS.md

before implementation.

Windows Agent must adapt through API only.

---

# 17. Demo V1 Minimum Tables

Users

Devices

Modes

PairCodes

Heartbeats

DeviceLogs

No additional tables unless approved.

---

# 18. Migration Policy

Every schema change must create a new Entity Framework Migration.

Migration naming examples

AddDeviceToken

CreateHeartbeatsTable

CreateDeviceLogs

Never edit old migrations after they have been shared with the team.

---

# 19. Performance Targets

Maximum login response

< 500 ms

Mode update

< 300 ms

Heartbeat insert

< 100 ms

Log insert

< 150 ms

---

# 20. Future Scalability

Database is designed to support

- Multiple parents
- Multiple children
- Multiple devices per parent
- Android/iOS clients
- Future Web Dashboard
- AI Recommendation Engine

Current Demo V1 only implements the minimum required schema.
