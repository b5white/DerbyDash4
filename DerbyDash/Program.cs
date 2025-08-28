using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Captcha.ReCaptcha;
using Blazorise.Icons.FontAwesome;
using DerbyDash.Components;
using DerbyDash.Components.Account;
using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DerbyDash {
    public class Program {
        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            //    .AddInteractiveWebAssemblyComponents()
            //    .AddAuthenticationStateSerialization();

            // Add API Controllers
            builder.Services.AddControllers();

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.EnableRetryOnFailure()
                )
            );
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add Scoped services
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();

            // Register both auth state providers
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            // Add authentication services and cookie options
            builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddCookie(IdentityConstants.ApplicationScheme, cookieOptions => {
                    cookieOptions.LoginPath = "/Account/Login";
                    cookieOptions.LogoutPath = "/Account/Logout";
                    cookieOptions.AccessDeniedPath = "/Account/AccessDenied";
                    cookieOptions.ExpireTimeSpan = TimeSpan.FromDays(90); // Set cookie expiration
                    cookieOptions.SlidingExpiration = true;              // Optional: Reset expiration if active
                    cookieOptions.Cookie.SameSite = SameSiteMode.Lax;  // Or None if cross-site
                    cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;  // Enable for HTTPS
                    cookieOptions.Cookie.HttpOnly = true;  // Protect against XSS
                    cookieOptions.Cookie.Name = "TurboFlash";
                    cookieOptions.Cookie.IsEssential = true;
                })
                .AddJwtBearer("ApiScheme", options => {
                    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
                    var secretKey = jwtSettings["SecretKey"] ?? "DerbyDash_Super_Secret_Key_That_Is_At_Least_32_Characters_Long";
                    var issuer = jwtSettings["Issuer"] ?? "DerbyDash";
                    var audience = jwtSettings["Audience"] ?? "DerbyDashAPI";

                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddScoped<IUserService, UserService>();
            
            // Conditionally register services based on configuration
            var enableApiServices = builder.Configuration.GetValue<bool>("ApiMode:EnableApiServices");
            var useApiForRaceTeam = builder.Configuration.GetValue<bool>("ApiMode:UseApiForRaceTeam");
            var useApiForFAQ = builder.Configuration.GetValue<bool>("ApiMode:UseApiForFAQ");
            var useApiForFeedback = builder.Configuration.GetValue<bool>("ApiMode:UseApiForFeedback");
            
            if (enableApiServices && useApiForRaceTeam) {
                // Register the original service as a dependency for the API service
                builder.Services.AddScoped<RaceTeamService>();
                builder.Services.AddScoped<IRaceTeamService, ApiRaceTeamService>();
            } else {
                builder.Services.AddScoped<IRaceTeamService, RaceTeamService>();
            }

            if (enableApiServices && useApiForFAQ) {
                // Register the original service as a dependency for the API service
                builder.Services.AddScoped<FAQService>();
                builder.Services.AddScoped<IFAQService, ApiFAQService>();
            } else {
                builder.Services.AddScoped<IFAQService, FAQService>();
            }

            builder.Services.AddScoped<FeedbackService>();
            
            builder.Services.AddScoped<RaceService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<DatabaseKeepAliveService>();
            builder.Services.AddScoped<IAvatarService, AvatarService>();
            builder.Services.AddScoped<GameStateService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IApiAuthService, ApiAuthService>();

            // Add HttpClient for API calls
            builder.Services.AddHttpClient("default", client => {
                var baseUrl = builder.Configuration["BaseUrl"] ?? "https://localhost:7028";
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            
            // Register HttpClient as a service for DI
            builder.Services.AddScoped<HttpClient>(provider => {
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                return factory.CreateClient("default");
            });

            // Email
            //builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            // SessionData and Logging
            builder.Services.AddScoped<SessionData>();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.IsEssential = true;
            });

                // recaptcha settings
                builder.Services
                    .AddBlazorise(options => {
                        options.Immediate = true;
                    })
                    .AddBootstrap5Providers()  // Provides IClassProvider and other core services
                    .AddFontAwesomeIcons()     // For icons (optional)
                    .AddBlazoriseGoogleReCaptcha(reCaptchaOptions => {
                        reCaptchaOptions.SiteKey = "6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI"; // Google's test key for testing
                        // reCaptchaOptions.SiteKey = builder.Configuration["ReCaptcha:SiteKey"] ?? "";
                    });
                builder.Services.AddHttpClient("ReCaptcha", client => {
                    client.BaseAddress = new Uri("https://www.google.com/recaptcha/api/");
                    client.Timeout = TimeSpan.FromSeconds(30);
                });
            // Add Identity services with Entity Framework stores
            builder.Services.AddIdentityCore<ApplicationUser>(options => {
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
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            // builder.Services.AddSingleton<IUserStore<ApplicationUser>, FakeUserStore>();

            // Add authorization services
            builder.Services.AddAuthorization(options => {
                // Default policy for Blazor components (uses cookies)
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                    .Build();

                // API policy for JWT-based authentication OR cookie authentication (for seamless integration)
                options.AddPolicy("ApiPolicy", policy => {
                    policy.RequireAuthenticatedUser();
                    policy.AddAuthenticationSchemes("ApiScheme", IdentityConstants.ApplicationScheme);
                });

                // Internal API policy for server-side calls with special header
                options.AddPolicy("InternalApiPolicy", policy => {
                    policy.RequireAssertion(context => {
                        // Allow if user is authenticated OR if it's an internal call
                        if (context.User.Identity?.IsAuthenticated == true) {
                            return true;
                        }
                        
                        // Check for internal call header
                        var httpContext = context.Resource as HttpContext;
                        return httpContext?.Request.Headers.ContainsKey("X-Internal-Call") == true &&
                               httpContext.Request.Headers["X-Internal-Call"] == "true";
                    });
                });
            });

            WebApplication app = builder.Build();

            app.MapGet("/throwerror", async () => {
                await Task.CompletedTask; // Just to use 'await'
                var inner = new Exception("Inner exception message");
                throw new Exception("Simulated exception", inner);
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
            app.UseSession();
            app.UseMiddleware<CurrentSessionMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();
            app.MapStaticAssets();
            
            // Map API Controllers
            app.MapControllers();
            
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
