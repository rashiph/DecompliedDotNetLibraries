namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    public class PopupEventArgs : CancelEventArgs
    {
        private Control associatedControl;
        private IWin32Window associatedWindow;
        private bool isBalloon;
        private Size size;

        public PopupEventArgs(IWin32Window associatedWindow, Control associatedControl, bool isBalloon, Size size)
        {
            this.associatedWindow = associatedWindow;
            this.size = size;
            this.associatedControl = associatedControl;
            this.isBalloon = isBalloon;
        }

        public Control AssociatedControl
        {
            get
            {
                return this.associatedControl;
            }
        }

        public IWin32Window AssociatedWindow
        {
            get
            {
                return this.associatedWindow;
            }
        }

        public bool IsBalloon
        {
            get
            {
                return this.isBalloon;
            }
        }

        public Size ToolTipSize
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }
    }
}

