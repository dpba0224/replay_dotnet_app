using System.ComponentModel.DataAnnotations;

namespace RePlay.Application.Interfaces;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(SendMessageDto dto, Guid senderId);
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId);
    Task<List<MessageDto>> GetConversationMessagesAsync(Guid userId, Guid otherUserId);
    Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}

public class SendMessageDto
{
    [Required]
    public Guid ReceiverId { get; set; }
    public Guid? TradeId { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public Guid? TradeId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ConversationDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserProfileImage { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
