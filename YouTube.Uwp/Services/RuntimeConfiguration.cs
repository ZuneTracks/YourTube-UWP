using System;
using Windows.Storage;

namespace YouTube.Uwp.Services
{
    public sealed class RuntimeConfiguration
    {
        private const string ApiKeyResource = "YourTube.ApiKey";
        private const string ApiKeyUserName = "PublicReadOnly";
        private const string OAuthClientIdKey = "OAuthClientId";
        private const string OAuthClientSecretResource = "YourTube.OAuthClientSecret";
        private const string OAuthClientSecretUserName = "LimitedInputDevice";

        public string OAuthClientId
        {
            get { return ReadSetting(OAuthClientIdKey); }
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

        public string GetOAuthClientSecret()
        {
            return SecureCredentialStore.Read(OAuthClientSecretResource, OAuthClientSecretUserName);
        }

        public void SaveOAuthDeviceSettings(string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("A limited-input device OAuth client ID is required.", "clientId");
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentException("A limited-input device OAuth client secret is required.", "clientSecret");
            }

            string normalizedClientId = clientId.Trim();
            string normalizedClientSecret = clientSecret.Trim();
            if (!string.Equals(OAuthClientId, normalizedClientId, StringComparison.Ordinal)
                || !string.Equals(GetOAuthClientSecret(), normalizedClientSecret, StringComparison.Ordinal))
            {
                OAuthDeviceAuthorizationService.ClearStoredToken();
            }

            SaveSetting(OAuthClientIdKey, normalizedClientId);
            SecureCredentialStore.Write(OAuthClientSecretResource, OAuthClientSecretUserName, normalizedClientSecret);
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
