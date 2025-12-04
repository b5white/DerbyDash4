using Microsoft.Extensions.Logging;

namespace DerbyDash.Services {
    public class StubAvatarService : IAvatarService {
        private readonly ILogger<StubAvatarService>? _logger;

        public StubAvatarService(ILogger<StubAvatarService>? logger = null) {
            _logger = logger;
        }

        /// <summary>
        /// Event that fires when a user's avatar is updated
        /// </summary>
        public event Func<Task>? OnAvatarChanged;

        /// <summary>
        /// Notifies subscribers that the current user's avatar has been updated
        /// </summary>
        public async Task NotifyAvatarChanged() {
            _logger?.LogInformation("Avatar changed, notifying subscribers");
            await InvokeOnAvatarChanged();
        }

        private async Task InvokeOnAvatarChanged() {
            if (OnAvatarChanged != null) {
                var handlers = OnAvatarChanged.GetInvocationList().Cast<Func<Task>>();
                foreach (var handler in handlers) {
                    await handler(); // Await each handler
                }
            }
        }

        public Task<List<string>> GetAvailableAvatarsAsync() {
            // Return default avatars (using .jpg extension to match RaceTeam page)
            return Task.FromResult(new List<string> {
                "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg",
                "6.jpg", "7.jpg", "8.jpg", "9.jpg", "10.jpg"
            });
        }

        public Task<string?> GetAvatarUrlAsync(string? avatarFileName) {
            if (string.IsNullOrEmpty(avatarFileName)) {
                return Task.FromResult<string?>(null);
            }
            // Return URL to avatar image
            return Task.FromResult<string?>($"/images/avatars/{avatarFileName}");
        }
    }
}

