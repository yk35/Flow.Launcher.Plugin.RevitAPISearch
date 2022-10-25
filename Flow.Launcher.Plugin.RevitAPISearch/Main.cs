using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.SharedCommands;

namespace Flow.Launcher.Plugin.RevitAPISearch
{
    public class RevitAPISearch : IAsyncPlugin
    {
        private PluginInitContext _context;

        private Uri _baseUrl = new Uri("https://www.revitapidocs.com");

        private HttpClient _httpClient;

        private readonly string defaultVersion = "2023";
        public Task InitAsync(PluginInitContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
            return Task.CompletedTask;
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            string version = defaultVersion;
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

    }
}