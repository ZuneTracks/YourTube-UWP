using System;
using System.Globalization;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using YouTube.Uwp.Models;
using YouTube.Uwp.Services;

namespace YouTube.Uwp.Views
{
    public sealed partial class VideoDetailsPage : Page
    {
        private readonly YouTubeDataApiClient client;
        private readonly TrendingTileService trendingTileService;
        private VideoDetails video;

        public VideoDetailsPage()
        {
            InitializeComponent();
            client = new YouTubeDataApiClient(App.Configuration.GetApiKey);
            trendingTileService = new TrendingTileService();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string videoId = e.Parameter as string;
            if (string.IsNullOrWhiteSpace(videoId))
            {
                StatusText.Text = "A video ID is required.";
                return;
            }

            try
            {
                StatusText.Text = "Loading video details...";
                video = await client.GetVideoAsync(videoId);
                if (video == null)
                {
                    StatusText.Text = "The requested video is unavailable.";
                    return;
                }

                TitleText.Text = video.Title;
                ChannelText.Text = video.ChannelTitle;
                MetadataText.Text = "Published " + (video.PublishedAt.HasValue ? video.PublishedAt.Value.ToString("u") : "unknown")
                    + " | " + video.ViewCount.ToString("N0", CultureInfo.CurrentCulture) + " views"
                    + " | duration " + video.Duration;
                DescriptionText.Text = video.Description;
                if (!string.IsNullOrWhiteSpace(video.ThumbnailUrl))
                {
                    ThumbnailImage.Source = new BitmapImage(new Uri(video.ThumbnailUrl));
                }

                PlayInAppButton.IsEnabled = true;
                PlaybackStatusText.Text = "Play in app opens YouTube's official mobile watch page.";
                StatusText.Text = string.Empty;
            }
            catch (InvalidOperationException exception)
            {
                StatusText.Text = exception.Message;
            }
            catch (YouTubeApiException exception)
            {
                StatusText.Text = exception.Message;
            }
            catch (HttpRequestException)
            {
                StatusText.Text = "The YouTube Data API could not be reached. Check the network connection.";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void OpenChannelButton_Click(object sender, RoutedEventArgs e)
        {
            if (video != null && !string.IsNullOrWhiteSpace(video.ChannelId))
            {
                Frame.Navigate(typeof(ChannelDetailsPage), video.ChannelId);
            }
        }

        private void PlayInAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (video != null && !string.IsNullOrWhiteSpace(video.Id))
            {
                UpdateLastPlayedTile();
                Frame.Navigate(typeof(VideoPlayerPage), video.Id);
            }
        }

        private async void WatchOnYouTubeButton_Click(object sender, RoutedEventArgs e)
        {
            if (video != null && !string.IsNullOrWhiteSpace(video.Id))
            {
                UpdateLastPlayedTile();
                await Launcher.LaunchUriAsync(new Uri("https://www.youtube.com/watch?v=" + Uri.EscapeDataString(video.Id)));
            }
        }

        private void UpdateLastPlayedTile()
        {
            try
            {
                trendingTileService.UpdateLastPlayed(video);
            }
            catch (UnauthorizedAccessException)
            {
                PlaybackStatusText.Text = "Playback started, but Windows did not allow the live tile to update.";
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                PlaybackStatusText.Text = "Playback started, but the live tile could not be updated.";
            }
            catch (ArgumentException)
            {
                PlaybackStatusText.Text = "Playback started, but its metadata could not be saved for the live tile.";
            }
        }
    }
}
