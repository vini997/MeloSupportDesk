using MeloSupportDesk.Api.Domain.Enums;

namespace MeloSupportDesk.Api.Contracts.Authentication;

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string FullName,
    string Email,
    UserRole Role
);