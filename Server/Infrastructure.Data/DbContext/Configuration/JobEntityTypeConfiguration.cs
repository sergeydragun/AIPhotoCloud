using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.DbContext.Configuration;

public class JobEntityTypeConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasIndex(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Jobs)
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.TargetFolder)
            .WithMany(x => x.Jobs)
            .HasForeignKey(x => x.TargetFolderId);

        builder.HasOne(x => x.TargetFile)
            .WithMany(x => x.Jobs)
            .HasForeignKey(x => x.TargetFileId);
    }
}