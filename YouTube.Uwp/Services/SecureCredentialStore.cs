using System;
using Windows.Security.Credentials;

namespace YouTube.Uwp.Services
{
    internal static class SecureCredentialStore
    {
        private const int ElementNotFoundHResult = unchecked((int)0x80070490);
        private static readonly PasswordVault Vault = new PasswordVault();

        public static string Read(string resource, string userName)
        {
            try
            {
                PasswordCredential credential = Vault.Retrieve(resource, userName);
                credential.RetrievePassword();
                return credential.Password;
            }
            catch (Exception exception) when (exception.HResult == ElementNotFoundHResult)
            {
                return null;
            }
        }

        public static void Write(string resource, string userName, string value)
        {
            string previousValue = Read(resource, userName);
            if (previousValue != null)
            {
                Vault.Remove(Vault.Retrieve(resource, userName));
            }

            Vault.Add(new PasswordCredential(resource, userName, value));
        }

        public static void Delete(string resource, string userName)
        {
            try
            {
                Vault.Remove(Vault.Retrieve(resource, userName));
            }
            catch (Exception exception) when (exception.HResult == ElementNotFoundHResult)
            {
                // Missing credentials are already in the requested state.
            }
        }
    }
}
