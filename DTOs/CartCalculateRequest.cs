namespace VeraPizza.DTOs;

public class CartItemRequest
{
    public int ProductId { get; set; }
    public string Size { get; set; } = "Mediana";
    public int Quantity { get; set; }
}

public class CartCalculateRequest
{
    public List<CartItemRequest> Items { get; set; } = new();
}

public class CartCalculationResponse
{
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
}
