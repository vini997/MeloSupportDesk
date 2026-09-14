using MeloSupportDesk.Api.Domain.Enums;

namespace MeloSupportDesk.Api.Contracts.Users;

public sealed record CreateStaffRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role
);