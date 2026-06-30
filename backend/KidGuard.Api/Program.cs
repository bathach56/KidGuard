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

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<SetupTokenSettings>(builder.Configuration.GetSection(SetupTokenSettings.SectionName));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        Description = "Parental Control System API for Parent Mobile and Windows Agent integration. Parent endpoints use JWT. Windows Agent endpoints use Device Token. Pair code creation uses Setup Token."
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await DemoDataSeeder.SeedAsync(app.Services);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapPairCodeEndpoints();
app.MapDeviceEndpoints();

app.MapGet("/health", () => Results.Ok(new
{
    success = true,
    message = "API is healthy.",
    data = new
    {
        status = "healthy",
        version = "1.0.0"
    }
}))
.WithName("GetHealth")
.WithOpenApi();

app.Run();



