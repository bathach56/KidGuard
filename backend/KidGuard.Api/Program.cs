using System.Text;
using KidGuard.Api.Data;
using KidGuard.Api.Endpoints;
using KidGuard.Api.Options;
using KidGuard.Api.Services.Auth;
using KidGuard.Api.Services.Pairing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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


