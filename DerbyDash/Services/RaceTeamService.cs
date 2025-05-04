using DerbyDash.Data;
using DerbyDash.Exceptions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Services {
    public class RaceTeamService: IRaceTeamService {
        private readonly AuthenticationStateProvider _authorizationState;
        private readonly ILogger<RaceTeamService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private List<Racer> raceTeam = new() {
                new Racer { Id = "1", Name = "Alice", LastRaced = new DateOnly(2025, 2, 1) },
                new Racer { Id = "2", Name = "Bob", LastRaced = new DateOnly(2025, 3, 15) },
                new Racer { Id = "3", Name = "Charlie" }
            };
        private Racer? Active;

        // Event that components can subscribe to for updates
        public event Action? OnRacerChanged;

        public RaceTeamService(
            AuthenticationStateProvider authorizationState,
            ILogger<RaceTeamService> logger,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager) {
            _authorizationState = authorizationState;
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<List<Racer>> GetRacers() {
            string name = await GetUserName("GetRacers");
            if (string.IsNullOrEmpty(name)) {
                return new List<Racer>();
            }
            ApplicationUser user = await GetUserByNameAsync(name);
            if (user == null) {
                return new List<Racer>();
            }
            return await GetRacers(user);
        }

        public async Task<List<Racer>> GetRacers(ApplicationUser user) {
            return raceTeam;
        }

        public async Task<Racer?> GetRacerByIdAsync(string racerId) {
            return raceTeam.FirstOrDefault(r => r.Id == racerId);
        }

        public async Task<ApplicationUser?> GetUserByNameAsync(string name) {
            return raceTeam.FirstOrDefault(r => r.NormalizedUserName == name.ToUpper());
        }

        public async Task<Racer> AddRacer(Racer racer) {
            raceTeam.Add(racer);
            if (Active is null) {
                Active = racer;
            }
            return racer;
        }

        public async Task UpdateRacer(Racer racer) {

        }

        public async Task RemoveRacer(string racerId) {

        }

        public Racer ActiveRacer {
            get => GetActiveRacer().GetAwaiter().GetResult();
            set => SetActiveRacer(value).Wait();
        }

        public async Task<Racer?> GetActiveRacer() {
            return Active;
        }

        public async Task SetActiveRacer(Racer racer) {
            Active = racer;
        }

        public async Task<string> GetUserName(string purpose) {
            AuthenticationState authState = await _authorizationState.GetAuthenticationStateAsync();
            if (authState == null) {
                _logger.LogError($"No authentication state found when trying to {purpose}.");
                throw new MissingUserException();
            }
            string? userName = authState?.User?.Identity?.Name;
            if (userName == null) {
                _logger.LogError($"Unable to determine the user name when trying to {purpose}.");
                throw new MissingUserException();
            }
            return userName;
        }
    }
}
