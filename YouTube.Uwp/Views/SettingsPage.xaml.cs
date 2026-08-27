using System;
using Windows.ApplicationModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using YouTube.Uwp.Services;

namespace YouTube.Uwp.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly OAuthDeviceAuthorizationService oauthService;
        private CancellationTokenSource authorizationCancellation;

        public SettingsPage()
        {
            InitializeComponent();
            oauthService = new OAuthDeviceAuthorizationService(App.Configuration);
            ApiKeyStatusText.Text = App.Configuration.HasApiKey ? "Your API key is now stored in Windows Credential Locker." : "No API key is configured.";
            OAuthClientIdBox.Text = App.Configuration.OAuthClientId ?? string.Empty;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CancelAuthorization();
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
                App.Configuration.SaveOAuthClientId(OAuthClientIdBox.Text);
                AuthStatusText.Text = "Limited-input device OAuth client ID saved. If it changed, sign in again before uploading.";
            }
            catch (ArgumentException exception)
            {
                AuthStatusText.Text = exception.Message;
            }
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if (authorizationCancellation != null)
            {
                return;
            }

            authorizationCancellation = new CancellationTokenSource();
            CancelAuthorizationButton.IsEnabled = true;
            VerificationUrlText.Text = string.Empty;
            VerificationCodeText.Text = string.Empty;

            try
            {
                DeviceAuthorizationInfo authorization = await oauthService.BeginAuthorizationAsync(authorizationCancellation.Token);
                VerificationUrlText.Text = "On another phone, tablet, or computer with a current browser, visit: "
                    + authorization.VerificationUri.AbsoluteUri;
                VerificationCodeText.Text = "Code: " + authorization.UserCode;
                AuthStatusText.Text = "Enter the code in that browser. This phone will wait for Google authorization.";
                await oauthService.CompleteAuthorizationAsync(authorization, authorizationCancellation.Token);
                AuthStatusText.Text = "Google authorization completed. You can now upload a selected video.";
            }
            catch (OAuthException exception)
            {
                AuthStatusText.Text = exception.Message;
            }
            catch (TaskCanceledException)
            {
                AuthStatusText.Text = "Google authorization canceled.";
            }
            catch (OperationCanceledException)
            {
                AuthStatusText.Text = "Google authorization canceled.";
            }
            catch (HttpRequestException)
            {
                AuthStatusText.Text = "Google authorization could not contact the token endpoint. Check the network connection and try again.";
            }
            finally
            {
                authorizationCancellation.Dispose();
                authorizationCancellation = null;
                CancelAuthorizationButton.IsEnabled = false;
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

        private void CancelAuthorizationButton_Click(object sender, RoutedEventArgs e)
        {
            CancelAuthorization();
        }

        private void CancelAuthorization()
        {
            if (authorizationCancellation != null && !authorizationCancellation.IsCancellationRequested)
            {
                authorizationCancellation.Cancel();
            }
        }
    }
}
