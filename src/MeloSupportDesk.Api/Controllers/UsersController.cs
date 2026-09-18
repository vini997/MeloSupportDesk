using MeloSupportDesk.Api.Contracts.Authentication;
using MeloSupportDesk.Api.Contracts.Users;
using MeloSupportDesk.Api.Domain.Entities;
using MeloSupportDesk.Api.Domain.Enums;
using MeloSupportDesk.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeloSupportDesk.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly SupportDeskDbContext _database;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(
        SupportDeskDbContext database,
        IPasswordHasher<User> passwordHasher)
    {
        _database = database;
        _passwordHasher = passwordHasher;
    }
[HttpGet("technicians")]
public async Task<ActionResult<List<UserResponse>>> GetTechnicians()
{
    var technicians = await _database.Users
        .AsNoTracking()
        .Where(user =>
            user.Role == UserRole.Technician &&
            user.IsActive
        )
        .OrderBy(user => user.FullName)
        .Select(user => new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAtUtc
        ))
        .ToListAsync();

    return Ok(technicians);
}
    [HttpPost("staff")]
public async Task<ActionResult<UserResponse>> CreateStaff(
    CreateStaffRequest request)
{
    if (string.IsNullOrWhiteSpace(request.FullName) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return BadRequest(
            "Full name, email, and password are required."
        );
    }

    if (request.Password.Length < 8)
    {
        return BadRequest(
            "The password must contain at least 8 characters."
        );
    }

    if (request.Role != UserRole.Admin &&
        request.Role != UserRole.Technician)
    {
        return BadRequest(
            "Staff role must be Admin or Technician."
        );
    }

    var normalizedEmail = request.Email
        .Trim()
        .ToLowerInvariant();

    var emailAlreadyExists = await _database.Users
        .AnyAsync(user => user.Email == normalizedEmail);

    if (emailAlreadyExists)
    {
        return Conflict(
            "A user with this email already exists."
        );
    }

    var user = new User
    {
        FullName = request.FullName.Trim(),
        Email = normalizedEmail,
        Role = request.Role
    };

    user.PasswordHash = _passwordHasher.HashPassword(
        user,
        request.Password
    );

    _database.Users.Add(user);
    await _database.SaveChangesAsync();

    var response = new UserResponse(
        user.Id,
        user.FullName,
        user.Email,
        user.Role,
        user.IsActive,
        user.CreatedAtUtc
    );

    return StatusCode(
        StatusCodes.Status201Created,
        response
    );
}
}
