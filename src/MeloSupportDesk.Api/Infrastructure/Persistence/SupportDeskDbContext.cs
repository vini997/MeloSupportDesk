using MeloSupportDesk.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeloSupportDesk.Api.Infrastructure.Persistence;

public class SupportDeskDbContext : DbContext
{
    public SupportDeskDbContext(
        DbContextOptions<SupportDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(SupportDeskDbContext).Assembly
    );
}
}