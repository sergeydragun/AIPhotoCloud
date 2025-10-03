using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.DbContext.Configuration;

public class FileEntityTypeConfiguration : IEntityTypeConfiguration<FileModel>
{
    public void Configure(EntityTypeBuilder<FileModel> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.User)
            .WithMany(c => c.Files)
            .HasForeignKey(x => x.UserId);
        
        builder.HasOne(x => x.Folder)
            .WithMany(c => c.Files)
            .HasForeignKey(x => x.FolderId);
    }
}