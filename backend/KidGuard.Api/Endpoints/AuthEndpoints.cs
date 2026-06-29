using KidGuard.Api.Common;
using KidGuard.Api.Contracts.Auth;
using KidGuard.Api.Data;
using KidGuard.Api.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace KidGuard.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth").WithTags("Authentication");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Parent login")
            .WithDescription("Authenticates a parent account and returns a JWT access token.")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid login request.",
                new[] { "Email and password are required." }));
        }

        if (request.Password.Length < 8)
        {
            return Results.BadRequest(ApiResponse<object>.Fail(
                "Invalid login request.",
                new[] { "Password must be at least 8 characters." }));
        }

        try
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Email.ToLower() == normalizedEmail, cancellationToken);

            if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                return Results.Json(
                    ApiResponse<object>.Fail("Invalid email or password.", new[] { "AUTH001" }),
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var accessToken = jwtTokenService.CreateAccessToken(user);
            var response = new LoginResponse(accessToken, jwtTokenService.ExpiresInSeconds);

            return Results.Ok(ApiResponse<LoginResponse>.Ok("Login successful.", response));
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Authentication configuration is invalid.");

            return Results.Json(
                ApiResponse<object>.Fail("Something went wrong.", new[] { "SERVER001" }),
                statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected login error for {Email}.", request.Email);

            return Results.Json(
                ApiResponse<object>.Fail("Something went wrong.", new[] { "SERVER001" }),
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}




