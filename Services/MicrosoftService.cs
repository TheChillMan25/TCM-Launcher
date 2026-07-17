using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using TCM_Launcher.Core.Utils;
using TCM_Launcher.Interfaces;

namespace TCM_Launcher.Services
{
    public class MicrosoftService : IMicrosoftService
    {

        private JELoginHandler LoginHandler;

        public MSession MSession { get; set; }

        public async Task<MSession> MicrosoftLoginAsync()
        {
            string tmpFilePath = Path.GetTempFileName();
            try
            {
                if (File.Exists(Constants.AccountsJSONPath))
                {
                    byte[] encryptedBytes = File.ReadAllBytes(Constants.AccountsJSONPath);
                    byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

                    File.WriteAllBytes(tmpFilePath, decryptedBytes);
                }

                var httpClient = new HttpClient();
                LoginHandler = new JELoginHandlerBuilder()
                    .WithHttpClient(httpClient)
                    .WithAccountManager(tmpFilePath)
                    .Build();

                MSession = await LoginHandler.Authenticate();
                return MSession;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception signing in to microsoft account.", ex);
                return null;
            }
            finally
            {
                if (File.Exists(tmpFilePath))
                {
                    try
                    {
                        byte[] rawBytes = File.ReadAllBytes(tmpFilePath);
                        if (rawBytes.Length > 0)
                        {
                            byte[] newlyEncryptedBytes = ProtectedData.Protect(rawBytes, null, DataProtectionScope.CurrentUser);
                            File.WriteAllBytes(Constants.AccountsJSONPath, newlyEncryptedBytes);
                        }
                    }
                    finally
                    {
                        File.Delete(tmpFilePath);
                    }
                }
            }
        }

        public async Task<bool> MicrosoftSignOutAsync()
        {
            try
            {
                await LoginHandler.Signout();
                if (File.Exists(Constants.AccountsJSONPath))
                {
                    File.Delete(Constants.AccountsJSONPath);
                }
                MSession = null;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("There was an exception signing out of microsoft account.", ex);
                return false;
            }
        }
    }
}
