namespace DerbyDash.Services.Offline {
    public interface IOfflineServiceProvider {
        bool IsOfflineMode { get; }
        IRaceService GetRaceService();
        IOfflineRaceTeamService GetRaceTeamService();
    }

    public class OfflineServiceProvider : IOfflineServiceProvider {
        private readonly IRaceService _onlineRaceService;
        private readonly OfflineRaceService _offlineRaceService;
        private readonly IOfflineRaceTeamService _offlineRaceTeamService;
        private readonly ILogger<OfflineServiceProvider> _logger;

        public bool IsOfflineMode { get; private set; }

        public OfflineServiceProvider(
            IRaceService onlineRaceService,
            OfflineRaceService offlineRaceService,
            IOfflineRaceTeamService offlineRaceTeamService,
            ILogger<OfflineServiceProvider> logger) {
            _onlineRaceService = onlineRaceService;
            _offlineRaceService = offlineRaceService;
            _offlineRaceTeamService = offlineRaceTeamService;
            _logger = logger;
        }

        public void SetOfflineMode(bool isOffline) {
            IsOfflineMode = isOffline;
            _logger.LogInformation($"Service provider switched to {(isOffline ? "offline" : "online")} mode");
        }

        public IRaceService GetRaceService() {
            return IsOfflineMode ? _offlineRaceService : _onlineRaceService;
        }

        public IOfflineRaceTeamService GetRaceTeamService() {
            return _offlineRaceTeamService;
        }
    }
}
