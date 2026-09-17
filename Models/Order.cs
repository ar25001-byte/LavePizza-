namespace VeraPizza.Models;

public class Order
{
    public string Id { get; set; } = string.Empty; // Formato como 'PED-79262'
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = OrderStatus.Confirmado.ToString(); // Confirmado, EnPreparacion, EnCamino, Entregado, Recibido
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public int AddressId { get; set; }
    public Address? Address { get; set; }
    public int PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
