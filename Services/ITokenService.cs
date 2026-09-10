using APICargadores.Models;

namespace APICargadores.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
