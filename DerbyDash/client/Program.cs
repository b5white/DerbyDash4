using DerbyDash.Components;
using DerbyDash.Components.Account;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise.Captcha.ReCaptcha;

namespace DerbyDash {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Configure HttpClient for API calls with auth handler
            var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7027";
            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
            builder.Services.AddScoped<AuthHttpMessageHandler>();
            builder.Services.AddScoped(sp => {
                var authHandler = sp.GetRequiredService<AuthHttpMessageHandler>();
                authHandler.InnerHandler = new HttpClientHandler();
                var client = new HttpClient(authHandler) { 
                    BaseAddress = new Uri(apiBaseUrl)
                };
                return client;
            });

            // Add API Services
            builder.Services.AddScoped<ApiAuthService>();
            builder.Services.AddScoped<ApiRaceTeamService>();
            builder.Services.AddScoped<ApiRaceService>();
            builder.Services.AddScoped<ApiProblemsService>();

            // Add Adapter Services (implement interfaces by calling API services)
            builder.Services.AddScoped<IUserService, ClientUserService>();
            builder.Services.AddScoped<IRaceTeamService, ClientRaceTeamService>();
            builder.Services.AddScoped<IRaceService, ClientRaceService>();
            builder.Services.AddScoped<GameStateService>();

            // Add Stub Services (to be replaced with API implementations)
            builder.Services.AddScoped<IFeedbackService, StubFeedbackService>();
            builder.Services.AddScoped<IFAQService, ClientFAQService>();
            builder.Services.AddScoped<ISubscriptionService, StubSubscriptionService>();
            builder.Services.AddScoped<ILogService, StubLogService>();
            builder.Services.AddScoped<IAvatarService, StubAvatarService>();
            
            // Add Offline Race Team Service (uses local storage)
            builder.Services.AddScoped<IOfflineRaceTeamService, StubOfflineRaceTeamService>();

            // Add Authorization
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            // Add Identity helpers (client-side versions)
            builder.Services.AddScoped<Components.Account.IdentityRedirectManager>();
            builder.Services.AddScoped<Components.Account.IdentityUserAccessor>();

            // Add Blazorise
            builder.Services
                .AddBlazorise(options => {
                    options.Immediate = true;
                })
                .AddBootstrap5Providers()
                .AddFontAwesomeIcons()
                .AddBlazoriseGoogleReCaptcha(reCaptchaOptions => {
                    reCaptchaOptions.SiteKey = "6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI"; // Google's test key
                });

            var host = builder.Build();

            // Initialize auth with stored token
            var authService = host.Services.GetRequiredService<ApiAuthService>();
            await authService.InitializeAuthAsync();

            await host.RunAsync();
        }
    }
}

