using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Layout {
    public partial class TopNavbar {
        private List<Racer> Racers { get; set; } = new List<Racer>();
        private Racer SelectedRacer { get; set; }
        private string CurrentUrl => NavigationManager.Uri;

        protected override void OnInitialized() {
            // Subscribe to racer changes
            RaceTeamService.OnRacerChanged += StateHasChanged;

            // Subscribe to navigation changes
            NavigationManager.LocationChanged += (sender, e) => StateHasChanged();

            // Get racers from the service
            try {
                Racers = RaceTeamService.GetRacers().GetAwaiter().GetResult();
            } catch (Exception) {
                // user isn't logged in. Nothing to do here.
                return;
            }

            // Set the selected racer to the current racer
            Racer? currentRacer = RaceTeamService.GetActiveRacer().GetAwaiter().GetResult();
            if (currentRacer != null) {
                SelectedRacer = currentRacer;
            } else if (Racers.Any()) {
                SelectedRacer = Racers.First();
                RaceTeamService.SetActiveRacer(SelectedRacer);
            }
        }

        private void OnRacerChanged(ChangeEventArgs e) {
            string newRacerId = e.Value?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(newRacerId)) {
                Racer? newRacer = RaceTeamService.GetRacerByIdAsync(newRacerId).GetAwaiter().GetResult();
                if (newRacer != null) {
                    SelectedRacer = newRacer;
                    RaceTeamService.SetActiveRacer(newRacer);
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

        public void Dispose() {
            // Unsubscribe from racer changes
            RaceTeamService.OnRacerChanged -= StateHasChanged;

            // Unsubscribe from navigation changes
            NavigationManager.LocationChanged -= (sender, e) => StateHasChanged();
        }
    }
}