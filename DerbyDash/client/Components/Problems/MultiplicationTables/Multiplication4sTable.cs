namespace DerbyDash.Components.Problems {
    public class Multiplication4sTable: MultiplicationProblemsBase {
        public override void Init() {
            Title = "Multiplication Problems with Fours";
            min1 = 4;
            max1 = 4; // Always multiply by 4
            min2 = 1;
            max2 = 12; // Multiply 4 by numbers 1-12
        }
    }
}
