using DerbyDash.Data;
using System.Threading.Tasks;

namespace DerbyDash.Services
{
    public interface ISubscriptionService
    {
        Task<Subscription> GetCurrentSubscriptionAsync(string userId);
        Task<bool> ToggleAutoRenewalAsync(string userId);
        Task<bool> PauseSubscriptionAsync(string userId);
        Task<bool> ResumeSubscriptionAsync(string userId);
        Task<bool> CancelSubscriptionAsync(string userId);
        Task<bool> RenewSubscriptionAsync(string userId, SubscriptionType type);
        Task<bool> UseReferralCreditAsync(string userId);
    }
}