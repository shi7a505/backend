using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ChatBot
{
    public interface IChatbotClient
    {
        Task<string> GetReplyAsync(string userId, Guid conversationId, string message, CancellationToken ct = default);
    }
}
