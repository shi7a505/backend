using Core.Entities;

namespace Core.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(User user);
    string GenerateRefreshToken();
    string SecretKey { get; } // لتسهيل التحقق من التوكن
}