using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class RaceTeam {
        private string? message;
        private ApplicationUser user = new ApplicationUser { NormalizedUserName = "USER@GMAIL.COM" };

        private List<Racer> raceTeam = new();

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [Inject]
        public UserManager<ApplicationUser> UserManager { get; set; } = default!;

        [Inject]
        internal IdentityUserAccessor UserAccessor { get; set; } = default!;

        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;

        [Inject]
        internal IRaceTeamService RaceTeamService { get; set; } = default!;

        [Inject]
        public ILogger<RaceTeam> Logger { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync() {
            // Subscribe to racer changes
            RaceTeamService.OnRacerChanged += HandleRacerChanged;
            
            await ReloadUsers();
        }
        
        private async void HandleRacerChanged()
        {
            await ReloadUsers();
            StateHasChanged();
        }

        private async Task OnValidSubmitAsync() {
            try {
                Racer newTeamMember = new() {
                    UserName = user.UserName ?? "",
                    Name = Input.MemberName,
                };

                await RaceTeamService.AddRacer(newTeamMember);
                // The list will be refreshed via the OnRacerChanged event
                
                message = "The team member has been added";
                Input = new(); // Clear the form
            } catch (Exception ex) {
                Logger.LogError(ex, "Error adding racer");
                message = "Error adding racer: " + ex.Message;
            }
        }

        private async Task ReloadUsers() {
            try {
                // Check if the user is authenticated before trying to get the user
                if (HttpContext.User.Identity?.IsAuthenticated == true) {
                    try {
                        user = await UserAccessor.GetRequiredUserAsync(HttpContext);
                    } catch (Exception) {
                        // Use the default user we already have
                        // No need to log a warning as this is expected for unauthenticated users
                    }
                }
                
                // Get the racers directly from the service
                raceTeam.Clear();
                var racers = await RaceTeamService.GetRacers();
                raceTeam.AddRange(racers);
                
                Logger.LogInformation($"Loaded {raceTeam.Count} racers");
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in ReloadUsers");
                // Initialize with empty list to avoid null reference exceptions
                raceTeam = new List<Racer>();
            }
        }

        private sealed class InputModel {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Racing name")]
            public string MemberName { get; set; } = "";
        }
        
        public void Dispose()
        {
            // Unsubscribe from racer changes
            RaceTeamService.OnRacerChanged -= HandleRacerChanged;
        }
    }
}