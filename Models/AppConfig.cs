using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace IEVRModManager.Models
{
    public class AppConfig : INotifyPropertyChanged
    {
        private string _gamePath = string.Empty;
        private string _cfgBinPath = string.Empty;
        private string _violaCliPath = string.Empty;
        private string _tmpDir = string.Empty;
        private string _selectedCpkName = string.Empty;
        private string _lastKnownPacksSignature = string.Empty;
        private string _lastKnownSteamBuildId = string.Empty;
        private DateTime _vanillaFallbackUntilUtc = DateTime.MinValue;
        private string _theme = "System";
        private string _language = "System";
        private string _lastAppliedProfile = string.Empty;

        public string GamePath
        {
            get => _gamePath;
            set { _gamePath = value; OnPropertyChanged(); }
        }

        public string CfgBinPath
        {
            get => _cfgBinPath;
            set { _cfgBinPath = value; OnPropertyChanged(); }
        }

        public string ViolaCliPath
        {
            get => _violaCliPath;
            set { _violaCliPath = value; OnPropertyChanged(); }
        }

        public string SelectedCpkName
        {
            get => _selectedCpkName;
            set { _selectedCpkName = value; OnPropertyChanged(); }
        }

        public string TmpDir
        {
            get => _tmpDir;
            set { _tmpDir = value; OnPropertyChanged(); }
        }

        public string LastKnownPacksSignature
        {
            get => _lastKnownPacksSignature;
            set { _lastKnownPacksSignature = value; OnPropertyChanged(); }
        }

        public string LastKnownSteamBuildId
        {
            get => _lastKnownSteamBuildId;
            set { _lastKnownSteamBuildId = value; OnPropertyChanged(); }
        }

        public DateTime VanillaFallbackUntilUtc
        {
            get => _vanillaFallbackUntilUtc;
            set { _vanillaFallbackUntilUtc = value; OnPropertyChanged(); }
        }

        public string Theme
        {
            get => _theme;
            set { _theme = value; OnPropertyChanged(); }
        }

        public string Language
        {
            get => _language;
            set { _language = value; OnPropertyChanged(); }
        }

        public string LastAppliedProfile
        {
            get => _lastAppliedProfile;
            set { _lastAppliedProfile = value; OnPropertyChanged(); }
        }

        public List<ModData> Mods { get; set; } = new List<ModData>();

        public static AppConfig Default()
        {
            return new AppConfig
            {
                GamePath = string.Empty,
                CfgBinPath = string.Empty,
                ViolaCliPath = string.Empty,
                SelectedCpkName = string.Empty,
                LastKnownPacksSignature = string.Empty,
                LastKnownSteamBuildId = string.Empty,
                VanillaFallbackUntilUtc = DateTime.MinValue,
                TmpDir = Config.DefaultTmpDir,
                Theme = "System",
                Language = "System",
                LastAppliedProfile = string.Empty,
                Mods = new List<ModData>()
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

