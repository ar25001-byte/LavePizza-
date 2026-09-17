using System.Security.Claims;
using VeraPizza.DTOs;
using VeraPizza.Services;

namespace VeraPizza.EndPoints;

public static class PaymentMethodEndpoints
{
    public static void MapPaymentMethodEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payment-methods")
                       .WithTags("Payment Methods")
                       .RequireAuthorization();

        // GET /api/payment-methods - Obtener métodos de pago del usuario
        group.MapGet("/", async (ClaimsPrincipal userClaims, IPaymentMethodServices paymentMethodService) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            var methods = await paymentMethodService.GetPaymentMethodsByUserIdAsync(userId);
            return Results.Ok(methods);
        })
        .WithName("GetUserPaymentMethods")
        .WithSummary("Obtener tarjetas/métodos de pago guardados del usuario")
        .Produces<IEnumerable<PaymentMethodDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();

        // POST /api/payment-methods - Guardar método de pago
        group.MapPost("/", async (PaymentMethodCreateRequest request, ClaimsPrincipal userClaims, IPaymentMethodServices paymentMethodService) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            var createdMethod = await paymentMethodService.CreatePaymentMethodAsync(userId, request);
            return Results.Created($"/api/payment-methods/{createdMethod.Id}", createdMethod);
        })
        .WithName("CreatePaymentMethod")
        .WithSummary("Registrar un nuevo método de pago")
        .Produces<PaymentMethodDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();

        // DELETE /api/payment-methods/{id} - Eliminar método de pago
        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal userClaims, IPaymentMethodServices paymentMethodService) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            var success = await paymentMethodService.DeletePaymentMethodAsync(id, userId);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { message = $"Método de pago con ID {id} no encontrado." });
        })
        .WithName("DeletePaymentMethod")
        .WithSummary("Eliminar método de pago")
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
