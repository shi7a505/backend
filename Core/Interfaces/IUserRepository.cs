using Core.Entities;

namespace Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User?> GetByExternalProviderAsync(string provider, string providerId);
    Task<bool> EmailExistsAsync(string email);

    Task AddAsync(User user);
    Task UpdateAsync(User user);
}