using VeraPizza.DTOs;

namespace VeraPizza.Services;

public interface IPaymentMethodServices
{
    Task<IEnumerable<PaymentMethodDto>> GetPaymentMethodsByUserIdAsync(int userId);
    Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(int id, int userId);
    Task<PaymentMethodDto> CreatePaymentMethodAsync(int userId, PaymentMethodCreateRequest request);
    Task<bool> DeletePaymentMethodAsync(int id, int userId);
}
