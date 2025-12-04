using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IEVRModManager.Models;

namespace IEVRModManager.Managers
{
    public class ModManager
    {
        private readonly string _modsDir;

        public ModManager(string? modsDir = null)
        {
            _modsDir = modsDir ?? Config.DefaultModsDir;
        }

        public List<ModEntry> ScanMods(List<ModData>? savedMods = null, 
            List<ModEntry>? existingEntries = null)
        {
            var modsRoot = Path.GetFullPath(_modsDir);
            Directory.CreateDirectory(modsRoot);

            // Get all mod directories
            var modNames = Directory.GetDirectories(modsRoot)
                .Select(d => Path.GetFileName(d))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            // Preserve enabled state from existing entries
            var oldMap = (existingEntries ?? new List<ModEntry>())
                .ToDictionary(me => me.Name, me => me.Enabled);

            // Preserve enabled state from saved config
            var savedMap = (savedMods ?? new List<ModData>())
                .ToDictionary(m => m.Name, m => m.Enabled);

            // Preserve order from saved config
            var savedOrder = (savedMods ?? new List<ModData>())
                .Select(m => m.Name)
                .ToList();

            var orderedNames = savedOrder
                .Where(n => modNames.Contains(n))
                .Concat(modNames.Where(n => !savedOrder.Contains(n)))
                .ToList();

            // Create mod entries
            var modEntries = new List<ModEntry>();
            foreach (var name in orderedNames)
            {
                var modPath = Path.Combine(modsRoot, name);
                var modData = LoadModMetadata(modPath);

                // Determine enabled state (priority: existing > saved > default)
                bool enabled;
                if (oldMap.ContainsKey(name))
                {
                    enabled = oldMap[name];
                }
                else if (savedMap.ContainsKey(name))
                {
                    enabled = savedMap[name];
                }
                else
                {
                    enabled = true;
                }

                var modEntry = new ModEntry(
                    name: name,
                    path: modsRoot,
                    enabled: enabled,
                    displayName: modData.DisplayName,
                    author: modData.Author,
                    modVersion: modData.ModVersion,
                    gameVersion: modData.GameVersion
                );
                modEntries.Add(modEntry);
            }

            return modEntries;
        }

        private ModMetadata LoadModMetadata(string modPath)
        {
            var modDataPath = Path.Combine(modPath, "mod_data.json");
            var metadata = new ModMetadata
            {
                DisplayName = Path.GetFileName(modPath),
                Author = string.Empty,
                ModVersion = string.Empty,
                GameVersion = string.Empty
            };

            if (!File.Exists(modDataPath))
            {
                return metadata;
            }

            try
            {
                var json = File.ReadAllText(modDataPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (data != null)
                {
                    if (data.TryGetValue("Name", out var nameElement))
                        metadata.DisplayName = nameElement.GetString() ?? metadata.DisplayName;

                    if (data.TryGetValue("Author", out var authorElement))
                        metadata.Author = authorElement.GetString() ?? string.Empty;

                    if (data.TryGetValue("ModVersion", out var versionElement))
                        metadata.ModVersion = versionElement.GetString() ?? string.Empty;

                    if (data.TryGetValue("GameVersion", out var gameVersionElement))
                        metadata.GameVersion = gameVersionElement.GetString() ?? string.Empty;
                }
            }
            catch
            {
                // Ignore errors, return default metadata
            }

            return metadata;
        }

        public List<string> GetEnabledMods(List<ModEntry> modEntries)
        {
            return modEntries
                .Where(me => me.Enabled)
                .Select(me => me.FullPath)
                .ToList();
        }

        private class ModMetadata
        {
            public string DisplayName { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string ModVersion { get; set; } = string.Empty;
            public string GameVersion { get; set; } = string.Empty;
        }
    }
}

