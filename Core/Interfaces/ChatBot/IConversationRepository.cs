using Core.Entities.ChatBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.ChatBot
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Conversation>> GetByUserIdAsync(string userId, CancellationToken ct = default);
        Task AddAsync(Conversation conversation, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
