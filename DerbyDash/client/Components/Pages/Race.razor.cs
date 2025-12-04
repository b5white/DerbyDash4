using DerbyDash.Components.Layout;
using DerbyDash.Components.Problems;
using DerbyDash.Track;
using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Services;
using DerbyDash.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DerbyDash.Components.Pages {

    public partial class Race: ComponentBase, IAsyncDisposable {
        [Inject]
        public required IRaceService RaceService { get; set; }

        [Inject]
        public required IRaceTeamService RaceTeamService { get; set; }

        [Inject]
        public required NavigationManager NavManager { get; set; }

        [Inject]
        public required IConfiguration configuration { get; set; }

        [Inject]
        public required ILogger<Race> Logger { get; set; }

        [Inject]
        public required IJSRuntime JSRuntime { get; set; }

        [Inject]
        public required IUserService UserService { get; set; }

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
                Logger.LogError(ex, "Error handling racer change in Race component");
            }
        }
        protected override async Task OnInitializedAsync() {
            Logger.LogInformation("Race.OnInitializedAsync()");

            // Initialize configuration and timers
            ShowDebug = configuration.GetValue<bool>("ShowDebug");
            InactivityTimer = new Timer(INACTIVITY_TIMER_INTERVAL);
            InactivityTimer.Elapsed += ShowHint;
            InactivityTimer.AutoReset = false;

            FlashTimer = new Timer(FLASH_TIMER_INTERVAL);
            FlashTimer.Elapsed += HideHint;
            FlashTimer.AutoReset = false;

            // Notify GameStateService that we're on a race page
            await GameStateService.SetCurrentRacePage(ProblemClassString ?? "race");

            // Subscribe to racer changes to update the UI when racer selection changes
            RaceTeamService.OnRacerChanged += HandleRacerChanged;

            // Cache the active racer once during initialization
            _cachedActiveRacer = await RaceTeamService.EnsureActiveRacerInitializedAsync();
            
            // ENFORCE: Must have a chosen racer to race
            if (_cachedActiveRacer == null) {
                Logger.LogWarning("No active racer found. Redirecting to RaceTeam page.");
                NavManager.NavigateTo("/Account/Manage/RaceTeam", true);
                return;
            }
        }

        // OnAfterRenderAsync is defined later in the file

        private async Task Reset() {
            Logger.LogInformation("Reset");
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
                        Logger.LogInformation("Timer was disposed before it could be started");
                    }
                }
            });
        }

        private async Task StartClick() {
            // Check if the user is logged in
            bool isAuthenticated = await UserService.IsLoggedInAsync();

            if (!isAuthenticated) {
                // User is not logged in, redirect to login page with return URL
                NavManager.NavigateTo($"/Account/Login?returnUrl={Uri.EscapeDataString(NavManager.Uri)}", true);
                return;
            }

            // Refresh the active racer cookie if there is an active racer
            try {
                Racer? activeRacer = await RaceTeamService.GetActiveRacer();
                if (activeRacer != null) {
                    await RaceTeamService.SetActiveRacer(activeRacer); // This will refresh the cookie
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error refreshing active racer cookie");
            }

            await Reset();
        }

        private void CreateProblems() {
            Logger.LogInformation("CreateProblems");
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
                    //         CalculateFlexBasis(6, 10, Margin++);
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

                    // Apply special handling for the current player car only when inactive
                    // if (i == 0 && car.Speed <= 0 && isAnyCarAtTop && Running && !Finished) {
                    //     // Calculate time since last answer for the active player
                    //     float timeSinceLastAnswer = 0;

                    //     if (currentTimeIndex > 0 && starttime > 0) {
                    //         timeSinceLastAnswer = GetSpan(starttime) - ElapsedAnswerTimes[currentTimeIndex - 1];
                    //     }

                    //     // Apply fall-behind effect only for the active player when they're inactive
                    //     float fallBehindFactor = Math.Min(1.0f, timeSinceLastAnswer / FALL_BEHIND_TIME_THRESHOLD);
                    //     car.Top = INITIAL_START_LINE_TOP * fallBehindFactor + targetTop * (1 - fallBehindFactor);
                    // }
                }
            }
        }

        private void SynchronizeAnimationStart() {
            Logger.LogInformation("SynchronizeAnimationStart");
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
            Logger.LogInformation("EndRace");
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
                // Save the race with finishing position
                await RaceTeamService.SaveRaceCompletionAsync(timeSpan, ProblemClassString!, track.Cars[0].SpeedIncrements, finishingPosition);
                await RaceTeamService.SaveLastPlayedRaceAsync(ProblemClassString!);
            } catch (Exception ex) {
                LogMessage(ex);
            }
        }

        /// <summary>
        /// Maps problem class string to problem set ID
        /// </summary>
        private int GetProblemSetId(string problemClassString) {
            return problemClassString switch {
                "addition-4stable" => 1,
                "subtraction-4stable" => 2,
                "multiplication-4stable" => 3,
                "division-4stable" => 4,
                "addition-100" => 5,
                "subtraction-100" => 6,
                "multiplication-100" => 7,
                "division-100" => 8,
                _ => 1 // Default to addition-4stable
            };
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
            Logger.LogInformation("ResetResults");
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
            Logger.LogInformation("CalculateAverage");
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
            Logger.LogInformation("ave: {average} prev: {prevAverage} improv {improvedTime}", average, prevAverage, improvedTime);
            prevAverage = average;
        }

        //private void ignoremouse(MouseEventArgs e) {
        //    try {
        //        await textInput.FocusAsync();
        //    } catch (Exception e) {
        //    }
        //}

        public async Task InitializeTrack(string? problemSetIdentifier) {
            Logger.LogInformation("InitializeTrack");
            if (string.IsNullOrEmpty(problemSetIdentifier)) {
                throw new Exception("problemSetIdentifier is empty or null.");
            }
            try {
                // Get the current racer from the _raceTeamService
                Racer? currentRacer = await RaceTeamService.GetActiveRacer();
                if (currentRacer == null) {
                    // If no racer is selected, redirect to the RaceTeam page
                    Logger.LogInformation($"Redirecting to /Account/Manage/RaceTeam");
                    NavManager.NavigateTo("/Account/Manage/RaceTeam");
                    return;
                }

                // Create the track with the current racer
                track = await RaceService.CreateTrack(problemSetIdentifier);
            } catch (MissingRacerException ex) {
                LogMessage(ex);
                Logger.LogInformation($"Redirecting to /Account/Manage/RaceTeam");
                NavManager.NavigateTo("/Account/Manage/RaceTeam");
            } catch (MissingUserException ex) {
                LogMessage(ex);
                Logger.LogInformation($"Redirecting to /Account/login");
                NavManager.NavigateTo("/Account/login");
            } catch (Exception ex) {
                LogMessage(ex);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            try {
                // ENFORCE: Must have a chosen racer to race
                var activeRacer = await RaceTeamService.GetActiveRacer();
                if (activeRacer == null) {
                    Logger.LogWarning("No active racer found. Redirecting to RaceTeam page.");
                    NavManager.NavigateTo("/Account/Manage/RaceTeam", true);
                    return;
                }

                // Try to focus the text input if it exists
                await textInput.FocusAsync();
            } catch (Exception) {
                // Ignore focus errors
            }

            // No need to check authentication status on first render
            // The UI already shows a login message for unauthenticated users
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
            Logger.LogInformation("StartPeriodicTimerAsync");
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
            Logger.LogInformation("StopPeriodicTimer");
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
            "Great job sticking with it!",
            "You did it!",
            "I'm so proud of your hard work!",
            "Your effort is really paying off!",
            "You're doing fantastic work!",
            "Keep it up, you're doing great!",
            "Fantastic effort, keep it up!",
            "You're doing a wonderful job!",
            "Your hard work is really showing!",
            "Great perseverance!",
            "You're making great progress!",
            "You should be proud of yourself!",
            "Your hard work is paying off!",
            "You're doing an excellent job!",
            "Fantastic!",
            "You're showing great determination!",
            "You're doing a great job staying focused!",

            //Recognition of Improvement:
            "You're getting better every day!",
            "I can see how much you've improved!",
            "You are really improving!",
            "You're mastering these problems!",
            "You are becoming a math whiz!",
            "You're really getting the hang of this!",
            "I'm impressed with your progress!",
            "You're getting better with every race!",
            "You're really shining in math!",

            //Motivational and Positive Reinforcement:
            "I love how you don't give up!",
            "Wow, look at you go!",
            "Like a boss.",
            "Complaining doesn't solve problems, you do.",
            "Problems aren't solved by complaining — they're solved by you!",
            "Slicing through those problems like a champ.",
            "You tackled those problems like a pro!",
            "Your dedication is inspiring!",
            "Solving those problems - like a boss.",
            "Practice strengthens those brain muscles.",
            "Sailing through those problems like a pro.",

            // Funny?
            "Do it the same, but better!"
        };
    }
}

