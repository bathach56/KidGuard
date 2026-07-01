using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KidGuard.Api.Common;
using KidGuard.Api.Contracts.Pairing;
using KidGuard.Api.Data;
using KidGuard.Api.Entities;
using KidGuard.Api.Services.Pairing;
using Microsoft.EntityFrameworkCore;

namespace KidGuard.Api.Endpoints;

public static class PairingEndpoints
{
    private const int ConnectionCodeExpiresInSeconds = 600;

    public static RouteGroupBuilder MapPairingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/pairing").WithTags("Pairing");

        group.MapPost("/child/connection-code", CreateChildConnectionCodeAsync)
            .WithName("CreateChildConnectionCode")
            .WithSummary("Create child connection code")
            .WithDescription("Child Windows Client creates a temporary connection code before parent pairing. No parent login required.")
            .WithOpenApi();

        group.MapPost("/requests", CreateParentPairingRequestAsync)
            .RequireAuthorization()
            .WithName("CreateParentPairingRequest")
            .WithSummary("Create parent pairing request")
            .WithDescription("Parent enters a child connection code and creates a pending request. Requires JWT.")
            .WithOpenApi();

        group.MapGet("/child/pending", GetChildPendingRequestAsync)
            .WithName("GetChildPendingPairingRequest")
            .WithSummary("Get child pending pairing request")
            .WithDescription("Child Windows Client polls with its connection code to see pending parent requests.")
            .WithOpenApi();

        group.MapPost("/child/approve", ApprovePairingAsync)
            .WithName("ApprovePairing")
            .WithSummary("Approve pairing")
            .WithDescription("Child approves a pending pairing request and receives the Device Token once.")
            .WithOpenApi();

        group.MapPost("/child/reject", RejectPairingAsync)
            .WithName("RejectPairing")
            .WithSummary("Reject pairing")
            .WithDescription("Child rejects a pending pairing request.")
            .WithOpenApi();

