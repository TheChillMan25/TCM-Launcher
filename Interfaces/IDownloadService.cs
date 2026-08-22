namespace TCM_Launcher.Interfaces
{
    public interface IDownloadService
    {
        Task DownloadFileAsync(string url, string destinationPath, string fileName);
    }
}
