using VeraPizza.DTOs;

namespace VeraPizza.Services;

public interface IProductServices
{
    Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
    Task<ProductResponse?> GetProductByIdAsync(int id);
    Task<ProductResponse> CreateProductAsync(ProductRequest request);
    Task<ProductResponse?> UpdateProductAsync(int id, ProductRequest request);
    Task<bool> DeleteProductAsync(int id);
}
