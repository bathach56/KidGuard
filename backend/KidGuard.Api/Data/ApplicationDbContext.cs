using KidGuard.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidGuard.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Mode> Modes => Set<Mode>();
    public DbSet<PairCode> PairCodes => Set<PairCode>();
    public DbSet<Heartbeat> Heartbeats => Set<Heartbeat>();
    public DbSet<DeviceLog> DeviceLogs => Set<DeviceLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplyCamelCaseColumnNames(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureModes(modelBuilder);
        ConfigureDevices(modelBuilder);
        ConfigurePairCodes(modelBuilder);
        ConfigureHeartbeats(modelBuilder);
        ConfigureDeviceLogs(modelBuilder);
    }
    private static void ApplyCamelCaseColumnNames(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                var propertyName = property.Name;
                property.SetColumnName(char.ToLowerInvariant(propertyName[0]) + propertyName[1..]);
            }
        }
    }
    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();

            entity.Property(user => user.FullName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(255).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.Property(user => user.PhoneNumber).HasMaxLength(20);
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(user => user.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private static void ConfigureModes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mode>(entity =>
        {
            entity.ToTable("Modes");
            entity.HasKey(mode => mode.Id);
            entity.HasAlternateKey(mode => mode.Name);

            entity.Property(mode => mode.Name).HasMaxLength(30).IsRequired();
            entity.Property(mode => mode.Description).HasMaxLength(255).IsRequired();

            entity.HasData(
                new Mode { Id = 1, Name = "fun", Description = "Relaxation mode with no application blocking." },
                new Mode { Id = 2, Name = "study", Description = "Study mode that blocks distracting applications." },
                new Mode { Id = 3, Name = "punishment", Description = "Strict mode that allows only approved applications." });
        });
    }

    private static void ConfigureDevices(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("Devices");
            entity.HasKey(device => device.Id);
            entity.HasIndex(device => device.DeviceToken).IsUnique().HasFilter("[deviceToken] IS NOT NULL");
            entity.HasIndex(device => device.UserId);
            entity.HasIndex(device => device.CurrentMode);

            entity.Property(device => device.DeviceName).HasMaxLength(100).IsRequired();
            entity.Property(device => device.ComputerName).HasMaxLength(100).IsRequired();
            entity.Property(device => device.CurrentMode).HasMaxLength(30).HasDefaultValue("fun").IsRequired();
            entity.Property(device => device.IsOnline).HasDefaultValue(false);
            entity.Property(device => device.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(device => device.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(device => device.User)
                .WithMany(user => user.Devices)
                .HasForeignKey(device => device.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(device => device.Mode)
                .WithMany(mode => mode.Devices)
                .HasForeignKey(device => device.CurrentMode)
                .HasPrincipalKey(mode => mode.Name)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePairCodes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PairCode>(entity =>
        {
            entity.ToTable("PairCodes");
            entity.HasKey(pairCode => pairCode.Id);
            entity.HasIndex(pairCode => pairCode.DeviceId).IsUnique();
            entity.HasIndex(pairCode => pairCode.Code).IsUnique();

            entity.Property(pairCode => pairCode.Code).HasColumnName("pairCode").HasMaxLength(20).IsRequired();
            entity.Property(pairCode => pairCode.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(pairCode => pairCode.Device)
                .WithOne(device => device.PairCode)
                .HasForeignKey<PairCode>(pairCode => pairCode.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureHeartbeats(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Heartbeat>(entity =>
        {
            entity.ToTable("Heartbeats");
            entity.HasKey(heartbeat => heartbeat.Id);
            entity.HasIndex(heartbeat => heartbeat.DeviceId);
            entity.HasIndex(heartbeat => heartbeat.CreatedAt);

            entity.Property(heartbeat => heartbeat.Status).HasMaxLength(20).IsRequired();
            entity.Property(heartbeat => heartbeat.AgentVersion).HasMaxLength(20).IsRequired();
            entity.Property(heartbeat => heartbeat.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(heartbeat => heartbeat.Device)
                .WithMany(device => device.Heartbeats)
                .HasForeignKey(heartbeat => heartbeat.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureDeviceLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceLog>(entity =>
        {
            entity.ToTable("DeviceLogs");
            entity.HasKey(deviceLog => deviceLog.Id);
            entity.HasIndex(deviceLog => deviceLog.DeviceId);
            entity.HasIndex(deviceLog => deviceLog.CreatedAt);

            entity.Property(deviceLog => deviceLog.ProcessName).HasMaxLength(255).IsRequired();
            entity.Property(deviceLog => deviceLog.Action).HasMaxLength(100).IsRequired();
            entity.Property(deviceLog => deviceLog.Mode).HasMaxLength(20).IsRequired();
            entity.Property(deviceLog => deviceLog.Message).IsRequired();
            entity.Property(deviceLog => deviceLog.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(deviceLog => deviceLog.Device)
                .WithMany(device => device.DeviceLogs)
                .HasForeignKey(deviceLog => deviceLog.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}



