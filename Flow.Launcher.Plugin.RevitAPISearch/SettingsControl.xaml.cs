using System.Windows;
using System.Windows.Controls;

namespace Flow.Launcher.Plugin.RevitAPISearch
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private readonly Settings _settings;
        private readonly RevitAPISearch _plugin;

        public SettingsControl(RevitAPISearch plugin, Settings settings)
        {
            InitializeComponent();
            _plugin = plugin;
            _settings = settings;

            VersionComboBox.SelectedIndex = VersionToIndex(_settings.DefaultVersion);
        }

        private static int VersionToIndex(string version)
        {
            switch ($"R{version}")
            {
                case "R2022": return 0;
                case "R2023": return 1;
                case "R2024": return 2;
                case "R2025": return 3;
                case "R2025.3": return 4;
                case "R2026": return 5;
                default: return 1;
            }
        }

        private void VersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VersionComboBox.SelectedItem is ComboBoxItem item)
            {
                var text = item.Content.ToString();
                if (text.StartsWith("R"))
                    text = text.Substring(1);
                _settings.DefaultVersion = text;
                _plugin.SaveSettings();
            }
        }
    }
}

