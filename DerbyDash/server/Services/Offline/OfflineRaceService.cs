using DerbyDash.Track;
using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Utilities;

namespace DerbyDash.Services.Offline {
    public class OfflineRaceService : IRaceService {
        private readonly IOfflineRaceTeamService _raceTeamService;
        private RaceComponents track = new();
        private readonly ILogger<OfflineRaceService> Logger;
        public float TotalDistance { get; } = 400; // Increased from 200 to make races longer
        private Random random = new Random();
        private readonly List<Race> _offlineRaces = new();
        private int _nextRaceId = 1;

        public OfflineRaceService(
            ILogger<OfflineRaceService> logger,
            IOfflineRaceTeamService raceTeamService) {
            Logger = logger;
            _raceTeamService = raceTeamService;
        }

        public async Task<RaceComponents> CreateTrack(string problemSetIdentifier) {
            Logger.LogInformation("CreateTrack (Offline Mode)");
            
            // Get active racer from offline service
            var activeRacer = await _raceTeamService.GetActiveRacer();
            if (activeRacer == null) {
                throw new MissingRacerException("No active racer selected. Please select a racer from your race team.");
            }
            
            return await CreateTrack(activeRacer.Id, problemSetIdentifier);
        }

        private async Task<RaceComponents> CreateTrack(int racerId, string problemSet) {
            Logger.LogInformation("CreateTrack (Offline Mode)");
            const int CAR_GAP = 30;
            const int MAX_CARS = 6;

            int problemSetId = UtilityMethods.GetUniqueIntFromString(problemSet);
            
            try {
                // Get previous races from offline storage
                List<Race> previousRaces = _offlineRaces
                    .Where(r => r.RacerId == racerId && r.ProblemSetId == problemSetId)
                    .OrderByDescending(r => r.RaceDateTime)
                    .Take(5)
                    .ToList();

                // Generate a random ImageId for the car at index 0
                int randomImageId = random.Next(1, 11);

                // Initialize the cars list
                List<Car> Cars = new List<Car> {
                    new Car { index = 0, ImageId = randomImageId, RaceDateTime = DateTime.Now, Top = 9999  },
                    new Car { index = 1, ImageId = 2, Top = 9999  },
                    new Car { index = 2, ImageId = 3, Top = 9999  },
                    new Car { index = 3, ImageId = 4, Top = 9999  },
                };

                // Assign the previous races to cars 1 to 5
                for (int i = 1; i <= previousRaces.Count; i++) {
                    Race race = previousRaces[i - 1];
                    Car car = new();
                    car.ImageId = race.ImageId;
                    car.RaceId = race.Id;
                    car.TotalTime = race.TotalTime;
                    car.RaceDateTime = race.RaceDateTime;
                    car.SpeedIncrements.AddRange(race.SpeedIncrements ?? Enumerable.Empty<SpeedIncrement>());
                    car.ResetFlexBasis(MAX_CARS, CAR_GAP);
                    car.Top = 9999;
                    Cars.Add(car);
                }

                // Initialize all cars
                for (int i = 0; i < Cars.Count; i++) {
                    int safeIndex = Math.Min(i, 4);
                    Cars[i].InitializeFastEddyTimeIncrements(random, safeIndex);
                    Cars[i].ResetFlexBasis(MAX_CARS, CAR_GAP);
                }
                
                // Set the track properties
                track.Cars = Cars;
                track.ProblemId = problemSetId;
                track.RacerId = racerId;

                // Initialize lines off-screen
                track.StartLine = new RaceComponent { Top = 490f, ImageUrl = "StartLine.png" };
                track.FinishLine = new RaceComponent { Top = 9999f, ImageUrl = "FinishLine.png" };
                
                await Task.CompletedTask;
            } catch (Exception ex) {
                Logger.LogError(ex, "CreateTrack (Offline Mode)");
            }
            return track;
        }

        public async Task SaveRaceAsync(Car car, int racerId, int problemId) {
            Logger.LogInformation("SaveRaceAsync (Offline Mode)");

            Race race = CreateNewRace(car, racerId, problemId);
            race.Id = _nextRaceId++;
            _offlineRaces.Add(race);
            
            car.RaceId = race.Id;

            var activeRacer = await _raceTeamService.GetActiveRacer();
            if (activeRacer != null) {
                activeRacer.LastRaced = DateOnly.FromDateTime(DateTime.Now);
                await _raceTeamService.UpdateRacer(activeRacer);
            }
        }

