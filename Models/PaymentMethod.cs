namespace VeraPizza.Models;

public class PaymentMethod
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string CardType { get; set; } = string.Empty; // ej: 'Visa', 'MasterCard'
    public string LastFourDigits { get; set; } = string.Empty; // ej: '4242'
}
