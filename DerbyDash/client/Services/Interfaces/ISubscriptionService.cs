using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface ISubscriptionService {
        Task<Subscription> GetCurrentSubscriptionAsync(string userId);
        Task<bool> ToggleAutoRenewalAsync(string userId);
        Task<bool> PauseSubscriptionAsync(string userId);
        Task<bool> ResumeSubscriptionAsync(string userId);
        Task<bool> CancelSubscriptionAsync(string userId);
        Task<bool> RenewSubscriptionAsync(string userId, SubscriptionType type);
        Task<bool> UseReferralCreditAsync(string userId);
        Task<int> GetRacerLimitAsync(string userId);
        Task<bool> SetRacerLimitAsync(string userId, int limit, bool isAdminOverride = false);
        Task<bool> IsSubscribedAsync(string userId);
        Task SubscribeAsync(string userId);
        Task UnsubscribeAsync(string userId);
    }
}

