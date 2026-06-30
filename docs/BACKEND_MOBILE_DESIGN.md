# BACKEND_MOBILE_DESIGN.md

# Backend And Mobile Design

Version: 1.0.0

---

# Purpose

This document defines the Backend and Mobile design for Demo V1.

Backend and Mobile are owned by Trần Phúc Thịnh.

This document helps the Backend/Mobile developer start implementation without changing the agreed architecture.

---

# Owner

Name

Trần Phúc Thịnh

Responsibilities

- ASP.NET Core Web API
- SQL Server
- Entity Framework Core
- Authentication
- Authorization
- API Specification
- Swagger
- Flutter Mobile
- Mobile API integration

---

# Backend Responsibilities

Backend must:

- Authenticate parents.
- Authorize parent requests.
- Authorize Windows Agent requests.
- Own all database access.
- Create and validate pair codes.
- Pair devices with parents.
- Store current device mode.
- Store heartbeat records.
- Store device logs.
- Return API responses using the standard format in API_SPEC.md.

Backend must never:

- Kill Windows processes.
- Execute Windows commands.
- Store password in plain text.
- Expose another parent's devices or logs.
- Return raw API arrays or objects outside the response standard.
- Allow Mobile or Windows Agent to access SQL Server directly.

---

# Mobile Responsibilities

Mobile must:

- Allow parent login.
- Store JWT securely.
- Show device list.
- Pair device by pair code.
- Show device detail.
- Change device mode.
- Show basic device status.
- Show basic logs for Demo V1.

Mobile must never:

- Store business logic inside UI widgets.
- Access SQL Server directly.
- Communicate directly with Windows Agent.
- Hardcode API URL, JWT, or secrets.
- Define API behavior independently from API_SPEC.md.

---

# Recommended Backend Layers

## Controllers

Responsibilities:

- Receive HTTP requests.
- Validate request model shape.
- Call services.
- Return standard API responses.

Controllers should stay thin.

---

## Services

Responsibilities:

- Authentication logic.
- Device ownership rules.
- Pairing rules.
- Mode update rules.
- Heartbeat handling.
- Log handling.

Business logic belongs here.

---

## Repositories

Responsibilities:

- Read and write SQL Server data.
- Use Entity Framework Core.
- Avoid business logic.

Repositories should not decide whether a parent can access a device. That belongs in services.

---

## DTOs

Responsibilities:

- Define request models.
- Define response models.
- Prevent exposing database entities directly.

Examples:

- LoginRequest
- LoginResponse
- CreatePairCodeRequest
- PairDeviceRequest
- DeviceResponse
- ModeResponse
- HeartbeatRequest
- DeviceLogRequest

---

## Middleware

Responsibilities:

- Handle unexpected errors.
- Return safe error responses.
- Avoid exposing stack traces.
- Add request logging if needed.

---

# Recommended Backend Services

## AuthenticationService

Responsibilities:

- Validate parent email and password.
- Generate JWT.
- Hash and verify passwords with BCrypt.
- Reject invalid credentials safely.

---

## DeviceService

Responsibilities:

- Return devices owned by the current parent.
- Return a single device.
- Remove paired device.
- Validate parent ownership.

---

## PairingService

Responsibilities:

- Create pair code for unpaired Windows Agent.
- Validate Setup Token.
- Expire pair code after 10 minutes.
- Mark pair code as used after successful pairing.
- Generate Device Token after parent pairs device.
- Bind device to parent.

Security Rule

Device Token must only be returned once after successful pairing.

---

## ModeService

Responsibilities:

- Validate mode values.
- Allow only `fun`, `study`, and `punishment`.
- Update current device mode.
- Return current mode to Windows Agent.

---

## HeartbeatService

Responsibilities:

- Validate Device Token.
- Store heartbeat record.
- Update device `lastSeen`.
- Update device online status.

---

## DeviceLogService

Responsibilities:

- Validate Device Token for Agent log upload.
- Store process log.
- Return logs to the parent.
- Ensure parent can only read own device logs.

---

# Recommended Database Entities

Demo V1 minimum entities:

