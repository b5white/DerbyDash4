using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Models;
using DerbyDash.Track;
using DerbyDash.Utilities;
using Microsoft.Extensions.Logging;

namespace DerbyDash.Services {
    /// <summary>
    /// Client-side adapter that implements IRaceService
    /// Handles client-side track creation and API calls for race management
    /// </summary>
    public class ClientRaceService : IRaceService {
        private readonly IRaceTeamService _raceTeamService;
        private readonly ApiRaceService _apiRaceService;
        private readonly ILogger<ClientRaceService> _logger;
        private RaceComponents track = new();
        public float TotalDistance { get; } = 400; // Increased from 200 to make races longer
        private Random random = new Random();
        private Racer? activeRacer;

        public ClientRaceService(
            ILogger<ClientRaceService> logger,
            IRaceTeamService raceTeamService,
            ApiRaceService apiRaceService) {
            _logger = logger;
            _raceTeamService = raceTeamService;
            _apiRaceService = apiRaceService;
        }

        public async Task<RaceComponents> CreateTrack(string problemSetIdentifier) {
            _logger.LogInformation("CreateTrack");
            // Use cached active racer or ensure it's initialized
            activeRacer = await _raceTeamService.GetActiveRacer();
            if (activeRacer == null) {
                throw new MissingRacerException("No active racer selected. Please select a racer from your race team.");
            }
            return await CreateTrack(activeRacer.Id, problemSetIdentifier);
        }

        private async Task<RaceComponents> CreateTrack(int racerId, string problemSet) {
            _logger.LogInformation("CreateTrack for racer {RacerId}, problem set {ProblemSet}", racerId, problemSet);
            const int CAR_GAP = 30;
            const int MAX_CARS = 6;

            int problemSetId = UtilityMethods.GetUniqueIntFromString(problemSet);
            
            // Retrieve the 5 previous races for the given RacerID from the API
            List<Race> previousRaces = new List<Race>();
            try {
                var raceHistory = await _apiRaceService.GetRaceHistoryAsync(page: 1, pageSize: 5, racerId: racerId);
                
                // Filter by problem set ID and convert DTOs to Race models
                foreach (var raceDto in raceHistory.Items) {
                    if (raceDto.ProblemSetId == problemSetId) {
                        previousRaces.Add(ConvertToRace(raceDto));
                    }
                }
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Error fetching previous races, continuing without them");
            }

            // Generate a random ImageId for the car at index 0
            int randomImageId = random.Next(1, 11); // Assuming you have 10 car images available

            // Initialize the cars list
            List<Car> Cars = new List<Car> {
                new Car { index = 0, ImageId = randomImageId, RaceDateTime = DateTime.Now, Top = 9999  },
                new Car { index = 1, ImageId = 2, Top = 9999  },
                new Car { index = 2, ImageId = 3, Top = 9999  },
                new Car { index = 3, ImageId = 4, Top = 9999  },
            };

            // Assign the previous races to cars 1 to 5
            for (int i = 1; i <= previousRaces.Count && i < Cars.Count; i++) {
                Race race = previousRaces[i - 1];
                Car car = Cars[i];
                car.ImageId = race.ImageId;
                car.RaceId = race.Id;
                car.TotalTime = race.TotalTime;
                car.RaceDateTime = race.RaceDateTime;
                if (race.SpeedIncrements != null) {
                    car.SpeedIncrements.AddRange(race.SpeedIncrements);
                }
                car.ResetFlexBasis(MAX_CARS, CAR_GAP);
                car.Top = 9999; // Initialize the top position off-screen
            }

            // Start from index 0 to initialize all cars
            for (int i = 0; i < Cars.Count; i++) {
                // Make sure we don't exceed the array bounds in the Car class
                int safeIndex = Math.Min(i, 4); // The times/distances arrays have 5 rows (0-4)
                
                // Only initialize FastEddy if car doesn't have a previous race (RaceId == 0)
                // Cars with previous races already have their speed increments set
                if (Cars[i].RaceId == 0) {
                    Cars[i].InitializeFastEddyTimeIncrements(random, safeIndex);
                }
                Cars[i].ResetFlexBasis(MAX_CARS, CAR_GAP);
            }
            
            // Set the track properties
            track.Cars = Cars;
            track.ProblemId = problemSetId;
            track.RacerId = racerId;

            // Initialize lines off-screen
            track.StartLine = new RaceComponent { Top = 490f, ImageUrl = "StartLine.png" };
            track.FinishLine = new RaceComponent { Top = 9999f, ImageUrl = "FinishLine.png" };
            
            return track;
        }

