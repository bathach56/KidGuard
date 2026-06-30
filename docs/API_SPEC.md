# API_SPEC.md

# Parental Control System API

Version: 1.0.0

Current Version

v1

Base URL

https://your-domain.com/api/v1

Local Development

https://localhost:5001/api/v1

Content-Type

application/json

---

# Authentication

Parent

Authorization: Bearer <JWT>

Windows Agent

Authorization: Bearer <DeviceToken>

Pair Code Creation

Authorization: Bearer <SetupToken>

The Setup Token is only used before the device is paired. It must come from configuration or installation input and must not be hardcoded in source code.

---

# Response Standard

Every API must follow this response structure.

Success

```json
{
  "success": true,
  "message": "Success",
  "data": {}
}
```

Error

```json
{
  "success": false,
  "message": "Something went wrong.",
  "errors": []
}
```

Do not return raw arrays or raw objects directly. Always wrap response data inside `data`.

---

# Authentication

## POST /auth/login

Description

Parent login.

Authorization

None

Request

```json
{
  "email": "parent@gmail.com",
  "password": "12345678"
}
```

Response

```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "accessToken": "JWT",
    "expiresIn": 3600
  }
}
```

---

## POST /auth/refresh

Description

Refresh JWT.

Authorization

JWT

---

## POST /auth/logout

Description

Invalidate current token.

Authorization

JWT

---

# Device Pairing Flow

Pairing must follow this flow in Demo V1.

1. Windows Agent starts with a configured Setup Token.
2. Windows Agent calls `POST /pair-code` to create a temporary pair code.
3. Parent enters the pair code in Mobile.
4. Mobile calls `POST /devices/pair` with JWT.
5. Backend binds the device to the parent and returns the Device Token.
6. Windows Agent stores the Device Token securely and uses it for heartbeat, mode sync, and log upload.

The Setup Token must only allow pair-code creation. It must not allow mode sync, heartbeat, log upload, or access to parent data.

---

# Pair Code

## POST /pair-code

Description

Create a temporary pair code for a Windows Agent before it is paired.

Authorization

Setup Token

Request

```json
{
  "deviceName": "Study PC",
  "computerName": "DESKTOP-001",
  "agentVersion": "1.0.0"
}
```

Response

```json
{
  "success": true,
  "message": "Pair code created.",
  "data": {
    "pairCode": "ABCD1234",
    "expiresIn": 600
  }
}
```

Pair Code Rules

- Length: 8 characters
- Expiration: 10 minutes
- One-time use only

---

# Devices

## GET /devices

Description

Get all devices of the current parent.

Authorization

JWT

Response

```json
{
  "success": true,
  "message": "Devices retrieved.",
  "data": {
    "items": [
      {
        "deviceId": "00000000-0000-0000-0000-000000000000",
        "deviceName": "Study PC",
        "mode": "study",
        "isOnline": true,
        "lastSeen": "2026-06-28T10:00:00Z"
      }
    ]
  }
}
```

---

## GET /devices/{deviceId}

Description

Return a single device owned by the current parent.

Authorization

JWT

Response

```json
{
  "success": true,
  "message": "Device retrieved.",
  "data": {
    "deviceId": "00000000-0000-0000-0000-000000000000",
    "deviceName": "Study PC",
    "mode": "study",
    "isOnline": true,
    "lastSeen": "2026-06-28T10:00:00Z"
  }
}
```

---

## POST /devices/pair

Description

Pair Windows Agent with the current parent.

Authorization

JWT

Request

```json
{
  "pairCode": "ABCD1234"
}
```

Response

```json
{
  "success": true,
  "message": "Device paired.",
  "data": {
    "deviceId": "00000000-0000-0000-0000-000000000000",
    "deviceName": "Study PC",
    "mode": "fun",
    "deviceToken": "DeviceToken"
  }
}
```

Security Rule

The Device Token must only be returned once after successful pairing.

---

## DELETE /devices/{deviceId}

Description

Remove paired device.

Authorization

JWT

Response

```json
{
  "success": true,
  "message": "Device removed.",
  "data": {}
}
```

