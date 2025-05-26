using System.Reflection;

namespace DerbyDash.Components {

    public static class AppInfo {
        static string BuildNo = "0476";
 
        public static string Version = GitVersionInformation.Major + "." + GitVersionInformation.Minor + "." + GitVersionInformation.BuildMetaData + "." + BuildNo;
    }
} 
  
