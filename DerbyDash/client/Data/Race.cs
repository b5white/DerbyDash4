namespace DerbyDash.Data {
    // Client-side Race model (without EF attributes)
    public class Race {
        public int Id { get; set; }
        public int RacerId { get; set; }
        public DateTime RaceDateTime { get; set; } = DateTime.Now;
        public double TotalTime { get; set; }
        public int ProblemSetId { get; set; }
        public int ImageId { get; set; }
        public int FinishingPosition { get; set; } = 0;
        public List<SpeedIncrement>? SpeedIncrements { get; set; }
    }
}

