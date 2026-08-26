using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YouTube.Uwp.Models;
using YouTube.Uwp.Services;
using YouTube.Uwp.Views;

namespace YouTube.Uwp
{
    public sealed partial class MainPage : Page
    {
        private readonly YouTubeDataApiClient client;
        private readonly TrendingTileService trendingTileService;

        public MainPage()
        {
            InitializeComponent();
            Results = new ObservableCollection<VideoSummary>();
            Categories = new ObservableCollection<VideoCategory>();
            DataContext = this;
            client = new YouTubeDataApiClient(App.Configuration.GetApiKey);
            trendingTileService = new TrendingTileService();
        }

        public ObservableCollection<VideoSummary> Results { get; private set; }

        public ObservableCollection<VideoCategory> Categories { get; private set; }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PublicStatusText.Text = "Searching...";
                DataPage<VideoSummary> page = await client.SearchVideosAsync(SearchBox.Text, null, 25);
                ReplaceResults(page);
                PublicStatusText.Text = Results.Count + " public video results.";
            }
            catch (ArgumentException exception)
            {
                PublicStatusText.Text = exception.Message;
            }
            catch (InvalidOperationException exception)
            {
                PublicStatusText.Text = exception.Message;
            }
            catch (YouTubeApiException exception)
            {
                PublicStatusText.Text = exception.Message;
            }
            catch (YouTubeApiResponseException exception)
            {
                PublicStatusText.Text = exception.Message;
            }
            catch (TaskCanceledException)
            {
                PublicStatusText.Text = "The YouTube Data API request timed out. Check the network connection and try again.";
            }
            catch (HttpRequestException)
            {
                PublicStatusText.Text = "The YouTube Data API could not be reached. Check the network connection.";
            }
        }

        private async void PopularButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PublicStatusText.Text = "Loading popular videos...";
                HomeStatusText.Text = "Loading popular videos...";
                DataPage<VideoSummary> page = await client.GetMostPopularVideosAsync(RegionBox.Text, null, 25);
                ReplaceResults(page);
                PublicStatusText.Text = Results.Count + " popular video results.";
                HomeStatusText.Text = Results.Count + " popular video results.";
                UpdateTrendingTile(page);
            }
            catch (InvalidOperationException exception)
            {
                PublicStatusText.Text = exception.Message;
                HomeStatusText.Text = exception.Message;
            }
            catch (YouTubeApiException exception)
            {
                PublicStatusText.Text = exception.Message;
                HomeStatusText.Text = exception.Message;
            }
            catch (YouTubeApiResponseException exception)
            {
                PublicStatusText.Text = exception.Message;
                HomeStatusText.Text = exception.Message;
            }
            catch (TaskCanceledException)
            {
                PublicStatusText.Text = "The YouTube Data API request timed out. Check the network connection and try again.";
                HomeStatusText.Text = PublicStatusText.Text;
            }
            catch (HttpRequestException)
            {
                PublicStatusText.Text = "The YouTube Data API could not be reached. Check the network connection.";
                HomeStatusText.Text = PublicStatusText.Text;
            }
        }

        private void UpdateTrendingTile(DataPage<VideoSummary> page)
        {
            if (page.Items.Count == 0)
            {
                return;
            }

            try
            {
                trendingTileService.Update(page.Items[0], RegionBox.Text);
                HomeStatusText.Text = Results.Count + " popular video results. The live tile now shows the top result.";
            }
            catch (UnauthorizedAccessException)
            {
                HomeStatusText.Text = Results.Count + " popular video results. Windows did not allow the live tile to update.";
            }
            catch (COMException)
            {
                HomeStatusText.Text = Results.Count + " popular video results. The live tile could not be updated.";
            }
        }

        private void ShowSearchButton_Click(object sender, RoutedEventArgs e)
        {
            MainPivot.SelectedIndex = 1;
        }

        private void ShowSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void ShowUploadButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UploadVideoPage));
        }

        private async void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePivotHeaders();
            if (MainPivot.SelectedIndex == 2 && Categories.Count == 0)
            {
                await LoadCategoriesAsync();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                CategoryStatusText.Text = "Loading categories...";
                IReadOnlyList<VideoCategory> categories = await client.GetVideoCategoriesAsync(RegionBox.Text);
                Categories.Clear();
                foreach (VideoCategory category in categories)
                {
                    Categories.Add(category);
                }

                CategoryStatusText.Text = Categories.Count + " categories available for " + GetRegionLabel() + ".";
            }
            catch (InvalidOperationException exception)
            {
                CategoryStatusText.Text = exception.Message;
            }
            catch (YouTubeApiException exception)
            {
                CategoryStatusText.Text = exception.Message;
            }
            catch (YouTubeApiResponseException exception)
            {
                CategoryStatusText.Text = exception.Message;
            }
            catch (TaskCanceledException)
            {
                CategoryStatusText.Text = "The YouTube Data API request timed out. Check the network connection and try again.";
            }
            catch (HttpRequestException)
            {
                CategoryStatusText.Text = "The YouTube Data API could not be reached. Check the network connection.";
            }
        }

        private async void CategoryToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            VideoCategory category = button == null ? null : button.Tag as VideoCategory;
            if (category == null)
            {
                return;
            }

            if (category.IsExpanded)
            {
                category.IsExpanded = false;
                return;
            }

            foreach (VideoCategory otherCategory in Categories)
            {
                if (otherCategory != category)
                {
                    otherCategory.IsExpanded = false;
                }
            }

            category.IsExpanded = true;
            if (category.HasLoaded)
            {
                return;
            }

            try
            {
                category.StatusMessage = "Loading " + category.Title + " videos...";
                DataPage<VideoSummary> page = await client.GetMostPopularVideosAsync(RegionBox.Text, category.Id, null, 25);
                category.SetVideos(page.Items);
                category.StatusMessage = category.Videos.Count + " popular " + category.Title + " videos for " + GetRegionLabel() + ".";
            }
            catch (InvalidOperationException exception)
            {
                category.StatusMessage = exception.Message;
            }
            catch (YouTubeApiException exception)
            {
                category.StatusMessage = exception.Message;
            }
            catch (YouTubeApiResponseException exception)
            {
                category.StatusMessage = exception.Message;
            }
            catch (TaskCanceledException)
            {
                category.StatusMessage = "The YouTube Data API request timed out. Check the network connection and try again.";
            }
            catch (HttpRequestException)
            {
                category.StatusMessage = "The YouTube Data API could not be reached. Check the network connection.";
            }
        }

        private void ResultsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            VideoSummary video = e.ClickedItem as VideoSummary;
            NavigateToVideo(video);
        }

        private void CategoryVideoButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            NavigateToVideo(button == null ? null : button.Tag as VideoSummary);
        }

        private void NavigateToVideo(VideoSummary video)
        {
            if (video != null)
            {
                Frame.Navigate(typeof(VideoDetailsPage), video.Id);
            }
        }

        private void ReplaceResults(DataPage<VideoSummary> page)
        {
            Results.Clear();
            foreach (VideoSummary video in page.Items)
            {
                Results.Add(video);
            }
        }

        private void UpdatePivotHeaders()
        {
            if (HomePivotHeader == null || SearchPivotHeader == null || CategoriesPivotHeader == null)
            {
                return;
            }

            HomePivotHeader.IsSelected = MainPivot.SelectedIndex == 0;
            SearchPivotHeader.IsSelected = MainPivot.SelectedIndex == 1;
            CategoriesPivotHeader.IsSelected = MainPivot.SelectedIndex == 2;
        }

        private string GetRegionLabel()
        {
            return string.IsNullOrWhiteSpace(RegionBox.Text)
                ? "your region"
                : RegionBox.Text.Trim().ToUpperInvariant();
        }
    }
}
