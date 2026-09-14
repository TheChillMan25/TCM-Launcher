using Google.Cloud.Firestore;

namespace TCM_Launcher.Core.Utils.Converters
{
    public class TimestampConverter
    {
        public static string ConvertToString(Timestamp timestamp)
        {
            DateTime date = timestamp.ToDateTime();
            TimeSpan timeSpan = DateTime.Now - date;

            if (timeSpan.TotalSeconds < 60)
            {
                return "Now";
            }

            if (timeSpan.TotalMinutes < 60)
            {
                int minutes = (int)timeSpan.TotalMinutes;
                return $"{minutes} mins ago";
            }

            if (timeSpan.TotalHours < 24 && date.Date == DateTime.Today)
            {
                int hours = (int)timeSpan.TotalHours;
                return $"{hours} hours ago";
            }

            if (date.Date == DateTime.Today.AddDays(-1))
            {
                return $"Yesterday {date:HH:mm}";
            }

            if (timeSpan.TotalDays < 7)
            {
                int days = (int)timeSpan.TotalDays;
                return $"{days} days ago";
            }

            return date.ToString("yyyy. MM. dd. HH:mm"); ;
        }
    }
}
