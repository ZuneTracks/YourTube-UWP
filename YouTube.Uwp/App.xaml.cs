using System;
using System.Net.Http;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YouTube.Uwp.Services;

namespace YouTube.Uwp
{
    sealed partial class App : Application
    {
        private readonly OAuthPkceService oauthService;

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Configuration = new RuntimeConfiguration();
            oauthService = new OAuthPkceService(Configuration);
        }

        public static RuntimeConfiguration Configuration { get; private set; }

        public event EventHandler<AuthorizationCompletedEventArgs> AuthorizationCompleted;

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            EnsureMainPage();
            Window.Current.Activate();
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            EnsureMainPage();
            Window.Current.Activate();

            ProtocolActivatedEventArgs protocolArgs = args as ProtocolActivatedEventArgs;
            if (protocolArgs == null)
            {
                return;
            }

            try
            {
                await oauthService.CompleteAuthorizationAsync(protocolArgs.Uri);
                OnAuthorizationCompleted("Google authorization completed. Account endpoints are not enabled by this read-only client.");
            }
            catch (OAuthException exception)
            {
                OnAuthorizationCompleted(exception.Message);
            }
            catch (HttpRequestException)
            {
                OnAuthorizationCompleted("Google authorization could not contact the token endpoint. Check the network connection and try again.");
            }
        }

        private static void EnsureMainPage()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage));
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        private void OnAuthorizationCompleted(string message)
        {
            EventHandler<AuthorizationCompletedEventArgs> handler = AuthorizationCompleted;
            if (handler != null)
            {
                handler(this, new AuthorizationCompletedEventArgs(message));
            }
        }
    }

    public sealed class AuthorizationCompletedEventArgs : EventArgs
    {
        public AuthorizationCompletedEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}
