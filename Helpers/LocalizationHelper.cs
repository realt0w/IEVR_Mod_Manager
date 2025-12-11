using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using YamlDotNet.Serialization;

namespace IEVRModManager.Helpers
{
    public static class LocalizationHelper
    {
        private static Dictionary<string, string>? _currentStrings;
        private static Dictionary<string, Dictionary<string, string>>? _allStrings;
        private static CultureInfo? _currentCulture;
        private static readonly object _lockObject = new object();

        static LocalizationHelper()
        {
            LoadStrings();
        }

        private static void LoadStrings()
        {
            lock (_lockObject)
            {
                _allStrings = new Dictionary<string, Dictionary<string, string>>();
                
                try
                {
                    // First, try to load from file system (for development and non-single-file apps)
                    var baseDir = AppContext.BaseDirectory;
                    var resourcesDir = Path.Combine(baseDir, "Resources");
                    
                    System.Diagnostics.Debug.WriteLine($"[Localization] Trying to load from: {resourcesDir}");
                    
                    // If Resources folder doesn't exist in base directory, try AppDomain base directory as fallback
                    if (!Directory.Exists(resourcesDir))
                    {
                        var fallbackDir = AppDomain.CurrentDomain.BaseDirectory;
                        var fallbackResourcesDir = Path.Combine(fallbackDir, "Resources");
                        System.Diagnostics.Debug.WriteLine($"[Localization] Fallback directory: {fallbackResourcesDir}");
                        if (Directory.Exists(fallbackResourcesDir))
                        {
                            resourcesDir = fallbackResourcesDir;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[Localization] Using resources directory: {resourcesDir}");
                    System.Diagnostics.Debug.WriteLine($"[Localization] Directory exists: {Directory.Exists(resourcesDir)}");

                    bool loadedFromFiles = false;

                    // Try to load from file system first
                    var defaultPath = Path.Combine(resourcesDir, "Strings.yaml");
                    var spanishPath = Path.Combine(resourcesDir, "Strings.es-ES.yaml");
                    
                    if (File.Exists(defaultPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Localization] Loading English from file: {defaultPath}");
                        _allStrings["en-US"] = LoadYamlFile(defaultPath);
                        System.Diagnostics.Debug.WriteLine($"[Localization] Loaded {_allStrings["en-US"].Count} English strings from file");
                        loadedFromFiles = true;
                    }

                    if (File.Exists(spanishPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Localization] Loading Spanish from file: {spanishPath}");
                        _allStrings["es-ES"] = LoadYamlFile(spanishPath);
                        System.Diagnostics.Debug.WriteLine($"[Localization] Loaded {_allStrings["es-ES"].Count} Spanish strings from file");
                        loadedFromFiles = true;
                    }

                    // If files not found, try loading from embedded resources (for single-file apps)
                    if (!loadedFromFiles)
                    {
                        System.Diagnostics.Debug.WriteLine("[Localization] Files not found in file system, trying embedded resources...");
                        
                        // Load English from embedded resource
                        var englishResource = LoadYamlFromEmbeddedResource("IEVRModManager.Resources.Strings.yaml");
                        if (englishResource != null && englishResource.Count > 0)
                        {
                            _allStrings["en-US"] = englishResource;
                            System.Diagnostics.Debug.WriteLine($"[Localization] Loaded {englishResource.Count} English strings from embedded resource");
                        }

                        // Load Spanish from embedded resource
                        var spanishResource = LoadYamlFromEmbeddedResource("IEVRModManager.Resources.Strings.es-ES.yaml");
                        if (spanishResource != null && spanishResource.Count > 0)
                        {
                            _allStrings["es-ES"] = spanishResource;
                            System.Diagnostics.Debug.WriteLine($"[Localization] Loaded {spanishResource.Count} Spanish strings from embedded resource");
                        }
                    }

                    // If still no strings loaded, create default empty dictionary
                    if (_allStrings.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("[Localization] WARNING: No localization files found in file system or embedded resources!");
                        _allStrings["en-US"] = new Dictionary<string, string>();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Localization] Error loading localization files: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[Localization] Stack trace: {ex.StackTrace}");
                    _allStrings["en-US"] = new Dictionary<string, string>();
                }
            }
        }

        private static Dictionary<string, string>? LoadYamlFromEmbeddedResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                
                // First, try the exact resource name
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Localization] Found embedded resource: {resourceName}");
                        return LoadYamlFromStream(stream);
                    }
                }

                // If not found, try alternative resource names
                System.Diagnostics.Debug.WriteLine($"[Localization] Embedded resource '{resourceName}' not found, trying alternatives...");
                var alternativeNames = new[]
                {
                    resourceName.Replace("IEVRModManager.", ""),
                    resourceName.Replace("IEVRModManager.Resources.", "Resources."),
                    resourceName.Replace("IEVRModManager.Resources.", ""),
                    resourceName.Replace("IEVRModManager.", "IEVRModManager.Resources.")
                };
                
