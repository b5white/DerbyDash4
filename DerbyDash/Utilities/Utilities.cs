using System.Security.Cryptography;
using System.Text;

namespace DerbyDash.HelperUtilities {
    public static class Utilities {
        private static readonly char[] padding = { '=' };
        private static readonly ILogger _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("HelperUtilities");

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

        public static void FireAndForget(Task task) {
            task.ContinueWith(t => {
                // Log or handle exceptions
                _logger.LogError(t.Exception, "An error occurred during a fire-and-forget operation.");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
