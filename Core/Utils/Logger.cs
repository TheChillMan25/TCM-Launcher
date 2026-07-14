using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TCM_Launcher.Core.Utils
{
    public static class Logger
    {
        private static readonly string logFilePath;
        private static readonly object logLock = new object();

        static Logger()
        {
            string logsDir = Path.Combine(Constants.LauncherFolder, "logs");

            if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);

            logFilePath = Path.Combine(logsDir, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
            Log("=== Log report ===", "SYSTEM");
        }

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                Console.WriteLine(formattedMessage);
                lock (logLock)
                {
                    File.AppendAllText(logFilePath, formattedMessage + Environment.NewLine);
                }
            }
            catch (Exception ex) {
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static void Error(string message, Exception ex =  null)
        {
            if (ex != null)
            {
                Log($"{message} | Exception: {ex.Message}\nStack Trace:\n{ex.StackTrace}", "ERROR");
            }
            else
            {
                Log(message, "ERROR");
            }
        }
    }
}
