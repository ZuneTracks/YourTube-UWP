using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using YouTube.Uwp.Services;

namespace YouTube.Uwp.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly OAuthPkceService oauthService;

        public SettingsPage()
        {
            InitializeComponent();
            oauthService = new OAuthPkceService(App.Configuration);
            ApiKeyStatusText.Text = App.Configuration.HasApiKey ? "Your API key is now stored in Windows Credential Locker." : "No API key is configured.";
            OAuthClientIdBox.Text = App.Configuration.OAuthClientId ?? string.Empty;
            RedirectProtocolBox.Text = App.Configuration.RedirectProtocol ?? string.Empty;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((App)Application.Current).AuthorizationCompleted += OnAuthorizationCompleted;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ((App)Application.Current).AuthorizationCompleted -= OnAuthorizationCompleted;
            base.OnNavigatedFrom(e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void SaveApiKeyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.Configuration.SaveApiKey(ApiKeyBox.Password);
                ApiKeyBox.Password = string.Empty;
                ApiKeyStatusText.Text = "Your API key is now stored in Windows Credential Locker.";
            }
            catch (ArgumentException exception)
            {
                ApiKeyStatusText.Text = exception.Message;
            }
        }

        private void ClearApiKeyButton_Click(object sender, RoutedEventArgs e)
        {
            App.Configuration.ClearApiKey();
            ApiKeyBox.Password = string.Empty;
            ApiKeyStatusText.Text = "API key removed.";
        }

        private void SaveOAuthSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.Configuration.SaveOAuthSettings(OAuthClientIdBox.Text, RedirectProtocolBox.Text);
                AuthStatusText.Text = "OAuth settings saved. The redirect protocol must also be declared in Package.appxmanifest.";
            }
            catch (ArgumentException exception)
            {
                AuthStatusText.Text = exception.Message;
            }
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool opened = await oauthService.BeginAuthorizationAsync();
                AuthStatusText.Text = opened
                    ? "The system browser was opened. Complete sign-in and return to this app."
                    : "Windows could not open the system browser for Google authorization.";
            }
            catch (OAuthException exception)
            {
                AuthStatusText.Text = exception.Message;
            }
        }

        private void OnAuthorizationCompleted(object sender, AuthorizationCompletedEventArgs e)
        {
            AuthStatusText.Text = e.Message;
        }
    }
}
