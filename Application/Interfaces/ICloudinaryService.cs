using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadProfileImageAsync(
            IFormFile file,
            string folderName,
            string? oldImagePublicId = null
        );

        Task DeleteImageAsync(string publicId);
    }
}
