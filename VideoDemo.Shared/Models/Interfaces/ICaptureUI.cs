using System;
using System.Threading.Tasks;

namespace VideoDemo.Models.Interfaces
{
    /// <summary>
    /// An interface for the Native Capture UI.
    /// </summary>
    public interface ICaptureUI : IDisposable
    {
        /// <summary>
        /// Begins recording a video to the Specified FilePath.
        /// </summary>
        /// <param name="FilePath">Video recording location</param>
        void Record(string FilePath);

        /// <summary>
        /// Stops the Recording.
        /// </summary>
        Task StopRecording();

        /// <summary>
        /// Starts the Capture Session.
        /// </summary>
        void StartSession();

        /// <summary>
        /// Stops the Capture Session.
        /// </summary>
        void StopSession();

        /// <summary>
        /// Gets the current status of the Recording.
        /// </summary>
        bool Recording { get; }

        /// <summary>
        /// Determines if the Capture Session is active.
        /// </summary>
        bool SessionActive { get; }
    }
}