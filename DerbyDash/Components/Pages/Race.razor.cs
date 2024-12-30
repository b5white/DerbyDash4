using Microsoft.AspNetCore.Components;
using DerbyDash.Components.Problems;
using Microsoft.AspNetCore.Components.Web;
using Timer = System.Timers.Timer;
using System.Timers;
using System.Threading;
using DerbyDash.Components.Layout;
using Microsoft.AspNetCore.Components.Forms;
using Mono.TextTemplating;

namespace DerbyDash.Components.Pages {

    public partial class Race: ComponentBase {
        private bool Started = false;
        private bool Running = false;

        private ProblemManagerBase? problems;
        private ProblemsBase? problem;
        private int speedIncrement = 1;
        private string Answer = "";
        private long starttime = 0;
        private long calctime = 0;
        private float span = 0;
        private float averageSpan = 0;
        private float increasedSpan = 0;
        private float prevAve = 0;
        private double currentDistance = 0;
        private float totalDistance = 150;
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
        private List<Car> Cars = new List<Car>();
        private int Margin = 10;
        private int MarginTop = 0;
        private string FlexBasis = "";
        private EditContext editContext = new EditContext(new object());
        private Random random = new Random();

        [Parameter] public string? ProblemClassString { get; set; }
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
                span = 0;
                currentDistance = 0;
                Answer = "";
                Started = true;
                Running = true;
                CreateProblems();
                CreateCars();
                encouragingWord = encouragingWords[Random.Shared.Next(0, encouragingWords.Length)];
                ReceivedError = false;
                currentTimeIndex = 0;
                for (int i = 0; i < ElapsedAnswerTimes.Length; i++) {
                    ElapsedAnswerTimes[i] = 0;
                }
                problem = problems!.Next();
                starttime = DateTime.Now.Ticks;
                InactivityTimer.Start();
                Task.Run(() => StartPeriodicTimerAsync());
            }
        }

        private void StartClick() {
            Reset();
        }

        private void CreateProblems() {
            if (problems == null) {
                if (String.IsNullOrEmpty(ProblemClassString)) {
                    ProblemClassString = "addition-4stable";
                }
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
                    CalculateDistance();
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
                } else {
                    if (Answer.Length > (problem?.Length ?? 999)) {
                        Answer = "";
                    }
                }
            }
            if (Running) {
                InactivityTimer.Start();
            }
            ScaleRace();
            StateHasChanged();
        }

        private void ScaleRace() {
            const double trackLength = 150.0;
            const double visibleLength = 60.0;
            const double topMargin = 0.0; // Space at the top of the container
            const float topMultiplier = 4.0f; // Adjusted from 2.5f to 2.0f

            // Find the lead car's distance
            double leadDistance = Cars.Max(car => car.Distance);

            // Calculate the visible range
            double visibleStart = Math.Max(0, leadDistance - visibleLength);
            double visibleEnd = leadDistance;

            foreach (var car in Cars) {
                //            if (car.Distance >= visibleStart) {
                // Calculate the car's position within the visible range
                double relativePosition = (car.Distance - visibleStart) / visibleLength;

                // Set the Top property (0 for the lead car, increasing for cars further back)
                car.Top = (float)(topMargin + (1 - relativePosition) * 70) * topMultiplier;
                //} else {
                //    // Car is behind the visible range
                //    car.Top = 400f;
                //}
            }

            // Set flex-basis for all cars
            int gap = 10;
            foreach (var car in Cars) {
                car.ResetFlexBasis(Cars.Count, gap);
            }
        }

        public void HandleKeyPress(KeyboardEventArgs e) {
            if (e.Key == "Enter") {
                if (!Running || span > 0) {
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
            if (Running || problems != null) { // avoid double execution
                Console.WriteLine("EndRace");
                span = GetSpan(starttime);
                StopPeriodicTimer();
                InactivityTimer.Stop();
                FlashTimer.Stop();
                ResetScores(span);
                CalculateAverage();
                Running = false;
                problems = null;
            }
        }

        private void CalculateDistance() {
            float span = GetSpan(starttime);
            Task.Run(() => CalculateNewDistanceAsync(span));
        }

        private async Task CalculateNewDistanceAsync(double time) {
            currentDistance = 0;
            int i;

            for (i = 0; (i < ElapsedAnswerTimes.Length) && (ElapsedAnswerTimes[i] > 0); i++) {
                double span = (float)(time - ElapsedAnswerTimes[i]);
                currentDistance += span * speedIncrement;
            }
            //  Console.WriteLine("curr: {0}  Total: {1}", currentDistance, totalDistance);
            if (currentDistance >= totalDistance) {
                EndRace();
            } else {
                Cars[0].Distance = currentDistance;
                Cars[0].Speed = i * speedIncrement;
            }
            Console.WriteLine($"0, {time}, {Cars[0].Speed}, {currentDistance}");

        }

        private async Task CalculateOldDistanceAsync(double time) {
            for (int i = 1; i < Cars.Count; i++) {
                Cars[i].CalculateCurrentDistance(time);
            }
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

        private void CreateCars() {
            Cars = [
                new Car { index = 0, ImageUrl = "Racecar1.png" },
                new Car { index = 1, ImageUrl = "Racecar2.png" },
                new Car { index = 2, ImageUrl = "Racecar3.png" },
                new Car { index = 3, ImageUrl = "Racecar4.png" },
                new Car { index = 4, ImageUrl = "Racecar5.png" },
                new Car { index = 5, ImageUrl = "Racecar6.png" }
            ];
            for (int i = 1; i < Cars.Count; i++) {
                Cars[i].InitializeFastEddyTimeIncrements(random);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            try {
                await textInput.FocusAsync();
            } catch (Exception e) {
            }
        }

        public async void OnAfterIgnore() {
            try {
                await textInput.FocusAsync();
            } catch (Exception e) {
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
                    Console.WriteLine("Timer triggered");
                    float span = GetSpan(starttime);
                    await CalculateNewDistanceAsync(span);
                    await CalculateOldDistanceAsync(span);
                    ScaleRace();
                    await InvokeAsync(() => {
                        StateHasChanged();
                    });
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
            "I’m so proud of your hard work!",
            "Your effort is really paying off!",
            "You’re doing fantastic work!",
            "Keep it up, you’re doing great!",
            "Fantastic effort, keep it up!",
            "You’re doing a wonderful job!",
            "Your hard work is really showing!",
            "Great perseverance!",
            "You’re making great progress!",
            "You should be proud of yourself!",
            "Your hard work is paying off!",
            "You’re doing an excellent job!",
            "Fantastic!",
            "You’re showing great determination!",
            "You’re doing a great job staying focused!",

			//Recognition of Improvement:
			"You’re getting better every day!",
            "I can see how much you’ve improved!",
            "You are really improving!",
            "You’re mastering these problems!",
            "You are becoming a math whiz!",
            "You’re really getting the hang of this!",
            "I’m impressed with your progress!",
            "You’re getting better with every race!",
            "You’re really shining in math!",

			//Motivational and Positive Reinforcement:
			"I love how you don’t give up!",
            "Wow, look at you go!",
            "Like a boss.",
            "Complaining doesn’t solve problems, you do.",
            "Problems aren't solved by complaining — they're solved by you!",
            "Slicing through those problems like a champ.",
            "You tackled those problems like a pro!",
            "Your dedication is inspiring!",
            "Solving those problems - like a boss.",
            "Practice strengthens those brain muscles.",
            "Sailing through those problems like a pro.",

			// Other
			"Do it the same, but better."
        };
    }
}

