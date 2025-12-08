using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DerbyDash {
    public class Program {
        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add API Controllers
            builder.Services.AddControllers();
            
            // Add Razor Pages
            builder.Services.AddRazorPages();

            // Add CORS for client access
            builder.Services.AddCors(options => {
                options.AddPolicy("AllowClient", policy => {
                    policy.WithOrigins(
                            "http://localhost:5000",
                            "https://localhost:5001",
                            "http://localhost:5173",
                            "https://localhost:5174"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.EnableRetryOnFailure()
                )
            );
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add HttpContextAccessor for SessionData
            builder.Services.AddHttpContextAccessor();

            // Add JWT Bearer authentication
            builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                var jwtSettings = builder.Configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] 
                    ?? "DerbyDash_Super_Secret_Key_That_Is_At_Least_32_Characters_Long";
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

            // Register services
            builder.Services.AddScoped<IUserService, UserService>();
            
            // Always use direct service implementations on the server (not API wrappers)
            // API wrapper services (ApiFAQService, ApiRaceTeamService, ApiAuthService) are client-only
            builder.Services.AddScoped<IRaceTeamService, RaceTeamService>();
            builder.Services.AddScoped<IFAQService, FAQService>();
            builder.Services.AddScoped<FeedbackService>();
            builder.Services.AddScoped<IRaceService, RaceService>();
            builder.Services.AddScoped<IOfflineRaceTeamService, Services.Offline.OfflineRaceTeamService>();
            builder.Services.AddScoped<Services.Offline.OfflineRaceService>();
            builder.Services.AddScoped<Services.Offline.IOfflineDetectionService, Services.Offline.OfflineDetectionService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<DatabaseKeepAliveService>();
            builder.Services.AddScoped<IAvatarService, AvatarService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();
            // IApiAuthService is client-only - server handles auth via JWT validation

            // Email service
            builder.Services.AddScoped<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            // No-op JSRuntime for services that need it (cookies handled client-side in API)
            builder.Services.AddSingleton<Microsoft.JSInterop.IJSRuntime, Services.NoOpJSRuntime>();

            // SessionData and Session support
            builder.Services.AddScoped<SessionData>();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.IsEssential = true;
            });

            // Add Identity services with Entity Framework stores
            builder.Services.AddIdentityCore<ApplicationUser>(options => {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            // Add authorization services
            builder.Services.AddAuthorization(options => {
                // API policy for JWT-based authentication
                options.AddPolicy("ApiPolicy", policy => {
                    policy.RequireAuthenticatedUser();
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                });

                // Internal API policy - allows authenticated users or internal calls with header
                options.AddPolicy("InternalApiPolicy", policy => {
                    policy.RequireAssertion(context => {
                        // Allow if user is authenticated via JWT
                        if (context.User.Identity?.IsAuthenticated == true) {
                            return true;
                        }
                        
                        // Check for internal call header (for backward compatibility)
                        var httpContext = context.Resource as HttpContext;
                        return httpContext?.Request.Headers.ContainsKey("X-Internal-Call") == true &&
                               httpContext.Request.Headers["X-Internal-Call"] == "true";
                    });
                });
            });

            WebApplication app = builder.Build();

            // Ensure Identity tables are created in development
            if (app.Environment.IsDevelopment()) {
                using (var scope = app.Services.CreateScope()) {
                    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
                    using var dbContext = dbContextFactory.CreateDbContext();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    try {
                        var canConnect = dbContext.Database.CanConnect();
                        if (!canConnect) {
                            logger.LogInformation("Database does not exist. Creating database and tables...");
                            dbContext.Database.EnsureCreated();
                            logger.LogInformation("Database and tables created successfully.");
                        } else {
                            try {
                                var testQuery = dbContext.Database.ExecuteSqlRaw("SELECT TOP 1 Id FROM AspNetUsers");
                                logger.LogInformation("AspNetUsers table exists.");
                            } catch {
                                logger.LogInformation("AspNetUsers table not found. Creating all database tables...");
                                dbContext.Database.EnsureCreated();
                                logger.LogInformation("Database tables created successfully.");
                            }
                        }
                    } catch (Exception ex) {
                        try {
                            logger.LogWarning(ex, "Error checking database. Attempting to create tables...");
                            dbContext.Database.EnsureCreated();
                            logger.LogInformation("Database tables created successfully.");
                        } catch (Exception createEx) {
                            logger.LogError(createEx, "Could not ensure database is created. Please run: dotnet ef database update");
                        }
                    }
                }
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            } else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Enable static files (CSS, JS, images)
            app.UseCors("AllowClient");
            app.UseRouting();
            app.UseSession();
            app.UseMiddleware<CurrentSessionMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Map API Controllers
            app.MapControllers();
            
            // Map Razor Pages
            app.MapRazorPages();
            
            app.Run();
        }
    }
}

