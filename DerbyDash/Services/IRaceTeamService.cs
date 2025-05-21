using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface IRaceTeamService {
        public event Action? OnRacerChanged;
        public Task<List<Racer>> GetRacers(ApplicationUser user);
        public Task<List<Racer>> GetRacers();
        public Task<Racer?> GetRacerByIdAsync(int racerId);
        public Task<Racer> AddRacer(Racer racer);
        public Task UpdateRacer(Racer racer);
        public Task RemoveRacer(int racerId);
        public Task<Racer> GetActiveRacer();
        public Task SetActiveRacer(Racer racer);
        public Task<string> GetUserName(string purpose);

        /// <summary>
        /// Gets the last played race for the current user from database
        /// </summary>
        /// <returns>The identifier of the last played race, or null if not found</returns>
        public Task<string?> GetLastPlayedRaceAsync();

        /// <summary>
        /// Saves the last played race for the current user in the database
        /// </summary>
        /// <param name="problemClassString">The identifier of the race (e.g., "addition-4stable")</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task SaveLastPlayedRaceAsync(string problemClassString);
    }
}
