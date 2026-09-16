using Microsoft.EntityFrameworkCore;

namespace VeraPizza.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de precisión para la propiedad Price de Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        // Configuración para la entidad User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Seed Data Inicial
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@verapizza.com",
                PasswordHash = adminPasswordHash,
                Role = "Admin",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = 2,
                Username = "juanperez",
                Email = "juan@verapizza.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                Role = "User",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Pizza Margherita Napoletana",
                Description = "Salsa de tomate San Marzano, mozzarella fior di latte, albahaca fresca y aceite de oliva virgen extra.",
                Price = 14.99m,
                Stock = 50,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 2,
                Name = "Pizza Quattro Formaggi",
                Description = "Mozzarella, gorgonzola, fontina y parmesano reggiano.",
                Price = 16.50m,
                Stock = 30,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 3,
                Name = "Calzone Tradizionale",
                Description = "Relleno de ricotta, salami napolitano, mozzarella y pimienta negra.",
                Price = 13.50m,
                Stock = 20,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
