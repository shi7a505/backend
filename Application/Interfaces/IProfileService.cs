using Application.DTOs.Profile;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IProfileService
{
    // 🔹 Get current user profile
    Task<ProfileDto> GetMyProfileAsync(int userId);

    // 🔹 Create OR Update profile
    Task<ProfileDto> UpsertMyProfileAsync(
        int userId,
        UpsertProfileRequestDto dto
    );

    // 🔹 Upload / Update profile image
    Task UploadProfileImageAsync(
        int userId,
        IFormFile image
    );

    // 🔹 Soft delete profile
    Task SoftDeleteMyProfileAsync(int userId);
}