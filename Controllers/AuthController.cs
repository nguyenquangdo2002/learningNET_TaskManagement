using Microsoft.AspNetCore.Mvc;
using TaskManagement.DTOs;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (result == null)
            return BadRequest(new { message = "Username already exists" });

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { message = "Invalid username or password" });

        return Ok(result);
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto dto)
    {
        var result = await _authService.GoogleLoginAsync(dto.IdToken);
        if (result == null)
            return Unauthorized(new { message = "Google login failed" });
        return Ok(result);
    }

    /// <summary>
    /// Gửi refresh token → nhận cặp access + refresh token mới (Token Rotation)
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        if (result == null)
            return Unauthorized(new { message = "Invalid or expired refresh token" });

        return Ok(result);
    }

    /// <summary>
    /// Revoke refresh token khi logout — vô hiệu hóa token
    /// </summary>
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RevokeTokenAsync(dto.RefreshToken);
        if (!result)
            return BadRequest(new { message = "Token not found or already revoked" });

        return Ok(new { message = "Token revoked successfully" });
    }
}