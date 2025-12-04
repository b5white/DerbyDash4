namespace DerbyDash.Components.Problems {
    public class Addition1Digits: AdditionProblemsBase {

        public Addition1Digits() : base() { }

        public override void Init() {
            Title = "Addition Problems with One Digit";
            min1 = 1;
            max1 = 10;
            min2 = 2;
        }
    }
}

