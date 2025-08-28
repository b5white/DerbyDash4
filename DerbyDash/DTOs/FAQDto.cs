using System.ComponentModel.DataAnnotations;

namespace DerbyDash.DTOs
{
    public class FAQDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateFAQDto
    {
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Question { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Answer { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int Order { get; set; } = 0;
    }

    public class UpdateFAQDto
    {
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(500)]
        public string Question { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Answer { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int Order { get; set; } = 0;
    }
}
