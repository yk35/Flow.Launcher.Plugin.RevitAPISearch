using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.SharedCommands;

namespace Flow.Launcher.Plugin.RevitAPISearch
{
    /// <summary>
    /// Represents a plugin for searching Revit API documentation via a web-based interface.
    /// This class implements IAsyncPlugin and ISettingProvider, enabling asynchronous query processing
    /// and the ability to manage user settings.
    /// </summary>
    public class RevitAPISearch : IAsyncPlugin, ISettingProvider
    {
        private PluginInitContext _context;

        private Uri _baseUrl = new Uri("https://www.revitapidocs.com");

        private HttpClient _httpClient;
        private Settings _settings;
        private string _settingsPath;

        /// <summary>
        /// Initializes the plugin asynchronously by setting up necessary context, loading settings, and preparing resources.
        /// </summary>
        /// <param name="context">The plugin initialization context providing metadata and API functionality.</param>
        /// <returns>A task representing the asynchronous initialization operation.</returns>
        public Task InitAsync(PluginInitContext context)
        {
            _context = context;
            _httpClient = new HttpClient();

            _settingsPath = Path.Combine(_context.CurrentPluginMetadata.PluginDirectory, "settings.json");
            if (File.Exists(_settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
                catch (Exception ex)
                {
                    _context.API.LogException(this.GetType().Name, $"Failed to load settings from {_settingsPath}: {ex.Message}", ex);
                    _settings = new Settings();
                }
            }
            else
            {
                _settings = new Settings();
                SaveSettings();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes an asynchronous query against the Revit API documentation and retrieves search results.
        /// </summary>
        /// <param name="query">The query object containing search terms and parameters to process the request.</param>
        /// <param name="token">A cancellation token to signal the operation should be canceled.</param>
        /// <returns>A task that represents the asynchronous query operation, returning a list of results.</returns>
        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            string version = _settings.DefaultVersion;
            string apiString = query.FirstSearch;
            if (query.SearchTerms.Length > 1)
            {
                apiString += " ";
                apiString += query.SecondToEndSearch;
            }

            if (string.IsNullOrEmpty(apiString))
            {
                return new List<Result>();
            }

            var httpQuery = new Uri(_baseUrl, $"/{version}/search?query={apiString}");

            var httpResponse = await _httpClient.GetAsync(httpQuery, token);
            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var content = await httpResponse.Content.ReadFromJsonAsync<RevitApiDocResult>(options: null, cancellationToken:token);
                if (content == null)
                {
                    return new List<Result>();
                }

                List<Result> queryResults = new List<Result>();

                foreach (var r in content.results)
                {
                    queryResults.Add(new Result()
                    {
                        Title = r["title"].ToString(),
                        SubTitle = r["description"].ToString(),
                        AutoCompleteText = r["short_title"].ToString(),
                        Action = (context) =>
                        {
                            if (context.SpecialKeyState.CtrlPressed)
                            {
                                SearchWeb.OpenInBrowserWindow(new Uri(_baseUrl, $"/{content.target_year}/{r["href"]}").AbsoluteUri);
                            }
                            else
                            {
                                SearchWeb.OpenInBrowserTab(new Uri(_baseUrl, $"/{content.target_year}/{r["href"]}").AbsoluteUri);
                            }

                            return true;
                        }

                    });
                }

                return queryResults;

            }
            else
            {
                return new List<Result>();
            }
        }


        record RevitApiDocResult(
            int max_result,
            string query,
            List<Dictionary<string, object>> results,
            string target_year,
            int total_results,
            bool truncated);

        /// <summary>
        /// Saves the current plugin settings to a persistent storage file.
        /// This method serializes the settings object and writes it to the configured file path.
        /// </summary>
        public void SaveSettings()
        {
            if (string.IsNullOrEmpty(_settingsPath))
                return;

            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // ignore persistence errors
            }
        }

        /// <summary>
        /// Creates and returns a settings panel for the plugin, facilitating user configuration of plugin settings.
        /// </summary>
        /// <returns>A Control representing the settings panel UI.</returns>
        public Control CreateSettingPanel()
        {
            return new SettingsControl(this, _settings);
        }

    }
}