---

# Device Mode

## GET /devices/{deviceId}/mode

Description

Windows Agent retrieves the current mode.

Authorization

Device Token

Response

```json
{
  "success": true,
  "message": "Mode retrieved.",
  "data": {
    "mode": "study",
    "updatedAt": "2026-06-28T10:00:00Z"
  }
}
```

---

## PUT /devices/{deviceId}/mode

Description

Parent changes the current device mode.

Authorization

JWT

Request

```json
{
  "mode": "punishment"
}
```

Allowed Values

- fun
- study
- punishment

Response

```json
{
  "success": true,
  "message": "Mode updated.",
  "data": {
    "mode": "punishment",
    "updatedAt": "2026-06-28T10:00:00Z"
  }
}
```

---

# Heartbeat

## POST /devices/{deviceId}/heartbeat

Description

Windows Agent reports that it is online.

Authorization

Device Token

Request

```json
{
  "status": "online",
  "agentVersion": "1.0.0"
}
```

Response

```json
{
  "success": true,
  "message": "Heartbeat received.",
  "data": {
    "nextHeartbeat": 30
  }
}
```

Heartbeat Interval

30 seconds

---

# Logs

## POST /devices/{deviceId}/logs

Description

Windows Agent uploads activity or blocking logs.

Authorization

Device Token

Request

```json
{
  "processName": "notepad.exe",
  "action": "blocked",
  "mode": "study",
  "message": "Blocked by Study Mode"
}
```

Response

```json
{
  "success": true,
  "message": "Log uploaded.",
  "data": {}
}
```

---

## GET /devices/{deviceId}/logs

Description

Parent views device logs.

Authorization

JWT

Query

?page=1&pageSize=20

Response

```json
{
  "success": true,
  "message": "Logs retrieved.",
  "data": {
    "items": [
      {
        "processName": "notepad.exe",
        "action": "blocked",
        "mode": "study",
        "createdAt": "2026-06-28T10:30:00Z"
      }
    ],
    "total": 100
  }
}
```

---

# Health

## GET /health

Description

Check API health.

Authorization

None

Response

```json
{
  "success": true,
  "message": "API is healthy.",
  "data": {
    "status": "healthy",
    "version": "1.0.0"
  }
}
```

---

# HTTP Status Code

200

Success

201

Created

204

No Content

400

Bad Request

401

Unauthorized

403

Forbidden

404

Not Found

409

Conflict

500

Internal Server Error

---

# Error Code

AUTH001

Invalid Login

AUTH002

Expired JWT

DEVICE001

Device Not Found

DEVICE002

Device Offline

PAIR001

Invalid Pair Code

PAIR002

Pair Code Expired

MODE001

Invalid Mode

SERVER001

Unexpected Error

---

# Validation

Email

Required

Password

Required, minimum 8 characters

Pair Code

Required

Mode

Must be one of:

- fun
- study
- punishment

DeviceId

Must be Guid

---

# Rate Limit

Login

10 requests / minute

Pair Code

5 requests / minute

Heartbeat

1 request / 30 seconds

Logs

Unlimited for Demo V1

Mode Change

30 requests / minute

---

# API Versioning

Current

v1

Future

v2

Old versions remain supported until officially deprecated.

---

# API Ownership

Backend Owner

Trần Phúc Thịnh

Windows Agent Consumer

Phạm Bá Thạch

No endpoint may be modified without updating API_SPEC.md and notifying both developers.

---

# Integration Rules

Backend owns API.

Windows Agent consumes API.

Mobile consumes API.

Neither Mobile nor Agent defines API behavior.

Backend is the source of truth.

---

# Demo V1 Required Endpoints

- POST /auth/login
- POST /pair-code
- GET /devices
- GET /devices/{deviceId}
- POST /devices/pair
- GET /devices/{deviceId}/mode
- PUT /devices/{deviceId}/mode
- POST /devices/{deviceId}/heartbeat
- POST /devices/{deviceId}/logs
- GET /devices/{deviceId}/logs
- GET /health

No additional endpoints are required for Demo V1.
