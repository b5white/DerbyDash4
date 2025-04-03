using DerbyDash.Components.Layout;
using DerbyDash.Components.Problems;
using DerbyDash.Components.Track;
using DerbyDash.HelperUtilities;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DerbyDash.Components.Pages {

    public partial class Race: ComponentBase {
        [Inject]
        public required RaceService RaceService { get; set; }
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
        private float RaceTime = 0;
        private float averageSpan = 0;
        private float increasedSpan = 0;
        private float prevAve = 0;
        private double currentDistance = 0;
        private float[] Scores = [0, 0, 0, 0, 0];
        private int currentScoreIndex = 0;
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
        private EditContext editContext = new EditContext(new object());

        private bool startLineHasDisappeared = false; // Track disappearance state
        private double previousScrollOffset = 0; // Track scroll cycled
        private int startLineAnimationCycles = 0;


        public ProblemsBase? ProblemClass { get; set; }
        TrackContainer? trackContainerInstance;

        protected override void OnInitialized() {
            InactivityTimer = new Timer(4000); // 4 seconds of inactivity
            InactivityTimer.Elapsed += ShowAnswer;
            InactivityTimer.AutoReset = false;

            FlashTimer = new Timer(800); // 1/2 second flash
            FlashTimer.Elapsed += HideAnswer;
            FlashTimer.AutoReset = false;
        }

        private void Reset() {
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
                startLineHasDisappeared = false;
                track.StartLine.Visible = true;
                previousScrollOffset = 0;

                // Delay the start of inactivity timer by 3 seconds
                Task.Run(async () => {
                    await Task.Delay(3000); // 3 second delay
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
                    CalculateNewDistance(GetSpan(starttime));
                    //         CalculateFlexBasis(6, 10, Margin++);
                    try {
                        ElapsedAnswerTimes[currentTimeIndex++] = GetSpan(starttime);
                    } catch (IndexOutOfRangeException) {
                        Console.WriteLine("currentTimeIndex = {0}", currentTimeIndex);
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

        private Dictionary<int, float> _lastPenaltyPositions = new Dictionary<int, float>();

        private void ScaleRace(int currentTimeIndex) {
            const double visibleLength = 60.0;
            const double topMargin = 0.0;
            const float topMultiplier = 7.0f;
            const float initialStartLineTop = 70.0f * topMultiplier;
            double relativePosition;

            // Find the lead car's distance
            double leadDistance = Math.Min(track.Cars.Max(car => car.Distance), RaceService.TotalDistance);

            // Calculate the visible range
            double visibleStart = Math.Max(0, leadDistance - visibleLength);

            // Check if any car has reached the top position
            bool isAnyCarAtTop = track.Cars.Any(car => car.Top <= topMargin * topMultiplier);
            track.IsAnyCarAtTop = isAnyCarAtTop;

            // Set speed class based on player car speed
            track.SpeedClass = Math.Clamp((int)track.Cars[0].Speed, 1, 15);

            // Check if finish line is in view (visible)
            relativePosition = (RaceService.TotalDistance - visibleStart) / visibleLength;
            track.FinishLine.Top = (float)(topMargin + (1 - relativePosition) * 70) * topMultiplier;
            track.IsFinishLineVisible = relativePosition >= 0 && relativePosition <= 1;

            // Track start line visibility cycles - but don't manipulate its position
            if (isAnyCarAtTop && !track.IsFinishLineVisible) {
                // Calculate offset based on the animation timing
                double cycleTime = 4000 / track.SpeedClass; // Match with CSS speed classes
                double progress = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond % cycleTime) / cycleTime;
                track.LaneOffset = progress * 280;

                // Only track animation cycles for visibility control
                if (!startLineHasDisappeared) {
                    // We detect when the animation completes one full cycle
                    if (previousScrollOffset > track.LaneOffset) {
                        // Start line has completed one cycle - hide it permanently
                        startLineHasDisappeared = true;
                        track.StartLine.Visible = false;
                    }
                    previousScrollOffset = track.LaneOffset;
                }
            } else {
                track.LaneOffset = 0;
                previousScrollOffset = 0;

                // Important: Only show start line if race hasn't properly started yet
                // This ensures it doesn't reappear after disappearing
                if (!startLineHasDisappeared && !isAnyCarAtTop) {
                    track.StartLine.Visible = true;
                }
            }

            // Position cars - handle player car separately from AI cars
            // First calculate positions for AI cars
            for (int i = 1; i < track.Cars.Count; i++) {
                var car = track.Cars[i];
                if (car.Distance <= 0) {
                    car.Top = initialStartLineTop;
                } else {
                    relativePosition = (car.Distance - visibleStart) / visibleLength;
                    float targetTop = (float)(topMargin + (1 - relativePosition) * 70) * topMultiplier;
                    double progressFactor = Math.Min(car.Distance / 10.0, 1.0);
                    car.Top = initialStartLineTop + (targetTop - initialStartLineTop) * (float)progressFactor;
                }
            }

            // Then handle player car (index 0) separately
            Car playerCar = track.Cars[0];
            if (playerCar.Speed <= 0 && isAnyCarAtTop) {
                float timeSinceLastAnswer = 0;
                if (currentTimeIndex > 0 && starttime > 0) {
                    timeSinceLastAnswer = GetSpan(starttime) - ElapsedAnswerTimes[currentTimeIndex - 1];
                }

                float fallBehindFactor = Math.Min(1.0f, timeSinceLastAnswer / 5.0f);

                relativePosition = (playerCar.Distance - visibleStart) / visibleLength;
                float normalTargetTop = (float)(topMargin + (1 - relativePosition) * 70) * topMultiplier;

                float penaltyPosition = initialStartLineTop * fallBehindFactor + normalTargetTop * (1 - fallBehindFactor);

                playerCar.Top = penaltyPosition;
            } else {
                relativePosition = (playerCar.Distance - visibleStart) / visibleLength;
                float targetTop = (float)(topMargin + (1 - relativePosition) * 70) * topMultiplier;
                playerCar.Top = initialStartLineTop + (targetTop - initialStartLineTop);
            }

            // Update car spacing
            int gap = 30;
            foreach (var car in track.Cars) {
                car.ResetFlexBasis(track.Cars.Count, gap);
            }
        }


        private void SynchronizeAnimationStart() {
            // Reset any existing animations
            track.IsAnyCarAtTop = false;

            // Force redraw without animation
            StateHasChanged();

            // After a brief delay, enable animations in sync
            Task.Run(async () => {
                await Task.Delay(50);
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
            if (Running) {
                RaceTime = GetSpan(starttime);
                track.Cars[0].TotalTime = RaceTime;
                track.Cars[0].SpeedIncrements = RaceService.CreateSpeedIncrements(ElapsedAnswerTimes);
                InactivityTimer.Stop();
                FlashTimer.Stop();
                UpdateScores(RaceTime);
                CalculateAverage();
                Running = true;
                problems = null;
            }
        }

        private void CalculateNewDistance(double time) {
            currentDistance = 0;
            int i;

            for (i = 0; (i < ElapsedAnswerTimes.Length) && (ElapsedAnswerTimes[i] > 0); i++) {
                double RaceTime = (float)(time - ElapsedAnswerTimes[i]);
                currentDistance += RaceTime * speedIncrement;
            }

            track.Cars[0].Distance = currentDistance;
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
                PeriodicTimerToken.Cancel();
                periodicTimer.Dispose();
                StopPeriodicTimer();
                Utilities.FireAndForget(RaceService.SaveRaceAsync(track));
            }

            return allFinished;
        }

        private void UpdateScores(float timeSpan) {
            RaceService.ReadScores(track.ProblemId);
            ResetScores(timeSpan);
            RaceService.WriteScores(track.ProblemId);
        }

        private void ResetScores(float timeSpan) {
            void swap(int i) {
                float prevScore = Scores[i - 1];
                Scores[i - 1] = Scores[i];
                Scores[i] = prevScore;
            }

            currentScoreIndex = -1;
            if ((Scores[4] == 0) || (timeSpan < Scores[4])) {
                Scores[4] = timeSpan;
                currentScoreIndex = 4;
                for (int i = 4; i > 0; i--) {
                    if ((Scores[i - 1] == 0) || (Scores[i] < Scores[i - 1])) {
                        swap(i);
                        currentScoreIndex = i - 1;
                    }
                }
            }
        }

        private void CalculateAverage() {
            int count = 0;
            float total = 0;

            for (int i = 0; i < Scores.Length; i++) {
                if (Scores[i] > 0) {
                    count++;
                    total += Scores[i];
                }
            }
            float average = total / count;
            if (averageSpan > 0) {
                increasedSpan = (average < averageSpan) ? (averageSpan - average) : 0;
            }
            prevAve = averageSpan;
            averageSpan = average;
        }

        //private void ignoremouse(MouseEventArgs e) {
        //    try {
        //        await textInput.FocusAsync();
        //    } catch (Exception e) {
        //    }
        //}

        public void InitializeTrack(string? problemSetIdentifier) {
            if (string.IsNullOrEmpty(problemSetIdentifier)) {
                throw new Exception("problemSetIdentifier is empty or null.");
            }
            track = RaceService.CreateTrack(problemSetIdentifier);
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
            // Create a new CancellationTokenSource each time the timer is started
            PeriodicTimerToken = new CancellationTokenSource();
            periodicTimer = new(TimeSpan.FromMicroseconds(PeriodicTimerSpan));
            //   Console.WriteLine("Timer started");
            try {
                while (await periodicTimer.WaitForNextTickAsync(PeriodicTimerToken.Token)) {
                    // Console.WriteLine("Timer triggered");
                    float RaceTime = GetSpan(starttime);
                    CalculateNewDistance(RaceTime);
                    CalculateOldDistance(RaceTime);
                    // TODO Use the results of these to tell if the race is over. Meanwhile need to not show finished races.
                    ScaleRace(currentTimeIndex);
                    await InvokeAsync(StateHasChanged);
                }
            } catch (OperationCanceledException E) {
                Console.WriteLine(E.Message);
                Console.WriteLine("Timer cancelled");
            }
            Console.WriteLine("Timer stopped");
        }

        private void StopPeriodicTimer() {
            // Cancel the token and dispose of the timer
            PeriodicTimerToken.Cancel();
            periodicTimer.Dispose();
        }

        private float GetSpan(double starttime) {
            return (float)(DateTime.Now.Ticks - starttime) / TimeSpan.TicksPerSecond;
        }

        public void LogMessage(Exception E) {
            System.Diagnostics.Debug.WriteLine(E.Message);
            if (E.InnerException != null) {
                LogMessage(E.InnerException);
            }
        }

        public void LogMessage(string message) {
            System.Diagnostics.Debug.WriteLine(message);
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

