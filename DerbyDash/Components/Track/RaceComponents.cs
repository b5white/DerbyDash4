namespace DerbyDash.Components.Track {
    public class RaceComponents {
        public List<Car> Cars { get; set; } = new();
        public RaceComponent StartLine { get; set; } = new();
        public RaceComponent FinishLine { get; set; } = new();
        public RaceComponent LeftFence { get; set; } = new();
        public double LaneOffset { get; set; } = 0;
        public bool IsAnyCarAtTop { get; set; } = false;
        public float SpeedMultiplier { get; set; } = 1.0f;
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
