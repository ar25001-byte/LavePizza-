namespace VeraPizza.DTOs;

public class AddressDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
}

public class AddressCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
}
