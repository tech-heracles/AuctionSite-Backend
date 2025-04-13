using AuctionSite.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionSite.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.WalletBalance)
                     .HasColumnType("decimal(18,2)")
                     .HasDefaultValue(1000.00M);
                entity.HasMany(u => u.Bids)
                     .WithOne(b => b.Bidder)
                     .HasForeignKey(b => b.BidderId)
                     .OnDelete(DeleteBehavior.Restrict);
            });

            //Auction
            modelBuilder.Entity<Auction>(entity =>
            {
                entity.HasMany(a => a.Bids)
                     .WithOne(b => b.Auction)
                     .HasForeignKey(b => b.AuctionId)
                     .OnDelete(DeleteBehavior.Cascade);
                entity.Property(a => a.Status)
                     .HasDefaultValue(true);
                entity.Property(a => a.StartDate)
                     .HasDefaultValueSql("GETUTCDATE()");
                entity.Property(a => a.StartingPrice)
                     .HasColumnType("decimal(18,2)");
            });

            //Bid
            modelBuilder.Entity<Bid>(entity =>
            {
                entity.Property(b => b.Amount)
                     .HasColumnType("decimal(18,2)");
            });
        }
    }
}