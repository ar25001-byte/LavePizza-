using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VeraPizza.DTOs;
using VeraPizza.Models;

namespace VeraPizza.Services;

public class PaymentMethodServices : IPaymentMethodServices
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PaymentMethodServices(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PaymentMethodDto>> GetPaymentMethodsByUserIdAsync(int userId)
    {
        var methods = await _context.PaymentMethods
            .AsNoTracking()
            .Where(pm => pm.UserId == userId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
    }

    public async Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(int id, int userId)
    {
        var method = await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId);

        if (method == null) return null;
        return _mapper.Map<PaymentMethodDto>(method);
    }

    public async Task<PaymentMethodDto> CreatePaymentMethodAsync(int userId, PaymentMethodCreateRequest request)
    {
        var method = _mapper.Map<PaymentMethod>(request);
        method.UserId = userId;

        _context.PaymentMethods.Add(method);
        await _context.SaveChangesAsync();

        return _mapper.Map<PaymentMethodDto>(method);
    }

    public async Task<bool> DeletePaymentMethodAsync(int id, int userId)
    {
        var method = await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == id && pm.UserId == userId);

        if (method == null) return false;

        _context.PaymentMethods.Remove(method);
        await _context.SaveChangesAsync();
        return true;
    }
}
