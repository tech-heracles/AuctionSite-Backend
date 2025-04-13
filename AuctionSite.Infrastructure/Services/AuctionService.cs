using AuctionSite.Core.DTOs;
using AuctionSite.Core.Entities;
using AuctionSite.Core.Interfaces.Services;
using AuctionSite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionSite.Infrastructure.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<AuctionService> _logger;

        public AuctionService(
            ApplicationDbContext context,
            IUserService userService,
            ILogger<AuctionService> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        public async Task<List<AuctionListItemDto>> GetActiveAuctionsAsync(AuctionListFilters auctionFilters = null)
        {
            var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));

            var query = _context.Auctions.Where(a => a.EndDate > currentTime && a.Status == true);
            if (auctionFilters != null)
            {
                if (!string.IsNullOrWhiteSpace(auctionFilters.searchTerm))
                {
                    string searchTerm = auctionFilters.searchTerm.ToLower();
                    query = query.Where(a => a.Title.ToLower().Contains(searchTerm) ||
                                            a.Description.ToLower().Contains(searchTerm));
                }

                if (auctionFilters.minPrice > 0)
                {
                    query = query.Where(a => a.CurrentHighestBid >= auctionFilters.minPrice);
                }

                if (auctionFilters.maxPrice > 0)
                {
                    query = query.Where(a => a.CurrentHighestBid <= auctionFilters.maxPrice);
                }
            }

            var auctions = await query
                .OrderBy(a => a.EndDate)
                .ToListAsync();

            //var auctions = await _context.Auctions
            //    .Where(a => a.EndDate > currentTime && a.Status == true)
            //    .OrderBy(a => a.EndDate)
            //    .ToListAsync();

            return auctions.Select(a => new AuctionListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                CurrentBid = a.CurrentHighestBid,
                EndDate = a.EndDate,
                TimeRemaining = a.EndDate - currentTime,
                SellerUsername = a.SellerUsername
            }).ToList();
        }

        public async Task<AuctionDto> GetAuctionByIdAsync(int id)
        {
            var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));

            var auction = await _context.Auctions
                .Include(a => a.Bids)
                    .ThenInclude(b => b.Bidder)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
            {
                _logger.LogWarning($"Auction with id {id} not found");
                return null;
            }

            return new AuctionDto
            {
                Id = auction.Id,
                Title = auction.Title,
                Description = auction.Description,
                StartingPrice = auction.StartingPrice,
                CurrentHighestBid = auction.CurrentHighestBid,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                ImageUrl = auction.ImageUrl,
                SellerId = auction.SellerId,
                SellerUsername = auction.SellerUsername,
                HighestBidderId = auction.HighestBidderId,
                HighestBidderUsername = auction.HighestBidderUsername,
                Status = auction.EndDate >= currentTime && auction.Status,
                TimeRemaining = auction.EndDate > currentTime ? auction.EndDate - currentTime : TimeSpan.Zero,
                Bids = auction.Bids.Select(b => new BidDto
                {
                    Id = b.Id,
                    AuctionId = b.AuctionId,
                    BidderId = b.BidderId,
                    BidderUsername = b.Bidder.Username,
                    Amount = b.Amount,
                    PlacedAt = b.PlacedAt
                }).OrderByDescending(b => b.Amount).ToList()
            };
        }

        public async Task<AuctionDto> CreateAuctionAsync(CreateAuctionDto auctionDto, int userId)
        {
            if (auctionDto.EndDate <= TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")))
            {
                _logger.LogWarning($"User {userId} attempted to create auction with invalid end date");
                throw new ArgumentException("End date must be in the future");
            }

            // Get the seller to set the username
            var seller = await _context.Users.FindAsync(userId);
            if (seller == null)
            {
                throw new ArgumentException("Seller not found");
            }

            var auction = new Auction
            {
                Title = auctionDto.Title,
                Description = auctionDto.Description,
                StartingPrice = auctionDto.StartingBid,
                CurrentHighestBid = auctionDto.StartingBid,
                StartDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")),
                EndDate = auctionDto.EndDate,
                ImageUrl = auctionDto.ImageUrl,
                SellerId = userId,
                SellerUsername = seller.Username,
                Bids = new List<Bid>()
            };

            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} created auction {auction.Id} with title '{auction.Title}'");

            return new AuctionDto
            {
                Id = auction.Id,
                Title = auction.Title,
                Description = auction.Description,
                StartingPrice = auction.StartingPrice,
                CurrentHighestBid = auction.CurrentHighestBid,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                ImageUrl = auction.ImageUrl,
                SellerId = auction.SellerId,
                SellerUsername = auction.SellerUsername,
                HighestBidderId = null,
                HighestBidderUsername = null,
                Status = true,
                TimeRemaining = auction.EndDate - TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")),
                Bids = new List<BidDto>()
            };
        }

        public async Task<BidDto> PlaceBidAsync(CreateBidDto bidDto, int userId)
        {
            // Find the auction
            var auction = await _context.Auctions
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == bidDto.AuctionId);

            if (auction == null)
            {
                _logger.LogWarning($"User {userId} attempted to bid on non-existent auction {bidDto.AuctionId}");
                throw new ArgumentException("Auction not found");
            }

            // Check if auction is still active
            if (auction.EndDate <= TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")) || auction.Status == false)
            {
                _logger.LogWarning($"User {userId} attempted to bid on ended auction {bidDto.AuctionId}");
                throw new ArgumentException("Auction has ended");
            }

            // Check if the user is trying to bid on their own auction
            if (auction.SellerId == userId)
            {
                _logger.LogWarning($"User {userId} attempted to bid on their own auction {bidDto.AuctionId}");
                throw new ArgumentException("Cannot bid on your own auction");
            }

            // Check if the bid amount is higher than the current highest bid
            if (bidDto.Amount <= auction.CurrentHighestBid)
            {
                _logger.LogWarning($"User {userId} attempted to place a bid lower than the current highest bid");
                throw new ArgumentException($"Bid must be higher than the current bid of {auction.CurrentHighestBid}");
            }

            // Check if the user has enough funds
            var userBalance = await _userService.GetUserBalanceAsync(userId);
            if (userBalance < bidDto.Amount)
            {
                _logger.LogWarning($"User {userId} attempted to place a bid with insufficient funds");
                throw new ArgumentException("Insufficient funds to place this bid");
            }

            // Get the bidder for username
            var bidder = await _context.Users.FindAsync(userId);
            if (bidder == null)
            {
                throw new ArgumentException("Bidder not found");
            }

            // Create the bid
            var bid = new Bid
            {
                AuctionId = bidDto.AuctionId,
                BidderId = userId,
                Amount = bidDto.Amount,
                PlacedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")),
                Bidder = bidder
            };

            // Update the auction with the new highest bid
            auction.CurrentHighestBid = bidDto.Amount;
            auction.HighestBidderId = userId;
            auction.HighestBidderUsername = bidder.Username;

            // Save the changes
            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} placed a bid of {bidDto.Amount} on auction {bidDto.AuctionId}");

            return new BidDto
            {
                Id = bid.Id,
                AuctionId = bid.AuctionId,
                BidderId = bid.BidderId,
                BidderUsername = bidder.Username,
                Amount = bid.Amount,
                PlacedAt = bid.PlacedAt
            };
        }

        public async Task CompleteEndedAuctionsAsync()
        {
            TimeZoneInfo albaniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, albaniaTimeZone);

            // Find all auctions that have ended but are still marked as active
            var endedAuctions = await _context.Auctions
                .Where(a => a.EndDate <= currentTime && a.Status == true)
                .ToListAsync();

            foreach (var auction in endedAuctions)
            {
                try
                {
                    // If there's a highest bidder, transfer the funds
                    if (auction.HighestBidderId.HasValue)
                    {
                        await _userService.TransferFundsAsync(
                            auction.HighestBidderId.Value,
                            auction.SellerId,
                            auction.CurrentHighestBid);

                        _logger.LogInformation($"Auction {auction.Id} completed. Funds transferred from user {auction.HighestBidderId} to seller {auction.SellerId}");
                    }
                    else
                    {
                        _logger.LogInformation($"Auction {auction.Id} ended with no bids");
                    }

                    // Mark the auction as completed
                    auction.Status = false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error completing auction {auction.Id}");
                    // Continue with other auctions even if one fails
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<AuctionDto>> GetUserAuctionsAsync(int userId)
        {
            var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));

            var auctions = await _context.Auctions
                .Include(a => a.Bids)
                    .ThenInclude(b => b.Bidder)
                .Where(a => a.SellerId == userId)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            return auctions.Select(auction => new AuctionDto
            {
                Id = auction.Id,
                Title = auction.Title,
                Description = auction.Description,
                StartingPrice = auction.StartingPrice,
                CurrentHighestBid = auction.CurrentHighestBid,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                ImageUrl = auction.ImageUrl,
                SellerId = auction.SellerId,
                SellerUsername = auction.SellerUsername,
                HighestBidderId = auction.HighestBidderId,
                HighestBidderUsername = auction.HighestBidderUsername,
                Status = auction.Status,
                TimeRemaining = auction.EndDate > currentTime ? auction.EndDate - currentTime : TimeSpan.Zero,
                Bids = auction.Bids.Select(b => new BidDto
                {
                    Id = b.Id,
                    AuctionId = b.AuctionId,
                    BidderId = b.BidderId,
                    BidderUsername = b.Bidder.Username,
                    Amount = b.Amount,
                    PlacedAt = b.PlacedAt
                }).OrderByDescending(b => b.Amount).ToList()
            }).ToList();
        }

        public async Task<List<AuctionDto>> GetUserBidsAsync(int userId)
        {
            var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));

            // Find all auctions the user has bid on
            var auctionIds = await _context.Bids
                .Where(b => b.BidderId == userId)
                .Select(b => b.AuctionId)
                .Distinct()
                .ToListAsync();

            var auctions = await _context.Auctions
                .Include(a => a.Bids)
                    .ThenInclude(b => b.Bidder)
                .Where(a => auctionIds.Contains(a.Id))
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            return auctions.Select(auction => new AuctionDto
            {
                Id = auction.Id,
                Title = auction.Title,
                Description = auction.Description,
                StartingPrice = auction.StartingPrice,
                CurrentHighestBid = auction.CurrentHighestBid,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                ImageUrl = auction.ImageUrl,
                SellerId = auction.SellerId,
                SellerUsername = auction.SellerUsername,
                HighestBidderId = auction.HighestBidderId,
                HighestBidderUsername = auction.HighestBidderUsername,
                Status = auction.Status,
                TimeRemaining = auction.EndDate > currentTime ? auction.EndDate - currentTime : TimeSpan.Zero,
                Bids = auction.Bids.Select(b => new BidDto
                {
                    Id = b.Id,
                    AuctionId = b.AuctionId,
                    BidderId = b.BidderId,
                    BidderUsername = b.Bidder.Username,
                    Amount = b.Amount,
                    PlacedAt = b.PlacedAt
                }).OrderByDescending(b => b.Amount).ToList()
            }).ToList();
        }
    }
}