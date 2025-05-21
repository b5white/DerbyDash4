using DerbyDash.Data;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Components.Account {
    // Note: This is an IN-MEMORY store. Users are lost when the application restarts.
    public class FakeUserStore: IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserEmailStore<ApplicationUser> {
        private readonly ILogger<FakeUserStore> Logger = null!;
        private readonly List<ApplicationUser> _users;

        public FakeUserStore(ILogger<FakeUserStore> logger) {
            Logger = logger;
            Logger?.LogWarning("FakeUserStore constructor");

            // Initialize with some default users
            _users = new List<ApplicationUser>
            {
                new ApplicationUser {
                    Id = Guid.NewGuid().ToString(), // Or use a fixed Guid string if needed elsewhere
                    UserName = "test@gmail.com",
                    NormalizedUserName = "TEST@GMAIL.COM", // Important for lookups
                    Email = "test@gmail.com",
                    NormalizedEmail = "TEST@GMAIL.COM", // Important for lookups
                    EmailConfirmed = true, // Set to true to bypass email confirmation check
                    PasswordHash = "" // Can be empty or placeholder since we bypass the check
                },

                new ApplicationUser {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "test@example.com",
                    NormalizedUserName = "TEST@EXAMPLE.COM",
                    Email = "test@example.com",
                    NormalizedEmail = "TEST@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "" // Example: Add a hashed password if needed for other tests
                                      // For testing, you might use UserManager.PasswordHasher.HashPassword(user, "Password123!")
                                      // But for the bypass, the hash doesn't matter.
                }
            };
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken) {
            Logger?.LogWarning($"FakeUserStore CreateAsync {user.Email}");
            user.Id ??= Guid.NewGuid().ToString(); // Ensure user has an ID
            _users.Add(user);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.Id);

        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.UserName);
        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken) {
            user.UserName = userName;
            return Task.CompletedTask;
        }
        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken) {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
            => Task.FromResult(_users.FirstOrDefault(u => u.NormalizedEmail == normalizedEmail));

        // Other IUserStore methods
        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
            => Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));

        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) {
            // Changed this to search by NormalizedUserName as intended
            return Task.FromResult(_users.FirstOrDefault(u => u.NormalizedUserName == normalizedUserName));
        }

        // Required by IUserPasswordStore
        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
            // Return true even if PasswordHash is empty, as we bypass the check later
            => Task.FromResult(true);
        public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.PasswordHash);

        public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken) {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        // Required by IUserEmailStore
        public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken) {
            user.Email = email;
            return Task.CompletedTask;
        }
        public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.Email);

        public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.NormalizedEmail);
        public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken) {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }
        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.EmailConfirmed);
        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken) {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        // Dispose and other methods
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);
        public void Dispose() {
            // Logger?.LogWarning("FakeUserStore destructor");
        }
    }
}
