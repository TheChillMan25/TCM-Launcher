using System.IO;
using System.Net.Http;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;

namespace TCM_Launcher.Services
{
    public class DownloadService : IDownloadService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public async Task<bool> DownloadFileAsync(string url, string destinationPath, string fileName)
        {
            if(!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            string targetDestination = Path.Combine(destinationPath, fileName);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                using var response = await httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                using var read = await response.Content.ReadAsStreamAsync();
                using var write = File.Open(targetDestination, FileMode.Create);

                await read.CopyToAsync(write);
                return true;
            }
            catch (Exception ex) 
            {
                Logger.Error("There was an exception during downloading file.", ex);
                return false;
            }
        }
    }
}
