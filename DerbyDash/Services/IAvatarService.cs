namespace DerbyDash.Services {
    public interface IAvatarService {
        /// <summary>
        /// Event that fires when a user's avatar is updated
        /// </summary>
        event Action? OnAvatarChanged;

        /// <summary>
        /// Notifies subscribers that the current user's avatar has been updated
        /// </summary>
        void NotifyAvatarChanged();
    }
}
