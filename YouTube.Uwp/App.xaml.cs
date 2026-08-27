using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YouTube.Uwp.Services;

namespace YouTube.Uwp
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            Configuration = new RuntimeConfiguration();
        }

        public static RuntimeConfiguration Configuration { get; private set; }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            DiagnosticLog.Write("App.Launch", "Application launched.");
            EnsureMainPage();
            Window.Current.Activate();
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
            DiagnosticLog.Write("App.Suspend", "Application suspending.");
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            DiagnosticLog.WriteException("App.UnhandledException", e.Exception);
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            DiagnosticLog.WriteException("App.UnobservedTask", e.Exception);
            e.SetObserved();
        }
    }
}
