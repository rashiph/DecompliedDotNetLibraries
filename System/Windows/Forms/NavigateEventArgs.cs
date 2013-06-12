namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class NavigateEventArgs : EventArgs
    {
        private bool isForward = true;

        public NavigateEventArgs(bool isForward)
        {
            this.isForward = isForward;
        }

        public bool Forward
        {
            get
            {
                return this.isForward;
            }
        }
    }
}

