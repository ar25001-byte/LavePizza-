namespace VeraPizza.DTOs;

public class TrackingStepDto
{
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
}
