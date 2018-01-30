using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;



namespace SolutionWizard.Utilities
{
    public static class FileSystemHelper
    {
        public static IList<string> FindFiles(string root, string wildcardPattern)
        {
            string[] filePaths = Directory.GetFiles(root, wildcardPattern, SearchOption.AllDirectories);
            List<string> res = new List<string>();
            res.AddRange(filePaths);

            return res;
        }

        public static void CopyFile(string sourcePath, string destinationPath)
        {
            if(!File.Exists(sourcePath))
            {
                throw new ArgumentException("File not found.");
            }

            File.Copy(sourcePath, destinationPath);
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                       + "$";
        }


    }
}
