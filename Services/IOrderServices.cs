using VeraPizza.DTOs;

namespace VeraPizza.Services;

public interface IOrderServices
{
    Task<OrderResponse> CreateOrderAsync(int userId, OrderCreateRequest request);
    Task<IEnumerable<OrderResponse>> GetOrdersByUserIdAsync(int userId);
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
    Task<OrderResponse?> GetOrderByIdAsync(string id, int? userId = null);
    Task<OrderResponse?> UpdateOrderStatusAsync(string id, string newStatus);
    Task<List<TrackingStepDto>?> GetOrderTrackingAsync(string id, int? userId = null);
    Task<CartCalculationResponse> CalculateCartAsync(CartCalculateRequest request);
}
