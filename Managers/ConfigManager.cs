using System;
using System.IO;
using System.Text.Json;
using IEVRModManager.Models;

namespace IEVRModManager.Managers
{
    public class ConfigManager
    {
        private readonly string _configPath;

        public ConfigManager()
        {
            _configPath = Config.ConfigPath;
        }

        public AppConfig Load()
        {
            if (!File.Exists(_configPath))
            {
                return AppConfig.Default();
            }

            try
            {
                var json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return config ?? AppConfig.Default();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
                return AppConfig.Default();
            }
        }

        public bool Save(string gamePath, string cfgBinPath, string violaCliPath, 
            string tmpDir, System.Collections.Generic.List<ModEntry> modEntries)
        {
            try
            {
                var config = new AppConfig
                {
                    GamePath = gamePath,
                    CfgBinPath = cfgBinPath,
                    ViolaCliPath = violaCliPath,
                    TmpDir = tmpDir,
                    Mods = modEntries.ConvertAll(me => me.ToData())
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_configPath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
                return false;
            }
        }
    }
}

