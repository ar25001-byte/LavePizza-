using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VeraPizza.DTOs;
using VeraPizza.Models;

namespace VeraPizza.Services;

public class ProductServices : IProductServices
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProductServices(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
    {
        var products = await _context.Products.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<ProductResponse>>(products);
    }

    public async Task<ProductResponse?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return null;

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> CreateProductAsync(ProductRequest request)
    {
        var product = _mapper.Map<Product>(request);
        product.CreatedAt = DateTime.UtcNow;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse?> UpdateProductAsync(int id, ProductRequest request)
    {
        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null) return null;

        _mapper.Map(request, existingProduct);
        await _context.SaveChangesAsync();

        return _mapper.Map<ProductResponse>(existingProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
