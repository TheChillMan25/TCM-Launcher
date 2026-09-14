namespace TCM_Launcher.Interfaces
{
    public interface IDownloadService
    {
        Task<bool> DownloadFileAsync(string url, string destinationPath, string fileName);
    }
}
