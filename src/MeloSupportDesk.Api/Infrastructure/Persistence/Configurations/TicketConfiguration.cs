using MeloSupportDesk.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeloSupportDesk.Api.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.TicketNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(ticket => ticket.TicketNumber)
            .IsUnique();

        builder.Property(ticket => ticket.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ticket => ticket.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(ticket => ticket.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ticket => ticket.Priority)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(ticket => ticket.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne(ticket => ticket.CreatedByUser)
            .WithMany()
            .HasForeignKey(ticket => ticket.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ticket => ticket.AssignedToUser)
            .WithMany()
            .HasForeignKey(ticket => ticket.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ticket => ticket.Status);

        builder.HasIndex(ticket => ticket.Priority);

        builder.HasIndex(ticket => ticket.AssignedToUserId);
    }
}