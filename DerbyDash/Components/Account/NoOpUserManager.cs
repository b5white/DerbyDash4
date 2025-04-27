using DerbyDash.Data;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Components.Account {
    public class NoOpUserManager: UserManager<ApplicationUser> {
        public NoOpUserManager()
            : base(
                new FakeUserStore(),
                null, null, null, null, null, null, null, null) { }

        public override Task<ApplicationUser> FindByNameAsync(string userName) {
            // Return a fake user or null based on your test logic
            if (userName != null && userName.StartsWith("test", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(new ApplicationUser { UserName = userName });
            return Task.FromResult<ApplicationUser>(null);
        }

        public override Task<IdentityResult> CreateAsync(ApplicationUser user, string password) {
            // Always succeed, or add logic as needed
            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<bool> CheckPasswordAsync(ApplicationUser user, string password) {
            // Always succeed, or add logic as needed
            return Task.FromResult(true);
        }
    }

    // Minimal fake store implementation
    public class FakeUserStore: IUserStore<ApplicationUser> {
        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => Task.FromResult(IdentityResult.Success);
        public Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<ApplicationUser>(null);
        public Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
            => Task.FromResult<ApplicationUser>(null);
        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => Task.FromResult(user.UserName.ToUpper());
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => Task.FromResult("1");
        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => Task.FromResult(user.UserName);
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
            => Task.FromResult(IdentityResult.Success);
        public void Dispose() { }
    }
}
