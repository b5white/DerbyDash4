using DerbyDash.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DerbyDash.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        // In-memory storage for demo purposes
        private static Dictionary<string, Subscription> _subscriptions = new Dictionary<string, Subscription>();

        public SubscriptionService()
        {
        }

        public Task<Subscription> GetCurrentSubscriptionAsync(string userId)
        {
            // Check if we already have a subscription for this user
            if (_subscriptions.ContainsKey(userId))
            {
                return Task.FromResult(_subscriptions[userId]);
            }

            // Create a sample subscription for demo purposes
            var subscription = new Subscription
            {
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
            };

            // Store it for future use
            _subscriptions[userId] = subscription;

            return Task.FromResult(subscription);
        }

        public async Task<bool> ToggleAutoRenewalAsync(string userId)
        {
            if (!_subscriptions.ContainsKey(userId))
            {
                await GetCurrentSubscriptionAsync(userId);
            }

            var subscription = _subscriptions[userId];
            subscription.AutoRenewal = !subscription.AutoRenewal;
            
            return subscription.AutoRenewal;
        }

        public async Task<bool> PauseSubscriptionAsync(string userId)
        {
            if (!_subscriptions.ContainsKey(userId))
            {
                await GetCurrentSubscriptionAsync(userId);
            }

            var subscription = _subscriptions[userId];
            
            if (subscription.IsPaused)
                return false;
                
            subscription.IsPaused = true;
            subscription.PausedDate = DateTime.Now;
            
            // We don't need to modify the end date when pausing
            // Just store the pause date and we'll calculate the extension when resuming
            
            return true;
        }

        public async Task<bool> ResumeSubscriptionAsync(string userId)
        {
            if (!_subscriptions.ContainsKey(userId))
            {
                await GetCurrentSubscriptionAsync(userId);
            }

            var subscription = _subscriptions[userId];
            
            if (!subscription.IsPaused)
                return false;
                
            // When resumed, we extend the end date based on the pause duration
            if (subscription.PausedDate.HasValue)
            {
                var pauseDuration = (DateTime.Now - subscription.PausedDate.Value).Days;
                if (pauseDuration > 0)
                {
                    subscription.EndDate = subscription.EndDate.AddDays(pauseDuration);
                }
            }
            
            subscription.IsPaused = false;
            subscription.PausedDate = null;
            
            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(string userId)
        {
            if (!_subscriptions.ContainsKey(userId))
            {
                await GetCurrentSubscriptionAsync(userId);
            }

            var subscription = _subscriptions[userId];
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.AutoRenewal = false;
            
            return true;
        }

        public async Task<bool> RenewSubscriptionAsync(string userId, SubscriptionType type)
        {
            if (!_subscriptions.ContainsKey(userId))
            {
                await GetCurrentSubscriptionAsync(userId);
            }

            var subscription = _subscriptions[userId];
            
            // Set new subscription details
            subscription.Type = type;
            subscription.Status = SubscriptionStatus.Active;
            subscription.StartDate = DateTime.Now;
            
            // Set end date based on subscription type
            switch (type)
            {
                case SubscriptionType.Monthly:
                    subscription.EndDate = DateTime.Now.AddMonths(1);
                    break;
                case SubscriptionType.Annual:
                    subscription.EndDate = DateTime.Now.AddYears(1);
                    break;
                case SubscriptionType.Lifetime:
                    subscription.EndDate = DateTime.Now.AddYears(99); // Effectively lifetime
                    break;
                default: // Trial
                    subscription.EndDate = DateTime.Now.AddDays(30);
                    break;
            }
            
            return true;
        }

        public async Task<bool> UseReferralCreditAsync(string userId)
        {
            if (!_subscriptions.ContainsKey(userId))
            {
                await GetCurrentSubscriptionAsync(userId);
            }

            var subscription = _subscriptions[userId];
            
            if (subscription.ReferralCredits <= 0)
                return false;
                
            subscription.ReferralCredits--;
            
            // Add 30 days to the subscription for each credit used
            subscription.EndDate = subscription.EndDate.AddDays(30);
            
            return true;
        }
    }
}