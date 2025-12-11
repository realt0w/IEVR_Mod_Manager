using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IEVRModManager.Models;

namespace IEVRModManager.Managers
{
    public class ProfileManager
    {
        private readonly string _profilesDir;

        public ProfileManager()
        {
            _profilesDir = Path.Combine(Config.AppDataDir, "Profiles");
            EnsureDirectoryExists(_profilesDir);
        }

        public List<ModProfile> GetAllProfiles()
        {
            var profiles = new List<ModProfile>();
            
            if (!Directory.Exists(_profilesDir))
            {
                return profiles;
            }

            var profileFiles = Directory.GetFiles(_profilesDir, "*.json");
            
            foreach (var file in profileFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var profile = JsonSerializer.Deserialize<ModProfile>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (profile != null)
                    {
                        profiles.Add(profile);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading profile {file}: {ex.Message}");
                }
            }

            return profiles.OrderByDescending(p => p.LastModifiedDate).ToList();
        }

        public ModProfile? LoadProfile(string profileName)
        {
            var filePath = GetProfilePath(profileName);
            
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var profile = JsonSerializer.Deserialize<ModProfile>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return profile;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile {profileName}: {ex.Message}");
                return null;
            }
        }

        public bool SaveProfile(ModProfile profile)
        {
            try
            {
                EnsureDirectoryExists(_profilesDir);
                
                // Sanitize profile name for filename
                var safeName = SanitizeFileName(profile.Name);
                if (string.IsNullOrWhiteSpace(safeName))
                {
                    safeName = "Unnamed";
                }

                var filePath = Path.Combine(_profilesDir, $"{safeName}.json");
                
                // Update last modified date
                profile.LastModifiedDate = DateTime.Now;
                if (profile.CreatedDate == DateTime.MinValue)
                {
                    profile.CreatedDate = DateTime.Now;
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(profile, options);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving profile: {ex.Message}");
                return false;
            }
        }

        public bool DeleteProfile(string profileName)
        {
            try
            {
                var filePath = GetProfilePath(profileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting profile {profileName}: {ex.Message}");
                return false;
            }
        }

        public bool ProfileExists(string profileName)
        {
            var filePath = GetProfilePath(profileName);
            return File.Exists(filePath);
        }

        private string GetProfilePath(string profileName)
        {
            var safeName = SanitizeFileName(profileName);
            return Path.Combine(_profilesDir, $"{safeName}.json");
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "Unnamed";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            
            // Limit length
            if (sanitized.Length > 100)
            {
                sanitized = sanitized.Substring(0, 100);
            }
            
            return sanitized.Trim();
        }

        private static void EnsureDirectoryExists(string? path)
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

