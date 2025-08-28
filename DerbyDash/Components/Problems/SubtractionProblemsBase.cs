namespace DerbyDash.Components.Problems {
    public class SubtractionProblemsBase: MathProblems {
        public SubtractionProblemsBase() : base() {
            PopulateOperands();
            Operation = "-";
            Result = (Operand1 - Operand2).ToString();
            Length = Result.Length;
        }

        protected override void PopulateOperands() {
            // For subtraction, ensure Operand1 is always >= Operand2 to avoid negative results
            do {
                Operand1 = Random.Shared.Next(min1, max1);
            } while (Check1(Operand1));
            
            do {
                Operand2 = Random.Shared.Next(min2, Math.Min(max2, Operand1 + 1)); // Ensure Operand2 <= Operand1
            } while (Check2(Operand2));
        }
    }
}
