using AuctionSite.Core.DTOs.Auth;
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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "An error occurred while retrieving your profile");
            }
        }

        [HttpGet("balance")]
        public async Task<ActionResult<decimal>> GetBalance()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var balance = await _userService.GetUserBalanceAsync(userId);

                return Ok(new { Balance = balance });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user balance");
                return StatusCode(500, "An error occurred while retrieving your balance");
            }
        }

        [HttpPost("updateBalance")]
        public async Task<ActionResult> UpdateBalance(decimal amount)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                if (amount == 0)
                {
                    return BadRequest("Amount cannot be zero");
                }

                var success = await _userService.UpdateUserBalanceAsync(userId, amount);

                if (!success)
                {
                    return BadRequest("Failed to update balance. User may not exist or operation would result in negative balance.");
                }

                var currentBalance = await _userService.GetUserBalanceAsync(userId);

                return Ok(new { Balance = currentBalance });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user balance");
                return StatusCode(500, "An error occurred while updating your balance");
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
