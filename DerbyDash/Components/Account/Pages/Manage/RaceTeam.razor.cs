using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class RaceTeam: IDisposable {
        private string? message;

        private List<Racer> raceTeam = new();
        protected int TeamRaceCount { get; set; } = 0;

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
            RaceTeamService.OnRacerChanged += HandleRacerChangedAsync;
            await ReloadUsers();
        }

        private async void HandleRacerChangedAsync() {
            try {
                await ReloadUsers();
                StateHasChanged();
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in HandleRacerChangedAsync");
            }
        }

        private async Task OnValidSubmitAsync() {
            try {
                Racer newTeamMember = new() {
                    Name = Input.MemberName,
                    RaceCount = 0 // Initialize race count to 0
                };

                await RaceTeamService.AddRacer(newTeamMember);
                // The list will be refreshed via the OnRacerChanged event

                message = "The team member has been added";
                Input = new(); // Clear the form
            } catch (DuplicateRacerException) {
                // Preserve the entered name and show error
                message = $"Error: '{Input.MemberName}' already exists on the team";
            } catch (Exception ex) {
                Logger.LogError(ex, "Error adding racer");
                message = "Error adding racer: " + ex.Message;
            }
        }
        
        protected async Task SetActiveRacer(Racer racer) {
            try {
                await RaceTeamService.SetActiveRacer(racer);
                message = $"{racer.Name} is now the active racer.";
            } catch (Exception ex) {
                Logger.LogError(ex, "Error setting active racer");
                message = "Error setting active racer.";
            }
        }

        private async Task ReloadUsers() {
            try {
                // Get the racers directly from the service
                // The service handles its own user authentication
                raceTeam.Clear();
                raceTeam.AddRange(await RaceTeamService.GetRacers());
                
                // Get team race count
                TeamRaceCount = await RaceTeamService.GetTeamRaceCountAsync();
                
                Logger.LogInformation($"Loaded {raceTeam.Count} racers with {TeamRaceCount} total races");
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in ReloadUsers");
                // Initialize with empty list to avoid null reference exceptions
                raceTeam = new List<Racer>();
                TeamRaceCount = 0;
            }
        }

        private sealed class InputModel {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Racing name")]
            public string MemberName { get; set; } = "";
        }

        public void Dispose() {
            // Unsubscribe from racer changes
            RaceTeamService.OnRacerChanged -= HandleRacerChangedAsync;
        }
    }
}