using Core.DTOS;

namespace Core.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(string email, string username, string firstName, string lastName, string password);
    Task<AuthResultDto> LoginAsync(string email, string password);
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
    Task<AuthResultDto> GoogleLoginAsync(string idToken);
    Task<bool> ValidateTokenAsync(string token);
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default);
}