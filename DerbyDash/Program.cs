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
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();

            // Register both auth state providers
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRaceTeamService, RaceTeamService>();
            builder.Services.AddScoped<RaceService>();
            builder.Services.AddScoped<IFAQService, FAQService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
            builder.Services.AddScoped<FeedbackService>();
            builder.Services.AddScoped<DatabaseKeepAliveService>();
            builder.Services.AddScoped<IAvatarService, AvatarService>();
            builder.Services.AddScoped<GameStateService>();
            //builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();            // Add Identity services with Entity Framework stores
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                // Sign-in requirements
                options.SignIn.RequireConfirmedAccount = false; // Set to false for easier testing
                options.SignIn.RequireConfirmedEmail = false;   // Set to false for easier testing

                // Password requirements  
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6; // Reduced for easier testing

                // User requirements
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity cookie options
            builder.Services.ConfigureApplicationCookie(cookieOptions => {
                cookieOptions.LoginPath = "/Account/Login";
                cookieOptions.LogoutPath = "/Account/Logout";
                cookieOptions.AccessDeniedPath = "/Account/AccessDenied";
                cookieOptions.ExpireTimeSpan = TimeSpan.FromDays(90); // Set cookie expiration
                cookieOptions.SlidingExpiration = true;              // Optional: Reset expiration if active
                cookieOptions.Cookie.SameSite = SameSiteMode.Lax;  // Or None if cross-site
                cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;  // Enable for HTTPS
                cookieOptions.Cookie.HttpOnly = true;  // Protect against XSS
                cookieOptions.Cookie.Name = "DerbyDash";
                cookieOptions.Cookie.IsEssential = true;
            });
            // Add authorization services
            builder.Services.AddAuthorization();

            WebApplication app = builder.Build();

            app.MapGet("/throwerror", async () => {
                await Task.CompletedTask; // Just to use 'await'
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
