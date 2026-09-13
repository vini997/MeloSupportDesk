using MeloSupportDesk.Api.Contracts.Authentication;
using MeloSupportDesk.Api.Domain.Entities;
using MeloSupportDesk.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace MeloSupportDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly SupportDeskDbContext _database;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        SupportDeskDbContext database,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _database = database;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    [HttpPost("register")]

    [HttpPost("login")]
public async Task<ActionResult<LoginResponse>> Login(
    LoginRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return BadRequest("Email and password are required.");
    }

    var normalizedEmail = request.Email
        .Trim()
        .ToLowerInvariant();

    var user = await _database.Users
        .SingleOrDefaultAsync(user =>
            user.Email == normalizedEmail
        );

    if (user is null || !user.IsActive)
    {
        return Unauthorized("Invalid email or password.");
    }

    var passwordResult = _passwordHasher.VerifyHashedPassword(
        user,
        user.PasswordHash,
        request.Password
    );

    if (passwordResult == PasswordVerificationResult.Failed)
    {
        return Unauthorized("Invalid email or password.");
    }

    var expiresAtUtc = DateTime.UtcNow.AddHours(8);

    var claims = new[]
    {
        new Claim(
            JwtRegisteredClaimNames.Sub,
            user.Id.ToString()
        ),
        new Claim(
            JwtRegisteredClaimNames.Email,
            user.Email
        ),
        new Claim(
            ClaimTypes.Name,
            user.FullName
        ),
        new Claim(
            ClaimTypes.Role,
            user.Role.ToString()
        ),
        new Claim(
            JwtRegisteredClaimNames.Jti,
            Guid.NewGuid().ToString()
        )
    };

    var jwtKey = _configuration["Jwt:Key"]
        ?? throw new InvalidOperationException(
            "JWT key is not configured."
        );

    var securityKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtKey)
    );

    var credentials = new SigningCredentials(
        securityKey,
        SecurityAlgorithms.HmacSha256
    );

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: expiresAtUtc,
        signingCredentials: credentials
    );

    var tokenValue = new JwtSecurityTokenHandler()
        .WriteToken(token);

    return Ok(new LoginResponse(
        tokenValue,
        expiresAtUtc,
        user.Id,
        user.FullName,
        user.Email,
        user.Role
    ));
}
[Authorize]
[HttpGet("me")]
public async Task<ActionResult<UserResponse>> GetCurrentUser()
{
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

    return Ok(new UserResponse(
        user.Id,
        user.FullName,
        user.Email,
        user.Role,
        user.IsActive,
        user.CreatedAtUtc
    ));
}
public async Task<ActionResult<UserResponse>> Register(
    RegisterRequest request)
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
        Email = normalizedEmail
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

    return StatusCode(StatusCodes.Status201Created, response);
}
}