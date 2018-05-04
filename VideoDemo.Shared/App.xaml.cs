using System;
using System.IO;
using VideoDemo.Models.Interfaces;
using VideoDemo.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace VideoDemo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new CapturePage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        /// <summary>
        /// The Capture UI Handler.
        /// </summary>
        public static ICaptureUI CurrentCaptureUI { get; set; }

        /// <summary>
        /// Video Output.
        /// </summary>
        public static readonly string VideoOutput = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vid.mp4");
    }
}