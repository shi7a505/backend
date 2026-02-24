using Application.DTOs.ChatBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ChatBot
{
    public interface IChatService
    {
        Task<SendChatMessageResponseDto> SendMessageAsync(string userId, SendChatMessageRequestDto request, CancellationToken ct = default);
        Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(string userId, CancellationToken ct = default);
        Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(string userId, Guid conversationId, CancellationToken ct = default);
    }
}
