namespace DerbyDash.Components.Problems {
    public class ProblemManager<T>: ProblemManagerBase where T : ProblemsBase, new() {
        public ProblemManager() {
            InitializeProblems();
        }

        public override ProblemsBase CreateProblem() {
            return new T();
        }
    }
}

