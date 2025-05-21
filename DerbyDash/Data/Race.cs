using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DerbyDash.Data {
    [Index(nameof(Id), nameof(RacerId), nameof(ProblemSetId))]
    public class Race {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Racer")]
        [Column("FamilyMemberId")]
        public int RacerId { get; set; }
        [Required]
        public DateTime RaceDateTime { get; set; } = DateTime.Now;
        [Required]
        public double TotalTime { get; set; }
        [Required]
        public int ProblemSetId { get; set; }
        public int ImageId { get; set; }
        [Required]
        public ICollection<SpeedIncrement>? SpeedIncrements { get; set; }
    }
}
