using DerbyDash.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Account.Pages {
    public partial class Login {
        private string? errorMessage;
        private bool showPassword = false;

        [Inject]
        UserManager<ApplicationUser> UserManager { get; set; } = null!;

        // [CascadingParameter]
        // private HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        private void TogglePasswordVisibility()
        {   
            showPassword = !showPassword;
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync() {
            // No HttpContext usage in interactive mode
            await Task.CompletedTask;
        }

        public async Task LoginUser() {
            Console.WriteLine("Logging in user.");
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            SignInResult result;

            string email = Input.Email?.Trim() ?? "";
            var user = await UserManager.FindByNameAsync(email);
            if (user != null) {
                await SignInManager.SignInAsync(user, true);
                result = SignInResult.Success;
            } else {
                result = SignInResult.Failed;
            }
            if (result.Succeeded) {
                Logger.LogInformation("User logged in.");
                RedirectManager.RedirectTo(ReturnUrl);
            } else if (result.RequiresTwoFactor) {
                RedirectManager.RedirectTo(
                    "Account/LoginWith2fa",
                    new() { ["returnUrl"] = ReturnUrl, ["rememberMe"] = Input.RememberMe });
            } else if (result.IsLockedOut) {
                Logger.LogWarning("User account locked out.");
                RedirectManager.RedirectTo("Account/Lockout");
            } else {
                errorMessage = "Error: Invalid login attempt.";
                // TODO make this pause longer each time
            }
        }

        private sealed class InputModel {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
    }
}