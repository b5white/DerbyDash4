using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class RaceTeam {
        private string? message;
        private ApplicationUser user = default!;

        private List<Racer> raceTeam;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [Inject]
        public UserManager<ApplicationUser> UserManager { get; set; } = default!;
        //[Inject]
        //internal IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;
        //[Inject]
        //internal IRaceTeamService RaceTeamService { get; set; } = default!;
        [Inject] public ILogger<ChangePassword> Logger { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync() {
            //user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            raceTeam = new() {
                new Racer { Name = "Alice" },
                new Racer { Name = "Bob" },
                new Racer { Name = "Charlie" }
            };

            //raceTeam.Clear();
            //raceTeam.AddRange(await RaceTeamService.GetRacers());
        }

        private async Task OnValidSubmitAsync() {
            //Racer newTeamMember = new() {
            //    UserName = user.UserName ?? "",
            //    Name = Input.MemberName,
            //};
            //await RaceTeamService.AddRacer(newTeamMember);

            RedirectManager.RedirectToCurrentPageWithStatus("The team member has been added", HttpContext);
        }

        private sealed class InputModel {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Racing name")]
            public string MemberName { get; set; } = "";
        }
    }
}