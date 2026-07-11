using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TCM_Launcher.Core.Utils
{
    public static class PathUtils
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        public static string GetSafePath(string longPath)
        {
            if (!Directory.Exists(longPath))
            {
                Directory.CreateDirectory(longPath);
            }

            StringBuilder shortPath = new StringBuilder(255);
            GetShortPathName(longPath, shortPath, shortPath.Capacity);

            string result = shortPath.ToString();
            return string.IsNullOrEmpty(result) ? longPath : result;
        }
    }
}
