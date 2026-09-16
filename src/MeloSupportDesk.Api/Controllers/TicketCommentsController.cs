using System.Security.Claims;
using MeloSupportDesk.Api.Contracts.Tickets;
using MeloSupportDesk.Api.Domain.Entities;
using MeloSupportDesk.Api.Domain.Enums;
using MeloSupportDesk.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeloSupportDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:guid}/comments")]
public class TicketCommentsController : ControllerBase
{
    private readonly SupportDeskDbContext _database;

    public TicketCommentsController(SupportDeskDbContext database)
    {
        _database = database;
    }

    [HttpPost]
    public async Task<ActionResult<TicketCommentResponse>> CreateComment(
        Guid ticketId,
        CreateTicketCommentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required.");
        }

        if (request.Message.Trim().Length > 2000)
        {
            return BadRequest(
                "Message cannot exceed 2000 characters."
            );
        }

        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        var roleValue = User.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            !Enum.TryParse<UserRole>(roleValue, out var role))
        {
            return Unauthorized();
        }

        var ticket = await _database.Tickets
            .SingleOrDefaultAsync(ticket => ticket.Id == ticketId);

        if (ticket is null)
        {
            return NotFound("Ticket not found.");
        }

        if (!CanAccessTicket(ticket, userId, role))
        {
            return Forbid();
        }

        if (request.IsInternal && role == UserRole.Customer)
        {
            return Forbid();
        }

        if (ticket.Status == TicketStatus.Closed)
        {
            return BadRequest(
                "Comments cannot be added to a closed ticket."
            );
        }

        var author = await _database.Users.FindAsync(userId);

        if (author is null || !author.IsActive)
        {
            return Unauthorized();
        }

        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            AuthorUserId = author.Id,
            Message = request.Message.Trim(),
            IsInternal = request.IsInternal,
            CreatedAtUtc = DateTime.UtcNow
        };

        _database.TicketComments.Add(comment);
        await _database.SaveChangesAsync();

        var response = new TicketCommentResponse(
            comment.Id,
            comment.TicketId,
            comment.AuthorUserId,
            author.FullName,
            comment.Message,
            comment.IsInternal,
            comment.CreatedAtUtc
        );

        return StatusCode(
            StatusCodes.Status201Created,
            response
        );
    }

    [HttpGet]
    public async Task<ActionResult<List<TicketCommentResponse>>> GetComments(
        Guid ticketId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        var roleValue = User.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            !Enum.TryParse<UserRole>(roleValue, out var role))
        {
            return Unauthorized();
        }

        var ticket = await _database.Tickets
            .AsNoTracking()
            .SingleOrDefaultAsync(ticket => ticket.Id == ticketId);

        if (ticket is null)
        {
            return NotFound("Ticket not found.");
        }

        if (!CanAccessTicket(ticket, userId, role))
        {
            return Forbid();
        }

        var commentsQuery = _database.TicketComments
            .AsNoTracking()
            .Include(comment => comment.AuthorUser)
            .Where(comment => comment.TicketId == ticketId);

        if (role == UserRole.Customer)
        {
            commentsQuery = commentsQuery.Where(
                comment => !comment.IsInternal
            );
        }

        var comments = await commentsQuery
            .OrderBy(comment => comment.CreatedAtUtc)
            .Select(comment => new TicketCommentResponse(
                comment.Id,
                comment.TicketId,
                comment.AuthorUserId,
                comment.AuthorUser.FullName,
                comment.Message,
                comment.IsInternal,
                comment.CreatedAtUtc
            ))
            .ToListAsync();

        return Ok(comments);
    }

    private static bool CanAccessTicket(
        Ticket ticket,
        Guid userId,
        UserRole role)
    {
        return role == UserRole.Admin ||
               ticket.CreatedByUserId == userId ||
               ticket.AssignedToUserId == userId;
    }
}