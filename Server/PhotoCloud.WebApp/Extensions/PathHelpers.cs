using Microsoft.ML;

namespace PhotoCloud.Models
{
    public static class PathHelpers
    {
        static string GetAbsolutePath(this string relativePath)
        {
            FileInfo dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string? assemblyFolderPath = dataRoot.Directory?.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
