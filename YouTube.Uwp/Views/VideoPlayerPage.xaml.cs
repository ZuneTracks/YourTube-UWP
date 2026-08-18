using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace YouTube.Uwp.Views
{
    public sealed partial class VideoPlayerPage : Page
    {
        private string videoId;

        public VideoPlayerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            videoId = e.Parameter as string;
            if (string.IsNullOrWhiteSpace(videoId))
            {
                StatusText.Text = "A video ID is required for in-app playback.";
                return;
            }

            StatusText.Text = "Loading the official YouTube watch page...";
            PlayerWebView.Navigate(CreateMobileWatchUri(videoId));
            base.OnNavigatedTo(e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void PlayerWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            StatusText.Text = args.IsSuccess
                ? string.Empty
                : "The in-app player could not load this video. Open it in the browser instead.";
        }

        private void PlayerWebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            StatusText.Text = "The in-app player could not load this video. Open it in the browser instead.";
        }

        private async void OpenInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(videoId))
            {
                await Launcher.LaunchUriAsync(CreateWatchUri(videoId));
            }
        }

        private static Uri CreateMobileWatchUri(string id)
        {
            return new Uri("https://m.youtube.com/watch?v=" + Uri.EscapeDataString(id));
        }

        private static Uri CreateWatchUri(string id)
        {
            return new Uri("https://www.youtube.com/watch?v=" + Uri.EscapeDataString(id));
        }
    }
}
