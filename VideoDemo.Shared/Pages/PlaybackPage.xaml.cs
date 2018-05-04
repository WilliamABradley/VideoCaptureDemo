using System;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Plugin.MediaManager;
using Plugin.MediaManager.Abstractions.Enums;
using Plugin.MediaManager.Abstractions.EventArguments;

namespace VideoDemo.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlaybackPage : ContentPage
    {
        public PlaybackPage()
        {
            InitializeComponent();
            CrossMediaManager.Current.StatusChanged += Current_StatusChanged;
        }

        private void Current_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case MediaPlayerStatus.Paused:
                case MediaPlayerStatus.Stopped:
                    PlayButton.IsVisible = true;
                    PlayButtonBorder.IsVisible = true;
                    break;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var filepath = NSUrl.FromFilename(App.VideoOutput);
            Player.Source = filepath.AbsoluteString;
            CrossMediaManager.Current.PlaybackController.Pause();
        }

        protected override void OnDisappearing()
        {
            CrossMediaManager.Current.PlaybackController.Stop();
            base.OnDisappearing();
        }

        private async void Preview(object sender, EventArgs e)
        {
            PlayButton.IsVisible = false;
            PlayButtonBorder.IsVisible = false;
            if (CrossMediaManager.Current.Status == MediaPlayerStatus.Stopped)
            {
                await CrossMediaManager.Current.PlaybackController.PlayPrevious();
            }
            else
            {
                await CrossMediaManager.Current.PlaybackController.Play();
            }
        }
    }
}