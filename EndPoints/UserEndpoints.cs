using VeraPizza.DTOs;
using VeraPizza.Services;

namespace VeraPizza.EndPoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
                       .WithTags("Users");

        // GET /api/users (Protegido)
        group.MapGet("/", async (IUserServices userService) =>
        {
            var users = await userService.GetAllUsersAsync();
            return Results.Ok(users);
        })
        .WithName("GetAllUsers")
        .WithSummary("Obtener usuarios")
        .WithDescription("Obtiene la lista de usuarios registrados en el sistema.")
        .Produces<IEnumerable<UserResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithOpenApi();

        // POST /api/users (Público para registro)
        group.MapPost("/", async (UserRequest request, IUserServices userService) =>
        {
            try
            {
                var createdUser = await userService.CreateUserAsync(request);
                return Results.Created($"/api/users/{createdUser.Id}", createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("CreateUser")
        .WithSummary("Crear usuario")
        .WithDescription("Registra un nuevo usuario en el sistema con su contraseña encriptada.")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi();
    }
}
