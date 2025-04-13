using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionSite.Core.Entities
{
    public class Bid
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PlacedAt { get; set; }

        // // // // // // // // // // // //
        public int AuctionId { get; set; }
        public int BidderId { get; set; }

        // // // // // // // // // // // //
        [ForeignKey("AuctionId")]
        public Auction Auction { get; set; }

        [ForeignKey("BidderId")]
        public User Bidder { get; set; }
    }
}