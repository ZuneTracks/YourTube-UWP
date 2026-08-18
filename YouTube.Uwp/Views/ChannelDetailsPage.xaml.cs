using System;
using System.Globalization;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using YouTube.Uwp.Models;
using YouTube.Uwp.Services;

namespace YouTube.Uwp.Views
{
    public sealed partial class ChannelDetailsPage : Page
    {
        private readonly YouTubeDataApiClient client;

        public ChannelDetailsPage()
        {
            InitializeComponent();
            client = new YouTubeDataApiClient(App.Configuration.GetApiKey);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string channelId = e.Parameter as string;
            if (string.IsNullOrWhiteSpace(channelId))
            {
                StatusText.Text = "A channel ID is required.";
                return;
            }

            try
            {
                StatusText.Text = "Loading channel details...";
                ChannelDetails channel = await client.GetChannelAsync(channelId);
                if (channel == null)
                {
                    StatusText.Text = "The requested channel is unavailable.";
                    return;
                }

                TitleText.Text = channel.Title;
                MetadataText.Text = channel.SubscriberCount.ToString("N0", CultureInfo.CurrentCulture) + " subscribers | "
                    + channel.VideoCount.ToString("N0", CultureInfo.CurrentCulture) + " videos | "
                    + channel.ViewCount.ToString("N0", CultureInfo.CurrentCulture) + " views";
                DescriptionText.Text = channel.Description;
                if (!string.IsNullOrWhiteSpace(channel.ThumbnailUrl))
                {
                    ThumbnailImage.Source = new BitmapImage(new Uri(channel.ThumbnailUrl));
                }

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
    }
}
