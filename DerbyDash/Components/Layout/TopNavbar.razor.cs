using DerbyDash.Components.Account;
using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Components.Layout {
    public partial class TopNavbar: ComponentBase, IDisposable {
        private List<Racer> Racers { get; set; } = new List<Racer>();
        private Racer SelectedRacer { get; set; } = new Racer { Id = 0, Name = "" };

        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] private IRaceTeamService RaceTeamService { get; set; } = default!;
        [Inject] private ILogger<TopNavbar> Logger { get; set; } = default!;
        [Inject] private IAvatarService AvatarService { get; set; } = default!;
        [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject] private GameStateService GameStateService { get; set; } = default!;

        private string? RacerAvatarFileName;
        private string CurrentUrl => NavManager.Uri;
        private int CurrentRacerRaceCount { get; set; } = 0;
        private int TeamRaceCount { get; set; } = 0;
        private string? RacerInitial;
        private string? RacerName; // Add property for email

        private bool isGameRunning = false;

        protected override async Task OnInitializedAsync() {
            // Subscribe to racer changes
            RaceTeamService.OnRacerChanged += HandleRacerChangedAsync;

            // Subscribe to avatar changes
            AvatarService.OnAvatarChanged += HandleAvatarChangedAsync;

            // Subscribe to game state changes
            GameStateService.OnGameStateChanged += HandleGameStateChanged;

            // Listen for game running state
            isGameRunning = GameStateService.IsGameRunning;

            await LoadUserAvatar();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                await LoadRacers();
                StateHasChanged();
            }
        }

        private async Task LoadRacers() {
            try {
                // Get racers from the service
                Racers = await RaceTeamService.GetRacers();
                if (Racers.Count > 0) {
                    // Set the selected racer to the current racer
                    Racer? currentRacer = await RaceTeamService.GetActiveRacer();

                    if (currentRacer != null) {
                        SelectedRacer = currentRacer;
                        // Load race counts only when we have an active racer
                        CurrentRacerRaceCount = await RaceTeamService.GetCurrentRacerRaceCountAsync();
                        TeamRaceCount = await RaceTeamService.GetTeamRaceCountAsync();
                    } else {
                        // No active racer selected - don't default to first racer
                        SelectedRacer = new Racer { Id = 0, Name = "" };
                        CurrentRacerRaceCount = 0;
                        TeamRaceCount = await RaceTeamService.GetTeamRaceCountAsync();
                    }
                } else {
                    // Initialize with an empty racer to avoid null reference exceptions
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
                //await LoadRacers();
                StateHasChanged();
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in HandleRacerChangedAsync");
            }
        }

        private async Task HandleAvatarChangedAsync() {
            try {
                await LoadRacerAvatar();
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
                return "Join Derby Dash";
            else if (CurrentUrl.Contains("/Account/Login"))
                return "Welcome Back";
            else if (CurrentUrl.Contains("/Race/"))
                return "Race Time!";
            else
                return "Derby Dash";
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

        private async Task LoadRacerAvatar() {
            Racer? racer = await RaceTeamService.GetActiveRacer();
            if (racer != null) {

                RacerAvatarFileName = racer.AvatarFileName;
                RacerName = racer.Name;
                RacerInitial = !string.IsNullOrEmpty(RacerName) ? RacerName.Substring(0, 1).ToUpper() : null;
            } else { // Clear fields if Racer not found (e.g., after logout)                  
                RacerAvatarFileName = null;
                RacerInitial = null;
                RacerName = null;
            }
        }

        private async void HandleGameStateChangedAsync(bool running) {
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
                GameStateService.OnGameStateChanged -= HandleGameStateChanged;
        }
    }
}