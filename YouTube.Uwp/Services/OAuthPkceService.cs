using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace YouTube.Uwp.Services
{
    public sealed class OAuthDeviceAuthorizationService
    {
        public const string YouTubeUploadScope = "https://www.googleapis.com/auth/youtube.upload";
        private const string DeviceAuthorizationEndpoint = "https://oauth2.googleapis.com/device/code";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string TokenAccessResource = "YourTube.OAuthToken.Access";
        private const string TokenRefreshResource = "YourTube.OAuthToken.Refresh";
        private const string TokenExpiryResource = "YourTube.OAuthToken.Expiry";
        private const string CredentialUserName = "CurrentUser";
        private readonly RuntimeConfiguration configuration;
        private readonly HttpClient httpClient;

        public OAuthDeviceAuthorizationService(RuntimeConfiguration configuration)
            : this(configuration, new HttpClient())
        {
        }

        public OAuthDeviceAuthorizationService(RuntimeConfiguration configuration, HttpClient httpClient)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            this.configuration = configuration;
            this.httpClient = httpClient;
        }

        public async Task<DeviceAuthorizationInfo> BeginAuthorizationAsync(CancellationToken cancellationToken)
        {
            OAuthDeviceCredentials credentials = GetValidatedCredentials();
            DiagnosticLog.Write("OAuth.DeviceCode", "Requesting device authorization code.");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("client_id", credentials.ClientId);
            parameters.Add("scope", YouTubeUploadScope);

            using (HttpResponseMessage response = await httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, DeviceAuthorizationEndpoint)
                {
                    Content = new FormUrlEncodedContent(parameters)
                },
                cancellationToken))
            {
                string content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    DiagnosticLog.Write("OAuth.DeviceCode", "Device authorization request failed with HTTP " + ((int)response.StatusCode) + ".");
                    throw new OAuthException("Google device authorization could not start: " + content);
                }

                JsonObject responseJson = ParseJson(content, "Google device authorization returned an invalid response.");
                string deviceCode = responseJson.GetNamedString("device_code", string.Empty);
                string userCode = responseJson.GetNamedString("user_code", string.Empty);
                string verificationUrl = responseJson.GetNamedString("verification_url", string.Empty);
                if (string.IsNullOrWhiteSpace(verificationUrl))
                {
                    verificationUrl = responseJson.GetNamedString("verification_uri", string.Empty);
                }

                Uri verificationUri;
                if (string.IsNullOrWhiteSpace(deviceCode)
                    || string.IsNullOrWhiteSpace(userCode)
                    || !Uri.TryCreate(verificationUrl, UriKind.Absolute, out verificationUri))
                {
                    throw new OAuthException("Google device authorization did not return a usable verification code and URL.");
                }

                return new DeviceAuthorizationInfo(
                    deviceCode,
                    userCode,
                    verificationUri,
                    responseJson.GetNamedNumber("expires_in", 1800),
                    responseJson.GetNamedNumber("interval", 5));
            }
        }

        public async Task<OAuthToken> CompleteAuthorizationAsync(
            DeviceAuthorizationInfo authorization,
            CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException("authorization");
            }

            OAuthDeviceCredentials credentials = GetValidatedCredentials();
            DiagnosticLog.Write("OAuth.Poll", "Waiting for device authorization approval.");
            DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddSeconds(authorization.ExpiresInSeconds);
            int pollIntervalSeconds = Math.Max(5, authorization.PollIntervalSeconds);
            while (DateTimeOffset.UtcNow < expiresAt)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds), cancellationToken);

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("device_code", authorization.DeviceCode);
                parameters.Add("client_id", credentials.ClientId);
                parameters.Add("client_secret", credentials.ClientSecret);
                parameters.Add("grant_type", "urn:ietf:params:oauth:grant-type:device_code");

                DeviceTokenResponse response = await RequestDeviceTokenAsync(parameters, cancellationToken);
                if (response.Token != null)
                {
                    DiagnosticLog.Write("OAuth.Poll", "Google returned tokens; saving them in Credential Locker.");
                    SaveToken(response.Token);
                    DiagnosticLog.Write("OAuth.Poll", "Token persistence completed.");
                    return response.Token;
                }

                if (response.Error == "authorization_pending")
                {
                    continue;
                }

                if (response.Error == "slow_down")
                {
                    pollIntervalSeconds += 5;
                    continue;
                }

                DiagnosticLog.Write("OAuth.Poll", "Google returned terminal device authorization error: " + response.Error + ".");
                throw new OAuthException("Google device authorization was not completed: " + response.Error + ".");
            }

            throw new OAuthException("Google device authorization expired. Start sign-in again.");
        }

        public async Task<string> GetValidAccessTokenAsync()
        {
            OAuthDeviceCredentials credentials = GetValidatedCredentials();
            OAuthToken token = ReadToken();
            if (token == null)
            {
                throw new OAuthException("No Google account is authorized. Start sign-in first.");
            }

            if (token.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1) && !string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return token.AccessToken;
            }

            if (string.IsNullOrWhiteSpace(token.RefreshToken))
            {
                throw new OAuthException("Google authorization needs to be completed again before uploading.");
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("client_id", credentials.ClientId);
            parameters.Add("client_secret", credentials.ClientSecret);
            parameters.Add("refresh_token", token.RefreshToken);
            parameters.Add("grant_type", "refresh_token");
            OAuthToken refreshed = await RequestTokenAsync(parameters, CancellationToken.None);
            refreshed.RefreshToken = token.RefreshToken;
            SaveToken(refreshed);
            return refreshed.AccessToken;
        }

        private async Task<DeviceTokenResponse> RequestDeviceTokenAsync(
            IReadOnlyDictionary<string, string> parameters,
            CancellationToken cancellationToken)
        {
            using (HttpResponseMessage response = await httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
                {
                    Content = new FormUrlEncodedContent(parameters)
                },
                cancellationToken))
            {
                string content = await response.Content.ReadAsStringAsync();
                JsonObject responseJson = ParseJson(content, "Google token endpoint returned an invalid response.");
                if (!response.IsSuccessStatusCode)
                {
                    return new DeviceTokenResponse(null, responseJson.GetNamedString("error", "unknown_error"));
                }

                return new DeviceTokenResponse(CreateToken(responseJson), null);
            }
        }

        private async Task<OAuthToken> RequestTokenAsync(
            IReadOnlyDictionary<string, string> parameters,
            CancellationToken cancellationToken)
        {
            using (HttpResponseMessage response = await httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
                {
                    Content = new FormUrlEncodedContent(parameters)
                },
                cancellationToken))
            {
                string content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new OAuthException("Google token refresh failed: " + content);
                }

                return CreateToken(ParseJson(content, "Google token endpoint returned an invalid response."));
            }
        }

        private static OAuthToken CreateToken(JsonObject responseJson)
        {
            string accessToken = responseJson.GetNamedString("access_token", string.Empty);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new OAuthException("Google token endpoint did not return an access token.");
            }

            return new OAuthToken
            {
                AccessToken = accessToken,
                RefreshToken = responseJson.GetNamedString("refresh_token", null),
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(responseJson.GetNamedNumber("expires_in", 3600))
            };
        }

        private OAuthDeviceCredentials GetValidatedCredentials()
        {
            if (string.IsNullOrWhiteSpace(configuration.OAuthClientId))
            {
                throw new OAuthException("Set a limited-input device OAuth client ID before signing in.");
            }

            string clientSecret = configuration.GetOAuthClientSecret();
            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new OAuthException("Set the limited-input device OAuth client secret before signing in.");
            }

            return new OAuthDeviceCredentials(configuration.OAuthClientId, clientSecret);
        }

        public static void ClearStoredToken()
        {
            SecureCredentialStore.Delete(TokenAccessResource, CredentialUserName);
            SecureCredentialStore.Delete(TokenRefreshResource, CredentialUserName);
            SecureCredentialStore.Delete(TokenExpiryResource, CredentialUserName);
        }

        private OAuthToken ReadToken()
        {
            string accessToken = SecureCredentialStore.Read(TokenAccessResource, CredentialUserName);
            string refreshToken = SecureCredentialStore.Read(TokenRefreshResource, CredentialUserName);
            string expiryValue = SecureCredentialStore.Read(TokenExpiryResource, CredentialUserName);
            if (string.IsNullOrWhiteSpace(accessToken)
                && string.IsNullOrWhiteSpace(refreshToken)
                && string.IsNullOrWhiteSpace(expiryValue))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(expiryValue))
            {
                throw new OAuthException("The stored OAuth token is malformed. Sign in again.");
            }

            long expiresAt;
            if (!long.TryParse(expiryValue, out expiresAt))
            {
                throw new OAuthException("The stored OAuth token expiration is malformed. Sign in again.");
            }

            return new OAuthToken
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAt)
            };
        }

        private static void SaveToken(OAuthToken token)
        {
            try
            {
                DiagnosticLog.Write("OAuth.TokenStore", "Saving access token entry.");
                SecureCredentialStore.Write(TokenAccessResource, CredentialUserName, token.AccessToken);
                DiagnosticLog.Write("OAuth.TokenStore", "Saving refresh token entry.");
                SecureCredentialStore.Write(TokenRefreshResource, CredentialUserName, token.RefreshToken ?? string.Empty);
                DiagnosticLog.Write("OAuth.TokenStore", "Saving token expiry entry.");
                SecureCredentialStore.Write(
                    TokenExpiryResource,
                    CredentialUserName,
                    token.ExpiresAt.ToUnixTimeSeconds().ToString());
            }
            catch (Exception exception)
            {
                DiagnosticLog.WriteException("OAuth.TokenStore", exception);
                throw new OAuthException(
                    "Google authorization completed, but Windows Credential Locker could not save the token (0x"
                    + exception.HResult.ToString("X8")
                    + ").");
            }
        }

        private static JsonObject ParseJson(string content, string errorMessage)
        {
            JsonObject value;
            if (!JsonObject.TryParse(content, out value))
            {
                throw new OAuthException(errorMessage);
            }

            return value;
        }

        private sealed class DeviceTokenResponse
        {
            public DeviceTokenResponse(OAuthToken token, string error)
            {
                Token = token;
                Error = error;
            }

            public OAuthToken Token { get; private set; }

            public string Error { get; private set; }
        }

        private sealed class OAuthDeviceCredentials
        {
            public OAuthDeviceCredentials(string clientId, string clientSecret)
            {
                ClientId = clientId;
                ClientSecret = clientSecret;
            }

            public string ClientId { get; private set; }

            public string ClientSecret { get; private set; }
        }
    }

    public sealed class DeviceAuthorizationInfo
    {
        public DeviceAuthorizationInfo(
            string deviceCode,
            string userCode,
            Uri verificationUri,
            double expiresInSeconds,
            double pollIntervalSeconds)
        {
            DeviceCode = deviceCode;
            UserCode = userCode;
            VerificationUri = verificationUri;
            ExpiresInSeconds = Math.Max(1, (int)expiresInSeconds);
            PollIntervalSeconds = Math.Max(1, (int)pollIntervalSeconds);
        }

        internal string DeviceCode { get; private set; }

        public string UserCode { get; private set; }

        public Uri VerificationUri { get; private set; }

        internal int ExpiresInSeconds { get; private set; }

        internal int PollIntervalSeconds { get; private set; }
    }

    public sealed class OAuthToken
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }
    }

    public sealed class OAuthException : Exception
    {
        public OAuthException(string message)
            : base(message)
        {
        }
    }
}