        group.MapGet("/requests/{pairingRequestId:guid}/status", GetPairingStatusAsync)
            .RequireAuthorization()
            .WithName("GetPairingStatus")
            .WithSummary("Get pairing status")
            .WithDescription("Parent checks whether a pairing request is pending, approved, rejected, or expired. Requires JWT.")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> CreateChildConnectionCodeAsync(
        CreateChildConnectionCodeRequest request,
        ApplicationDbContext dbContext,
        IPairCodeGenerator pairCodeGenerator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("PairingEndpoints");
        var errors = ValidateChildConnectionCodeRequest(request);
        if (errors.Count > 0)
        {
            return Results.BadRequest(ApiResponse<object>.Fail("Invalid child connection code request.", errors));
        }

        try
        {
            var now = DateTime.UtcNow;
            var device = new Device
            {
                Id = Guid.NewGuid(),
                DeviceName = request.DeviceName.Trim(),
                ComputerName = request.ComputerName.Trim(),
                CurrentMode = "fun",
                IsOnline = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            var connectionCode = await GenerateUniqueConnectionCodeAsync(dbContext, pairCodeGenerator, cancellationToken);
            var expiresAt = now.AddSeconds(ConnectionCodeExpiresInSeconds);

            dbContext.Devices.Add(device);
            dbContext.PairCodes.Add(new PairCode
            {
                Id = Guid.NewGuid(),
                DeviceId = device.Id,
                Code = connectionCode,
                ExpiresAt = expiresAt,
                IsUsed = false,
                CreatedAt = now
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            var response = new CreateChildConnectionCodeResponse(
                device.Id,
                connectionCode,
                ConnectionCodeExpiresInSeconds,
                expiresAt);

            return Results.Ok(ApiResponse<CreateChildConnectionCodeResponse>.Ok("Connection code created.", response));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected child connection code error for {ComputerName}.", request.ComputerName);
            return ServerError();
        }
    }

    private static async Task<IResult> CreateParentPairingRequestAsync(
        CreateParentPairingRequest request,
        ClaimsPrincipal currentUser,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("PairingEndpoints");
        if (!TryGetCurrentUserId(currentUser, out var parentId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.ConnectionCode))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid pairing request.",
                new[] { "Connection code is required." }));
        }

        try
        {
            var now = DateTime.UtcNow;
            await ExpireOldPendingRequestsAsync(dbContext, now, cancellationToken);

            var normalizedCode = NormalizeCode(request.ConnectionCode);
            var pairCode = await dbContext.PairCodes
                .Include(item => item.Device)
                .FirstOrDefaultAsync(
                    item => item.Code == normalizedCode
                        && !item.IsUsed
                        && item.ExpiresAt > now,
                    cancellationToken);

            if (pairCode is null || pairCode.Device.UserId is not null)
            {
                return PairingCodeNotFound();
            }

            var existingPendingRequest = await dbContext.PairingRequests
                .Include(item => item.Device)
                .Where(item => item.ConnectionCode == normalizedCode
                    && item.Status == PairingRequestStatuses.Pending
                    && item.ExpiresAt > now)
                .OrderByDescending(item => item.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingPendingRequest is not null)
            {
                var existingResponse = new ParentPairingRequestResponse(
                    existingPendingRequest.Id,
                    existingPendingRequest.DeviceId,
                    existingPendingRequest.Device.DeviceName,
                    existingPendingRequest.Device.ComputerName,
                    existingPendingRequest.Status,
                    existingPendingRequest.ExpiresAt,
                    existingPendingRequest.CreatedAt);

                return Results.Conflict(ApiResponse<ParentPairingRequestResponse>.Ok(
                    "A pending pairing request already exists.",
                    existingResponse));
            }

            var pairingRequest = new PairingRequest
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                DeviceId = pairCode.DeviceId,
                ConnectionCode = normalizedCode,
                Status = PairingRequestStatuses.Pending,
                ExpiresAt = pairCode.ExpiresAt,
                CreatedAt = now,
                UpdatedAt = now
            };

            dbContext.PairingRequests.Add(pairingRequest);
            await dbContext.SaveChangesAsync(cancellationToken);

            var response = new ParentPairingRequestResponse(
                pairingRequest.Id,
                pairCode.Device.Id,
                pairCode.Device.DeviceName,
                pairCode.Device.ComputerName,
                pairingRequest.Status,
                pairingRequest.ExpiresAt,
                pairingRequest.CreatedAt);

            return Results.Created($"/pairing/requests/{pairingRequest.Id}/status", ApiResponse<ParentPairingRequestResponse>.Ok(
                "Pairing request created.",
                response));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected parent pairing request error for parent {ParentId}.", parentId);
            return ServerError();
        }
    }

    private static async Task<IResult> GetChildPendingRequestAsync(
        string connectionCode,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("PairingEndpoints");
        if (string.IsNullOrWhiteSpace(connectionCode))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid pending request poll.",
                new[] { "Connection code is required." }));
        }

        try
        {
            var now = DateTime.UtcNow;
            await ExpireOldPendingRequestsAsync(dbContext, now, cancellationToken);

            var normalizedCode = NormalizeCode(connectionCode);
            var pendingRequest = await dbContext.PairingRequests
                .AsNoTracking()
                .Include(item => item.Parent)
                .Include(item => item.Device)
                .Where(item => item.ConnectionCode == normalizedCode
                    && item.Status == PairingRequestStatuses.Pending
                    && item.ExpiresAt > now)
                .OrderByDescending(item => item.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendingRequest is null)
            {
                return Results.Ok(ApiResponse<ChildPendingPairingRequestResponse?>.Ok("No pending pairing request.", null));
            }

            var response = new ChildPendingPairingRequestResponse(
                pendingRequest.Id,
                pendingRequest.DeviceId,
                pendingRequest.Device.DeviceName,
                pendingRequest.Device.ComputerName,
                pendingRequest.Parent.FullName,
                pendingRequest.Parent.Email,
                pendingRequest.Status,
                pendingRequest.ExpiresAt,
                pendingRequest.CreatedAt);

            return Results.Ok(ApiResponse<ChildPendingPairingRequestResponse>.Ok("Pending pairing request retrieved.", response));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected child pending request poll error.");
            return ServerError();
        }
    }

    private static async Task<IResult> ApprovePairingAsync(
        ChildPairingDecisionRequest request,
        ApplicationDbContext dbContext,
        IDeviceTokenGenerator deviceTokenGenerator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("PairingEndpoints");
        var validationResult = ValidateChildDecisionRequest(request);
        if (validationResult is not null)
        {
            return validationResult;
        }

        try
        {
            var now = DateTime.UtcNow;
            await ExpireOldPendingRequestsAsync(dbContext, now, cancellationToken);
            var normalizedCode = NormalizeCode(request.ConnectionCode);

            var pairingRequest = await dbContext.PairingRequests
                .Include(item => item.Device)
                .FirstOrDefaultAsync(
                    item => item.Id == request.PairingRequestId
                        && item.ConnectionCode == normalizedCode,
                    cancellationToken);

            if (pairingRequest is null)
            {
                return PairingRequestNotFound();
            }

            if (pairingRequest.Status != PairingRequestStatuses.Pending || pairingRequest.ExpiresAt <= now)
            {
                return InvalidPairingState();
            }

            var pairCode = await dbContext.PairCodes
                .FirstOrDefaultAsync(
                    item => item.Code == normalizedCode
                        && item.DeviceId == pairingRequest.DeviceId
                        && !item.IsUsed
                        && item.ExpiresAt > now,
                    cancellationToken);

            if (pairCode is null || pairingRequest.Device.UserId is not null)
            {
                return InvalidPairingState();
            }

            var deviceToken = await GenerateUniqueDeviceTokenAsync(dbContext, deviceTokenGenerator, cancellationToken);

            pairCode.IsUsed = true;
            pairingRequest.Status = PairingRequestStatuses.Approved;
            pairingRequest.ApprovedAt = now;
            pairingRequest.UpdatedAt = now;
            pairingRequest.Device.UserId = pairingRequest.ParentId;
            pairingRequest.Device.DeviceToken = deviceToken;
            pairingRequest.Device.UpdatedAt = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            var response = new ChildApprovePairingResponse(
                pairingRequest.Id,
                pairingRequest.DeviceId,
                pairingRequest.Device.DeviceName,
                pairingRequest.Device.CurrentMode,
                pairingRequest.Status,
                deviceToken);

            return Results.Ok(ApiResponse<ChildApprovePairingResponse>.Ok("Pairing approved.", response));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected approve pairing error for request {PairingRequestId}.", request.PairingRequestId);
            return ServerError();
        }
    }

    private static async Task<IResult> RejectPairingAsync(
        ChildPairingDecisionRequest request,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("PairingEndpoints");
        var validationResult = ValidateChildDecisionRequest(request);
        if (validationResult is not null)
        {
            return validationResult;
        }

        try
        {
            var now = DateTime.UtcNow;
            await ExpireOldPendingRequestsAsync(dbContext, now, cancellationToken);
            var normalizedCode = NormalizeCode(request.ConnectionCode);

            var pairingRequest = await dbContext.PairingRequests
                .FirstOrDefaultAsync(
                    item => item.Id == request.PairingRequestId
                        && item.ConnectionCode == normalizedCode,
                    cancellationToken);

            if (pairingRequest is null)
            {
                return PairingRequestNotFound();
            }

            if (pairingRequest.Status != PairingRequestStatuses.Pending || pairingRequest.ExpiresAt <= now)
            {
                return InvalidPairingState();
            }

            pairingRequest.Status = PairingRequestStatuses.Rejected;
            pairingRequest.RejectedAt = now;
            pairingRequest.UpdatedAt = now;

            await dbContext.SaveChangesAsync(cancellationToken);

            var response = new ChildRejectPairingResponse(
                pairingRequest.Id,
                pairingRequest.Status,
                now);

            return Results.Ok(ApiResponse<ChildRejectPairingResponse>.Ok("Pairing rejected.", response));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected reject pairing error for request {PairingRequestId}.", request.PairingRequestId);
            return ServerError();
        }
    }

    private static async Task<IResult> GetPairingStatusAsync(
        Guid pairingRequestId,
        ClaimsPrincipal currentUser,
        ApplicationDbContext dbContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("PairingEndpoints");
        if (!TryGetCurrentUserId(currentUser, out var parentId))
        {
            return Unauthorized();
        }

        try
        {
            var now = DateTime.UtcNow;
            await ExpireOldPendingRequestsAsync(dbContext, now, cancellationToken);

            var pairingRequest = await dbContext.PairingRequests
                .AsNoTracking()
                .Include(item => item.Device)
                .FirstOrDefaultAsync(
                    item => item.Id == pairingRequestId
                        && item.ParentId == parentId,
                    cancellationToken);

            if (pairingRequest is null)
            {
                return PairingRequestNotFound();
            }

            var response = new PairingStatusResponse(
                pairingRequest.Id,
                pairingRequest.DeviceId,
                pairingRequest.Device.DeviceName,
                pairingRequest.Device.ComputerName,
                pairingRequest.Status,
                pairingRequest.ExpiresAt,
                pairingRequest.CreatedAt,
                pairingRequest.ApprovedAt,
                pairingRequest.RejectedAt);

            return Results.Ok(ApiResponse<PairingStatusResponse>.Ok("Pairing status retrieved.", response));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected pairing status error for request {PairingRequestId}.", pairingRequestId);
            return ServerError();
        }
    }

    private static IReadOnlyList<string> ValidateChildConnectionCodeRequest(CreateChildConnectionCodeRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.DeviceName))
        {
            errors.Add("Device name is required.");
        }
        else if (request.DeviceName.Trim().Length > 100)
        {
            errors.Add("Device name must be at most 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.ComputerName))
        {
            errors.Add("Computer name is required.");
        }
        else if (request.ComputerName.Trim().Length > 100)
        {
            errors.Add("Computer name must be at most 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.AgentVersion))
        {
            errors.Add("Agent version is required.");
        }
        else if (request.AgentVersion.Trim().Length > 20)
        {
            errors.Add("Agent version must be at most 20 characters.");
        }

        return errors;
    }

    private static IResult? ValidateChildDecisionRequest(ChildPairingDecisionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionCode))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid pairing decision.",
                new[] { "Connection code is required." }));
        }

        if (request.PairingRequestId == Guid.Empty)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid pairing decision.",
                new[] { "Pairing request id is required." }));
        }

        return null;
    }

    private static async Task ExpireOldPendingRequestsAsync(
        ApplicationDbContext dbContext,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var expiredRequests = await dbContext.PairingRequests
            .Where(item => item.Status == PairingRequestStatuses.Pending && item.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        if (expiredRequests.Count == 0)
        {
            return;
        }

        foreach (var request in expiredRequests)
        {
            request.Status = PairingRequestStatuses.Expired;
            request.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<string> GenerateUniqueConnectionCodeAsync(
        ApplicationDbContext dbContext,
        IPairCodeGenerator pairCodeGenerator,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var connectionCode = pairCodeGenerator.Generate();
            var exists = await dbContext.PairCodes
                .AnyAsync(item => item.Code == connectionCode && !item.IsUsed, cancellationToken);

            if (!exists)
            {
                return connectionCode;
            }
        }

        throw new InvalidOperationException("Could not generate a unique connection code.");
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

    private static bool TryGetCurrentUserId(ClaimsPrincipal currentUser, out Guid userId)
    {
        var userIdValue = currentUser.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out userId);
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    private static IResult Unauthorized()
    {
        return Results.Json(
            ApiResponse<object>.Fail("Unauthorized.", new[] { "AUTH001" }),
            statusCode: StatusCodes.Status401Unauthorized);
    }

    private static IResult PairingCodeNotFound()
    {
        return Results.Json(
            ApiResponse<object>.Fail("Invalid or expired connection code.", new[] { "PAIR001" }),
            statusCode: StatusCodes.Status404NotFound);
    }

    private static IResult PairingRequestNotFound()
    {
        return Results.Json(
            ApiResponse<object>.Fail("Pairing request not found.", new[] { "PAIR002" }),
            statusCode: StatusCodes.Status404NotFound);
    }

    private static IResult InvalidPairingState()
    {
        return Results.Json(
            ApiResponse<object>.Fail("Pairing request is not pending.", new[] { "PAIR003" }),
            statusCode: StatusCodes.Status409Conflict);
    }

    private static IResult ServerError()
    {
        return Results.Json(
            ApiResponse<object>.Fail("Something went wrong.", new[] { "SERVER001" }),
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
