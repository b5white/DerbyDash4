namespace DerbyDash.Components.Problems {
    public static class ProblemFactory {
        private static readonly Dictionary<string, Type> problemTypeMap = new Dictionary<string, Type> {
            { "addition-2stable",  typeof(Addition2sTable) }
        };

        public static ProblemManagerBase CreateProblemManager(string problemTypeName) {
            if (problemTypeMap.TryGetValue(problemTypeName, out Type? problemType)) {
                Type managerType = typeof(ProblemManager<>).MakeGenericType(problemType);
                var result = Activator.CreateInstance(managerType) as ProblemManagerBase;
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
