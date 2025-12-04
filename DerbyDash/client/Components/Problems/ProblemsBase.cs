namespace DerbyDash.Components.Problems {
    public class ProblemsBase {
        public string Title { get; set; }
        public virtual string Description { get; set; }
        public string Result { get; set; } = "";
        public int Length { get; set; }


        public ProblemsBase() {
            Title = "Base";
            Description = "Should Override";
            Init();
        }

        public virtual void Init() { }
        protected virtual bool Check1(int operand) {
            return false;
        }

        protected virtual bool Check2(int operand) {
            return false;
        }
    }
}
