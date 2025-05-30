using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class Subscription {

        [Inject]
        UserService UserService { get; set; } = default!;

        [Inject]
        ILogger<Subscription> Logger { get; set; } = default!;

        [Inject]
        ISubscriptionService SubscriptionService { get; set; } = default!;

        private Data.Subscription userSubscription = new Data.Subscription() { UserId = "none" };
        private bool isLoading = true;
        private bool showRenewOptions = false;
        private SubscriptionType selectedRenewalType = SubscriptionType.Monthly;
        private string userId = "";
        private string? errorMessage;

        protected override async Task OnInitializedAsync() {
            try {
                userId = await UserService.GetUserIdAsync("Subscription Management");
                userSubscription = await SubscriptionService.GetCurrentSubscriptionAsync(userId);
                if (userSubscription == null) {
                    errorMessage = "There was an error loading your subscription. Please try again later.";
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error loading subscription for user {UserId}", userId);
                errorMessage = "There was an error loading your subscription. Please try again later.";
            } finally {
                isLoading = false;
            }
        }
        private async Task ToggleAutoRenewal() {
            try {
                var result = await SubscriptionService.ToggleAutoRenewalAsync(userId);
                if (result) {
                    userSubscription.AutoRenewal = !userSubscription.AutoRenewal;
                    errorMessage = null;
                } else {
                    errorMessage = "There was an error updating your subscription. Please try again.";
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error toggling auto-renewal for user {UserId}", userId);
                errorMessage = "There was an error updating your subscription. Please try again.";
            }
        }
        private async Task UseReferralCredit() {
            try {
                if (userSubscription.ReferralCredits > 0) {
                    var result = await SubscriptionService.UseReferralCreditAsync(userId);
                    if (result) {
                        userSubscription.ReferralCredits--;
                        errorMessage = null;
                    } else {
                        errorMessage = "There was an error using your referral credit. Please try again.";
                    }
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error using referral credit for user {UserId}", userId);
                errorMessage = "There was an error using your referral credit. Please try again.";
            }
        }

        private void ToggleRenewOptions() {
            showRenewOptions = !showRenewOptions;
        }

        private void SelectRenewalType(SubscriptionType type) {
            selectedRenewalType = type;
        }
        private async Task RenewSubscription() {
            try {
                // Implementation for renewal logic
                var result = await SubscriptionService.RenewSubscriptionAsync(userId, selectedRenewalType);
                if (result) {
                    // Refresh subscription data
                    userSubscription = await SubscriptionService.GetCurrentSubscriptionAsync(userId);
                    showRenewOptions = false;
                    errorMessage = null;
                } else {
                    errorMessage = "There was an error renewing your subscription. Please try again.";
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error renewing subscription for user {UserId}", userId);
                errorMessage = "There was an error renewing your subscription. Please try again.";
            }
        }
        private async Task TogglePauseSubscription() {
            try {
                bool result;
                if (userSubscription.IsPaused) {
                    result = await SubscriptionService.ResumeSubscriptionAsync(userId);
                    if (result) {
                        userSubscription.IsPaused = false;
                    }
                } else {
                    result = await SubscriptionService.PauseSubscriptionAsync(userId);
                    if (result) {
                        userSubscription.IsPaused = true;
                    }
                }

                if (result) {
                    errorMessage = null;
                } else {
                    errorMessage = "There was an error updating your subscription. Please try again.";
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error toggling pause for user {UserId}", userId);
                errorMessage = "There was an error updating your subscription. Please try again.";
            }
        }
        private async Task CancelSubscription() {
            try {
                var result = await SubscriptionService.CancelSubscriptionAsync(userId);
                if (result) {
                    userSubscription.Status = SubscriptionStatus.Cancelled;
                    userSubscription.AutoRenewal = false;
                    errorMessage = null;
                } else {
                    errorMessage = "There was an error cancelling your subscription. Please try again.";
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error cancelling subscription for user {UserId}", userId);
                errorMessage = "There was an error cancelling your subscription. Please try again.";
            }
        }
    }
}