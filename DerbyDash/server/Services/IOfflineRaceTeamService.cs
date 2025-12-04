using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface IOfflineRaceTeamService {
        Task<List<Racer>> GetRacers(bool includeCount = true);
        Task<List<Racer>> GetRacersByUserId(string userId, bool includeCount);
        Task<Racer?> GetRacerByIdAsync(int racerId);
        Task<Racer> AddRacer(Racer racer);
        Task UpdateRacer(Racer racer);
        Task RemoveRacer(int racerId);
        Task<Racer?> GetActiveRacer();
        Task SetActiveRacer(Racer racer);
        Task<string?> GetLastPlayedRaceAsync();
        Task SaveLastPlayedRaceAsync(string problemClassString);
        Task<int> GetTeamRaceCountAsync();
        Task<int> GetCurrentRacerRaceCountAsync();
        Task<Racer?> GetRacerWithRaceCountAsync();
        Task<Race> SaveRaceCompletionAsync(double totalTime, string problemClassString, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0);
        Task<Race> SaveRaceCompletionAsync(double totalTime, int problemSetId, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0);
        Task<List<Racer>> GetRacersByUserId(string userId);
        Task<int> GetTotalRacerCountAsync();
        Task<Racer?> EnsureActiveRacerInitializedAsync();

        // Event for racer changes
        event Func<Task>? OnRacerChanged;
    }
}
