using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Profile
{
    public class ProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public string Email { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}
