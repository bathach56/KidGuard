using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KidGuard.Api.Common;
using KidGuard.Api.Contracts.Devices;
using KidGuard.Api.Data;
using KidGuard.Api.Services.Pairing;
using Microsoft.EntityFrameworkCore;

namespace KidGuard.Api.Endpoints;

public static class DeviceEndpoints
{
    private static readonly string[] AllowedModes = ["fun", "study", "punishment"];

    public static RouteGroupBuilder MapDeviceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/devices").WithTags("Devices");

        group.MapGet("/", GetDevicesAsync)
            .RequireAuthorization()
            .WithName("GetDevices")
            .WithOpenApi();

        group.MapGet("/{deviceId:guid}", GetDeviceAsync)
            .RequireAuthorization()
            .WithName("GetDevice")
            .WithOpenApi();

        group.MapPut("/{deviceId:guid}/mode", UpdateDeviceModeAsync)
            .RequireAuthorization()
            .WithName("UpdateDeviceMode")
            .WithOpenApi();

        group.MapPost("/pair", PairDeviceAsync)
            .RequireAuthorization()
            .WithName("PairDevice")
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
                .Where(device => device.UserId == userId)
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
                .Where(item => item.Id == deviceId && item.UserId == userId)
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
                .FirstOrDefaultAsync(item => item.Id == deviceId && item.UserId == userId, cancellationToken);

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
