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

            VersionComboBox.SelectedIndex = VersionToIndex(_settings.DefaultVersion) >= 0 
                ? VersionToIndex(_settings.DefaultVersion) 
                : 0; // Default to the first item if the version is not found
        }

        private int VersionToIndex(string version)
        {
            string versionString = $"R{version}";
            for (int i = 0; i < VersionComboBox.Items.Count; i++)
            {
                if (VersionComboBox.Items[i] is ComboBoxItem item && item.Content.ToString() == versionString)
                {
                    return i;
                }
            }
            return -1; // Return -1 if the version is not found
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

