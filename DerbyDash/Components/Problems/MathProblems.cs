using Azure;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace DerbyDash.Components.Problems {
    public class MathProblems: ProblemsBase {
        public int Operand1 { get; set; }
        public int Operand2 { get; set; }
        public string Operation { get; set; } = "";
        public int min1 { get; set; } = 1;
        public int max1 { get; set; } = 10;
        public int min2 { get; set; } = 3;
        public int max2 { get; set; } = 11;
        static int prev1;
        static int prev2;

        public override string Description {
            get {
                return $"{Operand1} {Operation} {Operand2}";
            }
        }

        protected virtual void PopulateOperands() {
            do {
                Operand1 = Random.Shared.Next(min1, max1);
            } while (Check1(Operand1));
            do {
                Operand2 = Random.Shared.Next(min2, max2);
            } while (Check2(Operand2));
        }

        /// <summary>
        /// Check whether this operand matches the previous 
        /// </summary>
        /// <param name="operand"></param>
        /// <returns>Match</returns>
        protected override bool Check1(int operand) {
            bool result = false;
            if (min1 != max1) {
                result = prev1 == operand;
            }
            if (!result) {
                prev1 = operand;
            }
            return result;
        }

        protected override bool Check2(int operand) {
            bool result = false;
            if (min2 != max2) {
                result = prev2 == operand;
            }
            if (!result) {
                prev2 = operand;
            }
            return result;
        }
    }
}

