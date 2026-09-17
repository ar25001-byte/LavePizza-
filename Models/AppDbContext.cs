using Microsoft.EntityFrameworkCore;

namespace VeraPizza.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración para la entidad Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(250);
        });

        // Configuración de precisión para la propiedad Price de Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.HasOne(p => p.Category)
                  .WithMany()
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración para la entidad User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Configuración para la entidad Address
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Title).IsRequired().HasMaxLength(100);
            entity.Property(a => a.AddressLine).IsRequired().HasMaxLength(250);
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración para la entidad PaymentMethod
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(pm => pm.Id);
            entity.Property(pm => pm.CardType).IsRequired().HasMaxLength(50);
            entity.Property(pm => pm.LastFourDigits).IsRequired().HasMaxLength(4);
            entity.HasOne(pm => pm.User)
                  .WithMany()
                  .HasForeignKey(pm => pm.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración para la entidad Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Status).IsRequired().HasMaxLength(50);
            entity.Property(o => o.Subtotal).HasPrecision(18, 2);
            entity.Property(o => o.ShippingFee).HasPrecision(18, 2);
            entity.Property(o => o.Total).HasPrecision(18, 2);

            entity.HasOne(o => o.User)
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Address)
                  .WithMany()
                  .HasForeignKey(o => o.AddressId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.PaymentMethod)
                  .WithMany()
                  .HasForeignKey(o => o.PaymentMethodId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(o => o.Items)
                  .WithOne(oi => oi.Order)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración para la entidad OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            entity.Property(oi => oi.Subtotal).HasPrecision(18, 2);
            entity.Property(oi => oi.Size).HasMaxLength(50);

            entity.HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
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

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                Name = "Pizzas Tradicionales",
                Description = "Auténticas pizzas napolitanas horneadas en horno de piedra."
            },
            new Category
            {
                Id = 2,
                Name = "Calzones & Especialidades",
                Description = "Deliciosos calzones rellenos e ingredientes premium."
            },
            new Category
            {
                Id = 3,
                Name = "Bebidas & Postres",
                Description = "Refrescos, cervezas artesanales e irresistibles postres italianos."
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
                CategoryId = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 2,
                Name = "Pizza Quattro Formaggi",
                Description = "Mozzarella, gorgonzola, fontina y parmesano reggiano.",
                Price = 16.50m,
                Stock = 30,
                CategoryId = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 3,
                Name = "Calzone Tradizionale",
                Description = "Relleno de ricotta, salami napolitano, mozzarella y pimienta negra.",
                Price = 13.50m,
                Stock = 20,
                CategoryId = 2,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Address>().HasData(
            new Address
            {
                Id = 1,
                UserId = 2,
                Title = "Casa",
                AddressLine = "Av. Principal #123, Depto 4B"
            },
            new Address
            {
                Id = 2,
                UserId = 2,
                Title = "Trabajo",
                AddressLine = "Calle Comercio #456, Edificio Central Piso 2"
            }
        );

        modelBuilder.Entity<PaymentMethod>().HasData(
            new PaymentMethod
            {
                Id = 1,
                UserId = 2,
                CardType = "Visa",
                LastFourDigits = "4242"
            },
            new PaymentMethod
            {
                Id = 2,
                UserId = 2,
                CardType = "MasterCard",
                LastFourDigits = "8888"
            }
        );

        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                Id = "PED-79262",
                UserId = 2,
                Date = new DateTime(2026, 9, 15, 12, 30, 0, DateTimeKind.Utc),
                Status = OrderStatus.Confirmado.ToString(),
                Subtotal = 28.49m,
                ShippingFee = 2.99m,
                Total = 31.48m,
                AddressId = 1,
                PaymentMethodId = 1
            }
        );

        modelBuilder.Entity<OrderItem>().HasData(
            new OrderItem
            {
                Id = 1,
                OrderId = "PED-79262",
                ProductId = 1,
                Size = "Mediana",
                Quantity = 1,
                UnitPrice = 14.99m,
                Subtotal = 14.99m
            },
            new OrderItem
            {
                Id = 2,
                OrderId = "PED-79262",
                ProductId = 3,
                Size = "Mediana",
                Quantity = 1,
                UnitPrice = 13.50m,
                Subtotal = 13.50m
            }
        );
    }
}
