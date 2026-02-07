using RePlay.Domain.Enums;

namespace RePlay.Application.Interfaces;

public interface IReturnService
{
    Task<ReturnResult> InitiateReturnAsync(CreateReturnDto dto, Guid userId);
    Task<PagedResult<ReturnDto>> GetUserReturnsAsync(Guid userId, ReturnQueryParameters parameters);
    Task<PagedResult<ReturnDto>> GetAllReturnsAsync(ReturnQueryParameters parameters);
    Task<ReturnResult> ApproveReturnAsync(Guid returnId, ApproveReturnDto dto, Guid adminId);
    Task<ReturnResult> RejectReturnAsync(Guid returnId, string adminNotes, Guid adminId);
}

public class CreateReturnDto
{
    public Guid ToyId { get; set; }
    public string? UserNotes { get; set; }
}

public class ApproveReturnDto
{
    public ToyCondition ConditionOnReturn { get; set; }
    public string? AdminNotes { get; set; }
    public int? UserRating { get; set; }
    public string? RatingComment { get; set; }
}

public class ReturnQueryParameters
{
    public ReturnStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ReturnDto
{
    public Guid Id { get; set; }
    public ToyDto Toy { get; set; } = null!;
    public UserDto ReturnedByUser { get; set; } = null!;
    public UserDto? ApprovedByAdmin { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ConditionOnReturn { get; set; }
    public string? UserNotes { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class ReturnResult
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public ReturnDto? Return { get; set; }

    public static ReturnResult Success(ReturnDto returnDto, string? message = null)
        => new() { Succeeded = true, Return = returnDto, Message = message };

    public static ReturnResult Failure(string message)
        => new() { Succeeded = false, Message = message };
}
