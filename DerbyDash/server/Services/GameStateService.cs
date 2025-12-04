namespace DerbyDash.Services {
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
                    await handler(isGameRunning); // Await each handler
                }
            }
        }

        async Task InvokeOnGameStateChanged(string newPage) {
            if (OnPageChanged != null) {
                var handlers = OnPageChanged.GetInvocationList().Cast<Func<string, Task>>();
                foreach (var handler in handlers) {
                    await handler(newPage); // Await each handler
                }
            }
        }

        public async Task SetCurrentRacePage(string racePage) {
            if (_currentRacePage != racePage) {
                _currentRacePage = racePage;
                await InvokeOnGameStateChanged(racePage);
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
