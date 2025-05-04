using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface IRaceTeamService {
        public event Action? OnRacerChanged;
        public Task<List<Racer>> GetRacers(ApplicationUser user);
        public Task<List<Racer>> GetRacers();
        public Task<Racer?> GetRacerByIdAsync(string racerId);
        public Task<Racer> AddRacer(Racer racer);
        public Task UpdateRacer(Racer racer);
        public Task RemoveRacer(string racerId);
        public Racer ActiveRacer { get; set; }
        public Task<Racer> GetActiveRacer();
        public Task SetActiveRacer(Racer racer);
        public Task<string> GetUserName(string purpose);
    }
}
