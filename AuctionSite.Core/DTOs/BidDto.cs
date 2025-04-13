using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionSite.Core.DTOs
{
    public class CreateBidDto
    {
        [Required]
        public int AuctionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
        public decimal Amount { get; set; }
    }

    public class BidDto
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public int BidderId { get; set; }
        public string BidderUsername { get; set; }
        public decimal Amount { get; set; }
        public DateTime PlacedAt { get; set; }
    }
}
