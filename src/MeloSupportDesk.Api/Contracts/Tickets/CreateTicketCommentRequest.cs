namespace MeloSupportDesk.Api.Contracts.Tickets;

public record CreateTicketCommentRequest(
    string Message,
    bool IsInternal = false
);