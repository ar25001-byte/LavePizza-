using VeraPizza.DTOs;
using VeraPizza.Services;

namespace VeraPizza.EndPoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
                       .WithTags("Users");

        // GET /api/users (Privado: Requiere política RepositoryMemberOnly)
        group.MapGet("/", async (IUserServices userService) =>
        {
            var users = await userService.GetAllUsersAsync();
            return Results.Ok(users);
        })
        .WithName("GetAllUsers")
        .WithSummary("Obtener usuarios")
        .WithDescription("Obtiene la lista de integrantes registrados en el repositorio.")
        .Produces<IEnumerable<UserResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization("RepositoryMemberOnly")
        .WithOpenApi();

        // POST /api/users (Registro de integrantes)
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
        .WithSummary("Crear/Registrar usuario")
        .WithDescription("Registra un nuevo integrante en el sistema con su contraseña encriptada.")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .AllowAnonymous()
        .WithOpenApi();
    }
}
