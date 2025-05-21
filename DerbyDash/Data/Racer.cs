namespace DerbyDash.Data {
    public class Racer {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly? LastRaced { get; set; } = null;
        // Avatar image file name (e.g., "1.png", "2.png", "3.png")
        public string? AvatarFileName { get; set; }
    }
}
