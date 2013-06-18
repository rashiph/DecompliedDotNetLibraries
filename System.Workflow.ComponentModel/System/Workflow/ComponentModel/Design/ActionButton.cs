namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;
    using System.Threading;

    internal sealed class ActionButton : IDisposable
    {
        private States buttonState;
        private string description = string.Empty;
        private Image[] stateImages;

        internal event EventHandler StateChanged;

        internal ActionButton(Image[] stateImages)
        {
            this.StateImages = stateImages;
        }

        void IDisposable.Dispose()
        {
        }

        internal string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.description;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.description = value;
            }
        }

        internal States State
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.buttonState;
            }
            set
            {
                if (this.buttonState != value)
                {
                    this.buttonState = value;
                    if (this.StateChanged != null)
                    {
                        this.StateChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        internal Image[] StateImages
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.stateImages;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((value.Length != 1) && (value.Length != 4))
                {
                    throw new ArgumentException(SR.GetString("Error_InvalidStateImages"), "value");
                }
                this.stateImages = value;
                foreach (Image image in this.stateImages)
                {
                    Bitmap bitmap = image as Bitmap;
                    if (bitmap != null)
                    {
                        bitmap.MakeTransparent(AmbientTheme.TransparentColor);
                    }
                }
            }
        }

        internal enum States
        {
            Normal,
            Highlight,
            Pressed,
            Disabled
        }
    }
}

