using System;
using System.IO;
using System.Text.Json;
using IEVRModManager.Models;
using IEVRModManager.Helpers;
using IEVRModManager.Exceptions;
using static IEVRModManager.Helpers.Logger;

namespace IEVRModManager.Managers
{
    /// <summary>
    /// Manages the last installation record, tracking which mods were last applied.
    /// </summary>
    public class LastInstallManager
    {
        private readonly string _recordPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LastInstallManager"/> class.
        /// </summary>
        public LastInstallManager()
        {
            _recordPath = Config.LastInstallPath;
            FileSystemHelper.EnsureDirectoryExists(Path.GetDirectoryName(_recordPath));
        }

        /// <summary>
        /// Loads the last installation information from disk.
        /// </summary>
        /// <returns>A <see cref="LastInstallInfo"/> object. Returns empty info if file doesn't exist or is invalid.</returns>
        public LastInstallInfo Load()
        {
            if (!File.Exists(_recordPath))
            {
                return LastInstallInfo.Empty();
            }

            try
            {
                var json = File.ReadAllText(_recordPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return LastInstallInfo.Empty();
                }

                var info = JsonSerializer.Deserialize<LastInstallInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return info ?? LastInstallInfo.Empty();
            }
            catch (JsonException ex)
            {
                Instance.Log(LogLevel.Warning, "Error parsing last install info JSON", true, ex);
                return LastInstallInfo.Empty();
            }
            catch (IOException ex)
            {
                Instance.Log(LogLevel.Warning, "IO error loading last install info", true, ex);
                return LastInstallInfo.Empty();
            }
            catch (Exception ex)
            {
                Instance.Log(LogLevel.Warning, "Unexpected error loading last install info", true, ex);
                return LastInstallInfo.Empty();
            }
        }

        /// <summary>
        /// Saves the last installation information to disk.
        /// </summary>
        /// <param name="info">The installation information to save.</param>
        /// <returns><c>true</c> if the information was saved successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is null.</exception>
        /// <exception cref="ModManagerException">Thrown when there's an error writing the file.</exception>
        public bool Save(LastInstallInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            try
            {
                FileSystemHelper.EnsureDirectoryExists(Path.GetDirectoryName(_recordPath));
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(info, options);
                File.WriteAllText(_recordPath, json);
                return true;
            }
            catch (IOException ex)
            {
                Instance.Log(LogLevel.Error, "IO error saving last install info", true, ex);
                throw new ModManagerException("Failed to save last install info. Check file permissions.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Instance.Log(LogLevel.Error, "Access denied saving last install info", true, ex);
                throw new ModManagerException("Access denied to last install info file.", ex);
            }
            catch (Exception ex)
            {
                Instance.Log(LogLevel.Error, "Unexpected error saving last install info", true, ex);
                throw new ModManagerException("An unexpected error occurred while saving last install info.", ex);
            }
        }

        /// <summary>
        /// Clears the last installation record by deleting the file.
        /// </summary>
        public void Clear()
        {
            if (!File.Exists(_recordPath))
            {
                return;
            }

            try
            {
                File.Delete(_recordPath);
            }
            catch (Exception ex)
            {
                Instance.Log(LogLevel.Warning, "Error deleting last install info", true, ex);
            }
        }

    }
}
