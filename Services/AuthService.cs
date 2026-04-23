using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.Data;
using TaskManagement.DTOs;
using TaskManagement.Models;

namespace TaskManagement.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<AuthResponseDto?> GoogleLoginAsync(string IdToken);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }
    public async Task<AuthResponseDto?> GoogleLoginAsync(string accessToken)
    {
        try
        {
            // Verify token bằng cách gọi Google API
            using var http = new HttpClient();
            var res = await http.GetAsync(
                $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}");

            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();
            var payload = System.Text.Json.JsonSerializer.Deserialize<GoogleUserInfo>(json);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == payload!.Email);

            if (user == null)
            {
                user = new User
                {
                    Username = payload!.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Role = Role.User
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return GenerateResponse(user);
        }
        catch { return null; }
    }
    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return null;

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Role.User
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return GenerateResponse(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return GenerateResponse(user);
    }

    private AuthResponseDto GenerateResponse(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }
}