namespace VeraPizza.DTOs;

public class OrderResponse
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public AddressDto? Address { get; set; }
    public PaymentMethodDto? PaymentMethod { get; set; }
    public List<TrackingStepDto> TrackingHistory { get; set; } = new();
}
