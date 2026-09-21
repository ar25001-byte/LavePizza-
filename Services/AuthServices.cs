using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VeraPizza.DTOs;
using VeraPizza.Models;

namespace VeraPizza.Services;

public class AuthServices : IAuthServices
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthServices(AppDbContext context, IMapper mapper, IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return null;

        // Verificación del Hash de la contraseña con BCrypt
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword) return null;

        // Generación del token JWT firmado con Claims
        return GenerateJwtToken(user);
    }

    public async Task<UserResponse> RegisterAsync(UserRequest request)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("El correo electrónico ya está registrado.");
        }

        var user = _mapper.Map<User>(request);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserResponse>(user);
    }

    public UserResponse GetCurrentUserProfile(ClaimsPrincipal userClaims)
    {
        var idClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usernameClaim = userClaims.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        var emailClaim = userClaims.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var roleClaim = userClaims.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        return new UserResponse
        {
            Id = int.TryParse(idClaim, out var id) ? id : 0,
            Username = usernameClaim,
            Email = emailClaim,
            Role = roleClaim,
            CreatedAt = DateTime.UtcNow
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Clave JWT no configurada.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expireMinutes = Convert.ToDouble(jwtSettings["ExpireMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
