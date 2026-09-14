using MeloSupportDesk.Api.Domain.Enums;

namespace MeloSupportDesk.Api.Contracts.Tickets;

public sealed record CreateTicketRequest(
    string Title,
    string Description,
    string Category,
    TicketPriority Priority
);
