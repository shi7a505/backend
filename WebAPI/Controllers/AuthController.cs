using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.DTOs.Password;
using Application.Services;
using Core.DTOS;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    // ✅ Register
    [HttpPost("register")]
    public async Task<ActionResult<ResponseDto<string>>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var token = await _authService.RegisterAsync(
                registerDto.Email,
                registerDto.Username,
                registerDto.FirstName,
                registerDto.LastName,
                registerDto.Password
            );

            return Ok(new ResponseDto<string>
            {
                Success = true,
                Message = "User registered successfully",
                Data = token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email: {Email}", registerDto.Email);
            return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
    }

    // ✅ Login
    [HttpPost("login")]
    public async Task<ActionResult<ResponseDto<AuthResultDto>>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var authResult = await _authService.LoginAsync(loginDto.Email, loginDto.Password);

            return Ok(new ResponseDto<AuthResultDto>
            {
                Success = true,
                Message = "Login successful",
                Data = authResult
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for email: {Email}", loginDto.Email);
            return Unauthorized(new ResponseDto<AuthResultDto>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for email: {Email}", loginDto.Email);
            return BadRequest(new ResponseDto<AuthResultDto>
            {
                Success = false,
                Message = "Login failed",
                Data = null
            });
        }
    }

    // ✅ Refresh Token
    [HttpPost("refresh")]
    public async Task<ActionResult<ResponseDto<AuthResultDto>>> Refresh([FromBody] RefreshTokenDto refreshDto)
    {
        if (refreshDto == null || string.IsNullOrWhiteSpace(refreshDto.RefreshToken))
        {
            return BadRequest(new ResponseDto<AuthResultDto>
            {
                Success = false,
                Message = "Refresh token is required",
                Data = null
            });
        }

        try
        {
            var authResult = await _authService.RefreshTokenAsync(refreshDto.RefreshToken);

            return Ok(new ResponseDto<AuthResultDto>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = authResult
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Refresh token failed");
            return Unauthorized(new ResponseDto<AuthResultDto>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token failed");
            return BadRequest(new ResponseDto<AuthResultDto>
            {
                Success = false,
                Message = "Token refresh failed",
                Data = null
            });
        }
    }

    // ✅ Validate Token (200 لو valid / 401 لو invalid / 400 لو token ناقص)
    [HttpPost("validate-token")]
    public async Task<ActionResult<ResponseDto<bool>>> ValidateToken([FromBody] TokenDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest(new ResponseDto<bool> { Success = false, Message = "Token is required", Data = false });

        var isValid = await _authService.ValidateTokenAsync(dto.Token);

        if (!isValid)
            return Unauthorized(new ResponseDto<bool> { Success = false, Message = "Invalid or expired token", Data = false });

        return Ok(new ResponseDto<bool> { Success = true, Message = "Token is valid", Data = true });
    }

    // ✅ Google Login (Login + Auto Register)
    [HttpPost("google")]
    public async Task<ActionResult<ResponseDto<AuthResultDto>>> Google([FromBody] GoogleLoginDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.IdToken))
        {
            return BadRequest(new ResponseDto<AuthResultDto>
            {
                Success = false,
                Message = "IdToken is required",
                Data = null
            });
        }

        try
        {
            var result = await _authService.GoogleLoginAsync(dto.IdToken);
            return Ok(new ResponseDto<AuthResultDto>
            {
                Success = true,
                Message = "Google login successful",
                Data = result
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ResponseDto<AuthResultDto>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google login failed");
            return BadRequest(new ResponseDto<AuthResultDto>
            {
                Success = false,
                Message = "Google login failed",
                Data = null
            });
        }
    }


[HttpPost("forgot-password")]
public async Task<ActionResult<ResponseDto<string>>> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken ct)
{
    await _authService.ForgotPasswordAsync(dto.Email, ct);

    return Ok(new ResponseDto<string>
    {
        Success = true,
        Message = "If the email exists, a reset link has been sent.",
        Data = null
    });
}

[HttpPost("reset-password")]
public async Task<ActionResult<ResponseDto<string>>> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct)
{
    try
    {
        await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword, ct);

        return Ok(new ResponseDto<string>
        {
            Success = true,
            Message = "Password reset successfully",
            Data = null
        });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Unauthorized(new ResponseDto<string> { Success = false, Message = ex.Message, Data = null });
    }
    catch (Exception ex)
    {
        return BadRequest(new ResponseDto<string> { Success = false, Message = ex.Message, Data = null });
    }
}
}