using KidGuard.Api.Common;
using KidGuard.Api.Contracts.PairCode;
using KidGuard.Api.Data;
using KidGuard.Api.Entities;
using KidGuard.Api.Services.Pairing;
using Microsoft.EntityFrameworkCore;

namespace KidGuard.Api.Endpoints;

public static class PairCodeEndpoints
{
    private const int PairCodeExpiresInSeconds = 600;

    public static RouteGroupBuilder MapPairCodeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/pair-code").WithTags("Pair Code");

        group.MapPost("/", CreatePairCodeAsync)
            .WithName("CreatePairCode")
            .WithSummary("Create pair code")
            .WithDescription("Windows Agent creates a temporary pair code before pairing. Requires Setup Token.")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> CreatePairCodeAsync(
        CreatePairCodeRequest request,
        HttpContext httpContext,
        ApplicationDbContext dbContext,
        ISetupTokenValidator setupTokenValidator,
        IPairCodeGenerator pairCodeGenerator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("PairCodeEndpoints");

        if (!setupTokenValidator.IsConfigured)
        {
            logger.LogError("Setup token is not configured.");

            return Results.Json(
                ApiResponse<object>.Fail("Something went wrong.", new[] { "SERVER001" }),
                statusCode: StatusCodes.Status500InternalServerError);
        }

        if (!setupTokenValidator.IsValid(httpContext.Request.Headers.Authorization))
        {
            return Results.Json(
                ApiResponse<object>.Fail("Unauthorized.", new[] { "AUTH001" }),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(request.DeviceName)
            || string.IsNullOrWhiteSpace(request.ComputerName)
            || string.IsNullOrWhiteSpace(request.AgentVersion))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid pair code request.",
                new[] { "Device name, computer name, and agent version are required." }));
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

            var pairCode = await GenerateUniquePairCodeAsync(
                dbContext,
                pairCodeGenerator,
                cancellationToken);

            dbContext.Devices.Add(device);
            dbContext.PairCodes.Add(new PairCode
            {
                Id = Guid.NewGuid(),
                DeviceId = device.Id,
                Code = pairCode,
                ExpiresAt = now.AddSeconds(PairCodeExpiresInSeconds),
                IsUsed = false,
                CreatedAt = now
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(ApiResponse<CreatePairCodeResponse>.Ok(
                "Pair code created.",
                new CreatePairCodeResponse(pairCode, PairCodeExpiresInSeconds)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected pair code creation error for {ComputerName}.", request.ComputerName);

            return Results.Json(
                ApiResponse<object>.Fail("Something went wrong.", new[] { "SERVER001" }),
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<string> GenerateUniquePairCodeAsync(
        ApplicationDbContext dbContext,
        IPairCodeGenerator pairCodeGenerator,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var pairCode = pairCodeGenerator.Generate();
            var exists = await dbContext.PairCodes
                .AnyAsync(item => item.Code == pairCode && !item.IsUsed, cancellationToken);

            if (!exists)
            {
                return pairCode;
            }
        }

        throw new InvalidOperationException("Could not generate a unique pair code.");
    }
}


