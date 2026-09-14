using MeloSupportDesk.Api.Contracts.Tickets;
using MeloSupportDesk.Api.Domain.Entities;
using MeloSupportDesk.Api.Domain.Enums;
using MeloSupportDesk.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MeloSupportDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly SupportDeskDbContext _database;

    public TicketsController(SupportDeskDbContext database)
    {
        _database = database;
    }
[HttpPost]
public async Task<ActionResult<TicketResponse>> CreateTicket(
    CreateTicketRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Title) ||
        string.IsNullOrWhiteSpace(request.Description) ||
        string.IsNullOrWhiteSpace(request.Category))
    {
        return BadRequest(
            "Title, description, and category are required."
        );
    }

    if (!Enum.IsDefined(request.Priority))
    {
        return BadRequest("Invalid ticket priority.");
    }

    var userIdValue = User.FindFirstValue(
        ClaimTypes.NameIdentifier
    );

    if (!Guid.TryParse(userIdValue, out var userId))
    {
        return Unauthorized();
    }

    var user = await _database.Users.FindAsync(userId);

    if (user is null || !user.IsActive)
    {
        return Unauthorized();
    }

    var ticket = new Ticket
    {
        TicketNumber =
            $"TKT-{DateTime.UtcNow:yyyyMMdd}-" +
            Guid.NewGuid()
                .ToString("N")[..6]
                .ToUpperInvariant(),

        Title = request.Title.Trim(),
        Description = request.Description.Trim(),
        Category = request.Category.Trim(),
        Priority = request.Priority,
        Status = TicketStatus.Open,
        CreatedByUserId = user.Id
    };

    _database.Tickets.Add(ticket);
    await _database.SaveChangesAsync();

    var response = new TicketResponse(
        ticket.Id,
        ticket.TicketNumber,
        ticket.Title,
        ticket.Description,
        ticket.Category,
        ticket.Priority,
        ticket.Status,
        ticket.CreatedByUserId,
        user.FullName,
        ticket.AssignedToUserId,
        null,
        ticket.CreatedAtUtc,
        ticket.UpdatedAtUtc,
        ticket.ResolvedAtUtc,
        ticket.ClosedAtUtc
    );

    return StatusCode(
        StatusCodes.Status201Created,
        response
    );
}
[HttpGet("my")]
public async Task<ActionResult<List<TicketResponse>>> GetMyTickets()
{
    var userIdValue = User.FindFirstValue(
        ClaimTypes.NameIdentifier
    );

    if (!Guid.TryParse(userIdValue, out var userId))
    {
        return Unauthorized();
    }

    var tickets = await _database.Tickets
        .AsNoTracking()
        .Include(ticket => ticket.CreatedByUser)
        .Include(ticket => ticket.AssignedToUser)
        .Where(ticket => ticket.CreatedByUserId == userId)
        .OrderByDescending(ticket => ticket.CreatedAtUtc)
        .ToListAsync();

    var response = tickets.Select(ticket =>
        new TicketResponse(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Title,
            ticket.Description,
            ticket.Category,
            ticket.Priority,
            ticket.Status,
            ticket.CreatedByUserId,
            ticket.CreatedByUser.FullName,
            ticket.AssignedToUserId,
            ticket.AssignedToUser?.FullName,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.ResolvedAtUtc,
            ticket.ClosedAtUtc
        )
    ).ToList();

    return Ok(response);
}
[Authorize(Roles = nameof(UserRole.Admin))]
[HttpPatch("{ticketId:guid}/assign")]
public async Task<ActionResult<TicketResponse>> AssignTicket(
    Guid ticketId,
    AssignTicketRequest request)
{
    var ticket = await _database.Tickets
        .Include(ticket => ticket.CreatedByUser)
        .Include(ticket => ticket.AssignedToUser)
        .SingleOrDefaultAsync(ticket => ticket.Id == ticketId);

    if (ticket is null)
    {
        return NotFound("Ticket not found.");
    }

    if (ticket.Status == TicketStatus.Closed)
    {
        return BadRequest(
            "A closed ticket cannot be assigned."
        );
    }

    var technician = await _database.Users
        .SingleOrDefaultAsync(user =>
            user.Id == request.TechnicianId &&
            user.Role == UserRole.Technician &&
            user.IsActive
        );

    if (technician is null)
    {
        return BadRequest(
            "Active technician not found."
        );
    }

    ticket.AssignedToUserId = technician.Id;
    ticket.AssignedToUser = technician;
    ticket.Status = TicketStatus.InProgress;
    ticket.UpdatedAtUtc = DateTime.UtcNow;

    await _database.SaveChangesAsync();

    return Ok(new TicketResponse(
        ticket.Id,
        ticket.TicketNumber,
        ticket.Title,
        ticket.Description,
        ticket.Category,
        ticket.Priority,
        ticket.Status,
        ticket.CreatedByUserId,
        ticket.CreatedByUser.FullName,
        ticket.AssignedToUserId,
        technician.FullName,
        ticket.CreatedAtUtc,
        ticket.UpdatedAtUtc,
        ticket.ResolvedAtUtc,
        ticket.ClosedAtUtc
    ));
}
[Authorize(Roles = nameof(UserRole.Technician))]
[HttpGet("assigned")]
public async Task<ActionResult<List<TicketResponse>>>
    GetAssignedTickets()
{
    var userIdValue = User.FindFirstValue(
        ClaimTypes.NameIdentifier
    );

    if (!Guid.TryParse(userIdValue, out var technicianId))
    {
        return Unauthorized();
    }

    var tickets = await _database.Tickets
        .AsNoTracking()
        .Include(ticket => ticket.CreatedByUser)
        .Include(ticket => ticket.AssignedToUser)
        .Where(ticket =>
            ticket.AssignedToUserId == technicianId
        )
        .OrderByDescending(ticket => ticket.CreatedAtUtc)
        .ToListAsync();

    var response = tickets.Select(ticket =>
        new TicketResponse(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Title,
            ticket.Description,
            ticket.Category,
            ticket.Priority,
            ticket.Status,
            ticket.CreatedByUserId,
            ticket.CreatedByUser.FullName,
            ticket.AssignedToUserId,
            ticket.AssignedToUser?.FullName,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.ResolvedAtUtc,
            ticket.ClosedAtUtc
        )
    ).ToList();
        return Ok(response);
}
    [Authorize(
    Roles = nameof(UserRole.Admin) + "," +
            nameof(UserRole.Technician)
)]
[HttpPatch("{ticketId:guid}/status")]
public async Task<ActionResult<TicketResponse>> UpdateStatus(
    Guid ticketId,
    UpdateTicketStatusRequest request)
{
    if (!Enum.IsDefined(request.Status))
    {
        return BadRequest("Invalid ticket status.");
    }

    var userIdValue = User.FindFirstValue(
        ClaimTypes.NameIdentifier
    );

    if (!Guid.TryParse(userIdValue, out var userId))
    {
        return Unauthorized();
    }

    var ticket = await _database.Tickets
        .Include(ticket => ticket.CreatedByUser)
        .Include(ticket => ticket.AssignedToUser)
        .SingleOrDefaultAsync(ticket => ticket.Id == ticketId);

    if (ticket is null)
    {
        return NotFound("Ticket not found.");
    }

    var currentRole = User.FindFirstValue(ClaimTypes.Role);

    if (currentRole == nameof(UserRole.Technician) &&
        ticket.AssignedToUserId != userId)
    {
        return Forbid();
    }

    if (request.Status == TicketStatus.Closed &&
        currentRole != nameof(UserRole.Admin))
    {
        return Forbid();
    }

    var now = DateTime.UtcNow;

    ticket.Status = request.Status;
    ticket.UpdatedAtUtc = now;

    if (request.Status == TicketStatus.Resolved)
    {
        ticket.ResolvedAtUtc = now;
        ticket.ClosedAtUtc = null;
    }
    else if (request.Status == TicketStatus.Closed)
    {
        ticket.ResolvedAtUtc ??= now;
        ticket.ClosedAtUtc = now;
    }
    else
    {
        ticket.ResolvedAtUtc = null;
        ticket.ClosedAtUtc = null;
    }

    await _database.SaveChangesAsync();

    return Ok(new TicketResponse(
        ticket.Id,
        ticket.TicketNumber,
        ticket.Title,
        ticket.Description,
        ticket.Category,
        ticket.Priority,
        ticket.Status,
        ticket.CreatedByUserId,
        ticket.CreatedByUser.FullName,
        ticket.AssignedToUserId,
        ticket.AssignedToUser?.FullName,
        ticket.CreatedAtUtc,
        ticket.UpdatedAtUtc,
        ticket.ResolvedAtUtc,
        ticket.ClosedAtUtc
    ));
}
}