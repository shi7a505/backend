using Core.Entities.ChatBot;
using Core.Interfaces.ChatBot;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.ChatBot
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly SecurityScannerDbContext _db;

        public ChatMessageRepository(SecurityScannerDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<ChatMessage>> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default)
            => await _db.ChatMessages.Where(m => m.ConversationId == conversationId).ToListAsync(ct);

        public Task AddAsync(ChatMessage message, CancellationToken ct = default)
            => _db.ChatMessages.AddAsync(message, ct).AsTask();

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
