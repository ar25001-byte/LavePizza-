using System.Security.Claims;
using VeraPizza.DTOs;
using VeraPizza.Models;

namespace VeraPizza.Services;

public interface IAuthServices
{
    Task<string?> LoginAsync(LoginRequest request);
    Task<UserResponse> RegisterAsync(UserRequest request);
    UserResponse GetCurrentUserProfile(ClaimsPrincipal userClaims);
}
