using AuctionSite.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionSite.Core.Interfaces.Services
{
    public interface IAuctionService
    {
        Task<List<AuctionListItemDto>> GetActiveAuctionsAsync(AuctionListFilters auctionFilters);
        Task<AuctionDto> GetAuctionByIdAsync(int id);
        Task<AuctionDto> CreateAuctionAsync(CreateAuctionDto auctionDto, int userId);
        Task<BidDto> PlaceBidAsync(CreateBidDto bidDto, int userId);
        Task CompleteEndedAuctionsAsync();
        Task<List<AuctionDto>> GetUserAuctionsAsync(int userId);
        Task<List<AuctionDto>> GetUserBidsAsync(int userId);
    }
}
