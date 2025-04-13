using AuctionSite.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionSite.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(int id);
        Task<decimal> GetUserBalanceAsync(int userId);
        Task<bool> UpdateUserBalanceAsync(int userId, decimal amount);
        Task<bool> TransferFundsAsync(int fromUserId, int toUserId, decimal amount);
    }
}
