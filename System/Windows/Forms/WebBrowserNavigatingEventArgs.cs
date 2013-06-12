namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class WebBrowserNavigatingEventArgs : CancelEventArgs
    {
        private string targetFrameName;
        private Uri url;

        public WebBrowserNavigatingEventArgs(Uri url, string targetFrameName)
        {
            this.url = url;
            this.targetFrameName = targetFrameName;
        }

        public string TargetFrameName
        {
            get
            {
                WebBrowser.EnsureUrlConnectPermission(this.url);
                return this.targetFrameName;
            }
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

