using CmlLib.Core.Auth.Microsoft.Sessions;
using System.Net.Http;
using System.Security.Policy;
using System.Text.Json;
using System.Windows;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCML_Class_library;

namespace TCM_Launcher.Services
{
    public class BackendService : IBackendService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        #if DEBUG
        private const string BaseUrl = "http://localhost:7050/api";
        #else
        private const string BaseUrl = "https://tcm-launcher-api-ceeda5a8hng9hebv.francecentral-01.azurewebsites.net/api";
        #endif
        private JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<List<ModSearchResult>> SearchModsAsync(string query, string version, CancellationToken cToken = default)
        {
            string encodedQuery = Uri.EscapeDataString(query);
            string encodedMCVersion = Uri.EscapeDataString(version);

            string url = $"{BaseUrl}/SearchMods?query={encodedQuery}&version={encodedMCVersion}";

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url, cToken);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                List<ModSearchResult> searchResult = JsonSerializer.Deserialize<List<ModSearchResult>>(jsonResponse, jsonSerializerOptions);

                return searchResult ?? new List<ModSearchResult>();
            }
            catch (OperationCanceledException ex)
            {
                return new List<ModSearchResult>();
            }
            catch (Exception ex)
            {
                Logger.Error("There was an error during searching mods.", ex);
                return new List<ModSearchResult>();
            }
        }

        public async Task<ModDetails?> GetModDetailsAsync(string projectId, string? modrinthId, string? curseforgeId, string version, ModSource source, bool needDesc = true)
        {
            string encodedQuery = Uri.EscapeDataString(projectId);
            string encodedMCVersion = Uri.EscapeDataString(version);

            string url = $"{BaseUrl}/GetModDetails?query={encodedQuery}&modrinthId={modrinthId}&curseforgeId={curseforgeId}&mcVersion={encodedMCVersion}&source={source}&needDesc={needDesc}";

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();

                var modDetails = JsonSerializer.Deserialize<ModDetails>(stream, jsonSerializerOptions);

                return modDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
