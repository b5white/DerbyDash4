using DerbyDash.Components.Track;
using DerbyDash.Data;

namespace DerbyDash.Services {
    public class RaceService {
        private readonly IRaceTeamService _raceTeamService;
        private readonly ApplicationDbContext _context;
        private RaceComponents track = new();
        private readonly ILogger<RaceService> Logger;
        public float TotalDistance = 200;
        private Random random = new Random();
        Racer? activeTeamMember;

        public RaceService(
            ILogger<RaceService> logger,
            IRaceTeamService raceTeamService,
            ApplicationDbContext context) {
            Logger = logger;
            _raceTeamService = raceTeamService;
            _context = context;
        }

        public async Task<List<Race>> GetRacesByTeamMemberIdAsync(int teamMemberId) {
            Logger.LogInformation("GetRacesByTeamMemberIdAsync");
            await Task.CompletedTask; // Just to use 'await'
            return new List<Race>();
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

            // Start from index 0 to initialize all cars
            for (int i = 0; i < Cars.Count; i++) {
                // Make sure we don't exceed the array bounds in the Car class
                int safeIndex = Math.Min(i, 4); // The times/distances arrays have 5 rows (0-4)
                Cars[i].InitializeFastEddyTimeIncrements(random, safeIndex);
                Cars[i].ResetFlexBasis(Cars.Count, CAR_GAP);
            }

            return track;
        }

        public async Task SaveRaceAsync(RaceComponents track) {
            await Task.CompletedTask; // Just to use 'await'
            return;
        }        public List<SpeedIncrement> CreateSpeedIncrements(float[] Times) {
            return new List<SpeedIncrement>();
        }
    }
}
