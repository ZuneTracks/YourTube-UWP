using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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
            RedirectUriBox.Text = App.Configuration.OAuthRedirectUri ?? string.Empty;
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
                App.Configuration.SaveOAuthSettings(OAuthClientIdBox.Text, RedirectUriBox.Text);
                AuthStatusText.Text = "OAuth settings saved. Sign in grants permission to upload videos.";
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

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new Flyout
            {
                Placement = FlyoutPlacementMode.Left
            };
            var panel = new StackPanel
            {
                Width = 320,
                Padding = new Thickness(12)
            };

            panel.Children.Add(new TextBlock
            {
                Text = "YourTube UWP",
                FontSize = 20,
                FontWeight = Windows.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 6)
            });
            panel.Children.Add(new TextBlock
            {
                Text = "YourTube is a modern UWP application for browsing YouTube via YouTube's public API.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            });

            PackageVersion version = Package.Current.Id.Version;
            panel.Children.Add(new TextBlock
            {
                Text = string.Format("Build {0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision),
                Margin = new Thickness(0, 0, 0, 8)
            });
            panel.Children.Add(new TextBlock
            {
                Text = "This app is not affiliated with Google or YouTube.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            });

            var developerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            developerPanel.Children.Add(new TextBlock
            {
                Text = "Developed by: ",
                VerticalAlignment = VerticalAlignment.Center
            });
            developerPanel.Children.Add(new HyperlinkButton
            {
                Content = "ZuneTracks",
                NavigateUri = new Uri("https://github.com/ZuneTracks/YourTube-UWP"),
                Margin = new Thickness(4, 0, 0, 0)
            });
            panel.Children.Add(developerPanel);

            var closeButton = new Button
            {
                Content = "CLOSE",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            closeButton.Click += (s, args) => flyout.Hide();
            panel.Children.Add(closeButton);

            flyout.Content = panel;
            flyout.ShowAt((FrameworkElement)sender);
        }

        private void OnAuthorizationCompleted(object sender, AuthorizationCompletedEventArgs e)
        {
            AuthStatusText.Text = e.Message;
        }
    }
}
