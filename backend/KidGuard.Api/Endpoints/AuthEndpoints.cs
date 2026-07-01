using KidGuard.Api.Common;
using KidGuard.Api.Contracts.Auth;
using KidGuard.Api.Data;
using KidGuard.Api.Entities;
using KidGuard.Api.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace KidGuard.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth").WithTags("Authentication");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Parent register")
            .WithDescription("Creates a parent account for KidGuard Parent clients.")
            .WithOpenApi();

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Parent login")
            .WithDescription("Authenticates a parent account and returns a JWT access token.")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");

        var validationErrors = ValidateRegisterRequest(request);
        if (validationErrors.Count > 0)
        {
            return Results.BadRequest(ApiResponse<object>.Fail("Invalid register request.", validationErrors));
        }

        try
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var emailExists = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(item => item.Email.ToLower() == normalizedEmail, cancellationToken);

            if (emailExists)
            {
                return Results.Conflict(ApiResponse<object>.Fail(
                    "Email already exists.",
                    new[] { "AUTH003" }));
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHasher.Hash(request.Password),
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            var response = new RegisterResponse(user.Id, user.FullName, user.Email, user.PhoneNumber);

            return Results.Created($"/auth/users/{user.Id}", ApiResponse<RegisterResponse>.Ok("Register successful.", response));
        }
        catch (DbUpdateException exception)
        {
            logger.LogError(exception, "Database register error for {Email}.", request.Email);

            return Results.Json(
                ApiResponse<object>.Fail("Something went wrong.", new[] { "SERVER001" }),
                statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected register error for {Email}.", request.Email);

            return Results.Json(
                ApiResponse<object>.Fail("Something went wrong.", new[] { "SERVER001" }),
                statusCode: StatusCodes.Status500InternalServerError);
        }
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

    private static IReadOnlyList<string> ValidateRegisterRequest(RegisterRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors.Add("Full name is required.");
        }
        else if (request.FullName.Trim().Length > 100)
        {
            errors.Add("Full name must be at most 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }
        else if (request.Email.Trim().Length > 255)
        {
            errors.Add("Email must be at most 255 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }
        else if (request.Password.Length < 8)
        {
            errors.Add("Password must be at least 8 characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber.Trim().Length > 20)
        {
            errors.Add("Phone number must be at most 20 characters.");
        }

        return errors;
    }
}
