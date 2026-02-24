using Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IProfileService
{
    Task<ProfileDto?> GetMyProfileAsync(int userId);
    Task<ProfileDto> UpsertMyProfileAsync(int userId, UpsertProfileRequestDto dto);
    Task SoftDeleteMyProfileAsync(int userId);
}
