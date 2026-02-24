using Application.DTOs.Auth;
using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly SecurityScannerDbContext _db;

    public ProfileService(SecurityScannerDbContext db)
    {
        _db = db;
    }

    public async Task<ProfileDto?> GetMyProfileAsync(int userId)
    {
        var profile = await _db.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return profile is null ? null : MapToDto(profile);
    }

    public async Task<ProfileDto> UpsertMyProfileAsync(int userId, UpsertProfileRequestDto dto)
    {
        // Because we added HasQueryFilter(DeletedAtUtc == null),
        // we use IgnoreQueryFilters() so we can "restore" deleted profiles.
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
                AvatarUrl = dto.AvatarUrl,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = null,
                DeletedAtUtc = null
            };

            _db.Profiles.Add(profile);
        }
        else
        {
            // restore if soft-deleted
            profile.DeletedAtUtc = null;

            profile.FullName = dto.FullName;
            profile.PhoneNumber = dto.PhoneNumber;
            profile.Address = dto.Address;
            profile.Bio = dto.Bio;
            profile.AvatarUrl = dto.AvatarUrl;
            profile.UpdatedAtUtc = DateTime.UtcNow;

            _db.Profiles.Update(profile);
        }

        await _db.SaveChangesAsync();
        return MapToDto(profile);
    }

    public async Task SoftDeleteMyProfileAsync(int userId)
    {
        var profile = await _db.Profiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            return; // or throw NotFound

        if (profile.DeletedAtUtc is null)
        {
            profile.DeletedAtUtc = DateTime.UtcNow;
            profile.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

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
}