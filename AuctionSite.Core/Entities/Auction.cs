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
        public bool IsActive { get; set; } = true;

        // // // // // // // // // // // //
        public int SellerId { get; set; }

        // // // // // // // // // // // //
        [ForeignKey("SellerId")]
        public User Seller { get; set; }
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}