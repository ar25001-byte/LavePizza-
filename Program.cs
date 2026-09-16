using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VeraPizza.EndPoints;
using VeraPizza.Mappings;
using VeraPizza.Models;
using VeraPizza.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de DbContext (Permite usar SQL Server o InMemory para pruebas rápidas)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Usa InMemoryDatabase para ejecución inmediata sin dependencias externas de SQL Server;
    // se puede alternar a UseSqlServer según se requiera en producción.
    options.UseInMemoryDatabase("VeraPizzaInMemoryDb");
    // options.UseSqlServer(connectionString);
});

// 2. Registro de AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 3. Inyección de Dependencias de Servicios de Negocio (Scoped)
builder.Services.AddScoped<IProductServices, ProductServices>();
builder.Services.AddScoped<IUserServices, UserServices>();

// 4. Configuración de Autenticación JWT Bearer
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "SuperSecretKeyVeraPizzaWebApi2026MasterKeyWithAtLeast32BytesLength!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// 5. Configuración de Swagger / OpenAPI con esquema de seguridad Bearer JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VeraPizza Web API",
        Version = "v1",
        Description = "API RESTful construida con .NET Core, Minimal APIs, EF Core, AutoMapper y Autenticación JWT."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingrese el token JWT en el formato: Bearer {su_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Asegurar creación y sembrado de la base de datos en memoria / inicial
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Pipeline de middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VeraPizza API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Mapeo modular de Endpoints por módulo
app.MapProductEndpoints();
app.MapUserEndpoints();
app.MapAuthEndpoints();

app.Run();
