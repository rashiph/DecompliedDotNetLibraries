namespace System.Windows.Forms
{
    using System;

    public class WebBrowserDocumentCompletedEventArgs : EventArgs
    {
        private Uri url;

        public WebBrowserDocumentCompletedEventArgs(Uri url)
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

