using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using YouTube.Uwp.Services;

namespace YouTube.Uwp.Views
{
    public sealed partial class UploadVideoPage : Page
    {
        private readonly IYouTubeUploadClient uploadClient;
        private StorageFile selectedFile;
        private CancellationTokenSource uploadCancellation;

        public UploadVideoPage()
        {
            InitializeComponent();
            OAuthPkceService oauthService = new OAuthPkceService(App.Configuration);
            uploadClient = new YouTubeResumableUploadClient(oauthService.GetValidAccessTokenAsync);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CancelUpload();
            base.OnNavigatedFrom(e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void SelectVideoButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mkv");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                ProgressText.Text = "Video selection was canceled.";
                return;
            }

            BasicProperties properties = await file.GetBasicPropertiesAsync();
            selectedFile = file;
            SelectedFileText.Text = file.Name + " (" + properties.Size + " bytes)";
            ProgressText.Text = "Ready to upload " + file.Name + ".";
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (uploadCancellation != null)
            {
                return;
            }

            uploadCancellation = new CancellationTokenSource();
            SetUploadControls(false);
            UploadProgressBar.Value = 0;
            ProgressText.Text = "Starting resumable upload...";

            try
            {
                ComboBoxItem privacyItem = PrivacyBox.SelectedItem as ComboBoxItem;
                VideoUploadRequest request = new VideoUploadRequest
                {
                    File = selectedFile,
                    Title = TitleBox.Text,
                    Description = DescriptionBox.Text,
                    PrivacyStatus = privacyItem == null ? null : privacyItem.Content as string
                };

                VideoUploadResult result = await uploadClient.UploadAsync(
                    request,
                    new Progress<VideoUploadProgress>(UpdateProgress),
                    uploadCancellation.Token);
                ProgressText.Text = string.IsNullOrWhiteSpace(result.VideoId)
                    ? "Upload completed. YouTube is processing the video."
                    : "Upload completed. Video ID: " + result.VideoId;
            }
            catch (TaskCanceledException)
            {
                ProgressText.Text = uploadCancellation.IsCancellationRequested
                    ? "Upload canceled. The partially uploaded resumable session was not continued."
                    : "The upload timed out. Check the network connection and try again.";
            }
            catch (OperationCanceledException)
            {
                ProgressText.Text = "Upload canceled. The partially uploaded resumable session was not continued.";
            }
            catch (ArgumentException exception)
            {
                ProgressText.Text = exception.Message;
            }
            catch (OAuthException exception)
            {
                ProgressText.Text = exception.Message;
            }
            catch (YouTubeUploadException exception)
            {
                ProgressText.Text = exception.Message;
            }
            catch (HttpRequestException)
            {
                ProgressText.Text = "The upload could not reach YouTube. Check the network connection and try again.";
            }
            finally
            {
                uploadCancellation.Dispose();
                uploadCancellation = null;
                SetUploadControls(true);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelUpload();
        }

        private void CancelUpload()
        {
            if (uploadCancellation != null && !uploadCancellation.IsCancellationRequested)
            {
                uploadCancellation.Cancel();
                ProgressText.Text = "Canceling upload...";
            }
        }

        private void SetUploadControls(bool ready)
        {
            UploadButton.IsEnabled = ready;
            CancelButton.IsEnabled = !ready;
        }

        private void UpdateProgress(VideoUploadProgress progress)
        {
            UploadProgressBar.Value = progress.Percentage;
            ProgressText.Text = progress.BytesUploaded
                + " of "
                + progress.TotalBytes
                + " bytes uploaded ("
                + progress.Percentage.ToString("F0")
                + "%).";
        }
    }
}
