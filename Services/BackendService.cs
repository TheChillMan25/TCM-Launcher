using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;
using TCM_Launcher.Model;
using TCM_Launcher.Model.DB;
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

        private readonly IMicrosoftService microsoftService;
        private readonly IDownloadService downloadService;
        public BackendService(IMicrosoftService microsoftService, IDownloadService downloadService)
        {
            this.microsoftService = microsoftService;
            this.downloadService = downloadService;
        }

        public Func<string, bool, FirestoreModpack, bool, GameProfile?, Task> OnModpackDownloadedFromStorage { get; set; }

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
                Logger.Error("There was an error during getting mod details.", ex);
                return null;
            }
        }

        public async Task<FirebaseTokenData?> GetCustomTokenAsync()
        {
            string url = $"{BaseUrl}/auth/firebase-token";
            string accessToken = microsoftService.MSession.AccessToken;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<FirebaseTokenData>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when fetching token data", ex);
                return null;
            }
        }

        public async Task<List<FirestoreUser>?> SearchFriendsAsync(string username)
        {
            string url = $"{BaseUrl}/friends/search?username={username}";
            string accessToken = microsoftService.MSession.AccessToken;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<FirestoreUser>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<FirestoreRequest?> AddFriendAsync(string uuid)
        {
            string url = $"{BaseUrl}/friends/add/{uuid}";
            string accessToken = microsoftService.MSession.AccessToken;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<FirestoreRequest?>();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task UpdateFriendRequestAsync(string requestId, bool value)
        {
            string url = $"{BaseUrl}/friends/update/{requestId}/{value}";
            string accessToken = microsoftService.MSession.AccessToken;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when updating friend request.", ex);
            }
        }

        public async Task RemoveFriendAsync(string uuid)
        {
            string url = $"{BaseUrl}/friends/remove/{uuid}";
            string accessToken = microsoftService.MSession.AccessToken;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when removing friend.", ex);
            }
        }

        public async Task<FirestoreModpack?> UploadModpackAsync(string filePath, string name, string version, string? modpackId, List<string> friends, bool isUpdate = false)
        {
            string url = $"{BaseUrl}/modpack/upload";
            string accessToken = microsoftService.MSession.AccessToken;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var form = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(filePath);
                using var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

                form.Add(streamContent, "file", Path.GetFileName(filePath));
                string modpackJson = JsonSerializer.Serialize(new FirestoreModpack
                {
                    Id = modpackId,
                    Name = name,
                    Version = version,
                });
                form.Add(new StringContent(modpackJson), "modpack");
                form.Add(new StringContent(JsonSerializer.Serialize(friends)), "friends");

                request.Content = form;

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<FirestoreModpack>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when uploading modpack.", ex);
                return null;
            }
        }

        public async Task DownloadModpackAsync(FirestoreModpack modpack, bool isUpdate = false, GameProfile? profileToUpdate = null)
        {
            string tempFolder = Path.Combine(Constants.LauncherFolder, "temp");
            string fileName = Path.GetFileName(modpack.StoragePath);
            string url = GetFirebaseStoragePath("tcm-launcher.firebasestorage.app", modpack.StoragePath);

            var success = await downloadService.DownloadFileAsync(url, tempFolder, fileName);
            if (success)
            {
                string sourceFile = Path.Combine(tempFolder, fileName);
                await OnModpackDownloadedFromStorage?.Invoke(sourceFile, true, modpack, isUpdate, profileToUpdate);
            }
        }

        private string GetFirebaseStoragePath(string bucketName, string storagePath, string? token = null)
        {
            string encodedPath = Uri.EscapeDataString(storagePath);

#if DEBUG
            string BaseUrl = "http://127.0.0.1:9199/v0/b";
#else
            string BaseUrl = "https://firebasestorage.googleapis.com/v0/b";
#endif
            string url = $"{BaseUrl}/{bucketName}/o/{encodedPath}?alt=media";
            if (!string.IsNullOrEmpty(token))
            {
                url += $"&token={token}";
            }
            return url;
        }

        public async Task DeleteModpackAsync(FirestoreModpack modpack)
        {
            string url = $"{BaseUrl}/modpack/delete";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", microsoftService.MSession.AccessToken);

                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(modpack.Id), "packId");
                form.Add(new StringContent(modpack.StoragePath), "storagePath");

                request.Content = form;

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<bool> ShareModpackAsync(FirestoreModpack modpack, List<string> selected)
        {
            string url = $"{BaseUrl}/modpack/share";

            try
            {
                var form = new MultipartFormDataContent();
                string modpackJson = JsonSerializer.Serialize(modpack);
                form.Add(new StringContent(modpackJson), "modpack");
                string json = JsonSerializer.Serialize(selected);
                form.Add(new StringContent(json), "friends");

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", microsoftService.MSession.AccessToken);
                request.Content = form;

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when sharing modpack.", ex);
                return false;
            }
        }

        public async Task<FirebaseTokenResponse?> RefreshFirebaseTokenAsync(string? refreshToken)
        {
            string url = $"{BaseUrl}/auth/refresh-token";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", microsoftService.MSession.AccessToken);
                var form = new MultipartFormDataContent();
                form.Add(new StringContent(refreshToken), "refreshToken");
                request.Content = form;
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadFromJsonAsync<FirebaseTokenResponse>();
                    if(token != null)
                    {
                        return token;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception when refreshing firebase token", ex);
                return null;
            }
        }
    }
}
