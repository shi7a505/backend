using Application.DTOs.Profile;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly SecurityScannerDbContext _db;
    private readonly ICloudinaryService _cloudinaryService;

    public ProfileService(
        SecurityScannerDbContext db,
        ICloudinaryService cloudinaryService)
    {
        _db = db;
        _cloudinaryService = cloudinaryService;
    }

    // =============================
    // GET MY PROFILE
    // =============================
    public async Task<ProfileDto> GetMyProfileAsync(int userId)
    {
        var profile = await _db.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            throw new Exception("Profile not found");

        return MapToDto(profile);
    }

    // =============================
    // CREATE OR UPDATE PROFILE
    // =============================
    public async Task<ProfileDto> UpsertMyProfileAsync(
        int userId,
        UpsertProfileRequestDto dto)
    {
        var profile = await _db.Profiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
        {
            profile = new Profile
            {
                UserId = userId,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                Bio = dto.Bio,
                AvatarUrl = GenerateAvatarUrl(dto.FullName),
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Profiles.Add(profile);
        }
        else
        {
            // restore if soft deleted
            profile.DeletedAtUtc = null;

            profile.FullName = dto.FullName;
            profile.PhoneNumber = dto.PhoneNumber;
            profile.Address = dto.Address;
            profile.Bio = dto.Bio;
            profile.UpdatedAtUtc = DateTime.UtcNow;

            _db.Profiles.Update(profile);
        }

        await _db.SaveChangesAsync();
        return MapToDto(profile);
    }

    // =============================
    // UPLOAD / UPDATE PROFILE IMAGE (Cloudinary)
    // =============================
    public async Task UploadProfileImageAsync(int userId, IFormFile image)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            throw new Exception("Profile not found");

        // upload image to Cloudinary and delete old image
        var imageUrl = await _cloudinaryService.UploadProfileImageAsync(
            image,
            "profiles",
            profile.AvatarPublicId
        );

        // update profile with new image URL and PublicId
        profile.AvatarUrl = imageUrl;
        profile.AvatarPublicId = ExtractPublicId(imageUrl);
        profile.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // =============================
    // SOFT DELETE PROFILE
    // =============================
    public async Task SoftDeleteMyProfileAsync(int userId)
    {
        var profile = await _db.Profiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            return;

        if (profile.DeletedAtUtc is null)
        {
            profile.DeletedAtUtc = DateTime.UtcNow;
            profile.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    // =============================
    // HELPERS
    // =============================
    private static ProfileDto MapToDto(Profile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        FullName = p.FullName,
        PhoneNumber = p.PhoneNumber,
        Address = p.Address,
        Bio = p.Bio,
        AvatarUrl = p.AvatarUrl,
        CreatedAtUtc = p.CreatedAtUtc,
        UpdatedAtUtc = p.UpdatedAtUtc
    };

    private static string GenerateAvatarUrl(string name)
    {
        var encodedName = Uri.EscapeDataString(name);
        return $"https://ui-avatars.com/api/?name={encodedName}&background=random&size=256";
    }

    private static string ExtractPublicId(string url)
    {
        // example URL: https://res.cloudinary.com/demo/image/upload/v123456/profiles/abc123.jpg
        var uri = new Uri(url);
        var segments = uri.AbsolutePath.Split('/');
        var fileName = segments[^1]; // take last segment "abc123.jpg"
        return fileName.Split('.')[0]; // remove extension
    }
}