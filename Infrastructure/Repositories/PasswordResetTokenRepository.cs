//using Core.Entities;
//using Core.Interfaces;
//using Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Infrastructure.Repositories
//{
//    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
//    {
//        private readonly SecurityScannerDbContext _db;

//        public PasswordResetTokenRepository(SecurityScannerDbContext db)
//        {
//            _db = db;
//        }

//        public Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
//            => _db.PasswordResetTokens.AddAsync(token, ct).AsTask();

//        public Task SaveChangesAsync(CancellationToken ct = default)
//            => _db.SaveChangesAsync(ct);

//        public Task<PasswordResetToken?> GetValidTokenAsync(int userId, string tokenHash, DateTime nowUtc, CancellationToken ct = default)
//            => _db.PasswordResetTokens
//                .Where(t => t.UserId == userId
//                         && t.TokenHash == tokenHash
//                         && t.UsedAtUtc == null
//                         && t.ExpiresAtUtc > nowUtc)
//                .OrderByDescending(t => t.CreatedAtUtc)
//                .FirstOrDefaultAsync(ct);

//        public async Task InvalidateAllActiveForUserAsync(int userId, DateTime nowUtc, CancellationToken ct = default)
//        {
//            var active = await _db.PasswordResetTokens
//                .Where(t => t.UserId == userId && t.UsedAtUtc == null && t.ExpiresAtUtc > nowUtc)
//                .ToListAsync(ct);

//            foreach (var t in active)
//                t.UsedAtUtc = nowUtc;
//        }

//    }
//}
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly SecurityScannerDbContext _db;

    public PasswordResetTokenRepository(SecurityScannerDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
        => _db.PasswordResetTokens.AddAsync(token, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public Task<PasswordResetToken?> GetValidTokenAsync(int userId, string tokenHash, DateTime nowUtc, CancellationToken ct = default)
        => _db.PasswordResetTokens
            .Where(t => t.UserId == userId
                     && t.TokenHash == tokenHash
                     && t.UsedAtUtc == null
                     && t.ExpiresAtUtc > nowUtc)
            .OrderByDescending(t => t.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public async Task InvalidateAllActiveForUserAsync(int userId, DateTime nowUtc, CancellationToken ct = default)
    {
        var tokens = await _db.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAtUtc == null && t.ExpiresAtUtc > nowUtc)
            .ToListAsync(ct);

        foreach (var t in tokens)
            t.UsedAtUtc = nowUtc;
    }
}