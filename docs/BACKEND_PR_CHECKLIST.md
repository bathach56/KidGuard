# Backend PR Checklist

Owner: Tran Phuc Thinh

Branch: `feature/backend/tran-phuc-thinh-initial-project`

Scope:

- ASP.NET Core Web API backend foundation.
- Entity Framework Core with SQL Server.
- Initial database migration and seed modes.
- JWT parent authentication.
- Setup Token pair-code creation.
- Device Token agent endpoints.
- Swagger documentation.

Verification completed:

- [x] `dotnet build backend/KidGuard.Backend.sln`
- [x] EF Core migration applies to SQL Server LocalDB.
- [x] `GET /health`
- [x] `POST /auth/login`
- [x] `POST /pair-code`
- [x] `POST /devices/pair`
- [x] `GET /devices`
- [x] `GET /devices/{deviceId}`
- [x] `PUT /devices/{deviceId}/mode`
- [x] `GET /devices/{deviceId}/mode`
- [x] `POST /devices/{deviceId}/heartbeat`
- [x] `POST /devices/{deviceId}/logs`
- [x] `GET /devices/{deviceId}/logs`
- [x] `GET /swagger/v1/swagger.json`

Review notes:

- Do not commit local secrets. Use environment variables for `Jwt__Secret` and `SetupToken__Token`.
- Do not merge directly to `main`.
- Open a pull request from this branch for team review.
- Keep API responses in camelCase JSON.
- Keep supported modes as exactly `fun`, `study`, and `punishment`.

Remaining before merge:

- [ ] Pull request created.
- [ ] Code reviewed by team.
- [ ] Merge target confirmed by team.
