using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Shared.Infraestructure.Messaging;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outbox_messages");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).UseIdentityAlwaysColumn();

        b.Property(m => m.Type).HasMaxLength(300).IsRequired();
        b.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();
        b.Property(m => m.OccurredAt).IsRequired();
        b.Property(m => m.LastError).HasMaxLength(OutboxMessage.MaxErrorLength);

        // Índice parcial: el processor solo lee pendientes, que son pocas aunque la tabla crezca
        b.HasIndex(m => m.Id)
            .HasFilter("processed_at IS NULL")
            .HasDatabaseName("ix_outbox_messages_pending");
    }
}
