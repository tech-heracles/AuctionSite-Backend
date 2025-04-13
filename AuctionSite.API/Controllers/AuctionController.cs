using AuctionSite.Core.DTOs;
using AuctionSite.Core.Entities;
using AuctionSite.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuctionSite.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly ILogger<AuctionController> _logger;

        public AuctionController(
            IAuctionService auctionService,
            ILogger<AuctionController> logger)
        {
            _auctionService = auctionService;
            _logger = logger;
        }

        [HttpPost("active")]
        public async Task<ActionResult<List<AuctionListItemDto>>> GetActiveAuctions([FromBody] AuctionListFilters auctionFilters)
        {
            try
            {
                await _auctionService.CompleteEndedAuctionsAsync();

                var auctions = await _auctionService.GetActiveAuctionsAsync(auctionFilters);
                return Ok(auctions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active auctions");
                return StatusCode(500, "An error occurred while retrieving auctions");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(int id)
        {
            try
            {
                var auction = await _auctionService.GetAuctionByIdAsync(id);

                if (auction == null)
                {
                    return NotFound($"Auction with ID {id} not found");
                }

                return Ok(auction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting auction with id {id}");
                return StatusCode(500, "An error occurred while retrieving the auction");
            }
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction([FromBody] CreateAuctionDto auctionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserIdFromClaims();
                var createdAuction = await _auctionService.CreateAuctionAsync(auctionDto, userId);

                return CreatedAtAction(nameof(GetAuctionById), new { id = createdAuction.Id }, createdAuction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating auction");
                return StatusCode(500, "An error occurred while creating the auction");
            }
        }

        [HttpPost("bid")]
        public async Task<ActionResult<BidDto>> PlaceBid([FromBody] CreateBidDto bidDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserIdFromClaims();
                var bid = await _auctionService.PlaceBidAsync(bidDto, userId);

                return Ok(bid);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error placing bid on auction {bidDto.AuctionId}");
                return StatusCode(500, "An error occurred while placing your bid");
            }
        }

        [HttpGet("my-auctions")]
        public async Task<ActionResult<List<AuctionDto>>> GetMyAuctions()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var auctions = await _auctionService.GetUserAuctionsAsync(userId);

                return Ok(auctions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user's auctions");
                return StatusCode(500, "An error occurred while retrieving your auctions");
            }
        }

        [HttpGet("my-bids")]
        public async Task<ActionResult<List<AuctionDto>>> GetMyBids()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var auctions = await _auctionService.GetUserBidsAsync(userId);

                return Ok(auctions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user's bids");
                return StatusCode(500, "An error occurred while retrieving your bids");
            }
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new InvalidOperationException("User ID claim not found");

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new InvalidOperationException("Invalid user ID claim");
            }

            return userId;
        }
    }
}
