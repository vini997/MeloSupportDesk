using MeloSupportDesk.Api.Domain.Enums;

namespace MeloSupportDesk.Api.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string TicketNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public Guid? AssignedToUserId { get; set; }

    public User? AssignedToUser { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? ResolvedAtUtc { get; set; }

    public DateTime? ClosedAtUtc { get; set; }
}