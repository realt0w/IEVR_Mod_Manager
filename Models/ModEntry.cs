using System;
using System.IO;
using System.Text.Json.Serialization;

namespace IEVRModManager.Models
{
    public class ModEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public string DisplayName { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ModVersion { get; set; } = string.Empty;
        public string GameVersion { get; set; } = string.Empty;

        [JsonIgnore]
        public string FullPath => System.IO.Path.Combine(Path, Name);

        public ModEntry()
        {
        }

        public ModEntry(string name, string path, bool enabled = true, 
            string? displayName = null, string? author = null, 
            string? modVersion = null, string? gameVersion = null)
        {
            Name = name;
            Path = path;
            Enabled = enabled;
            DisplayName = displayName ?? name;
            Author = author ?? string.Empty;
            ModVersion = modVersion ?? string.Empty;
            GameVersion = gameVersion ?? string.Empty;
        }

        public ModData ToData()
        {
            return new ModData
            {
                Name = Name,
                Enabled = Enabled
            };
        }

        public static ModEntry FromData(ModData data, string basePath)
        {
            return new ModEntry
            {
                Name = data.Name,
                Path = basePath,
                Enabled = data.Enabled
            };
        }
    }

    public class ModData
    {
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
    }
}

