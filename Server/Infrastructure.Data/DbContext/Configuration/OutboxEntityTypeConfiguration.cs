using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.DbContext.Configuration;

public class OutboxEntityTypeConfiguration : IEntityTypeConfiguration<Outbox>
{
    public void Configure(EntityTypeBuilder<Outbox> builder)
    {
        builder.HasIndex(x => x.Id);

        builder.Property(x => x.PayloadJson)
            .HasColumnType("jsonb");
    }
}