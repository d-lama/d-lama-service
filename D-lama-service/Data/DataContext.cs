using Data.ProjectEntities;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) {}

        public DbSet<Project> Projects { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DataPoint> DataPoints { get; set; }
        public DbSet<TextDataPoint> TextDataPoints { get; set; }
        public DbSet<ImageDataPoint> ImageDataPoints { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<LabeledDataPoint> LabeledDataPoints { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Project>()
                .Property(b => b.CreationDate)
                .HasDefaultValueSql("getutcdate()");

            modelBuilder.Entity<TextDataPoint>()
                .Property(b => b.CreationDate)
                .HasDefaultValueSql("getutcdate()");

            modelBuilder.Entity<ImageDataPoint>()
                .Property(b => b.CreationDate)
                .HasDefaultValueSql("getutcdate()");

            // is needed to create the database because of circular cascade options
            modelBuilder.Entity<LabeledDataPoint>()
                .HasOne(b => b.User)
                .WithMany(b => b.LabeledDataPoints)
                .OnDelete(DeleteBehavior.NoAction);

            // is needed to create the database because of circular cascade options
            modelBuilder.Entity<LabeledDataPoint>()
                .HasOne(b => b.Label)
                .WithMany(b => b.LabeledDataPoints)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
