using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.DbContext.Configuration;

public class FolderEntityTypeConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.HasIndex(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Folders)
            .HasForeignKey(x => x.UserId);

        builder.HasMany(x => x.ChildrenFolders)
            .WithOne(x => x.ParentFolder)
            .HasForeignKey(x => x.ParentFolderId);
    }
}