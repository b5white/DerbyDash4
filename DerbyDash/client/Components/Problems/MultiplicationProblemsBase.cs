namespace DerbyDash.Components.Problems {
    public class MultiplicationProblemsBase: MathProblems {

        public MultiplicationProblemsBase() : base() {
            //            Title = "Multiplication Problems";
            PopulateOperands();
            Operation = "*";
            Result = (Operand1 * Operand2).ToString();
            Length = Result.Length;
        }
    }
}
