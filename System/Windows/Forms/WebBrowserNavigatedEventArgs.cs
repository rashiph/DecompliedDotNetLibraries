namespace System.Windows.Forms
{
    using System;

    public class WebBrowserNavigatedEventArgs : EventArgs
    {
        private Uri url;

        public WebBrowserNavigatedEventArgs(Uri url)
        {
            this.url = url;
        }

        public Uri Url
        {
            get
            {
                WebBrowser.EnsureUrlConnectPermission(this.url);
                return this.url;
            }
        }
    }
}

