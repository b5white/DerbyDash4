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
        internal RacerService RacerService { get; set; } = default!;
        
        [Inject] 
        public ILogger<RaceTeam> Logger { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync() {
            try {
                user = await UserAccessor.GetRequiredUserAsync(HttpContext);
                // Get racers from the service
                raceTeam = RacerService.GetRacers();
            }
            catch (Exception ex) {
                Logger.LogError(ex, "Error loading race team");
                raceTeam = new List<Racer>();
            }
        }

        private async Task OnValidSubmitAsync() {
            try {
                Racer newTeamMember = new() {
                    UserName = user.UserName ?? "",
                    Name = Input.MemberName,
                };
                
                await RacerService.AddRacerAsync(newTeamMember);
                raceTeam = RacerService.GetRacers(); // Refresh the list
                
                message = "The team member has been added";
                Input = new(); // Clear the form
            }
            catch (Exception ex) {
                Logger.LogError(ex, "Error adding racer");
                message = "Error adding racer: " + ex.Message;
            }
        }

        private sealed class InputModel {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Racing name")]
            public string MemberName { get; set; } = "";
        }
    }
}