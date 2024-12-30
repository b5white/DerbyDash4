namespace DerbyDash.Components.Problems {
    public class AdditionProblemsBase: MathProblems {
        public AdditionProblemsBase() : base() {
            PopulateOperands();
            Operation = "+";
            Result = (Operand1 + Operand2).ToString();
            Length = Result.Length;
        }
    }
}
