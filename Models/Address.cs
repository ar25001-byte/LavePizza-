namespace VeraPizza.Models;

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Title { get; set; } = string.Empty; // ej: 'Casa', 'Trabajo'
    public string AddressLine { get; set; } = string.Empty;
}
