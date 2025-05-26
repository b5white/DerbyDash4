namespace DerbyDash.Services {
    public class AvatarService : IAvatarService {
        private readonly ILogger<AvatarService> _logger;

        public AvatarService(ILogger<AvatarService> logger) {
            _logger = logger;
        }

        /// <summary>
        /// Event that fires when a user's avatar is updated
        /// </summary>
        public event Action? OnAvatarChanged;

        /// <summary>
        /// Notifies subscribers that the current user's avatar has been updated
        /// </summary>
        public void NotifyAvatarChanged() {
            _logger.LogInformation("Avatar changed, notifying subscribers");
            OnAvatarChanged?.Invoke();
        }
    }
}
