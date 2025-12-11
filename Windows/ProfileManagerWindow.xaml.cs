using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using IEVRModManager.Helpers;
using IEVRModManager.Managers;
using IEVRModManager.Models;

namespace IEVRModManager.Windows
{
    public partial class ProfileManagerWindow : Window
    {
        private readonly ProfileManager _profileManager;
        private readonly ObservableCollection<ProfileViewModel> _profiles;
        public ModProfile? SelectedProfile { get; private set; }
        public bool ProfileLoaded { get; private set; }

        public ProfileManagerWindow(Window owner)
        {
            InitializeComponent();
            Owner = owner;
            
            _profileManager = new ProfileManager();
            _profiles = new ObservableCollection<ProfileViewModel>();
            
            ProfilesListBox.ItemsSource = _profiles;
            
            UpdateLocalizedTexts();
            LoadProfiles();
            
            ProfilesListBox.SelectionChanged += ProfilesListBox_SelectionChanged;
        }

        private void UpdateLocalizedTexts()
        {
            Title = LocalizationHelper.GetString("ModProfiles");
            TitleLabel.Content = LocalizationHelper.GetString("ModProfiles");
            LoadButton.Content = LocalizationHelper.GetString("Load");
            SaveButton.Content = LocalizationHelper.GetString("Save");
            DeleteButton.Content = LocalizationHelper.GetString("Delete");
            CloseButton.Content = LocalizationHelper.GetString("Close");
        }

        private void LoadProfiles()
        {
            _profiles.Clear();
            var profiles = _profileManager.GetAllProfiles();
            
            foreach (var profile in profiles)
            {
                var enabledCount = profile.Mods.Count(m => m.Enabled);
                var totalCount = profile.Mods.Count;
                _profiles.Add(new ProfileViewModel
                {
                    Name = profile.Name,
                    CreatedDate = profile.CreatedDate,
                    LastModifiedDate = profile.LastModifiedDate,
                    ModsCount = $"{enabledCount}/{totalCount} mods enabled",
                    Profile = profile
                });
            }
        }

        private void ProfilesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProfilesListBox.SelectedItem is ProfileViewModel selected)
            {
                ProfileNameTextBox.Text = selected.Name;
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesListBox.SelectedItem is ProfileViewModel selected)
            {
                SelectedProfile = selected.Profile;
                ProfileLoaded = true;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(LocalizationHelper.GetString("SelectProfileToLoad"), 
                    LocalizationHelper.GetString("ModProfiles"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var profileName = ProfileNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(profileName))
            {
                MessageBox.Show(LocalizationHelper.GetString("EnterProfileName"), 
                    LocalizationHelper.GetString("ModProfiles"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if profile name already exists
            if (_profileManager.ProfileExists(profileName) && 
                ProfilesListBox.SelectedItem is ProfileViewModel selected && 
                selected.Name != profileName)
            {
                var result = MessageBox.Show(
                    string.Format(LocalizationHelper.GetString("ProfileExistsOverwrite"), profileName),
                    LocalizationHelper.GetString("ModProfiles"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // Get current mod configuration from parent window
            if (Owner is MainWindow mainWindow)
            {
                var profile = mainWindow.CreateProfileFromCurrentState(profileName);
                
                if (_profileManager.SaveProfile(profile))
                {
                    MessageBox.Show(string.Format(LocalizationHelper.GetString("ProfileSaved"), profileName), 
                        LocalizationHelper.GetString("ModProfiles"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProfiles();
                    ProfileNameTextBox.Text = string.Empty;
                }
                else
                {
                    MessageBox.Show(LocalizationHelper.GetString("ErrorSavingProfile"), 
                        LocalizationHelper.GetString("ModProfiles"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesListBox.SelectedItem is ProfileViewModel selected)
            {
                var result = MessageBox.Show(
                    string.Format(LocalizationHelper.GetString("ConfirmDeleteProfile"), selected.Name),
                    LocalizationHelper.GetString("ModProfiles"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    if (_profileManager.DeleteProfile(selected.Name))
                    {
                        LoadProfiles();
                        ProfileNameTextBox.Text = string.Empty;
                    }
                    else
                    {
                        MessageBox.Show(LocalizationHelper.GetString("ErrorDeletingProfile"), 
                            LocalizationHelper.GetString("ModProfiles"),
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(LocalizationHelper.GetString("SelectProfileToDelete"), 
                    LocalizationHelper.GetString("ModProfiles"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string ModsCount { get; set; } = string.Empty;
        public ModProfile Profile { get; set; } = null!;
        
        public string LastModifiedDateDisplay => LastModifiedDate.ToString("yyyy-MM-dd HH:mm");
        
        public override string ToString() => Name;
    }
}

