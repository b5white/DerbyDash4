using DerbyDash.Components.Track;
using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface IRaceService {
        float TotalDistance { get; }
        Task<RaceComponents> CreateTrack(string problemSetIdentifier);
        Task SaveRaceAsync(Car car, int racerId, int problemId);
        Task DeleteRaces(string problemSet);
        List<SpeedIncrement> CreateSpeedIncrements(float[] times);
        Task<Race> SaveRaceAsync(Race race);
        Task<List<Race>> GetRaceHistoryAsync(int page, int pageSize, int? racerId = null);
        Task<int> GetRaceCountAsync(int? racerId = null);
        Task<Race?> GetRaceByIdAsync(int id);
        Task<object> GetRaceStatsAsync(int? racerId = null);
        Task DeleteRaceAsync(int id);
    }
}
