namespace DerbyDash.Problems {
    public class MultiplicationSquaresSmall: MultiplicationProblemsBase {

        public MultiplicationSquaresSmall() : base() { }
        int MaxSmallSquare = 12;

        public override void Init() {
            Title = "Multiplication Problems with Squares, Small";
        }

        protected override void PopulateOperands() {
            do {
                Operand1 = Random.Shared.Next(2, MaxSmallSquare + 1);
                Operand2 = Operand1;
            } while (Check1(Operand1));
        }
    }
}

