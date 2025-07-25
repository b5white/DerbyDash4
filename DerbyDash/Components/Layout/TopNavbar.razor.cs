using DerbyDash.Components.Account;
using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Components.Layout {
    public partial class TopNavbar: ComponentBase, IDisposable {
        private List<Racer> Racers { get; set; } = new List<Racer>();
        private Racer SelectedRacer { get; set; } = new Racer { Id = 0, Name = "" };

        [Inject] public required NavigationManager NavManager { get; set; }
        [Inject] internal IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] public required IRaceTeamService RaceTeamService { get; set; }
        [Inject] public required ILogger<TopNavbar> Logger { get; set; }
        [Inject] public required IAvatarService AvatarService { get; set; }
        [Inject] public required UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public required GameStateService GameStateService { get; set; }

        [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

        private string? UserAvatarFileName;
        private string CurrentUrl => NavManager.Uri;
        private int CurrentRacerRaceCount { get; set; } = 0;
        private int TeamRaceCount { get; set; } = 0;
        private string? UserInitial;
        private string? UserEmail; // Add property for email

        // Properties for racer avatar display
        private string? RacerAvatarFileName => UserAvatarFileName;
        private string? RacerInitial => UserInitial;

        private bool isGameRunning = false;

        protected override async Task OnInitializedAsync() {
            try {
                // Subscribe to racer changes
                RaceTeamService.OnRacerChanged += HandleRacerChangedAsync;

                // Subscribe to avatar changes
                AvatarService.OnAvatarChanged += HandleAvatarChangedAsync;

                // Subscribe to game state changes
                GameStateService.OnGameStateChanged += HandleGameStateChangedAsync;

                // Listen for game running state
                isGameRunning = GameStateService.IsGameRunning;

                // Load racers immediately on initialization
                await LoadRacers();
                await LoadUserAvatar();
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in TopNavbar OnInitializedAsync");
            }
        }

        protected override void OnAfterRender(bool firstRender) {
            if (firstRender) {
                // Trigger state change to ensure UI updates after data is loaded
                StateHasChanged();
            }
        }

        private async Task LoadRacers() {
            try {
                Logger.LogInformation("TopNavbar: Loading racers...");
                // Get racers from the service
                Racers = await RaceTeamService.GetRacers();
                Logger.LogInformation($"TopNavbar: Loaded {Racers.Count} racers");
                
                if (Racers.Count > 0) {
                    // Set the selected racer to the current racer
                    Racer? currentRacer = await RaceTeamService.GetRacerWithRaceCountAsync();

                    if (currentRacer != null) {
                        SelectedRacer = currentRacer;
                        Logger.LogInformation($"TopNavbar: Set active racer to {currentRacer.Name}");
                        // Load race counts only when we have an active racer
                        CurrentRacerRaceCount = SelectedRacer.RaceCount;
                        TeamRaceCount = await RaceTeamService.GetTeamRaceCountAsync();
                    } else {
                        // No active racer selected - don't default to first racer
                        Logger.LogWarning("TopNavbar: No active racer found");
                        SelectedRacer = new Racer { Id = 0, Name = "" };
                        CurrentRacerRaceCount = 0;
                        TeamRaceCount = await RaceTeamService.GetTeamRaceCountAsync();
                    }
                } else {
                    // Initialize with an empty racer to avoid null reference exceptions
                    Logger.LogWarning("TopNavbar: No racers found, initializing with empty racer");
                    SelectedRacer = new Racer { Id = 0, Name = "" };
                    CurrentRacerRaceCount = 0;
                    TeamRaceCount = 0;
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error loading racers");
                // User isn't logged in or other error occurred. Initialize with empty lists.
                Racers = new List<Racer>();
                SelectedRacer = new Racer { Id = 0, Name = "" };
                CurrentRacerRaceCount = 0;
                TeamRaceCount = 0;
            }
        }

        private async Task HandleRacerChangedAsync() {
            try {
                // Load racers and avatar when active racer changes
                await LoadRacers();
                await LoadUserAvatar(); // Add this line to update the avatar
                StateHasChanged();
            } catch (InvalidOperationException ex) when (ex.Message.Contains("second operation was started")) {
                // DbContext concurrency issue - this is temporary, retry after a short delay
                Logger.LogWarning(ex, "Temporary DbContext concurrency issue in HandleRacerChangedAsync - will retry");
                
                // Retry after a short delay
                _ = Task.Run(async () => {
                    await Task.Delay(100); // Short delay
                    try {
                        await InvokeAsync(async () => {
                            await LoadRacers();
                            await LoadUserAvatar(); // Add this line to update the avatar
                            StateHasChanged();
                        });
                    } catch (Exception retryEx) {
                        Logger.LogWarning(retryEx, "Retry failed in HandleRacerChangedAsync");
                    }
                });
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in HandleRacerChangedAsync");
            }
        }

        private async Task HandleAvatarChangedAsync() {
            try {
                await LoadUserAvatar();
                StateHasChanged();
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in HandleAvatarChangedAsync");
            }
        }

        private async Task OnRacerChangedAsync(ChangeEventArgs e) {
            string newRacerIdStr = e.Value?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(newRacerIdStr) && int.TryParse(newRacerIdStr, out int newRacerId)) {
                Racer? newRacer = await RaceTeamService.GetRacerByIdAsync(newRacerId);
                if (newRacer != null) {
                    SelectedRacer = newRacer;
                    await RaceTeamService.SetActiveRacer(newRacer);

                    // Update the current racer race count
                    CurrentRacerRaceCount = await RaceTeamService.GetCurrentRacerRaceCountAsync();

                    StateHasChanged();
                }
            }
        }

        private string GetPageTitle() {
            if (CurrentUrl.Contains("/RaceSetsMenu"))
                return "Race Sets";
            else if (CurrentUrl.Contains("/Account/Manage/RaceTeam"))
                return "Race Team Management";
            else if (CurrentUrl.Contains("/Account/Manage"))
                return "Account Settings";
            else if (CurrentUrl.Contains("/Account/Register"))
                return "Join TurboFlash";
            else if (CurrentUrl.Contains("/Account/Login"))
                return "Welcome Back";
            else if (CurrentUrl.Contains("/Race/"))
                return "Race Time!";
            else
                return "TurboFlash";
        }

        private string GetPageIcon() {
            if (CurrentUrl.Contains("/RaceSetsMenu"))
                return "fas fa-flag-checkered";
            else if (CurrentUrl.Contains("/Account/Manage/RaceTeam"))
                return "fas fa-users";
            else if (CurrentUrl.Contains("/Account/Manage"))
                return "fas fa-user-cog";
            else if (CurrentUrl.Contains("/Account/Register"))
                return "fas fa-user-plus";
            else if (CurrentUrl.Contains("/Account/Login"))
                return "fas fa-sign-in-alt";
            else if (CurrentUrl.Contains("/Race/"))
                return "fas fa-tachometer-alt";
            else
                return "fas fa-home";
        }

        private string GetPageMotivation() {
            // Random motivational messages for different pages
            if (CurrentUrl.Contains("/RaceSetsMenu"))
                return "Choose your challenge and start your engines!";
            else if (CurrentUrl.Contains("/Account/Manage/RaceTeam"))
                return "Build your dream team of math champions!";
            else if (CurrentUrl.Contains("/Account/Manage"))
                return "Customize your racing experience!";
            else if (CurrentUrl.Contains("/Account/Register"))
                return "Create an account and start your math racing journey!";
            else if (CurrentUrl.Contains("/Account/Login"))
                return "Ready to race? Log in and hit the track!";
            else if (CurrentUrl.Contains("/Race/")) {
                // Different motivational messages for race page
                string[] raceMotivations = new[]
                {
                "Solve fast, race faster!",
                "The checkered flag awaits the quickest mind!",
                "Math skills in the fast lane!",
                "Calculate your way to victory!",
                "Speed and accuracy win the race!"
            };

                // Use a deterministic but seemingly random selection based on the day
                int dayOfYear = DateTime.Now.DayOfYear;
                return raceMotivations[dayOfYear % raceMotivations.Length];
            } else
                return "Race through math challenges and become a champion!";
        }

        private async Task LoadUserAvatar() {
            try {
                // Get the active racer
                var activeRacer = await RaceTeamService.GetActiveRacer();
                
                if (activeRacer != null) {
                    // Use the active racer's avatar
                    UserAvatarFileName = activeRacer.AvatarFileName;
                    UserInitial = !string.IsNullOrEmpty(activeRacer.Name) ? activeRacer.Name.Substring(0, 1).ToUpper() : null;
                    
                    // Get user email if needed
                    var authState = await AuthStateTask;
                    var userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId)) {
                        var user = await UserManager.FindByIdAsync(userId);
                        UserEmail = user?.Email;
                    }
                } else {
                    // No active racer, try to get user info
                    var authState = await AuthStateTask;
                    var userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    
                    if (!string.IsNullOrEmpty(userId)) {
                        var user = await UserManager.FindByIdAsync(userId);
                        if (user != null) {
                            UserAvatarFileName = null; // No avatar
                            UserInitial = !string.IsNullOrEmpty(user.UserName) ? user.UserName.Substring(0, 1).ToUpper() : null;
                            UserEmail = user.Email;
                        } else {
                            // Clear fields if user not found
                            UserAvatarFileName = null;
                            UserInitial = null;
                            UserEmail = null;
                        }
                    } else {
                        // Clear fields if not authenticated
                        UserAvatarFileName = null;
                        UserInitial = null;
                        UserEmail = null;
                    }
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error loading user avatar");
                // Clear fields on error
                UserAvatarFileName = null;
                UserInitial = null;
                UserEmail = null;
            }
        }

        private async Task HandleGameStateChangedAsync(bool running) {
            try {
                isGameRunning = running;
                await InvokeAsync(StateHasChanged);
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in HandleGameStateChangedAsync");
            }
        }

        public bool ShouldShowNavbar => !isGameRunning;

        private string GetNavbarClasses() {
            var classes = new List<string>();

            // Check if we should hide the navbar on mobile when game is running
            if (GameStateService.ShouldHideNavbarOnMobile()) {
                classes.Add("game-running");
            }

            return string.Join(" ", classes);
        }

        void IDisposable.Dispose() {
            // Unsubscribe from racer changes
            if (RaceTeamService != null)
                RaceTeamService.OnRacerChanged -= HandleRacerChangedAsync;

            // Unsubscribe from avatar changes
            if (AvatarService != null)
                AvatarService.OnAvatarChanged -= HandleAvatarChangedAsync;

            // Unsubscribe from game state changes
            if (GameStateService != null)
                GameStateService.OnGameStateChanged -= HandleGameStateChangedAsync;
        }
    }
}