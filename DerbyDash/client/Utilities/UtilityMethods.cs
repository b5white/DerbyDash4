using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DerbyDash.Utilities {
    public static class UtilityMethods {
        private static readonly char[] padding = { '=' };
        // No-op logger for client-side (WebAssembly doesn't support console logging in the same way)
        private static readonly ILogger Logger = new NoOpLogger();
        
        private class NoOpLogger : ILogger {
            IDisposable? ILogger.BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        }

        public static string BytesToString(byte[] bytes) {
            try {
                return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd(padding);
            } catch (Exception) {
                return string.Empty;
            }
        }

        public static byte[] StringToBytes(string str) {
            try {
                string replacedString = str.Replace('-', '+').Replace('_', '/');
                switch (replacedString.Length % 4) {
                    case 1:
                        replacedString += "===";
                        break;
                    case 2:
                        replacedString += "==";
                        break;
                    case 3:
                        replacedString += "=";
                        break;
                }
                return Convert.FromBase64String(replacedString);
            } catch (Exception) {
                return Array.Empty<byte>();
            }
        }

        public static bool AreDoublesEqual(double value1, double value2, double errorMargin) {
            return Math.Abs(value1 - value2) <= errorMargin;
        }

        public static int GetUniqueIntFromString(string input) {
            using (SHA256 sha256Hash = SHA256.Create()) {
                // Compute the hash for the input string
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Use the first 4 bytes of the hash to create an integer
                int uniqueInt = BitConverter.ToInt32(bytes, 0);

                // Ensure the integer is positive
                uniqueInt = uniqueInt & 0x7FFFFFFF;

                return uniqueInt;
            }
        }

        public static void FireAndForget(Func<Task> taskFactory) {
            _ = Task.Run(async () => {
                try {
                    await taskFactory();
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error");
                }
            });
        }
    }
}
