using DerbyDash.Data;

namespace DerbyDash.Services {
    public class StubSubscriptionService : ISubscriptionService {
        public Task<Subscription> GetCurrentSubscriptionAsync(string userId) {
            // TODO: Implement API call to get subscription
            return Task.FromResult(new Subscription {
                Id = 1,
                UserId = userId,
                Type = SubscriptionType.Trial,
                StartDate = DateTime.Now.AddDays(-25),
                EndDate = DateTime.Now.AddDays(5),
                AutoRenewal = true,
                ReferralCredits = 5,
                IsPaused = false,
                Status = SubscriptionStatus.Active,
                SpecialOffersProgress = 30
            });
        }

        public Task<bool> ToggleAutoRenewalAsync(string userId) {
            // TODO: Implement API call
            return Task.FromResult(false);
        }

        public Task<bool> PauseSubscriptionAsync(string userId) {
            // TODO: Implement API call
            return Task.FromResult(false);
        }

        public Task<bool> ResumeSubscriptionAsync(string userId) {
            // TODO: Implement API call
            return Task.FromResult(false);
        }

        public Task<bool> CancelSubscriptionAsync(string userId) {
            // TODO: Implement API call
            return Task.FromResult(false);
        }

        public Task<bool> RenewSubscriptionAsync(string userId, SubscriptionType type) {
            // TODO: Implement API call
            return Task.FromResult(false);
        }

        public Task<bool> UseReferralCreditAsync(string userId) {
            // TODO: Implement API call
            return Task.FromResult(false);
        }

        public Task<int> GetRacerLimitAsync(string userId) {
            // TODO: Implement API call
            // Default to 8 racers for now
            return Task.FromResult(8);
        }

        public Task<bool> SetRacerLimitAsync(string userId, int limit, bool isAdminOverride = false) {
            // TODO: Implement API call
            return Task.FromResult(false);
        }

        public Task<bool> IsSubscribedAsync(string userId) {
            // TODO: Implement API call to check subscription
            return Task.FromResult(false);
        }

        public Task SubscribeAsync(string userId) {
            // TODO: Implement API call to subscribe
            return Task.CompletedTask;
        }

        public Task UnsubscribeAsync(string userId) {
            // TODO: Implement API call to unsubscribe
            return Task.CompletedTask;
        }
    }
}

