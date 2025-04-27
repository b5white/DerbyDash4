using DerbyDash.Components;
using DerbyDash.Components.Account;
using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace DerbyDash {
    public class Program {
        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            //    .AddInteractiveWebAssemblyComponents()
            //    .AddAuthenticationStateSerialization();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<CustomAuthStateProvider>());

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add Identity services
            builder.Services.AddIdentityCore<ApplicationUser>(options => {
                options.SignIn.RequireConfirmedAccount = false;  // TODO turned off until we have email set up.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            // Add authentication services
            builder.Services.AddAuthentication(options => {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
            })
                .AddIdentityCookies();

            // Add authorization services
            builder.Services.AddAuthorization();

            // Add Scoped services
            builder.Services.AddScoped<RaceService>();
            //builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
            builder.Services.AddSingleton<UserManager<ApplicationUser>, NoOpUserManager>();

            WebApplication app = builder.Build();

            app.MapGet("/throwerror", async () => {
                throw new Exception("Simulated exception");
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                //    app.UseWebAssemblyDebugging();
                app.UseMigrationsEndPoint();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();
            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
            //    .AddInteractiveWebAssemblyRenderMode()
            //    .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