- User
- Device
- Mode
- PairCode
- Heartbeat
- DeviceLog

Rules:

- Store all dates in UTC.
- Store password as BCrypt hash.
- Store only the allowed mode values.
- Do not store JWT.
- Device Token may be stored hashed or encrypted if possible.

---

# Required API Endpoints For Demo V1

Backend must implement:

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

API behavior must match API_SPEC.md.

---

# Recommended Mobile Structure

## Screens

- LoginScreen
- DeviceListScreen
- DeviceDetailScreen
- PairDeviceScreen
- DeviceLogsScreen

---

## Services

- ApiClient
- AuthService
- DeviceApiService
- SecureStorageService

---

## Models

- LoginRequest
- LoginResponse
- DeviceModel
- ModeModel
- DeviceLogModel
- ApiResponse

---

## State Management

Demo V1 can use a simple state management approach.

Recommended:

- Provider

Alternative:

- Riverpod

Do not add complex state management unless the team agrees.

---

# Mobile User Flow

## Login Flow

1. Parent enters email and password.
2. Mobile calls `POST /auth/login`.
3. Backend returns JWT.
4. Mobile stores JWT securely.
5. Mobile opens device list.

---

## Pair Device Flow

1. Parent opens pair device screen.
2. Parent enters pair code shown by Windows Agent.
3. Mobile calls `POST /devices/pair`.
4. Backend pairs device.
5. Mobile refreshes device list.

---

## Change Mode Flow

1. Parent opens device detail.
2. Parent selects `fun`, `study`, or `punishment`.
3. Mobile calls `PUT /devices/{deviceId}/mode`.
4. Backend stores new mode.
5. Mobile shows updated mode.

---

## View Logs Flow

1. Parent opens device logs.
2. Mobile calls `GET /devices/{deviceId}/logs`.
3. Backend returns paged logs.
4. Mobile displays process name, action, mode, and created time.

---

# Demo V1 Build Order

Backend first:

1. Create ASP.NET Core solution.
2. Configure SQL Server.
3. Create entities and DbContext.
4. Create migration.
5. Implement login.
6. Implement pair-code creation.
7. Implement device pairing.
8. Implement mode endpoints.
9. Implement heartbeat endpoint.
10. Implement log endpoints.
11. Add Swagger.

Mobile second:

1. Create Flutter project.
2. Create API client.
3. Create login screen.
4. Create secure token storage.
5. Create device list screen.
6. Create pair device screen.
7. Create device detail and mode switch.
8. Create basic logs screen.

Integration third:

1. Test login.
2. Test pair device.
3. Test mode update.
4. Test Agent mode sync.
5. Test heartbeat display.
6. Test log display.

---

# Backend Definition Of Done

Backend work is complete for Demo V1 when:

- [ ] Parent can login.
- [ ] JWT is generated and validated.
- [ ] Pair code can be created by Setup Token.
- [ ] Parent can pair device.
- [ ] Device Token is generated after pairing.
- [ ] Parent can view own devices.
- [ ] Parent can change mode.
- [ ] Agent can read current mode.
- [ ] Agent can send heartbeat.
- [ ] Agent can upload logs.
- [ ] Parent can view logs.
- [ ] Swagger documents required endpoints.
- [ ] API responses match API_SPEC.md.

---

# Mobile Definition Of Done

Mobile work is complete for Demo V1 when:

- [ ] Parent can login from Mobile.
- [ ] JWT is stored securely.
- [ ] Device list is displayed.
- [ ] Parent can pair device by pair code.
- [ ] Parent can open device detail.
- [ ] Parent can switch mode.
- [ ] Parent can see device status.
- [ ] Parent can see basic logs.
- [ ] Mobile handles loading state.
- [ ] Mobile handles API error messages.

---

# Coordination Rules

Backend/Mobile developer must coordinate with Windows Agent developer when:

- Changing API request or response body.
- Changing authentication headers.
- Changing mode values.
- Changing heartbeat interval.
- Changing pairing flow.
- Changing log fields.

Any change above requires updating API_SPEC.md and DECISIONS.md if it affects architecture or security.
