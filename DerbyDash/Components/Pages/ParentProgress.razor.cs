using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RaceData = DerbyDash.Data.Race;

namespace DerbyDash.Components.Pages {
    public partial class ParentProgress : ComponentBase {
        [Inject] private IRaceTeamService RaceTeamService { get; set; } = default!;
        [Inject] private IDbContextFactory<ApplicationDbContext> ContextFactory { get; set; } = default!;
        [Inject] private ILogger<ParentProgress> Logger { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private List<Racer> Racers { get; set; } = new();
        private Dictionary<int, RacerStatistics> RacerStats { get; set; } = new();
        private Dictionary<int, List<RaceData>> RecentRaces { get; set; } = new();
        
        private int TotalTeamRaces { get; set; }
        private int TotalFirstPlaceFinishes { get; set; }
        private double TeamAverageTime { get; set; }
        
        private bool isLoading = true;

        protected override async Task OnInitializedAsync() {
            try {
                await LoadData();
            } catch (Exception ex) {
                Logger.LogError(ex, "Error loading parent progress data");
            } finally {
                isLoading = false;
            }
        }

        private async Task LoadData() {
            // Load all racers
            Racers = await RaceTeamService.GetRacers(includeCount: true);
            
            if (!Racers.Any()) {
                return;
            }

            using var context = ContextFactory.CreateDbContext();
            
            // Calculate statistics for each racer
            foreach (var racer in Racers) {
                var races = await context.Races
                    .Where(r => r.RacerId == racer.Id)
                    .OrderByDescending(r => r.RaceDateTime)
                    .ToListAsync();

                if (races.Any()) {
                    var stats = new RacerStatistics {
                        TotalRaces = races.Count,
                        BestTime = races.Min(r => r.TotalTime),
                        AverageTime = races.Average(r => r.TotalTime),
                        FirstPlaceFinishes = races.Count(r => r.FinishingPosition == 1),
                        LastRaceDate = races.Max(r => r.RaceDateTime)
                    };
                    
                    // Calculate win percentage
                    stats.WinPercentage = stats.TotalRaces > 0 
                        ? (stats.FirstPlaceFinishes * 100.0 / stats.TotalRaces) 
                        : 0;
                    
                    RacerStats[racer.Id] = stats;
                    
                    // Store recent races (top 5)
                    RecentRaces[racer.Id] = races.Take(5).ToList();
                }
            }

            // Calculate team statistics
            TotalTeamRaces = RacerStats.Values.Sum(s => s.TotalRaces);
            TotalFirstPlaceFinishes = RacerStats.Values.Sum(s => s.FirstPlaceFinishes);
            TeamAverageTime = RacerStats.Values.Any() 
                ? RacerStats.Values.Average(s => s.AverageTime) 
                : 0;
        }

        private string GetProblemSetName(int problemSetId) {
            // Map problem set IDs to friendly names
            return problemSetId switch {
                1 => "Addition Tables",
                2 => "Subtraction Tables",
                3 => "Multiplication Tables",
                4 => "Division Tables",
                5 => "Addition Mixed",
                6 => "Subtraction Mixed",
                7 => "Multiplication Mixed",
                8 => "Division Mixed",
                _ => "Math Race"
            };
        }

        private string GetOrdinalSuffix(int number) {
            if (number <= 0) return "";
            
            int lastDigit = number % 10;
            int lastTwoDigits = number % 100;

            if (lastTwoDigits >= 11 && lastTwoDigits <= 13) {
                return "th";
            }

            return lastDigit switch {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

        public class RacerStatistics {
            public int TotalRaces { get; set; }
            public double BestTime { get; set; }
            public double AverageTime { get; set; }
            public int FirstPlaceFinishes { get; set; }
            public DateTime LastRaceDate { get; set; }
            public double WinPercentage { get; set; }
        }
    }
}

