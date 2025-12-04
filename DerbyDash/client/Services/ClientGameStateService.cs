namespace DerbyDash.Services {
    /// <summary>
    /// Client-side game state service for tracking game state
    /// </summary>
    public class GameStateService {
        private bool _isGameRunning = false;
        private string _currentRacePage = "";

        public event Func<bool, Task>? OnGameStateChanged;
        public event Func<string, Task>? OnPageChanged;

        public bool IsGameRunning => _isGameRunning;
        public string CurrentRacePage => _currentRacePage;

        public async Task SetGameRunning(bool isRunning) {
            if (_isGameRunning != isRunning) {
                _isGameRunning = isRunning;
                await InvokeOnGameStateChanged(_isGameRunning);
            }
        }

        async Task InvokeOnGameStateChanged(bool isGameRunning) {
            if (OnGameStateChanged != null) {
                var handlers = OnGameStateChanged.GetInvocationList().Cast<Func<bool, Task>>();
                foreach (var handler in handlers) {
                    await handler(isGameRunning);
                }
            }
        }

        async Task InvokeOnPageChanged(string newPage) {
            if (OnPageChanged != null) {
                var handlers = OnPageChanged.GetInvocationList().Cast<Func<string, Task>>();
                foreach (var handler in handlers) {
                    await handler(newPage);
                }
            }
        }

        public async Task SetCurrentRacePage(string racePage) {
            if (_currentRacePage != racePage) {
                _currentRacePage = racePage;
                await InvokeOnPageChanged(racePage);
            }
        }

        public bool IsOnRacePage() {
            return !string.IsNullOrEmpty(_currentRacePage);
        }

        public bool ShouldHideNavbarOnMobile() {
            return IsOnRacePage() && _isGameRunning;
        }
    }
}

