namespace DerbyDash.Components.Problems {
    public abstract class ProblemManagerBase {
        protected List<ProblemsBase> problems;
        protected int numberOfProblems = 30;  //XXX
        private int index = 0;
        public Boolean More {
            get {
                return index < numberOfProblems;
            }
        }

        public string Title {
            get {
                return First().Title;
            }
        }

        public ProblemManagerBase() {
            problems = new List<ProblemsBase>();
        }

        public abstract ProblemsBase CreateProblem();

        public void InitializeProblems() {
            for (int i = 0; i < numberOfProblems; i++) {
                problems.Add(CreateProblem());
            }
        }

        public ProblemsBase Next() {
            if (index < numberOfProblems) {
                return problems[index++];
            } else {
                throw new Exception($"Can't call Next with no problems left. Current index: {index}, Number of problems: {numberOfProblems}");
            }
        }

        public ProblemsBase First() {
            return problems[0];
        }
    }
}
