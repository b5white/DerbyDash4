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
            await ReloadUsers();
        }

        private async Task OnValidSubmitAsync() {
            try {
                Racer newTeamMember = new() {
                    UserName = user.UserName ?? "",
                    Name = Input.MemberName,
                };

                await RaceTeamService.AddRacer(newTeamMember);
                raceTeam = await RaceTeamService.GetRacers(user); // Refresh the list

                message = "The team member has been added";
                Input = new(); // Clear the form
            } catch (Exception ex) {
                Logger.LogError(ex, "Error adding racer");
                message = "Error adding racer: " + ex.Message;
            }
        }

        private async Task ReloadUsers() {
            try {
                user = await UserAccessor.GetRequiredUserAsync(HttpContext);
                raceTeam.Clear();
                raceTeam.AddRange(await RaceTeamService.GetRacers(user));
            } catch (Exception ex) {
                Logger.LogError(ex, "OnInitializedAsync");
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