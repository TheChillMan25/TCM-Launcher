using System;
using System.Globalization;
using System.Windows.Data;

namespace TCM_Launcher.Core.Utils.Converters
{
    public class DownloadCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double num = 0;
            if (value is uint u) num = u;
            else if (value is int i) num = i;
            else if (value is long l) num = l;
            else return "0";

            if (num >= 1_000_000_000)
                return (num / 1_000_000_000D).ToString("0.#", CultureInfo.InvariantCulture) + "B";

            if (num >= 1_000_000)
                return (num / 1_000_000D).ToString("0.#", CultureInfo.InvariantCulture) + "M";

            if (num >= 1_000)
                return (num / 1_000D).ToString("0.#", CultureInfo.InvariantCulture) + "K";

            return num.ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}