using System;
using Windows.Storage;

namespace YouTube.Uwp.Services
{
    public sealed class RuntimeConfiguration
    {
        private const string ApiKeyResource = "YourTube.ApiKey";
        private const string ApiKeyUserName = "PublicReadOnly";
        private const string OAuthClientIdKey = "OAuthClientId";
        private const string OAuthRedirectUriKey = "OAuthRedirectUri";

        public string OAuthClientId
        {
            get { return ReadSetting(OAuthClientIdKey); }
        }

        public string OAuthRedirectUri
        {
            get { return ReadSetting(OAuthRedirectUriKey) ?? OAuthPkceService.PackagedRedirectUri; }
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

        public void SaveOAuthSettings(string clientId, string redirectUri)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("An OAuth client ID is required.", "clientId");
            }

            if (!OAuthPkceService.IsPackagedRedirectUri(redirectUri))
            {
                throw new ArgumentException(
                    "The redirect URI must exactly match the protocol handler declared in Package.appxmanifest: "
                    + OAuthPkceService.PackagedRedirectUri + ".",
                    "redirectUri");
            }

            SaveSetting(OAuthClientIdKey, clientId.Trim());
            SaveSetting(OAuthRedirectUriKey, OAuthPkceService.PackagedRedirectUri);
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
