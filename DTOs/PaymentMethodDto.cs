namespace VeraPizza.DTOs;

public class PaymentMethodDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CardType { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;
}

public class PaymentMethodCreateRequest
{
    public string CardType { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;
}
