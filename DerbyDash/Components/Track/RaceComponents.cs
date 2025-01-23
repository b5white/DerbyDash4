namespace DerbyDash.Components.Track {
    public class RaceComponents {
        public List<Car> Cars { get; set; } = new();
        public RaceComponent StartLine { get; set; } = new();
        public RaceComponent FinishLine { get; set; } = new();
        public RaceComponent LeftFence { get; set; } = new();

    }

    public class RaceComponent {
        public float Top { get; set; } = 200;
        public string TopStr {
            get {
                return Top.ToString() + "px";
            }
        }

        public string ImageUrl { get; set; } = "";
    }
}
