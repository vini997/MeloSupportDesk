namespace MeloSupportDesk.Api.Domain.Entities;

public class TicketComment
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid AuthorUserId { get; set; }
    public User AuthorUser { get; set; } = null!;

    public string Message { get; set; } = string.Empty;

    public bool IsInternal { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}