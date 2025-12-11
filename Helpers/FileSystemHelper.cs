using System.IO;

namespace IEVRModManager.Helpers
{
    /// <summary>
    /// Provides utility methods for file system operations.
    /// </summary>
    public static class FileSystemHelper
    {
        /// <summary>
        /// Ensures that the specified directory exists, creating it if necessary.
        /// </summary>
        /// <param name="path">The directory path to ensure exists.</param>
        public static void EnsureDirectoryExists(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
