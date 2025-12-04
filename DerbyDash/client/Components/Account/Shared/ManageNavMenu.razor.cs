namespace DerbyDash.Components.Account.Shared {
    public partial class ManageNavMenu {
        private bool hasExternalLogins = false; // External logins not supported in client

        protected override async Task OnInitializedAsync() {
            // External logins would require server-side OAuth, so disable for now
            hasExternalLogins = false;
            await Task.CompletedTask;
        }
    }
}
