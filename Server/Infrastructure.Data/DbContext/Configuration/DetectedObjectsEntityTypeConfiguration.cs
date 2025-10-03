using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.DbContext.Configuration;

public class DetectedObjectsEntityTypeConfiguration : IEntityTypeConfiguration<DetectedObject>
{
    public void Configure(EntityTypeBuilder<DetectedObject> builder)
    {
        builder.HasIndex(x => x.Id);
        
        builder.HasOne(x => x.FileModel)
            .WithMany(x => x.DetectedObjects)
            .HasForeignKey(x => x.FileId);
    }
}