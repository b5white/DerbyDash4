using DerbyDash.Components.Track;
using DerbyDash.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Services {
    public class RaceService {
        private readonly IRaceTeamService _raceTeamService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthenticationStateProvider _authStateProvider;
        private RaceComponents track = new();
        private readonly ILogger<RaceService> _logger;
        public float TotalDistance = 200;
        private Random random = new Random();

        public RaceService(
            ILogger<RaceService> logger,
            IRaceTeamService raceTeamService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            AuthenticationStateProvider authStateProvider) {
            _logger = logger;
            _raceTeamService = raceTeamService;
            _context = context;
            _userManager = userManager;
            _authStateProvider = authStateProvider;
        }

        public Task<List<Race>> GetRacesByTeamMemberIdAsync(string teamMemberId) {
            _logger.LogInformation("GetRacesByTeamMemberIdAsync");
            return Task.FromResult(new List<Race>());
        }

        public RaceComponents CreateTrack(string problemSetIdentifier) {
            const int CAR_GAP = 30;
            List<Car> Cars = [
                new Car { index = 0, ImageId = 1, Top = 9999 }, // Initialize with off-screen position
                new Car { index = 1, ImageId = 2, Top = 9999 },
                new Car { index = 2, ImageId = 3, Top = 9999 },
                new Car { index = 3, ImageId = 4, Top = 9999 },
                new Car { index = 4, ImageId = 5, Top = 9999 },
                new Car { index = 5, ImageId = 6, Top = 9999 }
            ];

            RaceComponents track = new RaceComponents {
                Cars = Cars,
                StartLine = new RaceComponent {
                    Top = 490f, // Set to match the CSS value
                    ImageUrl = "startline.png"
                },
                FinishLine = new RaceComponent {
                    Top = 9999f,
                    ImageUrl = "finishline.png"
                }
            };

            // Start from index 0 to initialize all cars
            for (int i = 0; i < Cars.Count; i++) {
                // Make sure we don't exceed the array bounds in the Car class
                int safeIndex = Math.Min(i, 4); // The times/distances arrays have 5 rows (0-4)
                Cars[i].InitializeFastEddyTimeIncrements(random, safeIndex);
                Cars[i].ResetFlexBasis(Cars.Count, CAR_GAP);
            }

            return track;
        }

        public async Task SaveRaceAsync(RaceComponents track) {
            return;
        }

        public List<SpeedIncrement> CreateSpeedIncrements(float[] Times) {
            return new List<SpeedIncrement>();
        }

        /// <summary>
        /// Saves the last played race for the current user in the database
        /// </summary>
        /// <param name="problemClassString">The identifier of the race (e.g., "addition-4stable")</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveLastPlayedRaceAsync(string problemClassString) {
            try {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true) {
                    var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userId)) {
                        var appUser = await _userManager.FindByIdAsync(userId);

                        if (appUser != null) {
                            appUser.LastPlayedRace = problemClassString;
                            appUser.LastPlayedTime = DateTime.UtcNow;

                            await _userManager.UpdateAsync(appUser);
                            _logger.LogInformation($"Saved last played race '{problemClassString}' for user {userId}");
                        }
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error saving last played race to database");
            }
        }

        /// <summary>
        /// Gets the last played race for the current user from database
        /// </summary>
        /// <returns>The identifier of the last played race, or null if not found</returns>
        public async Task<string?> GetLastPlayedRaceAsync() {
            try {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true) {
                    var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userId)) {
                        var appUser = await _userManager.FindByIdAsync(userId);

                        if (appUser != null && !string.IsNullOrEmpty(appUser.LastPlayedRace)) {
                            _logger.LogInformation($"Retrieved last played race '{appUser.LastPlayedRace}' for user {userId}");
                            return appUser.LastPlayedRace;
                        }
                    }
                }

                return null;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error retrieving last played race from database");
                return null;
            }
        }
    }
}

