using PlatformBindings.Models;
using System;

namespace VideoDemo.ViewModels
{
    /// <summary>
    /// The Viewmodel for the Capture Page.
    /// </summary>
    public class CaptureViewModel : ViewModelBase
    {
        /// <summary>
        /// Begins a Capture Sequence, preparing the UI and the Timer.
        /// </summary>
        public void StartCaptureSequence()
        {
            Instruction = Prepare;
            Elapsed = 0;
            ShowInstruction = true;

            // Dispose of the old timer and event if exists.
            if (Timer != null)
            {
                Timer.Stop();
                Timer.Tick -= Timer_Tick;
            }

            Timer = new LoopTimer(TimeSpan.FromSeconds(1), true);
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        /// <summary>
        /// Ends the Sequence prematurely.
        /// </summary>
        public void StopCaptureSequence()
        {
            Timer.Stop();
            CaptureCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// A Tick of the Timer (Every one second)
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            Elapsed++;
            if (Elapsed <= 5)
            {
                // Count down from 5 to 0
                Counter = (6 - Elapsed).ToString();

                if (Elapsed == 5)
                {
                    Instruction = Ready;
                }
            }
            else if (Elapsed <= 20)
            {
                // Start Capture
                if (Elapsed == 6)
                {
                    CaptureStarted?.Invoke(this, EventArgs.Empty);
                    Instruction = Go;
                }
                else if (Elapsed == 7)
                {
                    // Capture Mode
                    ShowInstruction = false;
                }

                // Count down to 0 from 15
                Counter = (21 - Elapsed).ToString();
            }
            else
            {
                // End Capture
                Timer.Stop();
                CaptureCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The Counter message.
        /// </summary>
        public string Counter
        {
            get { return _Counter; }
            set
            {
                _Counter = value;
                UpdateProperty();
            }
        }

        /// <summary>
        /// The current Instruction to the User.
        /// </summary>
        public string Instruction
        {
            get { return _Instruction; }
            set
            {
                _Instruction = value;
                UpdateProperty();
            }
        }

        /// <summary>
        /// Determines if the Instruction message should be shown.
        /// </summary>
        public bool ShowInstruction
        {
            get { return _ShowInstruction; }
            set
            {
                _ShowInstruction = value;
                UpdateProperty();
                UpdateProperty(nameof(ShowStopButton));
            }
        }

        /// <summary>
        /// Determines if the Stop Button should be shown.
        /// </summary>
        public bool ShowStopButton => !ShowInstruction;

        /// <summary>
        /// Backing Field for <see cref="Counter"/>.
        /// </summary>
        private string _Counter;

        /// <summary>
        /// Backing Field for <see cref="Instruction"/>.
        /// </summary>
        private string _Instruction;

        /// <summary>
        /// Backing Field for <see cref="ShowInstruction"/>.
        /// </summary>
        private bool _ShowInstruction = true;

        /// <summary>
        /// The number of Seconds elapsed.
        /// </summary>
        private int Elapsed = 0;

        /// <summary>
        /// The Timer instance.
        /// </summary>
        private LoopTimer Timer { get; set; }

        /// <summary>
        /// The Capture is Starting.
        /// </summary>
        public event EventHandler CaptureStarted;

        /// <summary>
        /// The Capture is Complete.
        /// </summary>
        public event EventHandler CaptureCompleted;

        /// <summary>
        /// This Event should take 20 Seconds.
        /// </summary>
        private const int TotalSeconds = 20;

        /// <summary>
        /// The Preparation Message.
        /// </summary>
        private const string Prepare = "Get into position.";

        /// <summary>
        /// The Ready Message.
        /// </summary>
        private const string Ready = "Get ready.";

        /// <summary>
        /// The GO Message.
        /// </summary>
        private const string Go = "Recording Started.";
    }
}