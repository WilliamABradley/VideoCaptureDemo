using System;
using CoreGraphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using AVFoundation;
using Foundation;
using UIKit;
using System.Threading.Tasks;
using VideoDemo.Models.Interfaces;
using VideoDemo.Services;

/*
 * AVFoundation Reference: http://red-glasses.com/index.php/tutorials/ios4-take-photos-with-live-video-preview-using-avfoundation/
 * Additional Camera Settings Reference: http://stackoverflow.com/questions/4550271/avfoundation-images-coming-in-unusably-dark
 * Custom Renderers: http://blog.xamarin.com/using-custom-uiviewcontrollers-in-xamarin.forms-on-ios/
 */

[assembly: ExportRenderer(typeof(VideoDemo.Controls.CaptureUI), typeof(VideoDemo.iOS.Controls.CaptureUI))]

namespace VideoDemo.iOS.Controls
{
    public class CaptureUI : ViewRenderer, ICaptureUI, IAVCaptureFileOutputRecordingDelegate
    {
        public CaptureUI()
        {
            App.CurrentCaptureUI = this;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            SetupUserInterface();

            // Simulator does not work for taking pictures.
            if (AppService.OnDevice)
            {
                AuthorizeCameraUse();
                SetupSession();

                StartSession();
            }
            else
            {
                var heightScale = (double)9 / 16;
                var vidHeight = NativeView.Frame.Width * heightScale;
                var yPos = (NativeView.Frame.Height / 2) - (vidHeight / 2);

                liveCameraStream.Frame = new CGRect(0f, yPos, NativeView.Bounds.Width, vidHeight);

                liveCameraStream.BackgroundColor = UIColor.Blue;
                liveCameraStream.Add(new UILabel(new CGRect(0f, 0f, NativeView.Bounds.Width, 20)) { Text = "The Emulator does not support Camera Usage.", TextColor = UIColor.White });
            }
        }

        public async void AuthorizeCameraUse()
        {
            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

            if (authorizationStatus != AVAuthorizationStatus.Authorized)
            {
                await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
            }
        }

        public void SetupSession()
        {
            captureSession = new AVCaptureSession
            {
                SessionPreset = AVCaptureSession.PresetHigh
            };
            movieOutput = new AVCaptureMovieFileOutput();

            var viewLayer = liveCameraStream.Layer;
            var videoPreviewLayer = new AVCaptureVideoPreviewLayer(captureSession)
            {
                Frame = liveCameraStream.Bounds,
                Orientation = VideoOrientation
            };
            liveCameraStream.Layer.AddSublayer(videoPreviewLayer);

            var captureDevice = GetFrontCamera();
            ConfigureCameraForDevice(captureDevice);
            videoDeviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);

            captureSession.AddInput(videoDeviceInput);

            var microphone = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio);
            micDeviceInput = AVCaptureDeviceInput.FromDevice(microphone);

            captureSession.AddInput(micDeviceInput);
            captureSession.AddOutput(movieOutput);
        }

        public AVCaptureDevice GetFrontCamera()
        {
            var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);

            foreach (var device in devices)
            {
                if (device.Position == AVCaptureDevicePosition.Front)
                {
                    return device;
                }
            }

            return null;
        }

        public void ConfigureCameraForDevice(AVCaptureDevice device)
        {
            var error = new NSError();
            if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
            {
                device.LockForConfiguration(out error);
                device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                device.UnlockForConfiguration();
            }
            else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
            {
                device.LockForConfiguration(out error);
                device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                device.UnlockForConfiguration();
            }
            else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
            {
                device.LockForConfiguration(out error);
                device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                device.UnlockForConfiguration();
            }
        }

        private void SetupUserInterface()
        {
            liveCameraStream = new UIView()
            {
                Frame = new CGRect(0f, 0f, NativeView.Bounds.Width, NativeView.Bounds.Height)
            };

            NativeView.Add(liveCameraStream);
        }

        /// <summary>
        /// Begins recording a video to the Specified FilePath.
        /// </summary>
        /// <param name="FilePath">Video recording location</param>
        public void Record(string FilePath)
        {
            if (AppService.OnDevice)
            {
                InvokeOnMainThread(() =>
                {
                    if (movieOutput.Recording)
                    {
                        // Stop the Current Recording.
                        movieOutput.StopRecording();
                    }

                    var connection = movieOutput.ConnectionFromMediaType(AVMediaType.Video);
                    if (connection?.SupportsVideoOrientation == true)
                    {
                        connection.VideoOrientation = VideoOrientation;
                    }

                    var path = NSUrl.FromFilename(FilePath);
                    movieOutput.StartRecordingToOutputFile(path, recordingDelegate: this);
                });
            }
        }

        /// <summary>
        /// Stops the Recording.
        /// </summary>
        public Task StopRecording()
        {
            if (AppService.OnDevice)
            {
                recordingTask = new TaskCompletionSource<bool>();

                InvokeOnMainThread(() =>
                {
                    movieOutput.StopRecording();
                });

                // Waits for Recording to Finish.
                return recordingTask.Task;
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Starts the Capture Session.
        /// </summary>
        public void StartSession()
        {
            captureSession?.StartRunning();
        }

        /// <summary>
        /// Stops the Capture Session.
        /// </summary>
        public void StopSession()
        {
            captureSession?.StopRunning();
        }

        public void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, NSError error)
        {
            if (error != null)
            {
                recordingTask?.TrySetException(new Exception(error.DebugDescription));
            }
            else
            {
                recordingTask?.TrySetResult(true);
            }
        }

        /// <summary>
        /// Cleanup the Resources.
        /// </summary>
        /// <param name="disposing">Is Disposing</param>
        protected override void Dispose(bool disposing)
        {
            captureSession?.Dispose();
            videoDeviceInput?.Dispose();
            micDeviceInput?.Dispose();
            movieOutput?.Dispose();

            base.Dispose(disposing);
        }

        // Recording Objects.

        private AVCaptureSession captureSession;
        private AVCaptureDeviceInput videoDeviceInput;
        private AVCaptureDeviceInput micDeviceInput;
        private AVCaptureMovieFileOutput movieOutput;

        private UIView liveCameraStream;
        private TaskCompletionSource<bool> recordingTask;

        /// <summary>
        /// Determines the Current Recording status.
        /// </summary>
        public bool Recording => movieOutput?.Recording ?? false;

        /// <summary>
        /// Determines if the Capture Session is active.
        /// </summary>
        public bool SessionActive => captureSession?.Running ?? false;

        /// <summary>
        /// The HardCoded Video orientation.
        /// </summary>
        private const AVCaptureVideoOrientation VideoOrientation = AVCaptureVideoOrientation.LandscapeLeft;
    }
}