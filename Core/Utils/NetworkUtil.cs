using System.Net.NetworkInformation;

namespace TCM_Launcher.Core.Utils
{
    public static class NetworkUtil
    {
        public static async Task<bool> IsInternetAvailableAsync()
        {
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable()) return false;

                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8");
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidServerAddress(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            string hostToValidate = input;

            if (input.Contains(':'))
            {
                var parts = input.Split(':');
                if (parts.Length != 2) return false;

                hostToValidate = parts[0];
                string portString = parts[1];

                if (!int.TryParse(portString, out int port) || port < 1 || port > 65535)
                {
                    return false;
                }
            }

            UriHostNameType hostNameType = Uri.CheckHostName(hostToValidate);
            return hostNameType != UriHostNameType.Unknown;
        }
    }
}
