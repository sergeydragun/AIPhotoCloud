using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.DbContext.Configuration;

public class UserCredentialsEntityTypeConfiguration : IEntityTypeConfiguration<UserCredentials>
{
    public void Configure(EntityTypeBuilder<UserCredentials> builder)
    {
        builder
            .HasOne(u => u.User)
            .WithOne(c => c.Credentials)
            .HasForeignKey<UserCredentials>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}