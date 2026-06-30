using KidGuard.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidGuard.Api.Data;

public static class DemoDataSeeder
{
    private static readonly Guid DemoParentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string DemoParentEmail = "parent@gmail.com";
    private const string DemoParentPasswordHash = "$2a$11$YAYI9gjgY0u7MAqnWt.dCe4Y3YEyZ.aFEli.f9BhVD2dimFq6uL6G";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DemoDataSeeder");

        await dbContext.Database.MigrateAsync();

        var parentExists = await dbContext.Users
            .AnyAsync(user => user.Email == DemoParentEmail);

        if (parentExists)
        {
            return;
        }

        var now = DateTime.UtcNow;
        dbContext.Users.Add(new User
        {
            Id = DemoParentId,
            FullName = "Demo Parent",
            Email = DemoParentEmail,
            PasswordHash = DemoParentPasswordHash,
            CreatedAt = now,
            UpdatedAt = now
        });

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Development demo parent account was created.");
    }
}
