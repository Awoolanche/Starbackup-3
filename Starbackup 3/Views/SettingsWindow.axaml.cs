using Avalonia.Controls;

namespace Starbackup_3.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }
        private void CloseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
    }
}
