using MeloSupportDesk.Api.Domain.Enums;

namespace MeloSupportDesk.Api.Contracts.Tickets;

public class TicketQueryParameters
{
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? Category { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}