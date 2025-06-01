using DerbyDash.Data;
using System.Security.Claims;

namespace DerbyDash.Services {
    public interface IUserService {
        /// <summary>
        /// Gets the current user's ID from the authentication state
        /// </summary>
        /// <param name="purpose">Optional purpose description for logging</param>
        /// <returns>The user ID</returns>
        /// <exception cref="MissingUserException">Thrown when no authenticated user is found</exception>
        Task<string> GetUserIdAsync(string purpose = "");

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        /// <returns>True if the user is logged in, false otherwise</returns>
        Task<bool> IsLoggedInAsync();

        /// <summary>
        /// Gets the current user's ApplicationUser
        /// </summary>
        /// <returns>The current user, or null if not authenticated</returns>
        Task<ApplicationUser?> GetCurrentUserAsync();

        /// <summary>
        /// Gets the current user's ClaimsPrincipal
        /// </summary>
        /// <returns>The current user's claims principal, or null if not authenticated</returns>
        Task<ClaimsPrincipal?> GetCurrentPrincipalUserAsync();

        /// <summary>
        /// Gets the current user's username/email
        /// </summary>
        /// <returns>The username/email of the current user, or null if not authenticated</returns>
        Task<string?> GetUserNameAsync();

        /// <summary>
        /// Gets a specific claim value for the current user
        /// </summary>
        /// <param name="claimType">The type of claim to retrieve</param>
        /// <returns>The claim value, or null if not found</returns>
        Task<string?> GetUserClaimAsync(string claimType);

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="role">The role to check for</param>
        /// <returns>True if the user has the role, false otherwise</returns>
        Task<bool> IsInRoleAsync(string role);
    }
}
