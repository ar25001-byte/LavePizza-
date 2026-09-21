using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VeraPizza.DTOs;
using VeraPizza.Models;

namespace VeraPizza.Services;

public class CategoryServices : ICategoryServices
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CategoryServices(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
    }

    public async Task<CategoryResponse?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;
        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CategoryRequest request)
    {
        var category = _mapper.Map<Category>(request);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(int id, CategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        _mapper.Map(request, category);
        await _context.SaveChangesAsync();
        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
