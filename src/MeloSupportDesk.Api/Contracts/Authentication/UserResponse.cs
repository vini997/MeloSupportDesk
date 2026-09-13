using MeloSupportDesk.Api.Domain.Enums;

namespace MeloSupportDesk.Api.Contracts.Authentication;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAtUtc
);