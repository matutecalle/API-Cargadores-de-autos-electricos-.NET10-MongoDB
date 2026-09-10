using System.ComponentModel.DataAnnotations;

namespace APICargadores.DTOs;

public record RegisterDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string DisplayName);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    string Email,
    string DisplayName,
    string Role);
