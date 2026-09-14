namespace MeloSupportDesk.Api.Contracts.Tickets;

public sealed record AssignTicketRequest(
    Guid TechnicianId
);