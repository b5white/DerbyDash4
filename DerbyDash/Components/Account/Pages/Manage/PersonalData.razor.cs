using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class PersonalData {
        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        //[Inject]
        //internal IdentityUserAccessor UserAccessor { get; set; } = default!;

        protected override async Task OnInitializedAsync() {
            //_ = await UserAccessor.GetRequiredUserAsync(HttpContext);
            await Task.CompletedTask; // Just to use 'await'
            return;
        }
    }
}