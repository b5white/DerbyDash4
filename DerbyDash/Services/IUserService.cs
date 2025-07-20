using System.Security.Claims;

namespace DerbyDash.Services {
    public interface IUserService {
        /// <summary>
        /// Gets the current user's ID from the authentication state
        /// </summary>
        /// <param name="purpose">Optional purpose description for logging</param>
        /// <param name="throwIfMissing">If true, throws MissingUserException when user is not found; if false, returns null</param>
        /// <returns>The user ID, or null if user is not authenticated and throwIfMissing is false</returns>
        /// <exception cref="MissingUserException">Thrown when no authenticated user is found and throwIfMissing is true</exception>
        Task<string?> GetUserIdAsync(string purpose = "", bool throwIfMissing = true);

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        /// <returns>True if the user is logged in, false otherwise</returns>
        Task<bool> IsLoggedInAsync();

        /// <summary>
        /// Gets the current user's ClaimsPrincipal
        /// </summary>
        /// <returns>The current user's claims principal, or null if not authenticated</returns>
        Task<ClaimsPrincipal?> GetCurrentUserAsync();

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

        /// <summary>
        /// Gets the total count of registered users
        /// </summary>
        /// <returns>The total number of users in the system</returns>
        Task<int> GetTotalUserCountAsync();
    }
}
