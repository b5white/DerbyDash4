using DerbyDash.Components.Track;
using DerbyDash.Data;

namespace DerbyDash.Services {
    public class RaceService {
        private RaceComponents track = new();
        private readonly ILogger<RaceService> _logger;
        public float TotalDistance = 200;
        private Random random = new Random();

        public RaceService(ILogger<RaceService> logger) {
            _logger = logger;
        }

        public RaceComponents CreateTrack(string problemSetIdentifier) {
            const int CAR_GAP = 30;
            List<Car> Cars = [
                new Car { index = 0, ImageId = 1, Top = 9999 }, // Initialize with off-screen position
                new Car { index = 1, ImageId = 2, Top = 9999 },
                new Car { index = 2, ImageId = 3, Top = 9999 },
                new Car { index = 3, ImageId = 4, Top = 9999 },
                new Car { index = 4, ImageId = 5, Top = 9999 },
                new Car { index = 5, ImageId = 6, Top = 9999 }
            ];

            RaceComponents track = new RaceComponents {
                Cars = Cars,
                StartLine = new RaceComponent {
                    Top = 490f, // Set to match the CSS value
                    ImageUrl = "startline.png"
                },
                FinishLine = new RaceComponent {
                    Top = 9999f,
                    ImageUrl = "finishline.png"
                }
            };

            for (int i = 1; i < Cars.Count; i++) {
                Cars[i].InitializeFastEddyTimeIncrements(random, i);
                Cars[i].ResetFlexBasis(Cars.Count, CAR_GAP);
            }

            return track;
        }

        public Task SaveRaceAsync(RaceComponents track) {
            return Task.CompletedTask;
        }

        public List<SpeedIncrement> CreateSpeedIncrements(float[] Times) {
            return new List<SpeedIncrement>();
        }

        public void ReadResults(int problemId) {
            ;
        }

        public void WriteResults(int problemId) {
            ;
        }
    }
}
