namespace DerbyDash.Shared.Problems {
    public class ProblemDto {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public int Length { get; set; }
    }

    public class ProblemSetDto {
        public string Title { get; set; } = string.Empty;
        public List<ProblemDto> Problems { get; set; } = new();
        public bool HasMore { get; set; }
    }
}
