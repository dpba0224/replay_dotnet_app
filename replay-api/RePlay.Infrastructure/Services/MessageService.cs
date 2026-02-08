using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RePlay.Application.Interfaces;
using RePlay.Domain.Entities;
using RePlay.Infrastructure.Data;

namespace RePlay.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MessageService> _logger;

    public MessageService(AppDbContext context, ILogger<MessageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MessageDto> SendMessageAsync(SendMessageDto dto, Guid senderId)
    {
        // Validate receiver exists
        var receiver = await _context.Users.FindAsync(dto.ReceiverId);
        if (receiver == null)
            throw new ArgumentException("Recipient not found.");

        if (dto.ReceiverId == senderId)
            throw new ArgumentException("You cannot send a message to yourself.");

        // Validate content is not empty
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ArgumentException("Message content cannot be empty.");

        // Validate trade exists if provided
        if (dto.TradeId.HasValue)
        {
            var tradeExists = await _context.Trades.AnyAsync(t => t.Id == dto.TradeId.Value);
            if (!tradeExists)
                throw new ArgumentException("Referenced trade not found.");
        }

        var sender = await _context.Users.FindAsync(senderId);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            TradeId = dto.TradeId,
            Content = dto.Content.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Message sent from {SenderId} to {ReceiverId} (MessageId: {MessageId})",
            senderId, dto.ReceiverId, message.Id);

        return new MessageDto
        {
            Id = message.Id,
            SenderId = senderId,
            SenderName = sender?.FullName ?? string.Empty,
            ReceiverId = dto.ReceiverId,
            ReceiverName = receiver.FullName,
            TradeId = dto.TradeId,
            Content = message.Content,
            IsRead = false,
            CreatedAt = message.CreatedAt
        };
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
    {
        // Get all messages where user is sender or receiver
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        // Group by the other user in the conversation
        var conversations = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var otherUserId = g.Key;
                var lastMessage = g.First(); // Already ordered by CreatedAt desc
                var otherUser = lastMessage.SenderId == userId
                    ? lastMessage.Receiver
                    : lastMessage.Sender;

                return new ConversationDto
                {
                    UserId = otherUserId,
                    UserName = otherUser.FullName,
                    UserProfileImage = otherUser.ProfileImageUrl,
                    LastMessage = lastMessage.Content.Length > 100
                        ? lastMessage.Content[..100] + "..."
                        : lastMessage.Content,
                    LastMessageAt = lastMessage.CreatedAt,
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                };
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        return conversations;
    }

    public async Task<List<MessageDto>> GetConversationMessagesAsync(Guid userId, Guid otherUserId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        // Mark unread messages from the other user as read
        var unreadMessages = messages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .ToList();

        if (unreadMessages.Count > 0)
        {
            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }

        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = m.Sender.FullName,
            ReceiverId = m.ReceiverId,
            ReceiverName = m.Receiver.FullName,
            TradeId = m.TradeId,
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId)
    {
        var message = await _context.Messages.FindAsync(messageId);

        if (message == null)
            return false;

        // Only the receiver can mark a message as read
        if (message.ReceiverId != userId)
            return false;

        if (message.IsRead)
            return true;

        message.IsRead = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
    }
}
