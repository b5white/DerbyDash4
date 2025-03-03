using DerbyDash.Components.Track;
using DerbyDash.Data;

namespace DerbyDash.Services {
    public class RaceService {
        private RaceComponents track = new();
        private readonly ILogger<RaceService> _logger;
        public float TotalDistance = 150;
        private Random random = new Random();

        public RaceService(ILogger<RaceService> logger) {
            _logger = logger;
        }

        public RaceComponents CreateTrack(string problemSetIdentifier) {
            List<Car> Cars = [
                new Car { index = 0, ImageUrl = "Racecar1.png", Top = 9999 }, // Initialize with off-screen position
                new Car { index = 1, ImageUrl = "Racecar2.png", Top = 9999 },
                new Car { index = 2, ImageUrl = "Racecar3.png", Top = 9999 },
                new Car { index = 3, ImageUrl = "Racecar4.png", Top = 9999 },
                new Car { index = 4, ImageUrl = "Racecar5.png", Top = 9999 },
                new Car { index = 5, ImageUrl = "Racecar6.png", Top = 9999 }
            ];

            for (int i = 1; i < Cars.Count; i++) {
                Cars[i].InitializeFastEddyTimeIncrements(random);
            }

            // Set the track properties
            track.Cars = Cars;

            // Initialize lines off-screen
            track.StartLine = new RaceComponent { Top = 9999, ImageUrl = "StartLine.png" };
            track.FinishLine = new RaceComponent { Top = 9999, ImageUrl = "FinishLine.png" };

            return track;
        }

        public Task SaveRaceAsync(RaceComponents track) {
            return Task.CompletedTask;
        }

        public List<SpeedIncrement> CreateSpeedIncrements(float[] Times) {
            return new List<SpeedIncrement>();
        }

        public void ReadScores(int problemId) {
            ;
        }

        public void WriteScores(int problemId) {
            ;
        }
    }
}
