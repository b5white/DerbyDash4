using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Components.Account.Pages.Manage {
    [Authorize]
    public partial class Preferences: ComponentBase {
        [Inject] public required UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public required SignInManager<ApplicationUser> SignInManager { get; set; }
        [Inject] internal IdentityRedirectManager RedirectManager { get; set; } = null!;
        [Inject] public required NavigationManager NavManager { get; set; }
        [Inject] public required ILogger<Preferences> Logger { get; set; }
        [Inject] public required IAvatarService AvatarService { get; set; }

        public class PreferencesModel {
            public string? Avatar { get; set; }
        }

        private PreferencesModel Model = new();
        private string[] Avatars = new[] { "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg", "6.jpg", "7.jpg", "8.jpg" };
        private string? SaveMessage;
        private ApplicationUser? user;
        [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

        protected override async Task OnInitializedAsync() {
            // TODO Move from user to racer
            // Force a fresh user fetch from the database
            //var authState = await AuthStateTask;
            //var userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            //if (!string.IsNullOrEmpty(userId)) {
            //    // Use FindByIdAsync to ensure we get a fresh copy from the database
            //    user = await UserManager.FindByIdAsync(userId);
            //}

            //if (user == null) {
            //    Logger.LogError("Failed to load user in OnInitializedAsync.");
            //    SaveMessage = "Error: Could not load user data.";
            //} else {
            //    Logger.LogInformation("User {UserId} loaded successfully in OnInitializedAsync.", user.Id);
            //   Logger.LogInformation("User avatar from database: {Avatar}", user.AvatarFileName ?? "null");

            // Set the Model.Avatar from the user's current avatar
            //if (string.IsNullOrEmpty(user.AvatarFileName) && Avatars.Length > 0) {
            //    Model.Avatar = Avatars[0]; // Default to first avatar
            //    Logger.LogInformation("No avatar found for user, defaulting to first avatar: {Avatar}", Model.Avatar);
            //} else {
            //    Model.Avatar = user.AvatarFileName;
            //    Logger.LogInformation("Initial Model.Avatar set to: {Avatar}", Model.Avatar ?? "null");
            //}
        }


        private void SelectAvatar(string avatar) {
            Model.Avatar = avatar;
            Logger.LogInformation("Avatar selected: {Avatar}. Model.Avatar is now: {ModelAvatar}", avatar, Model.Avatar);
            StateHasChanged(); // Ensure UI updates
        }

        private async Task OnSubmitAsync() {
            // TODO racer has options, not the user.
            // Get the user ID from claims to ensure we're working with the correct user
            //var authState = await AuthStateTask;
            //var userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            //if (string.IsNullOrEmpty(userId)) {
            //    Logger.LogError("Failed to get user ID from claims.");
            //    SaveMessage = "Error: Could not identify user.";
            //    return;
            //}

            //// Use FindByIdAsync to ensure we get a fresh copy from the database
            //var currentUser = await UserManager.FindByIdAsync(userId);

            //Logger.LogInformation("OnSubmitAsync started. Fetched User ID: {UserId}, Model.Avatar: {Avatar}", currentUser?.Id ?? "null", Model.Avatar ?? "null");
            //SaveMessage = null; // Clear previous message

            //// Debug: Log the current state of the Model.Avatar
            //Logger.LogInformation("Model.Avatar at submission time: {Avatar}", Model.Avatar ?? "null");

            //// Check if an avatar is selected - use string.IsNullOrEmpty to properly check for null or empty strings
            //if (currentUser != null && !string.IsNullOrEmpty(Model.Avatar)) {
            //    Logger.LogInformation("Attempting to save avatar '{Avatar}' for user '{UserId}'", Model.Avatar, currentUser.Id);

            //    // Update the avatar filename
            //    //currentUser.AvatarFileName = Model.Avatar;

            //    // Save changes to the database
            //    var result = await UserManager.UpdateAsync(currentUser);

            //    if (result.Succeeded) {
            //        Logger.LogInformation("UserManager.UpdateAsync succeeded for user '{UserId}'", currentUser.Id);

            //        // Refresh the sign-in session to update the authentication cookie/principal
            //        // REMOVED: This causes 'Headers already sent' error in interactive server mode
            //        // await SignInManager.RefreshSignInAsync(currentUser);
            //        // Logger.LogInformation("SignInManager.RefreshSignInAsync called for user '{UserId}'", currentUser.Id);                    

            //        // *** Explicitly update the Model to reflect the saved state ***
            //        //Model.Avatar = currentUser.AvatarFileName;
            //        Logger.LogInformation("Model.Avatar explicitly updated after save to: {Avatar}", Model.Avatar ?? "null");

            //        // Update the local user reference (optional, as navigation will likely reload)
            //        user = currentUser;

            //        // Force a refresh of the user from the database to verify changes (optional debug step)
            //        var refreshedUser = await UserManager.FindByIdAsync(userId);
            //        //Logger.LogInformation("After save and refresh, refreshed user avatar is: {Avatar}", refreshedUser?.AvatarFileName ?? "null");

            //        // Notify other components that the avatar has changed
            //        await AvatarService.NotifyAvatarChanged();
            //        Logger.LogInformation("Avatar change notification sent for user '{UserId}'", currentUser.Id);

            //        SaveMessage = "Preferences saved!";
            //        StateHasChanged(); // Update UI to show message and reflect Model change

            //        // Show success message for a short delay
            //        await Task.Delay(1200);

            //        // Instead of using RedirectManager, use NavigationManager for interactive components
            //        NavManager.NavigateTo(NavManager.Uri, forceLoad: true);
            //    } else {
            //        Logger.LogError("UserManager.UpdateAsync failed for user '{UserId}'. Errors: {Errors}",
            //            currentUser.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            //        SaveMessage = "Error saving preferences: " + string.Join(", ", result.Errors.Select(e => e.Description));
            //    }
            //} else {
            //    // This block is executed if currentUser or Model.Avatar is null/empty
            //    if (currentUser == null) {
            //        Logger.LogError("OnSubmitAsync check failed because user could not be found.");
            //        SaveMessage = "Error: Could not save preferences. User data could not be loaded.";
            //    } else // User is not null, so Model.Avatar must be null or empty
            //      {
            //        Logger.LogWarning("OnSubmitAsync check failed because Model.Avatar is null or empty for user {UserId}.", currentUser.Id);
            //        SaveMessage = "Please select an avatar before saving.";
            //    }
            //}
        }
    }
}
