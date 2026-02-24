using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(PasswordResetToken token, CancellationToken ct = default);
        Task<PasswordResetToken?> GetValidTokenAsync(int userId, string tokenHash, DateTime nowUtc, CancellationToken ct = default);
        Task InvalidateAllActiveForUserAsync(int userId, DateTime nowUtc, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
