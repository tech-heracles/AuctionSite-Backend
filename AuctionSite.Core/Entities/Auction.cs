using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionSite.Core.Entities
{
    public class Auction
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal StartingPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentHighestBid { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ImageUrl { get; set; }
        public bool Status { get; set; } = true;

        [ForeignKey("UserId")]
        public int SellerId { get; set; }

        [ForeignKey("Username")]
        public string? SellerUsername { get; set; }

        [ForeignKey("UserId")]
        public int? HighestBidderId { get; set; }

        [ForeignKey("Username")]
        public string? HighestBidderUsername { get; set; }
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}