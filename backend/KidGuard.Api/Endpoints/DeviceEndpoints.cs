using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KidGuard.Api.Common;
using KidGuard.Api.Contracts.Devices;
using KidGuard.Api.Data;
using KidGuard.Api.Entities;
using KidGuard.Api.Services.Pairing;
using Microsoft.EntityFrameworkCore;

namespace KidGuard.Api.Endpoints;

public static class DeviceEndpoints
{
    private const string BearerPrefix = "Bearer ";
    private const int NextHeartbeatSeconds = 30;
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private static readonly string[] AllowedModes = ["fun", "study", "punishment"];

    public static RouteGroupBuilder MapDeviceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/devices").WithTags("Devices");

        group.MapGet("/", GetDevicesAsync)
            .RequireAuthorization()
            .WithName("GetDevices")
            .WithSummary("Get parent devices")
            .WithDescription("Returns approved devices owned by the authenticated parent in the Version 1.0.1 approval-based pairing flow. Requires JWT.")
            .WithOpenApi();

        group.MapGet("/{deviceId:guid}", GetDeviceAsync)
            .RequireAuthorization()
            .WithName("GetDevice")
            .WithSummary("Get device detail")
            .WithDescription("Returns one approved device owned by the authenticated parent. Requires JWT.")
            .WithOpenApi();

        group.MapGet("/{deviceId:guid}/mode", GetDeviceModeAsync)
            .WithName("GetDeviceMode")
            .WithSummary("Sync device mode")
            .WithDescription("Approved Windows Service retrieves its current mode after child-approved pairing. Requires Device Token.")
            .WithOpenApi();

        group.MapPut("/{deviceId:guid}/mode", UpdateDeviceModeAsync)
            .RequireAuthorization()
            .WithName("UpdateDeviceMode")
            .WithSummary("Update device mode")
            .WithDescription("Parent changes mode only for an approved owned device. Allowed modes: fun, study, punishment. Requires JWT.")
            .WithOpenApi();

        group.MapPost("/{deviceId:guid}/heartbeat", ReceiveHeartbeatAsync)
            .WithName("ReceiveHeartbeat")
            .WithSummary("Receive device heartbeat")
            .WithDescription("Approved Windows Service reports online status and version after pairing. Requires Device Token.")
            .WithOpenApi();

        group.MapPost("/{deviceId:guid}/logs", UploadDeviceLogAsync)
            .WithName("UploadDeviceLog")
            .WithSummary("Upload device log")
            .WithDescription("Approved Windows Service uploads activity or blocking logs after pairing. Requires Device Token.")
            .WithOpenApi();

        group.MapGet("/{deviceId:guid}/logs", GetDeviceLogsAsync)
            .RequireAuthorization()
            .WithName("GetDeviceLogs")
            .WithSummary("Get device logs")
            .WithDescription("Returns paginated logs for an approved device owned by the authenticated parent. Requires JWT.")
            .WithOpenApi();

