using DerbyDash.Components;
using DerbyDash.Components.Account;
using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DerbyDash {
    public class Program {
        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            //    .AddInteractiveWebAssemblyComponents()
            //    .AddAuthenticationStateSerialization();

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add Scoped services
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<CustomAuthStateProvider>());
            builder.Services.AddScoped<IRaceTeamService, RaceTeamService>();
            builder.Services.AddScoped<RaceService>();
            builder.Services.AddScoped<IFAQService, FAQService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
            //builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            builder.Services.AddSingleton<IUserStore<ApplicationUser>>(provider =>
                new FakeUserStore(provider.GetRequiredService<ILogger<FakeUserStore>>()));
            //           builder.Services.AddSingleton<UserManager<ApplicationUser>, UserManager<ApplicationUser>>();
            builder.Services.AddScoped<UserManager<IdentityUser>>(provider => {
                var userManager = new UserManager<IdentityUser>(
                    provider.GetRequiredService<IUserStore<IdentityUser>>(),
                    provider.GetRequiredService<IOptions<IdentityOptions>>(),
                    provider.GetRequiredService<IPasswordHasher<IdentityUser>>(),
                    new List<IUserValidator<IdentityUser>>(),
                    new List<IPasswordValidator<IdentityUser>>(), // No password validators
                    provider.GetRequiredService<ILookupNormalizer>(),
                    provider.GetRequiredService<IdentityErrorDescriber>(),
                    provider.GetRequiredService<IServiceProvider>(),
                    provider.GetRequiredService<ILogger<UserManager<IdentityUser>>>()
                );
                return userManager;
            });

            // Add Identity services
            builder.Services.AddIdentityCore<ApplicationUser>(options => {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
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
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
                //.AddIdentityCookies()
                .AddCookie(IdentityConstants.ApplicationScheme, cookieOptions => {
                    cookieOptions.LoginPath = "/login";
                    cookieOptions.ExpireTimeSpan = TimeSpan.FromDays(90); // Set cookie expiration
                    cookieOptions.SlidingExpiration = true;              // Optional: Reset expiration if active
                    cookieOptions.Cookie.SameSite = SameSiteMode.Lax;  // Or None if cross-site
                    cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Enable for HTTPS
                    cookieOptions.Cookie.HttpOnly = true;  // Protect against XSS
                    cookieOptions.Cookie.Name = "DerbyDash";
                    cookieOptions.Cookie.IsEssential = true;
                });
            // Add authorization services
            builder.Services.AddAuthorization();

            WebApplication app = builder.Build();

            app.MapGet("/throwerror", () => {
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
            app.MapGet("/debug/routes",
                (IEnumerable<EndpointDataSource> sources) =>
                string.Join("\n", sources.SelectMany(s => s.Endpoints)));
            app.Run();
        }
    }
}
