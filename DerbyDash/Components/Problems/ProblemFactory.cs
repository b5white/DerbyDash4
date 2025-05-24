namespace DerbyDash.Components.Problems {
    public static class ProblemFactory {
        private static readonly Dictionary<string, Type> problemTypeMap = new Dictionary<string, Type> {
            { "addition-2stable",  typeof(Addition2sTable) },
            { "addition-3stable",  typeof(Addition3sTable) },
            { "addition-4stable",  typeof(Addition4sTable) },
            { "addition-5stable",  typeof(Addition5sTable) },
            { "addition-1digitsimple",  typeof(Addition1DigitsSimple) },
            { "addition-1digit",        typeof(Addition1Digits) },
            { "multiplication-squaressmall",        typeof(MultiplicationSquaresSmall) }
        };

        public static ProblemManagerBase CreateProblemManager(string problemTypeName) {
            if (problemTypeMap.TryGetValue(problemTypeName, out Type? problemType)) {
                Type managerType = typeof(ProblemManager<>).MakeGenericType(problemType);
                ProblemManagerBase? result = Activator.CreateInstance(managerType) as ProblemManagerBase;
                if (result == null) {
                    throw new ArgumentException($"Problem type '{problemTypeName}' could not be created.");
                }
                return result;
            }
            throw new ArgumentException($"Problem type '{problemTypeName}' not found.");
        }

        public static string GetTitle(string? problemTypeName) {
            string result = "";
            if (!string.IsNullOrEmpty(problemTypeName)) {
                if (problemTypeMap.TryGetValue(problemTypeName, out Type? problemType)) {
                    if (problemType != null) {
                        var instance = Activator.CreateInstance(problemType);
                        if (instance != null) {
                            result = ((dynamic)instance).Title;
                        }
                    }
                }
            }
            return result;
        }
    }
}
