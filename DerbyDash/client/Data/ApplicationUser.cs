namespace DerbyDash.Data {
    // Client-side ApplicationUser model (without Identity dependencies)
    public class ApplicationUser {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public int? RacerLimit { get; set; }
        public bool IsRacerLimitOverridden { get; set; } = false;
    }
}

