using Core.Entities.ChatBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.ChatBot
{
    public interface IChatMessageRepository
    {
        Task<IReadOnlyList<ChatMessage>> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default);
        Task AddAsync(ChatMessage message, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
