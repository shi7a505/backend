using Application.DTOs.ChatBot;
using Application.Interfaces.ChatBot;
using Core.Entities.ChatBot;
using Core.Enums;
using Core.Interfaces.ChatBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

public class ChatService : IChatService
{
    private const int MaxMessageLength = 4000;

    private readonly IConversationRepository _conversationRepository;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IChatbotClient _chatbotClient;

    public ChatService(
        IConversationRepository conversationRepository,
        IChatMessageRepository messageRepository,
        IChatbotClient chatbotClient)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _chatbotClient = chatbotClient;
    }

    public async Task<SendChatMessageResponseDto> SendMessageAsync(string userId, SendChatMessageRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var message = (request.Message ?? string.Empty).Trim();
        if (message.Length == 0)
            throw new ArgumentException("Message is required.");
        if (message.Length > MaxMessageLength)
            throw new ArgumentException($"Message is too long. Max length is {MaxMessageLength} characters.");

        Conversation conversation;
        if (request.ConversationId is null || request.ConversationId == Guid.Empty)
        {
            conversation = new Conversation { UserId = userId };
            await _conversationRepository.AddAsync(conversation, ct);
            await _conversationRepository.SaveChangesAsync(ct);
        }
        else
        {
            conversation = await _conversationRepository.GetByIdAsync(request.ConversationId.Value, ct)
                ?? throw new KeyNotFoundException("Conversation not found.");

            if (conversation.UserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to access this conversation.");
        }

        // Save user message
        await _messageRepository.AddAsync(new ChatMessage
        {
            ConversationId = conversation.Id,
            Sender = MessageSender.User,
            Content = message
        }, ct);

        // Get bot reply (Fake now, real later)
        var reply = await _chatbotClient.GetReplyAsync(userId, conversation.Id, message, ct);

        // Save bot message
        await _messageRepository.AddAsync(new ChatMessage
        {
            ConversationId = conversation.Id,
            Sender = MessageSender.Bot,
            Content = reply
        }, ct);

        conversation.UpdatedAtUtc = DateTime.UtcNow;
        await _messageRepository.SaveChangesAsync(ct);
        await _conversationRepository.SaveChangesAsync(ct);

        return new SendChatMessageResponseDto
        {
            ConversationId = conversation.Id,
            Reply = reply
        };
    }

    public async Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(string userId, CancellationToken ct = default)
    {
        var conversations = await _conversationRepository.GetByUserIdAsync(userId, ct);
        return conversations
            .OrderByDescending(c => c.UpdatedAtUtc)
            .Select(c => new ConversationDto
            {
                Id = c.Id,
                CreatedAtUtc = c.CreatedAtUtc,
                UpdatedAtUtc = c.UpdatedAtUtc
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(string userId, Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, ct)
            ?? throw new KeyNotFoundException("Conversation not found.");

        if (conversation.UserId != userId)
            throw new UnauthorizedAccessException("You are not allowed to access this conversation.");

        var messages = await _messageRepository.GetByConversationIdAsync(conversationId, ct);
        return messages
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                Sender = m.Sender.ToString(),
                Content = m.Content,
                CreatedAtUtc = m.CreatedAtUtc
            })
            .ToList();
    }
}