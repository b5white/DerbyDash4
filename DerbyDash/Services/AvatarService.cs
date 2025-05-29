namespace DerbyDash.Services {
    public class AvatarService: IAvatarService {
        private readonly ILogger<AvatarService> _logger;

        public AvatarService(ILogger<AvatarService> logger) {
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
            _logger.LogInformation("Avatar changed, notifying subscribers");
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
    }
}
