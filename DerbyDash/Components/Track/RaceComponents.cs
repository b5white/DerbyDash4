namespace DerbyDash.Components.Track {
    public class RaceComponents {
        public List<Car> Cars { get; set; } = new();
        public RaceComponent StartLine { get; set; } = new();
        public RaceComponent FinishLine { get; set; } = new();
        public RaceComponent LeftFence { get; set; } = new();
        public double LaneOffset { get; set; } = 0;
        public bool IsAnyCarAtTop { get; set; } = false;
        public bool IsFinishLineVisible { get; set; } = false;
        public float SpeedMultiplier { get; set; } = 1.0f;

        public int SpeedClass { get; set; } = 1;
        public int ProblemId { get; set; }
        public string FamilyMemberId { get; set; } = "";
        public bool ShowDebug = true;

        public void UpdateSpeedClass(int speed) {
            // Map speed value to CSS class (1-5)
            SpeedClass = Math.Clamp(speed / 2 + 1, 1, 5);
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
