using Application.Interfaces;
using Application.Interfaces.ChatBot;

namespace Infrastructure.Services;

public class FakeChatbotClient : IChatbotClient
{
    public Task<string> GetReplyAsync(string userId, Guid conversationId, string message, CancellationToken ct = default)
    {
        // Temporary reply until Python service integration
        return Task.FromResult($"(Bot) وصلتني رسالتك: {message}");
    }
}