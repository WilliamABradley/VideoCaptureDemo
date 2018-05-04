using System;
using System.Threading;
using VideoDemo.Models.Interfaces;
using VideoDemo.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
using System.Threading.Tasks;

namespace VideoDemo.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CapturePage : ContentPage
    {
        public CapturePage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            Sync = SynchronizationContext.Current;
            Viewmodel = new CaptureViewModel();
            BindingContext = Viewmodel;

            Viewmodel.CaptureStarted += Viewmodel_CaptureStarted;
            Viewmodel.CaptureCompleted += Viewmodel_CaptureCompleted;

            if (File.Exists(App.VideoOutput))
            {
                try
                {
                    File.Delete(App.VideoOutput);
                }
                catch { }
            }
        }

        /// <summary>
        /// Prepare Page on Navigation, reset on Back Navigation.
        /// </summary>
        protected override void OnAppearing()
        {
            CaptureHandler = App.CurrentCaptureUI;

            Task.Delay(TimeSpan.FromSeconds(4)).ContinueWith(t =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Setup();
                });
            });


            base.OnAppearing();
        }

        public async void Setup()
        {
            if (CaptureHandler != null)
            {
                // Start the Capture Session.
                if (!CaptureHandler.SessionActive)
                {
                    CaptureHandler.StartSession();
                }
            }

            // Stop the Current Recording if started.
            if (CaptureHandler?.Recording == true)
            {
                await CaptureHandler.StopRecording();
            }

            // Restarts the Capture Sequence.
            Viewmodel.StartCaptureSequence();
        }

        private void Viewmodel_CaptureStarted(object sender, EventArgs e)
        {
            if (CaptureHandler != null)
            {
                CaptureHandler?.Record(App.VideoOutput);
            }
        }

        private async void Viewmodel_CaptureCompleted(object sender, EventArgs e)
        {
            try
            {
                if (CaptureHandler != null)
                {
                    await CaptureHandler.StopRecording();

                    // End the Preview Session.
                    CaptureHandler.StopSession();
                }

                Sync.Post(async state =>
                {
                    await Navigation.PushAsync(new PlaybackPage());
                }, null);
            }
            catch (Exception ex)
            {
                await DisplayAlert("An Error Occured", ex.Message, "OK");
            }
        }

        private void FinishRec(object sender, EventArgs e)
        {
            Viewmodel.StopCaptureSequence();
        }

        /// <summary>
        /// The Viewmodel.
        /// </summary>
        private CaptureViewModel Viewmodel { get; }

        /// <summary>
        /// The Handler for the Native CaptureUI control.
        /// </summary>
        private ICaptureUI CaptureHandler { get; set; }

        /// <summary>
        /// The UI Syncing context.
        /// </summary>
        private SynchronizationContext Sync { get; }
    }
}