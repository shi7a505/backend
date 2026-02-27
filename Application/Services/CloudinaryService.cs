using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var account = new Account(
            config["Cloudinary:di5dsl4rr"],
            config["Cloudinary:144287828318698"],
            config["Cloudinary:muQ3kctSBX7kFCeZ-9vfFwB_2R8"]
        );

        _cloudinary = new Cloudinary(new Account("di5dsl4rr","144287828318698","muQ3kctSBX7kFCeZ-9vfFwB_2R8"));
    }

    public async Task<string> UploadProfileImageAsync(
        IFormFile file,
        string folderName,
        string? oldImagePublicId = null)
    {
        if (file.Length <= 0)
            throw new Exception("Empty file");

        // delete old image if exists
        if (!string.IsNullOrWhiteSpace(oldImagePublicId))
        {
            await DeleteImageAsync(oldImagePublicId);
        }

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folderName,
            Transformation = new Transformation()
                .Width(400)
                .Height(400)
                .Crop("fill")
                .Gravity("face")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("Image upload failed");

        return result.SecureUrl.ToString();
    }

    public async Task DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        };

        await _cloudinary.DestroyAsync(deleteParams);
    }
}