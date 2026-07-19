using System.Text;
using KidGuard.Api.Data;
using KidGuard.Api.Endpoints;
using KidGuard.Api.Options;
using KidGuard.Api.Services.Auth;
using KidGuard.Api.Services.Pairing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");
Environment.SetEnvironmentVariable("ASPNETCORE_hostBuilder__reloadConfigOnChange", "false");

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<SetupTokenSettings>(builder.Configuration.GetSection(SetupTokenSettings.SectionName));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        try
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
        }
    }
    if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("[YOUR-PASSWORD]") || connectionString.Contains("[YOUR_PASSWORD]"))
    {
        throw new InvalidOperationException("DATABASE ERROR: Chuỗi kết nối Database chưa được cấu hình! Vui lòng thiết lập biến môi trường DATABASE_URL hoặc cấu hình mật khẩu thực tế.");
    }
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ISetupTokenValidator, SetupTokenValidator>();
builder.Services.AddSingleton<IPairCodeGenerator, PairCodeGenerator>();
builder.Services.AddSingleton<IDeviceTokenGenerator, DeviceTokenGenerator>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (!string.IsNullOrWhiteSpace(jwtSettings.Secret))
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        }
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KidGuard API",
        Version = "v1",
        Description = "KidGuard Version 1.0.1 API for Windows-first Parent/Child approval-based pairing. Parent endpoints use JWT. Child pairing endpoints use temporary connection codes. Approved Windows Service endpoints use Device Token."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT / DeviceToken / SetupToken",
        In = ParameterLocation.Header,
        Description = "Use `Bearer <token>`. Parent endpoints require JWT. Agent endpoints require Device Token. Pair code creation requires Setup Token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
await DemoDataSeeder.SeedAsync(app.Services);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapPairCodeEndpoints();
app.MapPairingEndpoints();
app.MapDeviceEndpoints();

app.MapGet("/health", () => Results.Ok(new
{
    success = true,
    message = "API is healthy.",
    data = new
    {
        status = "healthy",
        version = "1.0.1"
    }
}))
.WithName("GetHealth")
.WithOpenApi();

app.Run();
