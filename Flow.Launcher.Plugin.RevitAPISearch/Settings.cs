using System;

namespace Flow.Launcher.Plugin.RevitAPISearch
{
    /// <summary>
    /// Stores user configurable settings for the plugin.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The default Revit API version used for queries.
        /// Only the numeric part is stored (e.g. "2023").
        /// </summary>
        public string DefaultVersion { get; set; } = "2023";
    }
}

