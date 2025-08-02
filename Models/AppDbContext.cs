using Microsoft.EntityFrameworkCore;

namespace SYLOGOS.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<AppSetting> Settings { get; set; }
        // ReceiptSequence is used to track the last issued receipt number for each year
        // so that it can be incremented for the next receipt without reusing numbers.
        public DbSet<ReceiptSequence> ReceiptSequences { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=sylogos.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Member → Children
            modelBuilder.Entity<Member>()
                .HasMany(m => m.Children)
                .WithOne(c => c.Member)
                .HasForeignKey(c => c.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Member → Memberships
            modelBuilder.Entity<Member>()
                .HasMany(m => m.Memberships)
                .WithOne(ms => ms.Member)
                .HasForeignKey(ms => ms.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
