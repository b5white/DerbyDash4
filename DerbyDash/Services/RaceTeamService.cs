﻿﻿﻿﻿﻿using DerbyDash.Data;
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
            try {
                // Try to get the username, but don't fail if we can't
                try {
                    string name = await GetUserName("GetRacers");
                    _logger.LogInformation($"Getting racers for user: {name}");
                } catch (Exception ex) {
                    _logger.LogWarning(ex, "Could not get username, but continuing");
                    // Continue even if we can't get the username
                }
                
                // Return a copy of the race team to avoid modification issues
                return raceTeam.ToList();
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error in GetRacers()");
                return new List<Racer>();
            }
        }

        public async Task<List<Racer>> GetRacers(ApplicationUser user) {
            // Return the same data as GetRacers() for consistency
            return await GetRacers();
        }

        public async Task<Racer?> GetRacerByIdAsync(string racerId) {
            return raceTeam.FirstOrDefault(r => r.Id == racerId);
        }

        public async Task<ApplicationUser?> GetUserByNameAsync(string name) {
            // This method should return an ApplicationUser, not a Racer
            // For now, we'll return null as a placeholder
            // In a real application, you would query your user store
            return null;
        }

        public async Task<Racer> AddRacer(Racer racer) {
            // Generate a unique ID if not provided
            if (string.IsNullOrEmpty(racer.Id)) {
                racer.Id = Guid.NewGuid().ToString();
            }
            
            raceTeam.Add(racer);
            
            // Set as active racer if none is selected
            if (Active is null) {
                Active = racer;
            }
            
            // Notify subscribers that the racer list has changed
            OnRacerChanged?.Invoke();
            
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
            // Notify subscribers that the active racer has changed
            OnRacerChanged?.Invoke();
        }

        public async Task<string> GetUserName(string purpose) {
            try {
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
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error getting username for {purpose}");
                throw new MissingUserException("Could not determine username", ex);
            }
        }
    }
}
