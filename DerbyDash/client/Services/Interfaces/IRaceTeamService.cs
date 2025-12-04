using DerbyDash.Data;

namespace DerbyDash.Services {
    // Client-side interface matching server interface
    public interface IRaceTeamService {
        event Func<Task>? OnRacerChanged;
        Task<List<Racer>> GetRacersByUserId(string userId, bool includeCount);
        Task<List<Racer>> GetRacers(bool includeCount = true);
        Task<Racer?> GetRacerByIdAsync(int racerId);
        Task<Racer?> GetRacerWithRaceCountAsync();
        Task<Racer> AddRacer(Racer racer);
        Task UpdateRacer(Racer racer);
        Task RemoveRacer(int racerId);
        Task<Racer?> GetActiveRacer();
        Task SetActiveRacer(Racer racer);
        Task<string?> GetLastPlayedRaceAsync();
        Task SaveLastPlayedRaceAsync(string problemClassString);
        Task<int> GetTeamRaceCountAsync();
        Task<int> GetCurrentRacerRaceCountAsync();
        Task<Race> SaveRaceCompletionAsync(double totalTime, string problemClassString, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0);
        Task<Race> SaveRaceCompletionAsync(double totalTime, int problemSetId, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0);
        Task<int> GetTotalRacerCountAsync();
        Task<Racer?> EnsureActiveRacerInitializedAsync();
    }
}

