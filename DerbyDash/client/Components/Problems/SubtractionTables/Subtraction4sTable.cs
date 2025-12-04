namespace DerbyDash.Components.Problems {
    public class Subtraction4sTable: SubtractionProblemsBase {
        public override void Init() {
            Title = "Subtraction Problems with Fours";
            min1 = 4;
            max1 = 15; // Make sure we have enough range for meaningful subtraction
            min2 = 1;
            max2 = 4; // Subtracting numbers up to 4
        }
    }
}