        public async Task SaveRaceAsync(Car car, int racerId, int problemId) {
            _logger.LogInformation("SaveRaceAsync (legacy method - use SaveRaceAsync(Race) instead)");
            // This method is for backward compatibility but should use the API version
            throw new NotImplementedException("Use SaveRaceAsync(Race) instead");
        }

        public async Task<Race> SaveRaceAsync(Race race) {
            _logger.LogInformation("SaveRaceAsync (API version)");
            
            // Convert Race to DTO and save via API
            // Note: This requires problemClassString, which we don't have from Race alone
            // For now, we'll need to track the problemClassString separately or derive it
            throw new NotImplementedException("Use IRaceTeamService.SaveRaceCompletionAsync instead");
        }

        public async Task DeleteRaces(string problemSet) {
            _logger.LogInformation($"DeleteRaces for current racer, {problemSet}");
            try {
                int problemSetId = UtilityMethods.GetUniqueIntFromString(problemSet);
                int racerId = (await _raceTeamService.GetActiveRacer())?.Id ?? 0;
                if (racerId != 0) {
                    // Get all races for this racer and problem set
                    var raceHistory = await _apiRaceService.GetRaceHistoryAsync(page: 1, pageSize: 1000, racerId: racerId);
                    var racesToDelete = raceHistory.Items.Where(r => r.ProblemSetId == problemSetId);
                    
                    foreach (var race in racesToDelete) {
                        await _apiRaceService.DeleteRaceAsync(race.Id);
                    }
                    _logger.LogInformation("Deleted {Count} races", racesToDelete.Count());
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "DeleteRaces");
            }
        }

        public List<SpeedIncrement> CreateSpeedIncrements(float[] Times) {
            _logger.LogInformation("CreateSpeedIncrements");
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

        public async Task<List<Race>> GetRaceHistoryAsync(int page, int pageSize, int? racerId = null) {
            _logger.LogInformation("GetRaceHistoryAsync");
            
            var response = await _apiRaceService.GetRaceHistoryAsync(page, pageSize, racerId);
            return response.Items.Select(ConvertToRace).ToList();
        }

        public async Task<int> GetRaceCountAsync(int? racerId = null) {
            _logger.LogInformation("GetRaceCountAsync");
            
            // Get first page to get total count
            var response = await _apiRaceService.GetRaceHistoryAsync(page: 1, pageSize: 1, racerId);
            return response.TotalCount;
        }

        public async Task<Race?> GetRaceByIdAsync(int id) {
            _logger.LogInformation("GetRaceByIdAsync");
            
            var raceDto = await _apiRaceService.GetRaceAsync(id);
            return raceDto != null ? ConvertToRace(raceDto) : null;
        }

        public async Task<object> GetRaceStatsAsync(int? racerId = null) {
            _logger.LogInformation("GetRaceStatsAsync");
            
            // Get all races for stats (we might need to implement a stats endpoint in the API)
            var response = await _apiRaceService.GetRaceHistoryAsync(page: 1, pageSize: 1000, racerId);
            var races = response.Items.Select(ConvertToRace).ToList();
            
            if (!races.Any()) {
                return new {
                    TotalRaces = 0,
                    AverageTime = 0.0,
                    BestTime = 0.0,
                    FirstPlaceFinishes = 0,
                    LastRaceDate = (DateTime?)null
                };
            }
            
            return new {
                TotalRaces = races.Count,
                AverageTime = races.Average(r => r.TotalTime),
                BestTime = races.Min(r => r.TotalTime),
                FirstPlaceFinishes = races.Count(r => r.FinishingPosition == 1),
                LastRaceDate = races.Max(r => r.RaceDateTime)
            };
        }

        public async Task DeleteRaceAsync(int id) {
            _logger.LogInformation("DeleteRaceAsync");
            await _apiRaceService.DeleteRaceAsync(id);
        }

        private Race ConvertToRace(RaceDto dto) {
            var speedIncrements = dto.SpeedIncrements?.Select(si => new SpeedIncrement {
                Id = si.Id,
                RaceId = si.RaceId,
                Time = si.Time,
                Speed = si.Speed,
                Distance = si.Distance
            }).ToList() ?? new List<SpeedIncrement>();
            
            return new Race {
                Id = dto.Id,
                RacerId = dto.RacerId,
                RaceDateTime = dto.RaceDateTime,
                TotalTime = dto.TotalTime,
                ProblemSetId = dto.ProblemSetId,
                ImageId = dto.ImageId,
                FinishingPosition = dto.FinishingPosition,
                SpeedIncrements = speedIncrements
            };
        }
    }
}