                foreach (var altName in alternativeNames)
                {
                    System.Diagnostics.Debug.WriteLine($"[Localization] Trying alternative resource name: {altName}");
                    using (var altStream = assembly.GetManifestResourceStream(altName))
                    {
                        if (altStream != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Localization] Found resource with alternative name: {altName}");
                            return LoadYamlFromStream(altStream);
                        }
                    }
                }
                
                // If still not found, search all resources for matching file names
                var allResources = assembly.GetManifestResourceNames();
                System.Diagnostics.Debug.WriteLine($"[Localization] Searching through {allResources.Length} embedded resources...");
                
                var fileName = Path.GetFileName(resourceName);
                foreach (var resName in allResources)
                {
                    if (resName.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Localization] Found matching resource by filename: {resName}");
                        using (var foundStream = assembly.GetManifestResourceStream(resName))
                        {
                            if (foundStream != null)
                            {
                                return LoadYamlFromStream(foundStream);
                            }
                        }
                    }
                }
                
                // List all available resources for debugging
                System.Diagnostics.Debug.WriteLine($"[Localization] Available embedded resources: {string.Join(", ", allResources)}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Localization] Error loading embedded resource '{resourceName}': {ex.Message}");
                return null;
            }
        }

        private static Dictionary<string, string> LoadYamlFromStream(Stream stream)
        {
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    var yamlContent = reader.ReadToEnd();
                    var deserializer = new DeserializerBuilder()
                        .Build();
                    
                    var yamlDict = deserializer.Deserialize<Dictionary<string, string>>(yamlContent);
                    return yamlDict ?? new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading YAML from stream: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        private static Dictionary<string, string> LoadYamlFile(string filePath)
        {
            try
            {
                var yamlContent = File.ReadAllText(filePath);
                var deserializer = new DeserializerBuilder()
                    .Build();
                
                var yamlDict = deserializer.Deserialize<Dictionary<string, string>>(yamlContent);
                return yamlDict ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading YAML file {filePath}: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        public static void SetLanguage(string languageCode)
        {
            try
            {
                CultureInfo culture;
                string langKey;
                
                System.Diagnostics.Debug.WriteLine($"[Localization] SetLanguage called with: {languageCode}");
                
                if (string.IsNullOrWhiteSpace(languageCode) || languageCode == "System")
                {
                    culture = CultureInfo.CurrentUICulture;
                    langKey = culture.Name;
                    System.Diagnostics.Debug.WriteLine($"[Localization] Using system language: {langKey}");
                }
                else
                {
                    culture = new CultureInfo(languageCode);
                    langKey = languageCode;
                    System.Diagnostics.Debug.WriteLine($"[Localization] Using specified language: {langKey}");
                }

                _currentCulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                // Load strings for the selected language
                lock (_lockObject)
                {
                    if (_allStrings == null)
                    {
                        LoadStrings();
                    }

                    System.Diagnostics.Debug.WriteLine($"[Localization] Available languages: {(_allStrings != null ? string.Join(", ", _allStrings.Keys) : "null")}");
                    System.Diagnostics.Debug.WriteLine($"[Localization] Looking for language key: {langKey}");

                    // Try to get strings for the language, fallback to en-US if not found
                    if (_allStrings != null && _allStrings.TryGetValue(langKey, out var strings))
                    {
                        _currentStrings = strings;
                        System.Diagnostics.Debug.WriteLine($"[Localization] Loaded {strings.Count} strings for {langKey}");
                    }
                    else if (_allStrings != null && _allStrings.TryGetValue("en-US", out var defaultStrings))
                    {
                        _currentStrings = defaultStrings;
                        System.Diagnostics.Debug.WriteLine($"[Localization] Fallback to English, loaded {defaultStrings.Count} strings");
                    }
                    else
                    {
                        _currentStrings = new Dictionary<string, string>();
                        System.Diagnostics.Debug.WriteLine("[Localization] WARNING: No strings loaded!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Localization] Error in SetLanguage: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Localization] Stack trace: {ex.StackTrace}");
                // Fallback to default culture if language code is invalid
                _currentCulture = CultureInfo.CurrentUICulture;
                lock (_lockObject)
                {
                    if (_allStrings != null && _allStrings.TryGetValue("en-US", out var defaultStrings))
                    {
                        _currentStrings = defaultStrings;
                    }
                }
            }
        }

        public static string GetString(string key)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_currentStrings == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Localization] GetString('{key}') - _currentStrings is null, initializing...");
                        if (_allStrings == null)
                        {
                            LoadStrings();
                        }
                        if (_allStrings != null && _allStrings.TryGetValue("en-US", out var defaultStringsInit))
                        {
                            _currentStrings = defaultStringsInit;
                        }
                    }

                    if (_currentStrings != null && _currentStrings.TryGetValue(key, out var value))
                    {
                        return value;
                    }

                    // Fallback to English if key not found in current language
                    if (_allStrings != null && _allStrings.TryGetValue("en-US", out var defaultStringsFallback))
                    {
                        if (defaultStringsFallback.TryGetValue(key, out var defaultValue))
                        {
                            return defaultValue;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[Localization] GetString('{key}') - Key not found, returning key as-is");
                    return key;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Localization] Error in GetString('{key}'): {ex.Message}");
                return key;
            }
        }

        public static string GetString(string key, params object[] args)
        {
            try
            {
                var format = GetString(key);
                return string.Format(format, args);
            }
            catch
            {
                return key;
            }
        }
    }
}

