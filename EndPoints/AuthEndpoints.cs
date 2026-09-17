using System.Security.Claims;
using VeraPizza.DTOs;
using VeraPizza.Services;

namespace VeraPizza.EndPoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
                       .WithTags("Auth");

        // POST /api/auth/login
        group.MapPost("/login", async (LoginRequest request, IAuthServices authService) =>
        {
            var token = await authService.LoginAsync(request);
            if (string.IsNullOrEmpty(token))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                token = token,
                tokenType = "Bearer",
                expiresInSeconds = 3600
            });
        })
        .WithName("Login")
        .WithSummary("Autenticar usuario y generar Token JWT")
        .WithDescription("Genera un token JWT válido tras verificar las credenciales de correo y contraseña.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();

        // POST /api/auth/register
        group.MapPost("/register", async (UserRequest request, IAuthServices authService) =>
        {
            try
            {
                var createdUser = await authService.RegisterAsync(request);
                return Results.Created($"/api/users/{createdUser.Id}", createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("Register")
        .WithSummary("Registrar nuevo usuario/integrante")
        .WithDescription("Crea un nuevo usuario en la plataforma encritando la contraseña con BCrypt.")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi();

        // GET /api/auth/me (Endpoint privado para obtener perfil del usuario autenticado)
        group.MapGet("/me", (ClaimsPrincipal userClaims, IAuthServices authService) =>
        {
            var profile = authService.GetCurrentUserProfile(userClaims);
            return Results.Ok(profile);
        })
        .WithName("GetCurrentUser")
        .WithSummary("Obtener información del usuario autenticado")
        .WithDescription("Decodifica los Claims del Token JWT del usuario actualmente autenticado.")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithOpenApi();
    }
}
