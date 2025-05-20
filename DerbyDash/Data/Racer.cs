namespace DerbyDash.Data {
    public class Racer {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateOnly? LastRaced { get; set; } = null;
    }
}
