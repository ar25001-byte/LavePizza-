using System.Security.Claims;
using VeraPizza.DTOs;
using VeraPizza.Services;

namespace VeraPizza.EndPoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
                       .WithTags("Orders");

        // POST /api/orders/cart/calculate - Cálculo de Totales del Carrito
        group.MapPost("/cart/calculate", async (CartCalculateRequest request, IOrderServices orderService) =>
        {
            var result = await orderService.CalculateCartAsync(request);
            return Results.Ok(result);
        })
        .WithName("CalculateCart")
        .WithSummary("Calcular totales del carrito de compras")
        .WithDescription("Calcula el subtotal, costo de envío y total estimado del carrito según los productos y tamaños seleccionados.")
        .Produces<CartCalculationResponse>(StatusCodes.Status200OK)
        .WithOpenApi();

        // POST /api/orders - Crear Pedido
        group.MapPost("/", async (OrderCreateRequest request, ClaimsPrincipal userClaims, IOrderServices orderService) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            try
            {
                var createdOrder = await orderService.CreateOrderAsync(userId, request);
                return Results.Created($"/api/orders/{createdOrder.Id}", createdOrder);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("CreateOrder")
        .WithSummary("Crear un nuevo pedido")
        .WithDescription("Genera una orden de compra para el usuario autenticado registrando productos, dirección y método de pago.")
        .Produces<OrderResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithOpenApi();

        // GET /api/orders - Listar Pedidos del Usuario (o todos si es Admin)
        group.MapGet("/", async (ClaimsPrincipal userClaims, IOrderServices orderService, bool? all = false) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            if (all == true && role == "Admin")
            {
                var allOrders = await orderService.GetAllOrdersAsync();
                return Results.Ok(allOrders);
            }

            var userOrders = await orderService.GetOrdersByUserIdAsync(userId);
            return Results.Ok(userOrders);
        })
        .WithName("GetOrders")
        .WithSummary("Obtener historial de pedidos")
        .WithDescription("Obtiene los pedidos pertenecientes al usuario autenticado (los administradores pueden listar todos).")
        .Produces<IEnumerable<OrderResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithOpenApi();

        // GET /api/orders/{id} - Detalle de un Pedido
        group.MapGet("/{id}", async (string id, ClaimsPrincipal userClaims, IOrderServices orderService) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            int? filterUserId = role == "Admin" ? null : userId;

            var order = await orderService.GetOrderByIdAsync(id, filterUserId);
            return order is not null ? Results.Ok(order) : Results.NotFound(new { message = $"Pedido con ID '{id}' no encontrado." });
        })
        .WithName("GetOrderById")
        .WithSummary("Obtener detalle de un pedido por ID")
        .Produces<OrderResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithOpenApi();

        // PUT /api/orders/{id}/status - Actualizar Estado del Pedido (Admin / Staff)
        group.MapPut("/{id}/status", async (string id, UpdateOrderStatusRequest request, IOrderServices orderService) =>
        {
            try
            {
                var updatedOrder = await orderService.UpdateOrderStatusAsync(id, request.NewStatus);
                return updatedOrder is not null
                    ? Results.Ok(updatedOrder)
                    : Results.NotFound(new { message = $"Pedido con ID '{id}' no encontrado." });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("UpdateOrderStatus")
        .WithSummary("Actualizar el estado de un pedido")
        .WithDescription("Permite actualizar el estado del pedido (Confirmado, EnPreparacion, EnCamino, Entregado, Recibido).")
        .Produces<OrderResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization("RepositoryMemberOnly")
        .WithOpenApi();

        // GET /api/orders/{id}/tracking - Seguimiento / Tracking del Pedido
        group.MapGet("/{id}/tracking", async (string id, ClaimsPrincipal userClaims, IOrderServices orderService) =>
        {
            var userId = GetUserId(userClaims);
            if (userId == 0) return Results.Unauthorized();

            var role = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            int? filterUserId = role == "Admin" ? null : userId;

            var trackingHistory = await orderService.GetOrderTrackingAsync(id, filterUserId);
            return trackingHistory is not null
                ? Results.Ok(trackingHistory)
                : Results.NotFound(new { message = $"Historial de seguimiento para el pedido '{id}' no encontrado." });
        })
        .WithName("GetOrderTracking")
        .WithSummary("Obtener información de seguimiento (Tracking) de un pedido")
        .WithDescription("Muestra el historial y estado actual del proceso de entrega del pedido.")
        .Produces<List<TrackingStepDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithOpenApi();
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;

        return int.TryParse(idClaim, out var id) ? id : 0;
    }
}
