namespace DerbyDash.Components.Track {
    public class RaceComponents {
        public const double SPEED_MULTIPLIER = 7.0;
        public List<Car> Cars { get; set; } = new();
        public RaceComponent StartLine { get; set; } = new();
        public RaceComponent FinishLine { get; set; } = new();
        public RaceComponent LeftFence { get; set; } = new();
        public double LaneOffset { get; set; } = 0;
        public bool IsAnyCarAtTop { get; set; } = false;
        public bool IsFinishLineVisible { get; set; } = false;
        public float SpeedMultiplier { get; set; } = 1.0f;
        public int SpeedClass { get; set; } = 1;
        public float RaceTime = 0;
        public int ProblemId { get; set; }
        public string TeamMemberId { get; set; } = "";
        public bool ShowDebug = true;
        const int MIN_SPEED_CLASS = 1;
        const int MAX_SPEED_CLASS = 15;

        public void UpdateSpeedClass(int speed) {
            // Map speed value to CSS class (1-15)
            SpeedClass = Math.Clamp(speed, MIN_SPEED_CLASS, MAX_SPEED_CLASS);
        }
    }

    public class RaceComponent {
        public float Top { get; set; } = 200;
        public bool Visible { get; set; } = true;
        public string TopStr {
            get {
                return Top.ToString() + "px";
            }
        }

        public string ImageUrl { get; set; } = "";
    }
}
