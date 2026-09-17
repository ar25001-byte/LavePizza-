using VeraPizza.DTOs;

namespace VeraPizza.Services;

public interface ICategoryServices
{
    Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
    Task<CategoryResponse?> GetCategoryByIdAsync(int id);
    Task<CategoryResponse> CreateCategoryAsync(CategoryRequest request);
    Task<CategoryResponse?> UpdateCategoryAsync(int id, CategoryRequest request);
    Task<bool> DeleteCategoryAsync(int id);
}
