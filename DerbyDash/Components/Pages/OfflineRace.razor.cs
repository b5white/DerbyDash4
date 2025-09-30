using DerbyDash.Components.Layout;
using DerbyDash.Components.Problems;
using DerbyDash.Components.Track;
using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Services;
using DerbyDash.Services.Offline;
using DerbyDash.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DerbyDash.Components.Pages {

    public partial class OfflineRace: ComponentBase, IAsyncDisposable {
        [Inject]
        public required IRaceService RaceService { get; set; }

        [Inject]
        public required IOfflineRaceTeamService RaceTeamService { get; set; }

        [Inject]
        public required NavigationManager NavManager { get; set; }

        [Inject]
        public required IConfiguration configuration { get; set; }

        [Inject]
        public required ILogger<OfflineRace> Logger { get; set; }

        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        [Inject]
        public required GameStateService GameStateService { get; set; }

        [Parameter]
        public string? ProblemClassString { get; set; }

        private bool Started = false;
        private bool Running = false;
        private bool Finished = false;
        private bool CurrentRacerFinished = false;

        private ProblemManagerBase? problems;
        private ProblemsBase? problem;
        private int speedIncrement = 1;
        private string Answer = "";
        private string PlaceholderText = "Type your answer...";
        private long starttime = 0;
        private float prevAverage = 0;
        private float improvedTime = 0;
        private double currentDistance = 0;
        private float[] previousResults = [0, 0, 0, 0, 0];
        public int currentResultIndex = 0;
        private int currentTimeIndex = 0;
        private float[] ElapsedAnswerTimes = new float[50];
        private PeriodicTimer periodicTimer = new(TimeSpan.FromMicroseconds(10000000));
        private CancellationTokenSource PeriodicTimerToken = new CancellationTokenSource();
        private Timer InactivityTimer = new Timer(1000);
        private Timer FlashTimer = new Timer(1000);
        private bool isShowingHint = false;
        private ElementReference textInput;
        private string encouragingWord = "";
        private bool ReceivedError = false;
        private RaceComponents track = new();
        private bool ShowDebug = false;
        private EditContext editContext = new EditContext(new object());

        private const int INACTIVITY_TIMER_INTERVAL = 4000; // 4 seconds
        private const int FLASH_TIMER_INTERVAL = 800; // 0.8 seconds
        private const int PERIODIC_TIMER_SPAN_MICROSECONDS = 200000; // 1/5 of a second
        private const int ANIMATION_SYNC_DELAY = 50; // milliseconds
        private const int INITIAL_TIMER_DELAY = 3000; // 3 seconds

        public ProblemsBase? ProblemClass { get; set; }
        public float RaceTime { get => track.RaceTime; set => track.RaceTime = value; }
        public float FinishTime = 0;
        private bool _inactivityTimerDisposed = false;
        private bool _flashTimerDisposed = false;

        TrackContainer? trackContainerInstance;

        // Results Popup Properties
        private bool ShowResultsPopup = false;
        private bool IsFirstPlace = false;
        private bool IsPersonalBest = false;
        private int CarsBeaten = 0;
        private Timer? ResultsPopupTimer;
        private Racer? _cachedActiveRacer;

        /// <summary>
        /// Gets the cached active racer, ensuring it's loaded once during component initialization
        /// </summary>
        private async Task<Racer?> GetCachedActiveRacerAsync() {
            if (_cachedActiveRacer == null) {
                _cachedActiveRacer = await RaceTeamService.GetActiveRacer();
            }
            return _cachedActiveRacer;
        }

        /// <summary>
        /// Handles racer changes from the service and updates the cached racer
        /// </summary>
        private async Task HandleRacerChanged() {
            try {
                // Update the cached racer to reflect the new selection
                _cachedActiveRacer = await RaceTeamService.GetActiveRacer();
                
                // Update the UI to show the new racer
                await InvokeAsync(StateHasChanged);
            } catch (Exception ex) {
                Logger.LogError(ex, "Error handling racer change in OfflineRace component");
            }
        }
        
        protected override async Task OnInitializedAsync() {
            Logger.LogInformation("OfflineRace.OnInitializedAsync()");

            // Initialize configuration and timers
            ShowDebug = configuration.GetValue<bool>("ShowDebug");
            InactivityTimer = new Timer(INACTIVITY_TIMER_INTERVAL);
            InactivityTimer.Elapsed += ShowHint;
            InactivityTimer.AutoReset = false;

            FlashTimer = new Timer(FLASH_TIMER_INTERVAL);
            FlashTimer.Elapsed += HideHint;
            FlashTimer.AutoReset = false;

            // Notify GameStateService that we're on a race page
            await GameStateService.SetCurrentRacePage($"offline-{ProblemClassString ?? "race"}");

            // Subscribe to racer changes to update the UI when racer selection changes
            RaceTeamService.OnRacerChanged += HandleRacerChanged;

            // Cache the active racer once during initialization
            _cachedActiveRacer = await RaceTeamService.EnsureActiveRacerInitializedAsync();
            
            // In offline mode, we always have a default racer, so no need to redirect
            if (_cachedActiveRacer == null) {
                Logger.LogWarning("No active racer found in offline mode. This should not happen.");
            }
        }

        private async Task Reset() {
            Logger.LogInformation("Reset (Offline Mode)");
            // Set GameStateService.SetGameRunning(false) before starting a new race (if not already false)
            await GameStateService.SetGameRunning(false);
            if (!Running) {
                RaceTime = 0;
                CurrentRacerFinished = false;
                Finished = false;
                currentDistance = 0;
                Answer = "";
                Started = true;
                Running = true;
                
                // Reset results popup state
                ShowResultsPopup = false;
                IsFirstPlace = false;
                IsPersonalBest = false;
                CarsBeaten = 0;

                // Notify GameStateService that the game is now running
                await GameStateService.SetGameRunning(true);

                CreateProblems();
                await InitializeTrack(ProblemClassString);
                ScaleRace(0);
                encouragingWord = encouragingWords[Random.Shared.Next(0, encouragingWords.Length)];
                ReceivedError = false;
                currentTimeIndex = 0;
                for (int i = 0; i < ElapsedAnswerTimes.Length; i++) {
                    ElapsedAnswerTimes[i] = 0;
                }
                problem = problems!.Next();

                // Ensure timers are recreated if they've been disposed
                if (InactivityTimer == null || _inactivityTimerDisposed) {
                    InactivityTimer = new Timer(INACTIVITY_TIMER_INTERVAL);
                    InactivityTimer.Elapsed += ShowHint;
                    InactivityTimer.AutoReset = false;
                    _inactivityTimerDisposed = false;
                }

                if (FlashTimer == null || _flashTimerDisposed) {
                    FlashTimer = new Timer(FLASH_TIMER_INTERVAL);
                    FlashTimer.Elapsed += HideHint;
                    FlashTimer.AutoReset = false;
                    _flashTimerDisposed = false;
                }

                // Start the race timers with delay
                StartRaceTimersAsync();
            }
        }

        /// <summary>
        /// Starts the race timers with a delay using fire-and-forget pattern
        /// </summary>
        private void StartRaceTimersAsync() {
            // Using FireAndForget pattern since we don't need to wait for this to complete
            UtilityMethods.FireAndForget(async () => {
                await Task.Delay(INITIAL_TIMER_DELAY);
                // Check if component is still active and timers are not disposed
                if (Running && !Finished && !_inactivityTimerDisposed) {
                    try {
                        await InvokeAsync(async () => {
                            // Double-check timer is not disposed before starting
                            if (InactivityTimer != null && !_inactivityTimerDisposed) {
                                InactivityTimer.Start();
                                starttime = DateTime.Now.Ticks;
                                await StartPeriodicTimerAsync().ConfigureAwait(false);
                            }
                        });
                    } catch (ObjectDisposedException) {
                        // Safely handle the case where the timer was disposed
                        Logger.LogInformation("Timer was disposed before it could be started (Offline Mode)");
                    }
                }
            });
        }

        private async Task StartClick() {
            // In offline mode, no authentication required
            Logger.LogInformation("Starting offline race");
            await Reset();
        }

        private void CreateProblems() {
            Logger.LogInformation("CreateProblems (Offline Mode)");
            if (String.IsNullOrEmpty(ProblemClassString)) {
                ProblemClassString = "addition-4stable";
            }
            if (problems == null) {
                problems = ProblemFactory.CreateProblemManager(ProblemClassString);
            }
        }

        public async Task OnAfter() {
            if (InactivityTimer != null && !_inactivityTimerDisposed) {
                InactivityTimer.Stop();
            }
            if (isShowingHint) {
                isShowingHint = false;
                if (FlashTimer != null && !_flashTimerDisposed) {
                    FlashTimer.Stop();
                }
                PlaceholderText = "Type your answer..."; // reset
            }
            if (problem != null) {
                if (Answer == problem.Result) {   // correct answer!
                    Answer = "";
                    await CalculateNewDistance(GetTimespan(starttime));
                    try {
                        ElapsedAnswerTimes[currentTimeIndex++] = GetTimespan(starttime);
                    } catch (IndexOutOfRangeException) {
                        LogMessage(String.Format("currentTimeIndex = {0}", currentTimeIndex));
                        currentTimeIndex = 0;
                        ReceivedError = true;
                    }
                    if (problems?.More ?? false) { // return false if problems is null
                        problem = problems.Next();
                    } else {
                        await EndRace();
                    }
                    ScaleRace(currentTimeIndex);
                    StateHasChanged();
                } else {
                    if (Answer.Length == (problem?.Length ?? 999)) {
                        Answer = "";
                    }
                }
            }
            if (Running && InactivityTimer != null && !_inactivityTimerDisposed) {
                InactivityTimer.Start();
            }
        }

        private void ScaleRace(int currentTimeIndex) {
            // Track and viewport constants
            const double VISIBLE_TRACK_LENGTH = 60.0;
            const double TOP_MARGIN = 0.0;
            const float TOP_MULTIPLIER = 7.0f;
            const float TRACK_HEIGHT = 70.0f;

            double relativePosition;

            if (track.Cars.Count > 0) {
                // Find the lead car's distance
                double leadDistance = Math.Min(track.Cars.Max(car => car.Distance), RaceService.TotalDistance);

                // Calculate the visible range
                double visibleStart = Math.Max(0, leadDistance - VISIBLE_TRACK_LENGTH);

                // Check if any car has reached the top position
                bool isAnyCarAtTop = track.Cars.Any(car => car.Top <= TOP_MARGIN * TOP_MULTIPLIER);
                track.IsAnyCarAtTop = isAnyCarAtTop;
                if (isAnyCarAtTop) {
                    // Use the fastest car's speed for animation, not just player car
                    double fastestSpeed = track.Cars.Max(car => car.Speed);
                    // Set speed class based on fastest car speed with scaling factor
                    track.UpdateSpeedClass((int)(fastestSpeed));
                }

                // Check if finish line is in view (visible)
                relativePosition = (RaceService.TotalDistance - visibleStart) / VISIBLE_TRACK_LENGTH;
                track.FinishLine.Top = (float)(TOP_MARGIN + (1 - relativePosition) * TRACK_HEIGHT) * TOP_MULTIPLIER;
                track.IsFinishLineVisible = relativePosition >= 0 && relativePosition <= 1;

                // Position all cars using the same logic for consistency
                for (int i = 0; i < track.Cars.Count; i++) {
                    var car = track.Cars[i];
                    // Calculate the target position based on distance
                    relativePosition = (car.Distance - visibleStart) / VISIBLE_TRACK_LENGTH;
                    float targetTop = (float)(TOP_MARGIN + (1 - relativePosition) * TRACK_HEIGHT) * TOP_MULTIPLIER;
                    car.Top = targetTop;
                }
            }
        }

        private void SynchronizeAnimationStart() {
            Logger.LogInformation("SynchronizeAnimationStart (Offline Mode)");
            // Reset any existing animations
            track.IsAnyCarAtTop = false;

            // Force redraw without animation
            StateHasChanged();

            // After a brief delay, enable animations in sync
            Task.Run(async () => {
                await Task.Delay(ANIMATION_SYNC_DELAY);
                await InvokeAsync(() => {
                    if (Running && !Finished) {
                        track.IsAnyCarAtTop = track.Cars.Any(car => car.Top <= 0);
                        StateHasChanged();
                    }
                });
            });
        }

        public async Task HandleKeyPress(KeyboardEventArgs e) {
            if (e.Key == "Enter") {
                if (!Running || RaceTime > 0) {
                    await StartClick();
                } else {
                    Answer = "";
                }
            }
        }

        private string RemoveHint(string answer) {
            string hint = GetHint();
            if (answer.StartsWith(hint)) {
                return answer.Substring(hint.Length);
            }
            return answer; // Return the original string if the prefix doesn't match.
        }

        private async Task EndRace() {
            Logger.LogInformation("EndRace (Offline Mode)");
            if (Running) {
                FinishTime = GetTimespan(starttime);
                track.Cars[0].TotalTime = FinishTime;
                track.Cars[0].SpeedIncrements = RaceService.CreateSpeedIncrements(ElapsedAnswerTimes);
                
                // Calculate finishing position
                int finishingPosition = CalculateFinishingPosition();
                
                if (InactivityTimer != null && !_inactivityTimerDisposed) {
                    InactivityTimer.Stop();
                }
                if (FlashTimer != null && !_flashTimerDisposed) {
                    FlashTimer.Stop();
                }
                
                // Pass finishing position to UpdateResultsAsync
                await UpdateResultsAsync(FinishTime, finishingPosition);
                
                // Show results popup immediately when user finishes
                await ShowResultsPopupAsync();
                
                Running = false;
                problems = null;
                StateHasChanged();
            }
        }

        private async Task CalculateNewDistance(double time) {
            currentDistance = 0;
            int i;

            // Apply a speed multiplier to make car movement match visual lane speed
            const double SPEED_MULTIPLIER = RaceComponents.SPEED_MULTIPLIER;

            for (i = 0; (i < ElapsedAnswerTimes.Length) && (ElapsedAnswerTimes[i] > 0); i++) {
                double RaceTime = (float)(time - ElapsedAnswerTimes[i]);
                // Apply the multiplier to the distance calculation
                currentDistance += RaceTime * speedIncrement * SPEED_MULTIPLIER;
            }

            track.Cars[0].Distance = currentDistance;
            // Keep the raw speed for counting purposes
            track.Cars[0].Speed = i * speedIncrement;

            if (currentDistance >= RaceService.TotalDistance && !CurrentRacerFinished) {
                CurrentRacerFinished = true;
                await EndRace();
            }

            bool wasCarAtTop = track.IsAnyCarAtTop;
            bool isCarAtTop = track.Cars.Any(car => car.Top <= 0);

            if (!wasCarAtTop && isCarAtTop) {
                SynchronizeAnimationStart();
            }
        }

        private bool CalculateOldDistance(double time) {
            Boolean allFinished = true;
            for (int i = 1; i < track.Cars.Count; i++) {
                double dist = track.Cars[i].CalculateCurrentDistance(time);
                if (dist < RaceService.TotalDistance) {
                    allFinished = false;
                } else if (track.Cars[i].TotalTime == 0) {
                    track.Cars[i].TotalTime = time;
                }
            }

            if (allFinished && CurrentRacerFinished && !Finished) {
                Finished = true;
                Running = false;
                StopPeriodicTimer();
            }

            return allFinished;
        }

        /// <summary>
        /// Calculates the finishing position of the player's car based on when they finished relative to other cars
        /// </summary>
        /// <returns>The finishing position (1 = first place, 2 = second place, etc.)</returns>
        private int CalculateFinishingPosition() {
            int position = 1;
            
            // Count how many cars finished before the player
            foreach (var car in track.Cars.Skip(1)) { // Skip player car (index 0)
                if (car.TotalTime > 0 && car.TotalTime < FinishTime) {
                    position++;
                }
            }
            
            return position;
        }

        private async Task UpdateResultsAsync(float timeSpan, int finishingPosition) {
            try {
                ResetResults(timeSpan);
                CalculateAverage();
                // Save the race with finishing position (offline mode)
                await RaceTeamService.SaveRaceCompletionAsync(timeSpan, ProblemClassString!, track.Cars[0].SpeedIncrements, finishingPosition);
                await RaceTeamService.SaveLastPlayedRaceAsync(ProblemClassString!);
            } catch (Exception ex) {
                LogMessage(ex);
            }
        }

        /// <summary>
        /// Shows the results popup with race statistics
        /// </summary>
        private async Task ShowResultsPopupAsync() {
            // Calculate statistics for the popup
            CalculateResultsStatistics();
            
            // Show the popup
            ShowResultsPopup = true;
            StateHasChanged();
            
            // Start timer to hide popup after all racers finish (with 2 second delay)
            await SetupResultsPopupTimerAsync();
        }

        /// <summary>
        /// Calculates statistics for the results popup
        /// </summary>
        private void CalculateResultsStatistics() {
            // Check if this is first place (fastest time among all cars)
            var allFinishedTimes = track.Cars
                .Where(car => car.TotalTime > 0)
                .OrderBy(car => car.TotalTime)
                .ToList();
            
            IsFirstPlace = allFinishedTimes.FirstOrDefault()?.TotalTime == track.Cars[0].TotalTime;
            
            // Count how many previous top 5 times were beaten
            CarsBeaten = 0;
            for (int i = 0; i < previousResults.Length; i++) {
                if (previousResults[i] > 0 && FinishTime < previousResults[i]) {
                    CarsBeaten++;
                }
            }
            
            // Check if this is a personal best (improvement over previous average)
            IsPersonalBest = improvedTime > 0;
        }

        /// <summary>
        /// Sets up timer to hide results popup after all racers finish
        /// </summary>
        private async Task SetupResultsPopupTimerAsync() {
            // Use a background task to monitor when all racers finish
            UtilityMethods.FireAndForget(async () => {
                // Wait for all racers to finish
                while (Running || !AllRacersFinished()) {
                    await Task.Delay(100);
                }
                
                // Wait additional 2 seconds after all racers finish
                await Task.Delay(2000);
                
                // Hide the popup
                await InvokeAsync(() => {
                    ShowResultsPopup = false;
                    StateHasChanged();
                });
            });
        }

        /// <summary>
        /// Checks if all racers have finished the race
        /// </summary>
        private bool AllRacersFinished() {
            return track.Cars.All(car => car.TotalTime > 0 || car.Distance >= RaceService.TotalDistance);
        }

        /// <summary>
        /// Gets ordinal number string (1st, 2nd, 3rd, etc.)
        /// </summary>
        private string GetOrdinalNumber(int number) {
            if (number <= 0) return number.ToString();
            
            switch (number % 100) {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }
            
            switch (number % 10) {
                case 1:
                    return number + "st";
                case 2:
                    return number + "nd";
                case 3:
                    return number + "rd";
                default:
                    return number + "th";
            }
        }

        private void ResetResults(float timeSpan) {
            Logger.LogInformation("ResetResults (Offline Mode)");
            List<Car> previousRaces = track.Cars
                .Where(car => car.TotalTime > 0)
                .OrderBy(car => car.TotalTime)  // take the 5 fastest
                .Take(5)
                .ToList();

            currentResultIndex = -1;
            for (int i = previousRaces.Count - 1; i >= 0; i--) {
                previousResults[i] = (float)previousRaces[i].TotalTime;
                if (UtilityMethods.AreDoublesEqual(previousResults[i], timeSpan, 0.000001)) {
                    currentResultIndex = i;
                }
            }
        }

        private void GoBack() {
            StopPeriodicTimer();
            NavManager.NavigateTo("javascript:history.back()");
        }

        private async Task ClearRaces() {
            await RaceService.DeleteRaces(ProblemClassString ?? "");
            FinishTime = 0;
            prevAverage = 0;
            improvedTime = 0;
            previousResults = [0, 0, 0, 0, 0];
            StateHasChanged();
        }

        private void CalculateAverage() {
            Logger.LogInformation("CalculateAverage (Offline Mode)");
            int count = 0;
            float total = 0;

            for (int i = 0; i < previousResults.Length; i++) {
                if (previousResults[i] > 0) {
                    count++;
                    total += previousResults[i];
                }
            }
            float average = total / count;
            if (prevAverage > 0) {
                improvedTime = (average < prevAverage) ? (prevAverage - average) : 0;
            }
            Logger.LogInformation("ave: {average} prev: {prevAverage} improv {improvedTime} (Offline Mode)", average, prevAverage, improvedTime);
            prevAverage = average;
        }

        public async Task InitializeTrack(string? problemSetIdentifier) {
            Logger.LogInformation("InitializeTrack (Offline Mode)");
            if (string.IsNullOrEmpty(problemSetIdentifier)) {
                throw new Exception("problemSetIdentifier is empty or null.");
            }
            try {
                // Get the current racer from the offline race team service
                Racer? currentRacer = await RaceTeamService.GetActiveRacer();
                if (currentRacer == null) {
                    Logger.LogWarning("No active racer found in offline mode. This should not happen.");
                    return;
                }

                // Create the track with the current racer
                track = await RaceService.CreateTrack(problemSetIdentifier);
            } catch (Exception ex) {
                LogMessage(ex);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            try {
                // Try to focus the text input if it exists
                await textInput.FocusAsync();
            } catch (Exception) {
                // Ignore focus errors
            }
        }

        public async Task OnAfterIgnore() {
            try {
                await textInput.FocusAsync();
            } catch (Exception) {
            }
        }

        private void ShowHint(object? sender, ElapsedEventArgs e) {
            InvokeAsync(() => {
                isShowingHint = true;
                PlaceholderText = GetHint();   // prompt them with the correct answer
                StateHasChanged();
                if (FlashTimer != null && !_flashTimerDisposed) {
                    FlashTimer.Start();
                }
            });
        }

        private void HideHint(object? sender, ElapsedEventArgs e) {
            InvokeAsync(() => {
                isShowingHint = false;
                PlaceholderText = "Type your answer...";
                StateHasChanged();
                if (FlashTimer != null && !_flashTimerDisposed) {
                    FlashTimer.Stop();
                }
                if (InactivityTimer != null && !_inactivityTimerDisposed) {
                    InactivityTimer.Start();
                }
            });
        }

        string GetHint() {
            if (problem != null) {
                return "      " + problem.Result;
            } else {
                return "";
            }
        }

        static string prevName = "";
        static string prevTitle = "";

        public string GetTitle(string? problemTypeName) {
            string result = "";
            if (String.IsNullOrEmpty(problemTypeName)) {
                problemTypeName = "addition-4stable";
            }
            if (problems == null) {
                if (problemTypeName != prevName) {
                    result = ProblemFactory.GetTitle(problemTypeName);
                } else {
                    result = prevTitle;
                }
            } else {
                result = problems.First().Title;
            }
            prevName = problemTypeName;
            prevTitle = result;
            return result;
        }

        private async Task StartPeriodicTimerAsync() {
            Logger.LogInformation("StartPeriodicTimerAsync (Offline Mode)");
            // Create a new CancellationTokenSource each time the timer is started
            PeriodicTimerToken = new CancellationTokenSource();
            periodicTimer = new(TimeSpan.FromMicroseconds(PERIODIC_TIMER_SPAN_MICROSECONDS));

            try {
                while (await periodicTimer.WaitForNextTickAsync(PeriodicTimerToken.Token)) {
                    RaceTime = GetSpan(starttime);
                    await CalculateNewDistance(RaceTime);
                    CalculateOldDistance(RaceTime);
                    ScaleRace(currentTimeIndex);
                    await InvokeAsync(StateHasChanged);
                }
            } catch (OperationCanceledException) {
            }
        }

        private void StopPeriodicTimer() {
            Logger.LogInformation("StopPeriodicTimer (Offline Mode)");
            // Cancel the token and dispose of the timer
            PeriodicTimerToken.Cancel();
            periodicTimer.Dispose();
        }

        private float GetSpan(double starttime) {
            return (float)(DateTime.Now.Ticks - starttime) / TimeSpan.TicksPerSecond;
        }

        private float GetTimespan(double starttime) {
            return (float)(DateTime.Now.Ticks - starttime) / TimeSpan.TicksPerSecond;
        }

        public void LogMessage(Exception E, string message = "") {
            Logger.LogError(E, message);
        }

        public void LogMessage(string message) {
            Logger.LogWarning(message);
        }

        public async ValueTask DisposeAsync() {
            // Reset game state when component is disposed
            await GameStateService.SetGameRunning(false);
            await GameStateService.SetCurrentRacePage("");

            // Unsubscribe from racer changes
            RaceTeamService.OnRacerChanged -= HandleRacerChanged;

            periodicTimer.Dispose();

            if (InactivityTimer != null) {
                InactivityTimer.Dispose();
                _inactivityTimerDisposed = true;
            }

            if (FlashTimer != null) {
                FlashTimer.Dispose();
                _flashTimerDisposed = true;
            }

            if (ResultsPopupTimer != null) {
                ResultsPopupTimer.Dispose();
            }
        }

        string[] encouragingWords = new string[] {
            //Encouragement and Praise for Effort:
            "Great job practicing!",
            "You did it!",
            "Keep up the great work!",
            "Your practice is paying off!",
            "You're doing fantastic!",
            "Nice work in practice mode!",
            "Fantastic effort!",
            "You're improving!",
            "Great perseverance!",
            "You're getting better!",
            "Excellent practice session!",
            "You're doing great!",
            "Keep practicing!",
            "Nice job!",
            "You're on fire!",
            "Great focus!",

            //Recognition of Improvement:
            "You're getting faster!",
            "I can see improvement!",
            "You are really improving!",
            "You're mastering these problems!",
            "You are becoming a math whiz!",
            "You're getting the hang of this!",
            "Great progress in practice!",
            "You're getting better with every try!",
            "You're really shining!",

            //Motivational and Positive Reinforcement:
            "I love how you keep trying!",
            "Wow, look at you go!",
            "Like a boss.",
            "Practice makes perfect!",
            "You're crushing those problems!",
            "Slicing through those problems!",
            "You tackled those like a pro!",
            "Your dedication shows!",
            "Solving those problems - like a boss.",
            "Practice strengthens your brain!",
            "Sailing through those problems!",

            // Practice Mode Specific
            "Great practice session!",
            "Keep up the practice!",
            "Practice mode champion!"
        };
    }
}
