namespace DerbyDash.Data {
    public class Racer {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateOnly? LastRaced { get; set; } = null;
        public int RaceCount { get; set; } = 0;
        // Last played race identifier (e.g., "addition-4stable")
        public string? LastPlayedRace { get; set; }
    }
}
