namespace VeraPizza.DTOs;

public class OrderItemCreateDto
{
    public int ProductId { get; set; }
    public string Size { get; set; } = "Mediana"; // Pequena, Mediana, Grande
    public int Quantity { get; set; }
}

public class OrderCreateRequest
{
    public List<OrderItemCreateDto> Items { get; set; } = new();
    public int AddressId { get; set; }
    public int PaymentMethodId { get; set; }
}
