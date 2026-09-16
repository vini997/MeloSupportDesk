using MeloSupportDesk.Api.Contracts.Tickets;
using MeloSupportDesk.Api.Domain.Enums;
using MeloSupportDesk.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeloSupportDesk.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/admin/tickets")]
public class AdminTicketsController : ControllerBase
{
    private readonly SupportDeskDbContext _database;

    public AdminTicketsController(SupportDeskDbContext database)
    {
        _database = database;
    }

    [HttpGet]
    public async Task<ActionResult> GetTickets(
        [FromQuery] TicketQueryParameters parameters)
    {
        var page = Math.Max(parameters.Page, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var query = _database.Tickets
            .AsNoTracking()
            .AsQueryable();

        if (parameters.Status.HasValue)
        {
            query = query.Where(ticket =>
                ticket.Status == parameters.Status.Value
            );
        }

        if (parameters.Priority.HasValue)
        {
            query = query.Where(ticket =>
                ticket.Priority == parameters.Priority.Value
            );
        }

        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            var category = parameters.Category.Trim();

            query = query.Where(ticket =>
                EF.Functions.ILike(ticket.Category, category)
            );
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = $"%{parameters.Search.Trim()}%";

            query = query.Where(ticket =>
                EF.Functions.ILike(ticket.TicketNumber, search) ||
                EF.Functions.ILike(ticket.Title, search) ||
                EF.Functions.ILike(ticket.Description, search)
            );
        }

        var totalCount = await query.CountAsync();

        var tickets = await query
            .OrderByDescending(ticket => ticket.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ticket => new TicketResponse(
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
                ticket.AssignedToUser == null
                    ? null
                    : ticket.AssignedToUser.FullName,
                ticket.CreatedAtUtc,
                ticket.UpdatedAtUtc,
                ticket.ResolvedAtUtc,
                ticket.ClosedAtUtc
            ))
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize
        );

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            totalPages,
            items = tickets
        });
    }
}