using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.System;

namespace YouTube.Uwp.Services
{
    public sealed class OAuthPkceService
    {
        // The UWP protocol handler is declared at package build time and cannot be changed at runtime.
        public const string PackagedRedirectProtocol = "com.zunetracks.yourtube";
        public const string PackagedRedirectUri = PackagedRedirectProtocol + ":/oauth2redirect";
        public const string YouTubeUploadScope = "https://www.googleapis.com/auth/youtube.upload";
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string PendingAuthorizationResource = "YourTube.PendingAuthorization";
        private const string TokenResource = "YourTube.OAuthToken";
        private const string CredentialUserName = "CurrentUser";
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly RuntimeConfiguration configuration;

        public OAuthPkceService(RuntimeConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.configuration = configuration;
        }

        public async Task<bool> BeginAuthorizationAsync()
        {
            OAuthSettings settings = GetValidatedSettings();
            string state = CreateRandomUrlSafeValue();
            string verifier = CreateRandomUrlSafeValue();
            string challenge = CreateCodeChallenge(verifier);

            SecureCredentialStore.Write(PendingAuthorizationResource, CredentialUserName, state + "\n" + verifier);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("client_id", settings.ClientId);
            parameters.Add("redirect_uri", settings.RedirectUri);
            parameters.Add("response_type", "code");
            parameters.Add("scope", YouTubeUploadScope);
            parameters.Add("code_challenge", challenge);
            parameters.Add("code_challenge_method", "S256");
            parameters.Add("state", state);
            parameters.Add("access_type", "offline");
            parameters.Add("prompt", "consent");

            return await Launcher.LaunchUriAsync(BuildUri(AuthorizationEndpoint, parameters));
        }

        public async Task<OAuthToken> CompleteAuthorizationAsync(Uri activationUri)
        {
            if (activationUri == null)
            {
                throw new OAuthException("Google returned an empty authorization callback.");
            }

            OAuthSettings settings = GetValidatedSettings();
            Uri expectedRedirectUri = new Uri(settings.RedirectUri);
            if (!string.Equals(activationUri.Scheme, expectedRedirectUri.Scheme, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(activationUri.Host, expectedRedirectUri.Host, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(activationUri.AbsolutePath, expectedRedirectUri.AbsolutePath, StringComparison.Ordinal))
            {
                throw new OAuthException("The authorization callback did not match the configured redirect URI.");
            }

            Dictionary<string, string> callback = ParseQuery(activationUri.Query);
            string providerError;
            if (callback.TryGetValue("error", out providerError))
            {
                SecureCredentialStore.Delete(PendingAuthorizationResource, CredentialUserName);
                throw new OAuthException("Google authorization was not completed: " + providerError + ".");
            }

            string authorizationCode;
            string returnedState;
            if (!callback.TryGetValue("code", out authorizationCode) || !callback.TryGetValue("state", out returnedState))
            {
                throw new OAuthException("The authorization callback did not contain both code and state values.");
            }

            string pending = SecureCredentialStore.Read(PendingAuthorizationResource, CredentialUserName);
            if (string.IsNullOrWhiteSpace(pending))
            {
                throw new OAuthException("No matching authorization request is pending. Start sign-in again.");
            }

            string[] pendingValues = pending.Split(new[] { '\n' }, 2);
            if (pendingValues.Length != 2 || !string.Equals(pendingValues[0], returnedState, StringComparison.Ordinal))
            {
                SecureCredentialStore.Delete(PendingAuthorizationResource, CredentialUserName);
                throw new OAuthException("The authorization callback state did not match the sign-in request.");
            }

            SecureCredentialStore.Delete(PendingAuthorizationResource, CredentialUserName);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("code", authorizationCode);
            parameters.Add("client_id", settings.ClientId);
            parameters.Add("redirect_uri", settings.RedirectUri);
            parameters.Add("grant_type", "authorization_code");
            parameters.Add("code_verifier", pendingValues[1]);

            OAuthToken token = await RequestTokenAsync(parameters);
            SaveToken(token);
            return token;
        }

        public async Task<string> GetValidAccessTokenAsync()
        {
            OAuthSettings settings = GetValidatedSettings();
            OAuthToken token = ReadToken();
            if (token == null || string.IsNullOrWhiteSpace(token.RefreshToken))
            {
                throw new OAuthException("No Google account is authorized. Start sign-in first.");
            }

            if (token.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1) && !string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return token.AccessToken;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("client_id", settings.ClientId);
            parameters.Add("refresh_token", token.RefreshToken);
            parameters.Add("grant_type", "refresh_token");
            OAuthToken refreshed = await RequestTokenAsync(parameters);
            refreshed.RefreshToken = token.RefreshToken;
            SaveToken(refreshed);
            return refreshed.AccessToken;
        }

        public static bool IsPackagedRedirectUri(string redirectUri)
        {
            return !string.IsNullOrWhiteSpace(redirectUri)
                && string.Equals(redirectUri.Trim(), PackagedRedirectUri, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<OAuthToken> RequestTokenAsync(IReadOnlyDictionary<string, string> parameters)
        {
            HttpResponseMessage response = await HttpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(parameters));
            string content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new OAuthException("Google token exchange failed: " + content);
            }

            JsonObject responseJson = JsonObject.Parse(content);
            string accessToken = responseJson.GetNamedString("access_token", string.Empty);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new OAuthException("Google token exchange did not return an access token.");
            }

            return new OAuthToken
            {
                AccessToken = accessToken,
                RefreshToken = responseJson.GetNamedString("refresh_token", null),
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(responseJson.GetNamedNumber("expires_in", 3600))
            };
        }

        private OAuthSettings GetValidatedSettings()
        {
            if (string.IsNullOrWhiteSpace(configuration.OAuthClientId))
            {
                throw new OAuthException("Set an OAuth client ID before signing in.");
            }

            if (!IsPackagedRedirectUri(configuration.OAuthRedirectUri))
            {
                throw new OAuthException("Set the OAuth redirect URI to " + PackagedRedirectUri + " before signing in.");
            }

            return new OAuthSettings(configuration.OAuthClientId, configuration.OAuthRedirectUri);
        }

        private OAuthToken ReadToken()
        {
            string persisted = SecureCredentialStore.Read(TokenResource, CredentialUserName);
            if (string.IsNullOrWhiteSpace(persisted))
            {
                return null;
            }

            string[] fields = persisted.Split(new[] { '\n' });
            if (fields.Length != 3)
            {
                throw new OAuthException("The stored OAuth token is malformed. Sign in again.");
            }

            long expiresAt;
            if (!long.TryParse(fields[2], out expiresAt))
            {
                throw new OAuthException("The stored OAuth token expiration is malformed. Sign in again.");
            }

            return new OAuthToken
            {
                AccessToken = Decode(fields[0]),
                RefreshToken = Decode(fields[1]),
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAt)
            };
        }

