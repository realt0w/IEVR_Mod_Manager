using System.Windows;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    public partial class PendingChangesWindow : Window
    {
        public bool UserChoseContinue { get; private set; }

        public PendingChangesWindow(Window owner, string message)
        {
            InitializeComponent();
            Owner = owner;
            
            // Update localized texts
            UpdateLocalizedTexts(message);
            
            // Ensure texts are updated after window is loaded
            Loaded += (s, e) => UpdateLocalizedTexts(message);
        }
        
        private void UpdateLocalizedTexts(string message)
        {
            Title = LocalizationHelper.GetString("ModsNotApplied");
            TitleText.Text = LocalizationHelper.GetString("PendingChangesDetected");
            MessageText.Text = message;
            ContinueButton.Content = LocalizationHelper.GetString("ContinueAnyway");
            CancelButton.Content = LocalizationHelper.GetString("Cancel");
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            UserChoseContinue = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            UserChoseContinue = false;
            DialogResult = false;
            Close();
        }
    }
}
