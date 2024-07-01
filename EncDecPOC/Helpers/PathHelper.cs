using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncDecPOC.Helpers
{
    public static class PathHelper
    {
        public static string FindProjectPath(string projectName)
        {
            // Get the current directory (assuming it's within the project directory)
            string currentDirectory = Directory.GetCurrentDirectory();

            // Navigate to the parent directory until reaching the solution directory
            string solutionDirectory = GetSolutionDirectory(currentDirectory);

            // Search for the project folder by its name within the solution directory
            string[] projectDirectories = Directory.GetDirectories(solutionDirectory, projectName, SearchOption.AllDirectories);

            // Return the path of the first matching project folder
            return projectDirectories.FirstOrDefault();
        }
        public static string GetSolutionDirectory(string directory)
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(directory);

            // Navigate up the directory tree until finding the solution file (.sln)
            while (currentDirectory != null)
            {
                FileInfo[] solutionFiles = currentDirectory.GetFiles("*.sln");
                if (solutionFiles.Length > 0)
                {
                    return currentDirectory.FullName;
                }

                currentDirectory = currentDirectory.Parent;
            }

            return null; // Solution directory not found
        }
    }
}
