using Core.Entities;
using Core.Entities.ChatBot;
using Core.Interfaces;
using Core.Interfaces.ChatBot;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly SecurityScannerDbContext _db;

    public ConversationRepository(SecurityScannerDbContext db)
    {
        _db = db;
    }

    public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Conversations.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Conversation>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await _db.Conversations.Where(c => c.UserId == userId).ToListAsync(ct);

    public Task AddAsync(Conversation conversation, CancellationToken ct = default)
        => _db.Conversations.AddAsync(conversation, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}