        private static void SaveToken(OAuthToken token)
        {
            SecureCredentialStore.Write(
                TokenResource,
                CredentialUserName,
                Encode(token.AccessToken) + "\n" + Encode(token.RefreshToken) + "\n" + token.ExpiresAt.ToUnixTimeSeconds().ToString());
        }

        private static string CreateRandomUrlSafeValue()
        {
            return ToUrlSafeBase64(CryptographicBuffer.EncodeToBase64String(CryptographicBuffer.GenerateRandom(32)));
        }

        private static string CreateCodeChallenge(string verifier)
        {
            IBuffer source = CryptographicBuffer.ConvertStringToBinary(verifier, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            return ToUrlSafeBase64(CryptographicBuffer.EncodeToBase64String(provider.HashData(source)));
        }

        private static string ToUrlSafeBase64(string value)
        {
            return value.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static Uri BuildUri(string endpoint, IDictionary<string, string> parameters)
        {
            List<string> query = new List<string>();
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                query.Add(Uri.EscapeDataString(parameter.Key) + "=" + Uri.EscapeDataString(parameter.Value));
            }

            return new Uri(endpoint + "?" + string.Join("&", query));
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string part in query.TrimStart('?').Split('&'))
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                int separator = part.IndexOf('=');
                string key = separator < 0 ? part : part.Substring(0, separator);
                string value = separator < 0 ? string.Empty : part.Substring(separator + 1);
                values[Uri.UnescapeDataString(key.Replace("+", " "))] = Uri.UnescapeDataString(value.Replace("+", " "));
            }

            return values;
        }

        private static string Encode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static string Decode(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        private sealed class OAuthSettings
        {
            public OAuthSettings(string clientId, string redirectUri)
            {
                ClientId = clientId;
                RedirectUri = redirectUri;
            }

            public string ClientId { get; private set; }

            public string RedirectUri { get; private set; }
        }
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
