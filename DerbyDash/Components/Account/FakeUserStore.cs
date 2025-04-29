using DerbyDash.Components.Pages;
using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Components.Account {
    public class FakeUserStore:
        IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>,
        IDisposable {

        [Inject]
        public required ILogger<Race> _logger { get; set; }

        private List<ApplicationUser> _users;

        public FakeUserStore() {
            if (_users == null) {
                _users = new List<ApplicationUser> {
                    new ApplicationUser {
                        Id = Guid.NewGuid().ToString(),
                        UserName = "test",
                        NormalizedUserName = "TEST",
                        Email = "test@gmail.com",
                        NormalizedEmail = "TEST@GMAIL.COM",
                        EmailConfirmed = true,
                        PasswordHash = "AQAAAAIAAYagAAAAECazbytJhsyR0U7FJHmN/9VBKGoLrqqfnSEm9x5tdD7QA5f4mLkLX5pFKYzLE5nc8w=="
                    }
                };
            }
        }

        // Required by IUserStore
        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken) {
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

        public Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
            => Task.FromResult(_users.FirstOrDefault(u => u.NormalizedEmail == normalizedEmail));

        // Other IUserStore methods 
        public Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
            => Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));

        public Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) {
            return Task.FromResult(_users.FirstOrDefault(u => u.NormalizedEmail == normalizedUserName));
        }

        // Required by IUserPasswordStore
        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(true);
        public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.PasswordHash);

        public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken) {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        // Required by IUserEmailStore
        public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken) {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.Email);

        public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.NormalizedEmail);
        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken) {
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
            _logger.LogWarning("FakeUserStore destructor");
        }
    }
}
