namespace TCM_Launcher.Core.Utils.Converters
{
    public class PlaytimeConverter
    {
        public static string ConvertToString(double seconds)
        {
            if(seconds < 3600)
            {
                return $"{Math.Round(seconds / 60, 1)} minutes";
            }
            else
            {
                return $"{Math.Round(seconds / 3600,1)} hours";
            }
        }
    }
}
