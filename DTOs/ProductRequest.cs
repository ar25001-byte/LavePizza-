using System.ComponentModel.DataAnnotations;

namespace VeraPizza.DTOs;

public class ProductRequest
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(0.01, 10000.00, ErrorMessage = "El precio debe estar entre 0.01 y 10000.00.")]
    public decimal Price { get; set; }

    [Range(0, 1000, ErrorMessage = "El stock debe ser un valor positivo.")]
    public int Stock { get; set; }
}
