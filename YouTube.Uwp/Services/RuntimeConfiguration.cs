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
            get { return FirstConfiguredValue(ReadSetting(OAuthClientIdKey), GetBuildDefaultOAuthClientId()); }
        }

        public bool HasApiKey
        {
            get { return !string.IsNullOrWhiteSpace(GetApiKey()); }
        }

        public bool HasStoredApiKey
        {
            get { return !string.IsNullOrWhiteSpace(SecureCredentialStore.Read(ApiKeyResource, ApiKeyUserName)); }
        }

        public bool HasBuildDefaultApiKey
        {
            get { return !string.IsNullOrWhiteSpace(GetBuildDefaultApiKey()); }
        }

        public string GetApiKey()
        {
            return FirstConfiguredValue(
                SecureCredentialStore.Read(ApiKeyResource, ApiKeyUserName),
                GetBuildDefaultApiKey());
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
            return FirstConfiguredValue(
                SecureCredentialStore.Read(OAuthClientSecretResource, OAuthClientSecretUserName),
                GetBuildDefaultOAuthClientSecret());
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

        private static string FirstConfiguredValue(string storedValue, string buildDefault)
        {
            return string.IsNullOrWhiteSpace(storedValue) ? buildDefault : storedValue;
        }

        private static string GetBuildDefaultApiKey()
        {
#if LOCAL_BUILD_CONFIGURATION
            return NormalizeBuildDefault(LocalBuildConfiguration.ApiKey);
#else
            return null;
#endif
        }

        private static string GetBuildDefaultOAuthClientId()
        {
#if LOCAL_BUILD_CONFIGURATION
            return NormalizeBuildDefault(LocalBuildConfiguration.OAuthClientId);
#else
            return null;
#endif
        }

        private static string GetBuildDefaultOAuthClientSecret()
        {
#if LOCAL_BUILD_CONFIGURATION
            return NormalizeBuildDefault(LocalBuildConfiguration.OAuthClientSecret);
#else
            return null;
#endif
        }

        private static string NormalizeBuildDefault(string value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.StartsWith("REPLACE_WITH_", StringComparison.Ordinal))
            {
                return null;
            }

            return value.Trim();
        }
    }
}
