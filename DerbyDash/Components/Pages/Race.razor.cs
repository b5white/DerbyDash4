﻿using DerbyDash.Components.Layout;
using DerbyDash.Components.Problems;
using DerbyDash.Components.Track;
using DerbyDash.Exceptions;
using DerbyDash.HelperUtilities;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DerbyDash.Components.Pages {

    public partial class Race: ComponentBase, IDisposable {
        [Inject]
        public required RaceService RaceService { get; set; }

        [Inject]
        public required NavigationManager NavigationManager { get; set; }

        [Inject]
        public required IConfiguration configuration { get; set; }

        [Inject]
        public required ILogger<Race> _logger { get; set; }

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
        private long starttime = 0;
        private long calctime = 0;
        private float prevAverage = 0;
        private float improvedTime = 0;
        private double currentDistance = 0;
        private float[] previousResults = [0, 0, 0, 0, 0];
        public int currentResultIndex = 0;
        private int currentTimeIndex = 0;
        private float[] ElapsedAnswerTimes = new float[50];
        int PeriodicTimerSpan = 200000;  // 1/5 of a second
        private PeriodicTimer periodicTimer = new(TimeSpan.FromMicroseconds(10000000));
        private CancellationTokenSource PeriodicTimerToken = new CancellationTokenSource();
        private Timer InactivityTimer = new Timer(1000);
        private Timer FlashTimer = new Timer(1000);
        private bool isShowingAnswer = false;
        private ElementReference textInput;
        private string encouragingWord = "";
        private bool ReceivedError = false;
        private RaceComponents track = new();
        private int Margin = 10;
        private int MarginTop = 0;
        private string FlexBasis = "";
        private bool ShowDebug = false;
        private EditContext editContext = new EditContext(new object());

        private bool startLineHasDisappeared = false; // Track disappearance state
        private double previousScrollOffset = 0; // Track scroll cycled
        private int startLineAnimationCycles = 0;

        private const int INACTIVITY_TIMER_INTERVAL = 4000; // 4 seconds
        private const int FLASH_TIMER_INTERVAL = 800; // 0.8 seconds
        private const int PERIODIC_TIMER_SPAN_MICROSECONDS = 200000; // 1/5 of a second
        private const int ANIMATION_SYNC_DELAY = 50; // milliseconds
        private const int INITIAL_TIMER_DELAY = 3000; // 3 seconds

        public ProblemsBase? ProblemClass { get; set; }
        public float RaceTime { get => track.RaceTime; set => track.RaceTime = value; }
        public float FinishTime = 0;

        TrackContainer? trackContainerInstance;

        protected override void OnInitialized() {
            _logger.LogInformation("OnInitialized");
            ShowDebug = configuration.GetValue<bool>("ShowDebug");
            InactivityTimer = new Timer(INACTIVITY_TIMER_INTERVAL);
            InactivityTimer.Elapsed += ShowAnswer;
            InactivityTimer.AutoReset = false;

            FlashTimer = new Timer(FLASH_TIMER_INTERVAL);
            FlashTimer.Elapsed += HideAnswer;
            FlashTimer.AutoReset = false;
        }

        private void Reset() {
            _logger.LogInformation("Reset");
            if (!Running) {
                RaceTime = 0;
                CurrentRacerFinished = false;
                Finished = false;
                currentDistance = 0;
                Answer = "";
                Started = true;
                Running = true;
                CreateProblems();
                InitializeTrack(ProblemClassString);
                ScaleRace(0);
                encouragingWord = encouragingWords[Random.Shared.Next(0, encouragingWords.Length)];
                ReceivedError = false;
                currentTimeIndex = 0;
                for (int i = 0; i < ElapsedAnswerTimes.Length; i++) {
                    ElapsedAnswerTimes[i] = 0;
                }
                problem = problems!.Next();

                // No need to manage start line visibility - it's handled by CSS now

                // Delay the start of inactivity timer
                Task.Run(async () => {
                    await Task.Delay(INITIAL_TIMER_DELAY);
                    if (Running && !Finished) {
                        await InvokeAsync(() => {
                            InactivityTimer.Start();
                            starttime = DateTime.Now.Ticks;
                            StartPeriodicTimerAsync();
                        });
                    }
                });
            }
        }


        private void StartClick() {
            Reset();
        }

        private void CreateProblems() {
            _logger.LogInformation("CreateProblems");
            if (String.IsNullOrEmpty(ProblemClassString)) {
                ProblemClassString = "addition-4stable";
            }
            if (problems == null) {
                problems = ProblemFactory.CreateProblemManager(ProblemClassString);
            }
        }

        public void OnAfter() {
            InactivityTimer.Stop();
            if (isShowingAnswer) {
                isShowingAnswer = false;
                FlashTimer.Stop();
                Answer = RemoveHint(Answer);
            }
            if (problem != null) {
                if (Answer == problem.Result) {   // correct answer!
                    Answer = "";
                    CalculateNewDistance(GetTimespan(starttime));
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
                        EndRace();
                    }
                    ScaleRace(currentTimeIndex);
                    StateHasChanged();
                } else {
                    if (Answer.Length > (problem?.Length ?? 999)) {
                        Answer = "";
                    }
                }
            }
            if (Running) {
                InactivityTimer.Start();
            }
        }

        private void ScaleRace(int currentTimeIndex) {
            // Track and viewport constants
            const double VISIBLE_TRACK_LENGTH = 60.0;
            const double TOP_MARGIN = 0.0;
            const float TOP_MULTIPLIER = 7.0f;
            const float TRACK_HEIGHT = 70.0f;
            const float INITIAL_START_LINE_TOP = TRACK_HEIGHT * TOP_MULTIPLIER;
            const float FALL_BEHIND_TIME_THRESHOLD = 7.0f;

            double relativePosition;

            if (track.Cars.Count > 0 && track.Cars.Any(car => car.SpeedIncrements.Any())) {
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
            _logger.LogInformation("SynchronizeAnimationStart");
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

        public void HandleKeyPress(KeyboardEventArgs e) {
            if (e.Key == "Enter") {
                if (!Running || RaceTime > 0) {
                    StartClick();
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

        private void EndRace() {
            _logger.LogInformation("EndRace");
            if (Running) {
                FinishTime = GetTimespan(starttime);
                track.Cars[0].TotalTime = FinishTime;
                track.Cars[0].SpeedIncrements = RaceService.CreateSpeedIncrements(ElapsedAnswerTimes);
                InactivityTimer.Stop();
                FlashTimer.Stop();
                Utilities.FireAndForget(() => UpdateResultsAsync(FinishTime));
                Running = true;
                problems = null;
            }
        }

        private void CalculateNewDistance(double time) {
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
                EndRace();
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

        private async Task UpdateResultsAsync(float timeSpan) {
            try {
                await Task.Run(() => {
                    ResetResults(timeSpan);
                    CalculateAverage();
                });
                await RaceService.SaveRaceAsync(track);
            } catch (Exception ex) {
                LogMessage(ex);
            }
        }

        private void ResetResults(float timeSpan) {
            _logger.LogInformation("ResetResults");
            List<Car> previousRaces = track.Cars
                .Where(car => car.TotalTime > 0)
                .OrderBy(car => car.TotalTime)  // take the 5 fastest
                .Take(5)
                .ToList();

            currentResultIndex = -1;
            for (int i = previousRaces.Count - 1; i >= 0; i--) {
                previousResults[i] = (float)previousRaces[i].TotalTime;
                if (Utilities.AreDoublesEqual(previousResults[i], timeSpan, 0.000001)) {
                    currentResultIndex = i;
                }
            }
        }

        private void GoBack() {
            StopPeriodicTimer();
            NavigationManager.NavigateTo("javascript:history.back()");
        }

        private void CalculateAverage() {
            _logger.LogInformation("CalculateAverage");
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
            prevAverage = average;
        }

        //private void ignoremouse(MouseEventArgs e) {
        //    try {
        //        await textInput.FocusAsync();
        //    } catch (Exception e) {
        //    }
        //}

        public void InitializeTrack(string? problemSetIdentifier) {
            _logger.LogInformation("InitializeTrack");
            if (string.IsNullOrEmpty(problemSetIdentifier)) {
                throw new Exception("problemSetIdentifier is empty or null.");
            }
            try {
                track = RaceService.CreateTrack(problemSetIdentifier);
            } catch (MissingTeamMemberException ex) {
                LogMessage(ex);
                NavigationManager.NavigateTo("/Account/Manage/RaceTeam");
            } catch (MissingUserException ex) {
                LogMessage(ex);
                NavigationManager.NavigateTo("/Account/login");
            } catch (Exception ex) {
                LogMessage(ex);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            try {
                await textInput.FocusAsync();
            } catch (Exception) {
            }
        }

        public async void OnAfterIgnore() {
            try {
                await textInput.FocusAsync();
            } catch (Exception) {
            }
        }

        private void ShowAnswer(object? sender, ElapsedEventArgs e) {
            InvokeAsync(() => {
                isShowingAnswer = true;
                Answer = GetHint();   // prompt them with the correct answer
                StateHasChanged();
                FlashTimer.Start();
            });
        }

        private void HideAnswer(object? sender, ElapsedEventArgs e) {
            InvokeAsync(() => {
                isShowingAnswer = false;
                Answer = "";
                StateHasChanged();
                FlashTimer.Stop();
                InactivityTimer.Start();
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
            _logger.LogInformation("StartPeriodicTimerAsync");
            // Create a new CancellationTokenSource each time the timer is started
            PeriodicTimerToken = new CancellationTokenSource();
            periodicTimer = new(TimeSpan.FromMicroseconds(PERIODIC_TIMER_SPAN_MICROSECONDS));

            try {
                while (await periodicTimer.WaitForNextTickAsync(PeriodicTimerToken.Token)) {
                    RaceTime = GetSpan(starttime);
                    CalculateNewDistance(RaceTime);
                    CalculateOldDistance(RaceTime);
                    ScaleRace(currentTimeIndex);
                    await InvokeAsync(StateHasChanged);
                }
            } catch (OperationCanceledException E) {
                LogMessage(E, "Timer cancelled");
            }
            LogMessage("Timer stopped");
        }

        private void StopPeriodicTimer() {
            _logger.LogInformation("StopPeriodicTimer");
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
            _logger.LogError(E, message);
            if (E.InnerException != null) {
                LogMessage(E.InnerException);
            }
        }

        public void LogMessage(string message) {
            _logger.LogWarning(message);
        }

        public void Dispose() {
            periodicTimer.Dispose();
            InactivityTimer?.Dispose();
            FlashTimer?.Dispose();
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

