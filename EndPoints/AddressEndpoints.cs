using System.Security.Claims;
using VeraPizza.DTOs;
using VeraPizza.Services;

namespace VeraPizza.EndPoints;

public static class AddressEndpoints
{
    public static void MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/addresses")
                       .WithTags("Addresses")
                       .RequireAuthorization();

        // GET /api/addresses - Obtener direcciones del usuario autenticado
        group.MapGet("/", async (ClaimsPrincipal userClaims, IAddressServices addressService) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            var addresses = await addressService.GetAddressesByUserIdAsync(userId);
            return Results.Ok(addresses);
        })
        .WithName("GetUserAddresses")
        .WithSummary("Obtener lista de direcciones del usuario")
        .Produces<IEnumerable<AddressDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();

        // POST /api/addresses - Crear nueva dirección
        group.MapPost("/", async (AddressCreateRequest request, ClaimsPrincipal userClaims, IAddressServices addressService) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            var createdAddress = await addressService.CreateAddressAsync(userId, request);
            return Results.Created($"/api/addresses/{createdAddress.Id}", createdAddress);
        })
        .WithName("CreateAddress")
        .WithSummary("Registrar una nueva dirección")
        .Produces<AddressDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();

        // DELETE /api/addresses/{id} - Eliminar dirección
        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal userClaims, IAddressServices addressService) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            var success = await addressService.DeleteAddressAsync(id, userId);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { message = $"Dirección con ID {id} no encontrada." });
        })
        .WithName("DeleteAddress")
        .WithSummary("Eliminar dirección")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;

        return int.TryParse(idClaim, out var id) ? id : 0;
    }
}
