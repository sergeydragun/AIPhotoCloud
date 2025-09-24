using Microsoft.EntityFrameworkCore;

namespace PhotoCloud.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<FileModel> Files { get; set; } = null!;
        public DbSet<FolderModel> FolderModel { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            //In future we can add some configuration by fluent api
        }
    }
}
