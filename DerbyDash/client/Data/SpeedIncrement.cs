namespace DerbyDash.Data {
    // Client-side SpeedIncrement model (without EF attributes)
    public class SpeedIncrement {
        public int Id { get; set; }
        public int RaceId { get; set; }
        public double Time { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
    }
}

