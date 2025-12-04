using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace DerbyDash.Components.Account.Pages.Manage {
    [Authorize]
    public partial class Preferences: ComponentBase {
        [Inject] private IdentityRedirectManager RedirectManager { get; set; } = null!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private ILogger<Preferences> Logger { get; set; } = default!;
        [Inject] private IAvatarService AvatarService { get; set; } = default!;
        [Inject] private IRaceTeamService RaceTeamService { get; set; } = default!;
        [Inject] private IUserService UserService { get; set; } = default!;

        public class PreferencesModel {
            public string? Avatar { get; set; }
        }

        private PreferencesModel Model = new();
        private string[] Avatars = new[] { "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg", "6.jpg", "7.jpg", "8.jpg" };
        private string? SaveMessage;
        private ApplicationUser? user;
        private Racer? activeRacer;
        [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

        protected override async Task OnInitializedAsync() {
            try {
                // Get the current user
                user = await UserService.GetCurrentUserAsync();
                
                // Get the active racer
                activeRacer = await RaceTeamService.GetActiveRacer();

                if (user == null) {
                    Logger.LogError("Failed to load user in OnInitializedAsync.");
                    SaveMessage = "Error: Could not load user data.";
                } else {
                    Logger.LogInformation("User {UserId} loaded successfully in OnInitializedAsync.", user.Id);
                    
                    // Set the Model.Avatar from the active racer's avatar
                    if (activeRacer != null) {
                        Model.Avatar = activeRacer.AvatarFileName;
                        Logger.LogInformation("Active racer avatar loaded: {Avatar}", Model.Avatar ?? "null");
                    } else if (Avatars.Length > 0) {
                        Model.Avatar = Avatars[0]; // Default to first avatar
                        Logger.LogInformation("No active racer found, defaulting to first avatar: {Avatar}", Model.Avatar);
                    }
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error loading preferences");
                SaveMessage = "Error loading preferences. Please try again.";
            }
        }

        private void SelectAvatar(string avatar) {
            Model.Avatar = avatar;
            Logger.LogInformation("Avatar selected: {Avatar}. Model.Avatar is now: {ModelAvatar}", avatar, Model.Avatar);
            StateHasChanged(); // Ensure UI updates
        }

        private async Task OnSubmitAsync() {
            // Get the active racer
            var currentRacer = await RaceTeamService.GetActiveRacer();
            
            if (currentRacer == null) {
                Logger.LogError("No active racer found.");
                SaveMessage = "Error: No active racer found. Please set an active racer first.";
                return;
            }

            Logger.LogInformation("OnSubmitAsync started. Active Racer: {RacerName}, Model.Avatar: {Avatar}", 
                currentRacer.Name, Model.Avatar ?? "null");
            SaveMessage = null; // Clear previous message

            // Check if an avatar is selected
            if (!string.IsNullOrEmpty(Model.Avatar)) {
                Logger.LogInformation("Attempting to save avatar '{Avatar}' for racer '{RacerName}'", Model.Avatar, currentRacer.Name);

                try {
                    // Update the avatar filename for the active racer
                    currentRacer.AvatarFileName = Model.Avatar;
                    
                    // Save changes via API
                    await RaceTeamService.UpdateRacer(currentRacer);
                    
                    Logger.LogInformation("Avatar updated successfully for racer '{RacerName}'", currentRacer.Name);

                    // Notify other components that the avatar has changed
                    await AvatarService.NotifyAvatarChanged();
                    Logger.LogInformation("Avatar change notification sent for racer '{RacerName}'", currentRacer.Name);

                    SaveMessage = "Preferences saved!";
                    StateHasChanged(); // Update UI to show message and reflect Model change

                    // Show success message for a short delay
                    await Task.Delay(1200);

                    // Refresh the page to show the updated avatar
                    NavManager.NavigateTo(NavManager.Uri, forceLoad: true);
                }
                catch (Exception ex) {
                    Logger.LogError(ex, "Error updating avatar for racer '{RacerName}'", currentRacer.Name);
                    SaveMessage = "Error saving preferences: " + ex.Message;
                }
            } else {
                Logger.LogWarning("OnSubmitAsync check failed because Model.Avatar is null or empty for racer {RacerName}.", currentRacer.Name);
                SaveMessage = "Please select an avatar before saving.";
            }
        }
    }
}
