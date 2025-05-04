using DerbyDash.Components;
using DerbyDash.Components.Account;
using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
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

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add Scoped services
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddRazorComponents();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<CustomAuthStateProvider>());
            builder.Services.AddScoped<IRaceTeamService, RaceTeamService>();
            builder.Services.AddScoped<RaceService>();
            //builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            builder.Services.ConfigureApplicationCookie(cookieOptions => {
                cookieOptions.ExpireTimeSpan = TimeSpan.FromDays(90); // Set cookie expiration
                cookieOptions.SlidingExpiration = true;              // Optional: Reset expiration if active
                cookieOptions.Cookie.SameSite = SameSiteMode.Lax;  // Or None if cross-site
                cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Enable for HTTPS
                cookieOptions.Cookie.HttpOnly = true;  // Protect against XSS
            });
            builder.Services.AddSingleton<IUserStore<ApplicationUser>>(provider =>
                new FakeUserStore(provider.GetService<ILogger<FakeUserStore>>()));
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
            })
                //.AddIdentityCookies()
                .AddCookie(IdentityConstants.ApplicationScheme, options => {
                    options.LoginPath = "/login";
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(90); // Set cookie expiration
                    options.SlidingExpiration = true;              // Optional: Reset expiration if active
                    options.Cookie.SameSite = SameSiteMode.Lax;  // Or None if cross-site
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Enable for HTTPS
                });
            // Add authorization services
            builder.Services.AddAuthorization();

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
