using DerbyDash.Data;
using System.Security.Claims;

namespace DerbyDash.Services {
    // Client-side interface matching server interface
    public interface IUserService {
        Task<string?> GetUserIdAsync(string purpose = "", bool throwIfMissing = true);
        Task<bool> IsLoggedInAsync();
        Task<ApplicationUser?> GetCurrentUserAsync();
        Task<ClaimsPrincipal?> GetCurrentPrincipalUserAsync();
        Task<string?> GetUserNameAsync();
        Task<string?> GetUserClaimAsync(string claimType);
        Task<bool> IsInRoleAsync(string role);
        Task<int> GetTotalUserCountAsync();
    }
}

