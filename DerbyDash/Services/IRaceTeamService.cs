using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface IRaceTeamService {
        public event Func<Task>? OnRacerChanged;
        public Task<List<Racer>> GetRacersByUserId(string userId);
        public Task<List<Racer>> GetRacers();
        public Task<Racer?> GetRacerByIdAsync(int racerId);
        public Task<Racer?> GetRacerWithRaceCountAsync();
        public Task<Racer> AddRacer(Racer racer);
        public Task UpdateRacer(Racer racer);
        public Task RemoveRacer(int racerId);

        /// <summary>
        /// Gets the active racer, initializing from cookie/database if needed (legacy method)
        /// Use ActiveRacer property for better performance when you know it's already initialized
        /// </summary>
        public Task<Racer?> GetActiveRacer();

        public Task SetActiveRacer(Racer racer);

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

        /// <summary>
        /// Gets the total number of races completed by the current user's team
        /// </summary>
        /// <returns>The total number of races</returns>
        public Task<int> GetTeamRaceCountAsync();

        /// <summary>
        /// Gets the number of races completed by the current active racer
        /// </summary>
        /// <returns>The number of races for the active racer</returns>
        public Task<int> GetCurrentRacerRaceCountAsync();

        /// <summary>
        /// Saves a completed race to the database for the current active racer
        /// </summary>
        /// <param name="totalTime">The total time taken to complete the race</param>
        /// <param name="problemClassString">The problem class string (e.g., "addition-4stable")</param>
        /// <param name="speedIncrements">The speed increments during the race</param>
        /// <returns>The saved race record</returns>
        public Task<Race> SaveRaceCompletionAsync(double totalTime, string problemClassString, List<SpeedIncrement>? speedIncrements = null);

        /// <summary>
        /// Saves a completed race to the database for the current active racer
        /// </summary>
        /// <param name="totalTime">The total time taken to complete the race</param>
        /// <param name="problemSetId">The identifier of the problem set (e.g., 1 for addition-4stable)</param>
        /// <param name="speedIncrements">The speed increments during the race</param>
        /// <returns>The saved race record</returns>
        public Task<Race> SaveRaceCompletionAsync(double totalTime, int problemSetId, List<SpeedIncrement>? speedIncrements = null);
    }
}
