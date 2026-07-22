using System;
using System.Collections.Generic;
using System.Text;
using TCML_Class_library;

namespace TCM_Launcher.Interfaces
{
    public interface IDownloadService
    {
        Task DownloadFileAsync(string url, string destinationPath, string fileName, IProgress<double>? progress = null);
    }
}
