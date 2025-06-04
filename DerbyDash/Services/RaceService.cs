using DerbyDash.Components.Track;
using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Utilities;

namespace DerbyDash.Services {
    public class RaceService {
        private readonly IRaceTeamService _raceTeamService;
        private readonly ApplicationDbContext _context;
        private RaceComponents track = new();
        private readonly ILogger<RaceService> Logger;
        public float TotalDistance = 200;
        private Random random = new Random();
        Racer? activeRacer;

        public RaceService(
            ILogger<RaceService> logger,
            IRaceTeamService raceTeamService,
            ApplicationDbContext context) {
            Logger = logger;
            _raceTeamService = raceTeamService;
            _context = context;
        }

        [Obsolete]
        public async Task<List<Race>> GetRacesByRacerIdAsync(int racerId) {
            Logger.LogInformation("GetRacesByRacerIdAsync");
            List<Race> races = new();
            // await _context.Races
            //    .Where(r => r.RacerId == racerId)
            //    .ToListAsync();
            //if (races.Count == 0) {
            //    Logger.LogWarning("No races found for ID: {ID}", racerId);
            //}
            await Task.CompletedTask; // Just to use 'await'
            return races;
        }        public async Task<RaceComponents> CreateTrack(string problemSetIdentifier) {
            Logger.LogInformation("CreateTrack");
            // Use cached active racer or ensure it's initialized
            activeRacer = _raceTeamService.ActiveRacer ?? await _raceTeamService.EnsureActiveRacerInitializedAsync();
            if (activeRacer == null) {
                throw new MissingRacerException("No active racer selected. Please select a racer from your race team.");
            }
            return await CreateTrack(activeRacer.Id, problemSetIdentifier);
        }

        private async Task<RaceComponents> CreateTrack(int racerId, string problemSet) {
            Logger.LogInformation("CreateTrack");
            const int CAR_GAP = 30;
            const int MAX_CARS = 6;

            int problemSetId = UtilityMethods.GetUniqueIntFromString(problemSet);
            // Retrieve the 5 previous races for the given RacerID
            try {
                List<Race> previousRaces = new List<Race>();
                //List<Race> previousRaces = await _context.Races
                //    .Where(r => r.RacerId == racerId && r.ProblemSetId == problemSetId)
                //    .Include(r => r.SpeedIncrements)
                //    .OrderByDescending(r => r.RaceDateTime)
                //    .Take(5)
                //    .ToListAsync().ConfigureAwait(false);

                // Generate a random ImageId for the car at index 0
                Random random = new Random();
                int randomImageId = random.Next(1, 11); // Assuming you have 10 car images available

                // Initialize the cars list
                List<Car> Cars = new List<Car> {
                new Car { index = 0, ImageId = randomImageId, RaceDateTime = DateTime.Now, Top = 9999  },
                new Car { index = 1, ImageId = 2, Top = 9999  },
                new Car { index = 2, ImageId = 3, Top = 9999  },
                new Car { index = 3, ImageId = 4, Top = 9999  },
                // new Car { index = 4, ImageId = 5, Top = 9999  },
                // new Car { index = 5, ImageId = 6, Top = 9999  } // Add more cars as needed
                // Only 3 cars for the race
                };

                // Assign the previous races to cars 1 to 5
                for (int i = 1; i <= previousRaces.Count; i++) {
                    Race race = previousRaces[i - 1];
                    Car car = new(); // Cars[i];
                    car.ImageId = race.ImageId;
                    car.RaceId = race.Id;
                    car.TotalTime = race.TotalTime;
                    car.RaceDateTime = race.RaceDateTime;
                    car.SpeedIncrements.AddRange(race.SpeedIncrements ?? Enumerable.Empty<SpeedIncrement>());
                    car.ResetFlexBasis(MAX_CARS, CAR_GAP);
                    car.Top = 9999; // Initialize the top position off-screen
                    Cars.Add(car);
                }

                // Start from index 0 to initialize all cars
                for (int i = 0; i < Cars.Count; i++) {
                    // Make sure we don't exceed the array bounds in the Car class
                    int safeIndex = Math.Min(i, 4); // The times/distances arrays have 5 rows (0-4)
                    Cars[i].InitializeFastEddyTimeIncrements(random, safeIndex);
                    Cars[i].ResetFlexBasis(MAX_CARS/*Cars.Count*/, CAR_GAP);
                }
                // Set the track properties
                track.Cars = Cars;
                track.ProblemId = problemSetId;
                track.RacerId = racerId;

                // Initialize lines off-screen
                track.StartLine = new RaceComponent { Top = 490f, ImageUrl = "StartLine.png" };
                track.FinishLine = new RaceComponent { Top = 9999f, ImageUrl = "FinishLine.png" };
                await Task.CompletedTask; // Just to use 'await'
            } catch (Exception ex) {
                Logger.LogError(ex, "CreateTrack");
            }
            return track;
        }

        public async Task SaveRaceAsync(Car car, int racerId, int problemId) {
            Logger.LogInformation("SaveRaceAsync");

            Race race = CreateNewRace(car, racerId, problemId);
            //_context.Races.Add(race);
            //await _context.SaveChangesAsync();
            car.RaceId = race.Id;

            activeRacer!.LastRaced = DateOnly.FromDateTime(DateTime.Now);
            await _raceTeamService.UpdateRacer(activeRacer);
            await Task.CompletedTask; // Just to use 'await'
        }

        private Race CreateNewRace(Car car, int racerId, int problemId) {
            Logger.LogInformation("CreateNewRace");
            Race race = new Race {
                ImageId = car.ImageId,
                TotalTime = car.TotalTime,
                RaceDateTime = car.RaceDateTime,
                RacerId = racerId,
                ProblemSetId = problemId,
            };

            if (car.SpeedIncrements != null) {
                race.SpeedIncrements = car.SpeedIncrements;
            } else {
                race.SpeedIncrements = new List<SpeedIncrement>();
            }

            return race;
        }

        public List<SpeedIncrement> CreateSpeedIncrements(float[] Times) {
            Logger.LogInformation("CreateSpeedIncrements");
            float currentTime = 0;
            double currentSpeed = 0;
            double currentDistance = 0;

            List<SpeedIncrement> speedIncrements = new List<SpeedIncrement>();
            for (int i = 0; i < Times.Length; i++) {
                double timeSpan = Times[i];
                if (timeSpan == 0) {
                    break; // Exit the loop after the last populated timeSpan 
                }
                if (i == 0) {
                    currentDistance = 0;
                } else {
                    currentDistance += currentSpeed * (timeSpan - Times[i - 1]);
                }
                currentSpeed++; // Assumes speed increment of 1
                speedIncrements.Add(new SpeedIncrement {
                    Time = timeSpan,
                    Speed = currentSpeed,
                    Distance = currentDistance * RaceComponents.SPEED_MULTIPLIER
                });
                Console.WriteLine($"timeSpan:{timeSpan}, currentTime:{currentTime}, currentSpeed:{currentSpeed}, currentDistance:{currentDistance}");
            }
            return speedIncrements;
        }
    }
}
