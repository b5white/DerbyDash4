using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DerbyDash.Data {
    [Table("FamilyMembers")]
    public class Racer {
        [Key]
        public int Id { get; set; }
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public DateOnly? LastRaced { get; set; } = null;
        public int RaceCount { get; set; } = 0;
        // Last played race identifier (e.g., "addition-4stable")
        public string? LastPlayedRace { get; set; }

        // Avatar image file name (e.g., "1.png", "2.png", "3.png")
        [MaxLength(255)]
        public string? AvatarFileName { get; set; }

        public IEnumerable<Race>? Races { get; set; }
    }
}
