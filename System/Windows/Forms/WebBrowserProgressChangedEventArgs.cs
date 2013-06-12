namespace System.Windows.Forms
{
    using System;

    public class WebBrowserProgressChangedEventArgs : EventArgs
    {
        private long currentProgress;
        private long maximumProgress;

        public WebBrowserProgressChangedEventArgs(long currentProgress, long maximumProgress)
        {
            this.currentProgress = currentProgress;
            this.maximumProgress = maximumProgress;
        }

        public long CurrentProgress
        {
            get
            {
                return this.currentProgress;
            }
        }

        public long MaximumProgress
        {
            get
            {
                return this.maximumProgress;
            }
        }
    }
}

