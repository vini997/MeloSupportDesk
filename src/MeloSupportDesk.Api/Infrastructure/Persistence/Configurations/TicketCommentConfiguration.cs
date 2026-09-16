using MeloSupportDesk.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeloSupportDesk.Api.Infrastructure.Persistence.Configurations;

public class TicketCommentConfiguration
    : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(comment => comment.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(comment => comment.Ticket)
            .WithMany(ticket => ticket.Comments)
            .HasForeignKey(comment => comment.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(comment => comment.AuthorUser)
            .WithMany(user => user.AuthoredComments)
            .HasForeignKey(comment => comment.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(comment => new
        {
            comment.TicketId,
            comment.CreatedAtUtc
        });
    }
}