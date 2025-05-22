namespace DerbyDash.Data {
    public class Racer {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly? LastRaced { get; set; } = null;
        public int RaceCount { get; set; } = 0;
        // Last played race identifier (e.g., "addition-4stable")
        public string? LastPlayedRace { get; set; }

        // Avatar image file name (e.g., "1.png", "2.png", "3.png")
        public string? AvatarFileName { get; set; }
    }
}