        private Race CreateNewRace(Car car, int racerId, int problemId) {
            Logger.LogInformation("CreateNewRace (Offline Mode)");
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

        public async Task DeleteRaces(string problemSet) {
            Logger.LogInformation($"DeleteRaces (Offline Mode) for current racer, {problemSet}");
            try {
                int problemSetId = UtilityMethods.GetUniqueIntFromString(problemSet);
                var activeRacer = await _raceTeamService.GetActiveRacer();
                int racerId = activeRacer?.Id ?? 0;
                
                if (racerId != 0) {
                    var racesToDelete = _offlineRaces
                        .Where(r => r.RacerId == racerId && r.ProblemSetId == problemSetId)
                        .ToList();
                    
                    foreach (var race in racesToDelete) {
                        _offlineRaces.Remove(race);
                    }
                    
                    Logger.LogInformation($"Deleted {racesToDelete.Count} races from offline storage");
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "DeleteRaces (Offline Mode)");
            }
        }

        public List<SpeedIncrement> CreateSpeedIncrements(float[] Times) {
            Logger.LogInformation("CreateSpeedIncrements (Offline Mode)");
            float currentTime = 0;
            double currentSpeed = 0;
            double currentDistance = 0;

            List<SpeedIncrement> speedIncrements = new List<SpeedIncrement>();
            for (int i = 0; i < Times.Length; i++) {
                double timeSpan = Times[i];
                if (timeSpan == 0) {
                    break;
                }
                if (i == 0) {
                    currentDistance = 0;
                } else {
                    currentDistance += currentSpeed * (timeSpan - Times[i - 1]);
                }
                currentSpeed++;
                speedIncrements.Add(new SpeedIncrement {
                    Time = timeSpan,
                    Speed = currentSpeed,
                    Distance = currentDistance * RaceComponents.SPEED_MULTIPLIER
                });
            }
            return speedIncrements;
        }

        public async Task<Race> SaveRaceAsync(Race race) {
            Logger.LogInformation("SaveRaceAsync (Offline Mode) - API version");
            
            race.Id = _nextRaceId++;
            _offlineRaces.Add(race);
            
            var activeRacer = await _raceTeamService.GetActiveRacer();
            if (activeRacer != null) {
                activeRacer.LastRaced = DateOnly.FromDateTime(DateTime.Now);
                await _raceTeamService.UpdateRacer(activeRacer);
            }
            
            return race;
        }

        public async Task<List<Race>> GetRaceHistoryAsync(int page, int pageSize, int? racerId = null) {
            Logger.LogInformation("GetRaceHistoryAsync (Offline Mode)");
            
            var query = _offlineRaces.AsQueryable();
                
            if (racerId.HasValue) {
                query = query.Where(r => r.RacerId == racerId.Value);
            }
            
            var result = query
                .OrderByDescending(r => r.RaceDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
                
            await Task.CompletedTask;
            return result;
        }

        public async Task<int> GetRaceCountAsync(int? racerId = null) {
            Logger.LogInformation("GetRaceCountAsync (Offline Mode)");
            
            var query = _offlineRaces.AsQueryable();
            
            if (racerId.HasValue) {
                query = query.Where(r => r.RacerId == racerId.Value);
            }
            
            await Task.CompletedTask;
            return query.Count();
        }

        public async Task<Race?> GetRaceByIdAsync(int id) {
            Logger.LogInformation("GetRaceByIdAsync (Offline Mode)");
            
            await Task.CompletedTask;
            return _offlineRaces.FirstOrDefault(r => r.Id == id);
        }

        public async Task<object> GetRaceStatsAsync(int? racerId = null) {
            Logger.LogInformation("GetRaceStatsAsync (Offline Mode)");
            
            var query = _offlineRaces.AsQueryable();
            
            if (racerId.HasValue) {
                query = query.Where(r => r.RacerId == racerId.Value);
            }
            
            var races = query.ToList();
            
            if (!races.Any()) {
                await Task.CompletedTask;
                return new {
                    TotalRaces = 0,
                    AverageTime = 0.0,
                    BestTime = 0.0,
                    FirstPlaceFinishes = 0,
                    LastRaceDate = (DateTime?)null
                };
            }
            
            await Task.CompletedTask;
            return new {
                TotalRaces = races.Count,
                AverageTime = races.Average(r => r.TotalTime),
                BestTime = races.Min(r => r.TotalTime),
                FirstPlaceFinishes = races.Count(r => r.FinishingPosition == 1),
                LastRaceDate = races.Max(r => r.RaceDateTime)
            };
        }

        public async Task DeleteRaceAsync(int id) {
            Logger.LogInformation("DeleteRaceAsync (Offline Mode)");
            
            var race = _offlineRaces.FirstOrDefault(r => r.Id == id);
            if (race != null) {
                _offlineRaces.Remove(race);
            }
            
            await Task.CompletedTask;
        }
    }
}
