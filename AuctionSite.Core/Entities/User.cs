using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionSite.Core.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WalletBalance { get; set; } = 1000.00m;

  
        public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}