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
        group.MapPost("/login", async (LoginRequest request, IUserServices userService) =>
        {
            var token = await userService.LoginAsync(request);
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
        .WithSummary("Autenticar usuario y obtener Token JWT")
        .WithDescription("Genera un token JWT válido tras verificar las credenciales de correo y contraseña.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();
    }
}
