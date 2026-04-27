namespace TaskManagement.DTOs;

public class RegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class GoogleAuthDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
}

public class GoogleUserInfo
{
    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}