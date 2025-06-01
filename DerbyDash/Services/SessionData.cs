namespace DerbyDash.Services {
    public class SessionData {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int? _cachedRacerId;
        private string? _cachedUserId;

        public SessionData(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public int RacerId {
            get {
                // Return cached value if available
                if (!_cachedRacerId.HasValue) {
                    // Fetch from session only once per request
                    _cachedRacerId = _httpContextAccessor.HttpContext?.Session.GetInt32("RacerId");
                }
                return _cachedRacerId ?? 0;
            }
            set {
                _cachedRacerId = value;
                if (_httpContextAccessor.HttpContext != null) {
                    _httpContextAccessor.HttpContext.Session.SetInt32("RacerId", value);
                }
            }
        }

        public string UserId {
            get {
                // Return cached value if available
                if (_cachedUserId == null) {
                    // Fetch from session only once per request
                    _cachedUserId = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
                }
                return _cachedUserId ?? "";
            }
            set {
                _cachedUserId = value;
                if (_httpContextAccessor.HttpContext != null) {
                    _httpContextAccessor.HttpContext.Session.SetString("UserId", value);
                }
            }
        }
    }
}