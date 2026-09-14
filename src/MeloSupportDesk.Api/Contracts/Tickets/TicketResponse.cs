using MeloSupportDesk.Api.Domain.Enums;

namespace MeloSupportDesk.Api.Contracts.Tickets;

public sealed record TicketResponse(
    Guid Id,
    string TicketNumber,
    string Title,
    string Description,
    string Category,
    TicketPriority Priority,
    TicketStatus Status,
    Guid CreatedByUserId,
    string CreatedByUserName,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc
);