        group.MapPost("/pair", PairDeviceAsync)
            .RequireAuthorization()
            .WithName("PairDevice")
            .WithSummary("Pair device")
            .WithDescription("Legacy Demo V1 direct pair endpoint. Version 1.0.1 clients should use /pairing approval APIs. Requires JWT.")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> GetDevicesAsync(
        ClaimsPrincipal currentUser,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("DeviceEndpoints");

        if (!TryGetCurrentUserId(currentUser, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var devices = await dbContext.Devices
                .AsNoTracking()
                .Where(device => device.UserId == userId
                    && device.PairingRequests.Any(request => request.ParentId == userId
                        && request.Status == PairingRequestStatuses.Approved))
                .OrderBy(device => device.DeviceName)
                .Select(device => new DeviceListItemResponse(
                    device.Id,
                    device.DeviceName,
                    device.CurrentMode,
                    device.IsOnline,
                    device.LastSeen))
                .ToListAsync(cancellationToken);

            return Results.Ok(ApiResponse<DeviceListResponse>.Ok(
                "Devices retrieved.",
                new DeviceListResponse(devices)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected device list error for user {UserId}.", userId);

            return ServerError();
        }
    }

    private static async Task<IResult> GetDeviceAsync(
        Guid deviceId,
        ClaimsPrincipal currentUser,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("DeviceEndpoints");

        if (!TryGetCurrentUserId(currentUser, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var device = await dbContext.Devices
                .AsNoTracking()
                .Where(item => item.Id == deviceId
                    && item.UserId == userId
                    && item.PairingRequests.Any(request => request.ParentId == userId
                        && request.Status == PairingRequestStatuses.Approved))
                .Select(item => new DeviceDetailResponse(
                    item.Id,
                    item.DeviceName,
                    item.CurrentMode,
                    item.IsOnline,
                    item.LastSeen))
                .FirstOrDefaultAsync(cancellationToken);

            if (device is null)
            {
                return DeviceNotFound();
            }

            return Results.Ok(ApiResponse<DeviceDetailResponse>.Ok("Device retrieved.", device));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected device detail error for device {DeviceId} and user {UserId}.", deviceId, userId);

            return ServerError();
        }
    }

    private static async Task<IResult> GetDeviceModeAsync(
        Guid deviceId,
        HttpContext httpContext,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("DeviceEndpoints");

        if (!TryGetBearerToken(httpContext.Request.Headers.Authorization, out var deviceToken))
        {
            return Unauthorized();
        }

        try
        {
            var device = await dbContext.Devices
                .AsNoTracking()
                .Where(item => item.Id == deviceId
                    && item.DeviceToken == deviceToken
                    && item.UserId != null
                    && item.PairingRequests.Any(request => request.Status == PairingRequestStatuses.Approved))
                .Select(item => new DeviceModeResponse(item.CurrentMode, item.UpdatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            if (device is null)
            {
                return DeviceNotFound();
            }

            return Results.Ok(ApiResponse<DeviceModeResponse>.Ok("Mode retrieved.", device));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected mode sync error for device {DeviceId}.", deviceId);

            return ServerError();
        }
    }

    private static async Task<IResult> UpdateDeviceModeAsync(
        Guid deviceId,
        UpdateDeviceModeRequest request,
        ClaimsPrincipal currentUser,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("DeviceEndpoints");

        if (!TryGetCurrentUserId(currentUser, out var userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Mode))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid mode request.",
                new[] { "Mode is required." }));
        }

        var normalizedMode = request.Mode.Trim().ToLowerInvariant();
        if (!AllowedModes.Contains(normalizedMode))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid mode.",
                new[] { "MODE001" }));
        }

        try
        {
            var device = await dbContext.Devices
                .FirstOrDefaultAsync(item => item.Id == deviceId
                    && item.UserId == userId
                    && item.PairingRequests.Any(request => request.ParentId == userId
                        && request.Status == PairingRequestStatuses.Approved), cancellationToken);

            if (device is null)
            {
                return DeviceNotFound();
            }

            var updatedAt = DateTime.UtcNow;
            device.CurrentMode = normalizedMode;
            device.UpdatedAt = updatedAt;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(ApiResponse<UpdateDeviceModeResponse>.Ok(
                "Mode updated.",
                new UpdateDeviceModeResponse(device.CurrentMode, updatedAt)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected mode update error for device {DeviceId} and user {UserId}.", deviceId, userId);

            return ServerError();
        }
    }

    private static async Task<IResult> ReceiveHeartbeatAsync(
        Guid deviceId,
        HeartbeatRequest request,
        HttpContext httpContext,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("DeviceEndpoints");

        if (!TryGetBearerToken(httpContext.Request.Headers.Authorization, out var deviceToken))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Status) || string.IsNullOrWhiteSpace(request.AgentVersion))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid heartbeat request.",
                new[] { "Status and agent version are required." }));
        }

        try
        {
            var device = await dbContext.Devices
                .FirstOrDefaultAsync(item => item.Id == deviceId
                    && item.DeviceToken == deviceToken
                    && item.UserId != null
                    && item.PairingRequests.Any(request => request.Status == PairingRequestStatuses.Approved), cancellationToken);

            if (device is null)
            {
                return DeviceNotFound();
            }

            var now = DateTime.UtcNow;
            device.IsOnline = string.Equals(request.Status.Trim(), "online", StringComparison.OrdinalIgnoreCase);
            device.LastSeen = now;
            device.UpdatedAt = now;

            dbContext.Heartbeats.Add(new Heartbeat
            {
                Id = Guid.NewGuid(),
                DeviceId = device.Id,
                Status = request.Status.Trim().ToLowerInvariant(),
                AgentVersion = request.AgentVersion.Trim(),
                CreatedAt = now
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(ApiResponse<HeartbeatResponse>.Ok(
                "Heartbeat received.",
                new HeartbeatResponse(NextHeartbeatSeconds)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected heartbeat error for device {DeviceId}.", deviceId);

            return ServerError();
        }
    }

    private static async Task<IResult> UploadDeviceLogAsync(
        Guid deviceId,
        UploadDeviceLogRequest request,
        HttpContext httpContext,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("DeviceEndpoints");

        if (!TryGetBearerToken(httpContext.Request.Headers.Authorization, out var deviceToken))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.ProcessName)
            || string.IsNullOrWhiteSpace(request.Action)
            || string.IsNullOrWhiteSpace(request.Mode)
            || string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid log request.",
                new[] { "Process name, action, mode, and message are required." }));
        }

        var normalizedMode = request.Mode.Trim().ToLowerInvariant();
        if (!AllowedModes.Contains(normalizedMode))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid mode.",
                new[] { "MODE001" }));
        }

        try
        {
            var deviceExists = await dbContext.Devices
                .AnyAsync(item => item.Id == deviceId
                    && item.DeviceToken == deviceToken
                    && item.UserId != null
                    && item.PairingRequests.Any(request => request.Status == PairingRequestStatuses.Approved), cancellationToken);

            if (!deviceExists)
            {
                return DeviceNotFound();
            }

            dbContext.DeviceLogs.Add(new DeviceLog
            {
                Id = Guid.NewGuid(),
                DeviceId = deviceId,
                ProcessName = request.ProcessName.Trim(),
                Action = request.Action.Trim(),
                Mode = normalizedMode,
                Message = request.Message.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(ApiResponse<object>.Ok("Log uploaded.", new { }));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected log upload error for device {DeviceId}.", deviceId);

            return ServerError();
        }
    }


    private static async Task<IResult> GetDeviceLogsAsync(
        Guid deviceId,
        int? page,
        int? pageSize,
        ClaimsPrincipal currentUser,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("DeviceEndpoints");

        if (!TryGetCurrentUserId(currentUser, out var userId))
        {
            return Unauthorized();
        }

        var pageNumber = page.GetValueOrDefault(DefaultPage);
        var size = pageSize.GetValueOrDefault(DefaultPageSize);

        if (pageNumber < 1 || size < 1 || size > MaxPageSize)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid pagination request.",
                new[] { "Page must be at least 1 and pageSize must be between 1 and 100." }));
        }

        try
        {
            var deviceExists = await dbContext.Devices
                .AsNoTracking()
                .AnyAsync(item => item.Id == deviceId
                    && item.UserId == userId
                    && item.PairingRequests.Any(request => request.ParentId == userId
                        && request.Status == PairingRequestStatuses.Approved), cancellationToken);

            if (!deviceExists)
            {
                return DeviceNotFound();
            }

            var query = dbContext.DeviceLogs
                .AsNoTracking()
                .Where(item => item.DeviceId == deviceId);

            var total = await query.CountAsync(cancellationToken);
            var logs = await query
                .OrderByDescending(item => item.CreatedAt)
                .Skip((pageNumber - 1) * size)
                .Take(size)
                .Select(item => new DeviceLogItemResponse(
                    item.ProcessName,
                    item.Action,
                    item.Mode,
                    item.CreatedAt))
                .ToListAsync(cancellationToken);

            return Results.Ok(ApiResponse<DeviceLogsResponse>.Ok(
                "Logs retrieved.",
                new DeviceLogsResponse(logs, total)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected log view error for device {DeviceId} and user {UserId}.", deviceId, userId);

            return ServerError();
        }
    }
    private static async Task<IResult> PairDeviceAsync(
        PairDeviceRequest request,
        ClaimsPrincipal currentUser,
        ApplicationDbContext dbContext,
        IDeviceTokenGenerator deviceTokenGenerator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("DeviceEndpoints");

        if (!TryGetCurrentUserId(currentUser, out var userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.PairCode))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid pair request.",
                new[] { "Pair code is required." }));
        }

        try
        {
            var normalizedPairCode = request.PairCode.Trim().ToUpperInvariant();
            var now = DateTime.UtcNow;

            var pairCode = await dbContext.PairCodes
                .Include(item => item.Device)
                .FirstOrDefaultAsync(
                    item => item.Code == normalizedPairCode
                        && !item.IsUsed
                        && item.ExpiresAt > now,
                    cancellationToken);

            if (pairCode is null || pairCode.Device.UserId is not null)
            {
                return Results.Json(
                    ApiResponse<object>.Fail("Invalid pair code.", new[] { "PAIR001" }),
                    statusCode: StatusCodes.Status404NotFound);
            }

            var deviceToken = await GenerateUniqueDeviceTokenAsync(
                dbContext,
                deviceTokenGenerator,
                cancellationToken);

            pairCode.IsUsed = true;
            pairCode.Device.UserId = userId;
            pairCode.Device.DeviceToken = deviceToken;
            pairCode.Device.UpdatedAt = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            var response = new PairDeviceResponse(
                pairCode.Device.Id,
                pairCode.Device.DeviceName,
                pairCode.Device.CurrentMode,
                deviceToken);

            return Results.Ok(ApiResponse<PairDeviceResponse>.Ok("Device paired.", response));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected device pair error for user {UserId}.", userId);

            return ServerError();
        }
    }

    private static bool TryGetCurrentUserId(ClaimsPrincipal currentUser, out Guid userId)
    {
        var userIdValue = currentUser.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out userId);
    }

    private static bool TryGetBearerToken(string? authorizationHeader, out string token)
    {
        token = string.Empty;

        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        token = authorizationHeader[BearerPrefix.Length..].Trim();
        return !string.IsNullOrWhiteSpace(token);
    }

    private static async Task<string> GenerateUniqueDeviceTokenAsync(
        ApplicationDbContext dbContext,
        IDeviceTokenGenerator deviceTokenGenerator,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var deviceToken = deviceTokenGenerator.Generate();
            var exists = await dbContext.Devices
                .AnyAsync(item => item.DeviceToken == deviceToken, cancellationToken);

            if (!exists)
            {
                return deviceToken;
            }
        }

        throw new InvalidOperationException("Could not generate a unique device token.");
    }

    private static IResult Unauthorized()
    {
        return Results.Json(
            ApiResponse<object>.Fail("Unauthorized.", new[] { "AUTH001" }),
            statusCode: StatusCodes.Status401Unauthorized);
    }

    private static IResult DeviceNotFound()
    {
        return Results.Json(
            ApiResponse<object>.Fail("Device not found.", new[] { "DEVICE001" }),
            statusCode: StatusCodes.Status404NotFound);
    }

    private static IResult ServerError()
    {
        return Results.Json(
            ApiResponse<object>.Fail("Something went wrong.", new[] { "SERVER001" }),
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
