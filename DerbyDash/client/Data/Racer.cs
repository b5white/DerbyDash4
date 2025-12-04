namespace DerbyDash.Data {
    // Client-side Racer model (without EF attributes)
    public class Racer {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateOnly? LastRaced { get; set; } = null;
        public string? LastPlayedRace { get; set; }
        public string? AvatarFileName { get; set; }
        public int RaceCount { get; set; }
    }
}

