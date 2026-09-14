using MeloSupportDesk.Api.Domain.Enums;

namespace MeloSupportDesk.Api.Contracts.Tickets;

public sealed record UpdateTicketStatusRequest(
    TicketStatus Status
);