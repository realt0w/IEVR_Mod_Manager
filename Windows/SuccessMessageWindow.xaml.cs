using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace IEVRModManager.Windows
{
    public partial class SuccessMessageWindow : Window
    {
        public SuccessMessageWindow(Window parent, string message, List<string>? modNames = null)
        {
            InitializeComponent();
            Owner = parent;
            MessageTextBlock.Text = message;
            
            // Mostrar lista de mods si se proporciona
            if (modNames != null && modNames.Any())
            {
                ModsListControl.ItemsSource = modNames;
            }
            else
            {
                // Ocultar la lista si no hay mods
                ModsListControl.Visibility = Visibility.Collapsed;
            }
            
            // Cerrar con Enter o Escape
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                {
                    Close();
                }
            };
            
            // Focus en el botón OK
            Loaded += (s, e) => Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

