using VeraPizza.DTOs;

namespace VeraPizza.Services;

public interface IUserServices
{
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<UserResponse> CreateUserAsync(UserRequest request);
    Task<string?> LoginAsync(LoginRequest request);
}
