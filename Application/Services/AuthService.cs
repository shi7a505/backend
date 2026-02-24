using Application.DTOs.Auth;
using Application.Interfaces;
using Core.DTOS;
using Core.Entities;
using Core.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class AuthService : IAuthService
{
    private const string GoogleProvider = "Google";

    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailSender _emailSender;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IConfiguration config,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _config = config;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailSender = emailSender;
    }

    public async Task<string> RegisterAsync(
        string email, string username,
        string firstName, string lastName,
        string password)
    {
        if (await _userRepository.EmailExistsAsync(email))
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Email = email,
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            Role = "User",
            IsActive = true,
            ExternalProvider = null,
            ExternalProviderId = null
        };

        await _userRepository.AddAsync(user);

        return _tokenService.CreateAccessToken(user);
    }

    public async Task<AuthResultDto> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is inactive");

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid refresh token");

        return await IssueTokensAsync(user);
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_tokenService.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    // ✅ Forgot Password: generate token + email link
    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        var user = await _userRepository.GetByEmailAsync(email);

        // IMPORTANT: do not reveal whether the email exists
        if (user == null || !user.IsActive)
            return;

        // If this is a social account (no password), you may choose to ignore
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return;

        var now = DateTime.UtcNow;

        // invalidate previously active tokens
        await _passwordResetTokenRepository.InvalidateAllActiveForUserAsync(user.Id, now, ct);

        var rawToken = GenerateSecureToken();
        var tokenHash = Sha256Hex(rawToken);

        var ttlMinutes = int.TryParse(_config["PasswordReset:TokenTtlMinutes"], out var m) ? m : 15;

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(ttlMinutes)
        };

        await _passwordResetTokenRepository.AddAsync(resetToken, ct);
        await _passwordResetTokenRepository.SaveChangesAsync(ct);

        var frontendBaseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:3000";
        var link = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(rawToken)}";

        var subject = "Reset your password";
        var body = $"""
            <p>We received a request to reset your password.</p>
            <p><a href="{link}">Click here to reset your password</a></p>
            <p>This link will expire in {ttlMinutes} minutes.</p>
            <p>If you did not request this, please ignore this email.</p>
        """;

        await _emailSender.SendAsync(email, subject, body, ct);
    }

    // ✅ Reset Password: validate token + set new password
    public async Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("Email, token, and new password are required.");

        var user = await _userRepository.GetByEmailAsync(email)
            ?? throw new UnauthorizedAccessException("Invalid or expired reset token.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is inactive.");

        var now = DateTime.UtcNow;

        var tokenHash = Sha256Hex(token);
        var validToken = await _passwordResetTokenRepository.GetValidTokenAsync(user.Id, tokenHash, now, ct);

        if (validToken == null)
            throw new UnauthorizedAccessException("Invalid or expired reset token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = now;

        validToken.UsedAtUtc = now;

        await _userRepository.UpdateAsync(user);
        await _passwordResetTokenRepository.SaveChangesAsync(ct);
    }

    // ✅ Google Login + Auto Register
    public async Task<AuthResultDto> GoogleLoginAsync(string idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            throw new UnauthorizedAccessException("IdToken is required");

        var googleClientId = _config["Authentication:Google:ClientId"];
        if (string.IsNullOrWhiteSpace(googleClientId))
            throw new InvalidOperationException("Google ClientId is not configured");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                });
        }
        catch
        {
            throw new UnauthorizedAccessException("Invalid Google token");
        }

        if (string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Subject))
            throw new UnauthorizedAccessException("Google token missing required claims");

        // 1) Look up by providerId
        var user = await _userRepository.GetByExternalProviderAsync(GoogleProvider, payload.Subject);

        // 2) Link by email if exists
        if (user == null)
        {
            var existingByEmail = await _userRepository.GetByEmailAsync(payload.Email);

            if (existingByEmail != null)
            {
                existingByEmail.ExternalProvider = GoogleProvider;
                existingByEmail.ExternalProviderId = payload.Subject;
                existingByEmail.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(existingByEmail);
                user = existingByEmail;
            }
        }

        // 3) Auto register
        if (user == null)
        {
            var firstName = payload.GivenName ?? payload.Name?.Split(' ').FirstOrDefault() ?? "Google";
            var lastName = payload.FamilyName ?? payload.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "User";

            user = new User
            {
                Email = payload.Email,
                Username = payload.Email.Split('@')[0],
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = string.Empty, // Social user
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ExternalProvider = GoogleProvider,
                ExternalProviderId = payload.Subject
            };

            await _userRepository.AddAsync(user);
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is inactive");

        return await IssueTokensAsync(user);
    }

    private async Task<AuthResultDto> IssueTokensAsync(User user)
    {
        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        // URL-safe Base64
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}