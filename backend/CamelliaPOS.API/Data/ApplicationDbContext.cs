using Microsoft.EntityFrameworkCore;
using CamelliaPOS.API.Models;

namespace CamelliaPOS.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<ComboItem> ComboItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Waste> Wastes { get; set; }
    public DbSet<HappyHour> HappyHours { get; set; }
    public DbSet<Settings> Settings { get; set; }
    public DbSet<StockLog> StockLogs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasMany(c => c.MenuItems)
                  .WithOne(m => m.Category)
                  .HasForeignKey(m => m.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // MenuItem configuration
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.HasMany(m => m.ComboItems)
                  .WithOne(c => c.Combo)
                  .HasForeignKey(c => c.ComboId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ComboItem configuration
        modelBuilder.Entity<ComboItem>(entity =>
        {
            entity.HasOne(c => c.Item)
                  .WithMany()
                  .HasForeignKey(c => c.ItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasOne(o => o.User)
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(o => o.OrderItems)
                  .WithOne(oi => oi.Order)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(oi => oi.MenuItem)
                  .WithMany(m => m.OrderItems)
                  .HasForeignKey(oi => oi.MenuItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Waste configuration
        modelBuilder.Entity<Waste>(entity =>
        {
            entity.HasOne(w => w.MenuItem)
                  .WithMany()
                  .HasForeignKey(w => w.MenuItemId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(w => w.User)
                  .WithMany()
                  .HasForeignKey(w => w.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // StockLog configuration
        modelBuilder.Entity<StockLog>(entity =>
        {
            entity.HasOne(sl => sl.MenuItem)
                  .WithMany()
                  .HasForeignKey(sl => sl.MenuItemId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sl => sl.User)
                  .WithMany()
                  .HasForeignKey(sl => sl.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed default user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "camellia",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("camellia123"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Fruit Juices", Icon = "🥤", Color = "#FF6B6B", DisplayOrder = 1, IsActive = true },
            new Category { Id = 2, Name = "Coffees", Icon = "☕", Color = "#8B4513", DisplayOrder = 2, IsActive = true },
            new Category { Id = 3, Name = "Foods", Icon = "🍔", Color = "#FFA500", DisplayOrder = 3, IsActive = true },
            new Category { Id = 4, Name = "Short Eats", Icon = "🥪", Color = "#32CD32", DisplayOrder = 4, IsActive = true },
            new Category { Id = 5, Name = "Ice Creams", Icon = "🍨", Color = "#87CEEB", DisplayOrder = 5, IsActive = true }
        );

        // Seed sample menu items
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem
            {
                Id = 1,
                Name = "Orange Juice",
                Description = "Fresh orange juice",
                Price = 300.00m,
                Cost = 150.00m,
                CategoryId = 1,
                IsCombo = false,
                IsActive = true,
                StockQuantity = 100,
                MinStockLevel = 20,
                ApprovalStatus = ApprovalStatus.Approved,
                CreatedAt = DateTime.UtcNow
            },
            new MenuItem
            {
                Id = 2,
                Name = "Mango Juice",
                Description = "Fresh mango juice",
                Price = 350.00m,
                Cost = 180.00m,
                CategoryId = 1,
                IsCombo = false,
                IsActive = true,
                StockQuantity = 100,
                MinStockLevel = 20,
                ApprovalStatus = ApprovalStatus.Approved,
                CreatedAt = DateTime.UtcNow
            },
            new MenuItem
            {
                Id = 3,
                Name = "Chicken Roll",
                Description = "Delicious chicken roll",
                Price = 350.00m,
                Cost = 200.00m,
                CategoryId = 4,
                IsCombo = false,
                IsActive = true,
                StockQuantity = 50,
                MinStockLevel = 10,
                ApprovalStatus = ApprovalStatus.Approved,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed happy hour
        var happyHour = new HappyHour
        {
            Id = 1,
            Name = "Evening Special",
            StartTime = new TimeOnly(16, 0), // 4:00 PM
            EndTime = new TimeOnly(18, 0),   // 6:00 PM
            DiscountPercentage = 10.00m,
            IsActive = true
        };
        happyHour.CategoryIds = new List<int> { 1 }; // Fruit Juices
        happyHour.DayOfWeek = new List<int> { 1, 2, 3, 4, 5, 6 }; // Monday to Saturday
        
        modelBuilder.Entity<HappyHour>().HasData(happyHour);

        // Seed settings
        modelBuilder.Entity<Settings>().HasData(new Settings
        {
            Id = 1,
            TaxPercentage = 0,
            Currency = "LKR",
            SessionTimeoutMinutes = 30,
            UpdatedAt = DateTime.UtcNow
        });
    }
}

