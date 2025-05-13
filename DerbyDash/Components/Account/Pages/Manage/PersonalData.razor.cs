using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class PersonalData {
        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        //[Inject]
        //internal IdentityUserAccessor UserAccessor { get; set; } = default!;

        protected override Task OnInitializedAsync() {
            //_ = await UserAccessor.GetRequiredUserAsync(HttpContext);
            return Task.CompletedTask;
        }
    }
}