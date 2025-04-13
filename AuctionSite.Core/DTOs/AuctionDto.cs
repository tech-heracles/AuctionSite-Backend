using AuctionSite.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionSite.Core.DTOs
{
    public class CreateAuctionDto
    {
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 100 characters")]
        public string Title { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Starting bid must be greater than 0")]
        public decimal StartingBid { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class AuctionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal CurrentHighestBid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ImageUrl { get; set; }
        public int SellerId { get; set; }
        public string SellerUsername { get; set; }
        public int? HighestBidderId { get; set; }
        public string? HighestBidderUsername { get; set; }
        public bool Status { get; set; } = true;
        public TimeSpan TimeRemaining { get; set; }
        public List<BidDto> Bids { get; set; }
    }

    public class AuctionListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal CurrentBid { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public string SellerUsername { get; set; }
    }

    public class AuctionListFilters
    {
        public string searchTerm { get; set; }
        public decimal minPrice { get; set; }
        public decimal maxPrice { get; set; }
    }
}
