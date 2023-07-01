using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace WebProjectCloud.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<FileModel> Files { get; set; } = null!;
        public DbSet<FolderModel> FolderModel { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var dep1 = new FolderModel { Id = 1, Name = "BaseFolder" };

            modelBuilder.Entity<FolderModel>().HasData(dep1);
        }
    }
}
