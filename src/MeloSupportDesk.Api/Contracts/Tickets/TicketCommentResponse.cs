namespace MeloSupportDesk.Api.Contracts.Tickets;

public record TicketCommentResponse(
    Guid Id,
    Guid TicketId,
    Guid AuthorUserId,
    string AuthorUserName,
    string Message,
    bool IsInternal,
    DateTime CreatedAtUtc
);