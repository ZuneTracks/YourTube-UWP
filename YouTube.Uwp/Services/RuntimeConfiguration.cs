using System;
using Windows.Storage;

namespace YouTube.Uwp.Services
{
    public sealed class RuntimeConfiguration
    {
        private const string ApiKeyResource = "YourTube.ApiKey";
        private const string ApiKeyUserName = "PublicReadOnly";
        private const string OAuthClientIdKey = "OAuthClientId";
        private const string RedirectProtocolKey = "RedirectProtocol";

        public string OAuthClientId
        {
            get { return ReadSetting(OAuthClientIdKey); }
        }

        public string RedirectProtocol
        {
            get { return ReadSetting(RedirectProtocolKey) ?? OAuthPkceService.PackagedRedirectProtocol; }
        }

        public bool HasApiKey
        {
            get { return !string.IsNullOrWhiteSpace(GetApiKey()); }
        }

        public string GetApiKey()
        {
            return SecureCredentialStore.Read(ApiKeyResource, ApiKeyUserName);
        }

        public void SaveApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Enter an API key or use Clear API key.", "apiKey");
            }

            SecureCredentialStore.Write(ApiKeyResource, ApiKeyUserName, apiKey.Trim());
        }

        public void ClearApiKey()
        {
            SecureCredentialStore.Delete(ApiKeyResource, ApiKeyUserName);
        }

        public void SaveOAuthSettings(string clientId, string redirectProtocol)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("An OAuth client ID is required.", "clientId");
            }

            if (!OAuthPkceService.IsValidProtocolScheme(redirectProtocol))
            {
                throw new ArgumentException("The redirect protocol must be a valid URI scheme.", "redirectProtocol");
            }

            if (!string.Equals(redirectProtocol.Trim(), OAuthPkceService.PackagedRedirectProtocol, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "The redirect protocol must match the protocol declared in Package.appxmanifest. Change both package and source before shipping.",
                    "redirectProtocol");
            }

            SaveSetting(OAuthClientIdKey, clientId.Trim());
            SaveSetting(RedirectProtocolKey, redirectProtocol.Trim().ToLowerInvariant());
        }

        private static string ReadSetting(string key)
        {
            object value;
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out value))
            {
                return value as string;
            }

            return null;
        }

        private static void SaveSetting(string key, string value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }
    }
}
