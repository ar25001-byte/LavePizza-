using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VeraPizza.DTOs;
using VeraPizza.Models;

namespace VeraPizza.Services;

public class OrderServices : IOrderServices
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public OrderServices(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderResponse> CreateOrderAsync(int userId, OrderCreateRequest request)
    {
        if (request.Items == null || !request.Items.Any())
        {
            throw new ArgumentException("El pedido debe contener al menos un producto.");
        }

        // Verificar dirección
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId);
        if (address == null)
        {
            throw new ArgumentException($"La dirección con ID {request.AddressId} no es válida para el usuario.");
        }

        // Verificar método de pago
        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId && pm.UserId == userId);
        if (paymentMethod == null)
        {
            throw new ArgumentException($"El método de pago con ID {request.PaymentMethodId} no es válido para el usuario.");
        }

        // Generar ID único como 'PED-XXXXX'
        var orderId = await GenerateUniqueOrderIdAsync();

        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            Date = DateTime.UtcNow,
            Status = OrderStatus.Confirmado.ToString(),
            AddressId = request.AddressId,
            PaymentMethodId = request.PaymentMethodId,
            ShippingFee = 2.99m
        };

        decimal subtotalAccumulated = 0m;

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Producto con ID {item.ProductId} no encontrado.");
            }

            if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"Stock insuficiente para el producto '{product.Name}'. Stock disponible: {product.Stock}");
            }

            // Descontar stock
            product.Stock -= item.Quantity;

            // Precio según tamaño
            var unitPrice = CalculateUnitPrice(product.Price, item.Size);
            var itemSubtotal = Math.Round(unitPrice * item.Quantity, 2);
            subtotalAccumulated += itemSubtotal;

            order.Items.Add(new OrderItem
            {
                OrderId = orderId,
                ProductId = item.ProductId,
                Size = item.Size,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                Subtotal = itemSubtotal
            });
        }

        order.Subtotal = subtotalAccumulated;

        // Envío gratis si el subtotal supera $40.00
        if (order.Subtotal >= 40.00m)
        {
            order.ShippingFee = 0.00m;
        }

        order.Total = order.Subtotal + order.ShippingFee;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId) ?? throw new InvalidOperationException("Error al recuperar el pedido recién creado.");
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersByUserIdAsync(int userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.PaymentMethod)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Date)
            .ToListAsync();

        return orders.Select(MapOrderToResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.PaymentMethod)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .OrderByDescending(o => o.Date)
            .ToListAsync();

        return orders.Select(MapOrderToResponse);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(string id, int? userId = null)
    {
        var query = _context.Orders
            .Include(o => o.Address)
            .Include(o => o.PaymentMethod)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .Where(o => o.Id == id);

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        var order = await query.FirstOrDefaultAsync();
        if (order == null) return null;

        return MapOrderToResponse(order);
    }

    public async Task<OrderResponse?> UpdateOrderStatusAsync(string id, string newStatus)
    {
        var order = await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.PaymentMethod)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return null;

        if (!Enum.TryParse<OrderStatus>(newStatus, true, out var parsedStatus))
        {
            throw new ArgumentException($"El estado '{newStatus}' no es válido. Estados válidos: Confirmado, EnPreparacion, EnCamino, Entregado, Recibido.");
        }

        order.Status = parsedStatus.ToString();
        await _context.SaveChangesAsync();

        return MapOrderToResponse(order);
    }

    public async Task<List<TrackingStepDto>?> GetOrderTrackingAsync(string id, int? userId = null)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new { o.Id, o.UserId, o.Status, o.Date })
            .FirstOrDefaultAsync();

        if (order == null) return null;
        if (userId.HasValue && order.UserId != userId.Value) return null;

        return BuildTrackingHistory(order.Status, order.Date);
    }

    public async Task<CartCalculationResponse> CalculateCartAsync(CartCalculateRequest request)
    {
        var response = new CartCalculationResponse();
        decimal subtotal = 0m;

        foreach (var item in request.Items)
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId);
            if (product != null)
            {
                var unitPrice = CalculateUnitPrice(product.Price, item.Size);
                var itemSubtotal = Math.Round(unitPrice * item.Quantity, 2);
                subtotal += itemSubtotal;

                response.Items.Add(new OrderItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Size = item.Size,
                    Quantity = item.Quantity,
                    Price = unitPrice,
                    Subtotal = itemSubtotal
                });
            }
        }

        response.Subtotal = subtotal;
        response.ShippingFee = subtotal >= 40.00m || subtotal == 0m ? 0.00m : 2.99m;
        response.Total = response.Subtotal + response.ShippingFee;

        return response;
    }

    private OrderResponse MapOrderToResponse(Order order)
    {
        var response = _mapper.Map<OrderResponse>(order);
        response.TrackingHistory = BuildTrackingHistory(order.Status, order.Date);
        return response;
    }

    private static List<TrackingStepDto> BuildTrackingHistory(string currentStatusStr, DateTime orderDate)
    {
        var statuses = new[]
        {
            (Status: OrderStatus.Confirmado.ToString(), Title: "Pedido Confirmado", Desc: "Tu pedido ha sido recibido y verificado por el restaurante."),
            (Status: OrderStatus.EnPreparacion.ToString(), Title: "En Preparación", Desc: "Nuestros pizzaiolos están elaborando tu orden con ingredientes frescos."),
            (Status: OrderStatus.EnCamino.ToString(), Title: "En Camino", Desc: "El repartidor ha salido con tu pedido hacia la dirección de entrega."),
            (Status: OrderStatus.Entregado.ToString(), Title: "Entregado", Desc: "El pedido ha sido entregado exitosamente."),
            (Status: OrderStatus.Recibido.ToString(), Title: "Recibido", Desc: "Entrega completada y confirmada por el cliente.")
        };

        Enum.TryParse<OrderStatus>(currentStatusStr, true, out var currentEnum);
        int currentIndex = (int)currentEnum;

        var history = new List<TrackingStepDto>();

        for (int i = 0; i < statuses.Length; i++)
        {
            var isCompleted = i <= currentIndex;
            var isCurrent = i == currentIndex;
            DateTime? stepDate = isCompleted ? orderDate.AddMinutes(i * 10) : null;

            history.Add(new TrackingStepDto
            {
                Status = statuses[i].Status,
                Description = statuses[i].Desc,
                Date = stepDate,
                IsCompleted = isCompleted,
                IsCurrent = isCurrent
            });
        }

        return history;
    }

    private static decimal CalculateUnitPrice(decimal basePrice, string size)
    {
        return size.ToLowerInvariant() switch
        {
            "pequena" or "pequeña" => Math.Round(basePrice * 0.85m, 2),
            "grande" => Math.Round(basePrice * 1.25m, 2),
            _ => basePrice // "mediana" u otro
        };
    }

    private async Task<string> GenerateUniqueOrderIdAsync()
    {
        var random = new Random();
        string orderId;
        do
        {
            var number = random.Next(10000, 99999);
            orderId = $"PED-{number}";
        }
        while (await _context.Orders.AnyAsync(o => o.Id == orderId));

        return orderId;
    }
}
