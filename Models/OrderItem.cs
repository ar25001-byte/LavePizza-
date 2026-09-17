namespace VeraPizza.Models;

public class OrderItem
{
    public int Id { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string Size { get; set; } = "Mediana"; // Pequena, Mediana, Grande
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
