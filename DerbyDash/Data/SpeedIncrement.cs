using System.ComponentModel.DataAnnotations.Schema;

namespace DerbyDash.Data {
    public class SpeedIncrement {
        public int Id { get; set; } // Primary key
        public int RaceId { get; set; } // Foreign key to Race
        public double Time { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
    }
}
