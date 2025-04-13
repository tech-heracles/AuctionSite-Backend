using AuctionSite.Core.DTOs.Auth;
using AuctionSite.Core.Interfaces.Services;
using AuctionSite.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionSite.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ApplicationDbContext context,
            ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                _logger.LogWarning($"User with id {id} not found");
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                WalletBalance = user.WalletBalance
            };
        }

        public async Task<decimal> GetUserBalanceAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning($"User with id {userId} not found when getting balance");
                throw new ArgumentException("User not found");
            }

            return user.WalletBalance;
        }

        public async Task<bool> UpdateUserBalanceAsync(int userId, decimal amount)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning($"User with id {userId} not found when updating balance");
                    return false;
                }

                user.WalletBalance += amount;

                // Ensure balance doesn't go negative
                if (user.WalletBalance < 0)
                {
                    _logger.LogWarning($"Attempted to set negative balance for user {userId}");
                    return false;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Updated user {userId} balance by {amount}, new balance: {user.WalletBalance}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating balance for user {userId}");
                return false;
            }
        }

        public async Task<bool> TransferFundsAsync(int fromUserId, int toUserId, decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogWarning($"Attempted to transfer invalid amount {amount}");
                throw new ArgumentException("Transfer amount must be positive");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // First, deduct from sender
                var fromUser = await _context.Users.FindAsync(fromUserId);
                if (fromUser == null)
                {
                    _logger.LogWarning($"User with id {fromUserId} not found when transferring funds");
                    return false;
                }

                if (fromUser.WalletBalance < amount)
                {
                    _logger.LogWarning($"User {fromUserId} has insufficient funds for transfer: {fromUser.WalletBalance} < {amount}");
                    return false;
                }

                fromUser.WalletBalance -= amount;

                // Then, add to receiver
                var toUser = await _context.Users.FindAsync(toUserId);
                if (toUser == null)
                {
                    _logger.LogWarning($"User with id {toUserId} not found when transferring funds");
                    return false;
                }

                toUser.WalletBalance += amount;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Transferred {amount} from user {fromUserId} to user {toUserId}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error transferring funds from user {fromUserId} to user {toUserId}");
                return false;
            }
        }
    }
}
