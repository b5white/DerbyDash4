namespace DerbyDash.Services {
    public class GameStateService {
        private bool _isGameRunning = false;
        private string _currentRacePage = "";

        public event Action<bool>? OnGameStateChanged;
        public event Action<string>? OnPageChanged;

        public bool IsGameRunning => _isGameRunning;
        public string CurrentRacePage => _currentRacePage;

        public void SetGameRunning(bool isRunning) {
            if (_isGameRunning != isRunning) {
                _isGameRunning = isRunning;
                OnGameStateChanged?.Invoke(_isGameRunning);
            }
        }

        public void SetCurrentRacePage(string racePage) {
            if (_currentRacePage != racePage) {
                _currentRacePage = racePage;
                OnPageChanged?.Invoke(_currentRacePage);
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
