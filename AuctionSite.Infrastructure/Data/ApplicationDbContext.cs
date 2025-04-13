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
            modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

            modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.WalletBalance)
                .HasDefaultValue(1000.00M);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Bids)
                .WithOne(b => b.Bidder)
                .HasForeignKey(b => b.BidderId)
                .OnDelete(DeleteBehavior.Restrict);

            //Auction
            modelBuilder.Entity<Auction>()
                .HasMany(a => a.Bids)
                .WithOne(b => b.Auction)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Auction>()
            .Property(a => a.Status)
            .HasDefaultValue(true);

            modelBuilder.Entity<Auction>()
                .Property(a => a.StartDate)
                .HasDefaultValueSql("GETUTCDATE()");

            //Bid
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Bidder)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.BidderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}