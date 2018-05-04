using System;
using System.Threading;
using VideoDemo.Models.Interfaces;
using VideoDemo.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;

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
        }

        /// <summary>
        /// Prepare Page on Navigation, reset on Back Navigation.
        /// </summary>
        protected override async void OnAppearing()
        {
            CaptureHandler = App.CurrentCaptureUI;

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

            // Delete any file if it exists
            try
            {
                File.Delete(App.VideoOutput);
            }
            catch { }

            // Restarts the Capture Sequence.
            Viewmodel.StartCaptureSequence();

            base.OnAppearing();
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