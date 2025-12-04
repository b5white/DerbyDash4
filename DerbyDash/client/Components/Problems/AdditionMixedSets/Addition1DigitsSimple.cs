namespace DerbyDash.Components.Problems {
    public class Addition1DigitsSimple: AdditionMixedSimple {

        public Addition1DigitsSimple() : base() { }

        public override void Init() {
            Title = "Addition Problems with One Digit, Without Carrying";
        }

        protected override void PopulateOperands() {
            do {
                int index = Random.Shared.Next(0, MaxWTen);
                Operands op = Items[index];
                Operand1 = op.Operand1;
                Operand2 = op.Operand2;
            } while (Check1(Operand1) || Check2(Operand2));
        }
    }
}
