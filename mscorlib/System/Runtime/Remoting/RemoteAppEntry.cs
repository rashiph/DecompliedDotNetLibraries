namespace System.Runtime.Remoting
{
    using System;

    internal class RemoteAppEntry
    {
        private string _remoteAppName;
        private string _remoteAppURI;

        internal RemoteAppEntry(string appName, string appURI)
        {
            this._remoteAppName = appName;
            this._remoteAppURI = appURI;
        }

        internal string GetAppURI()
        {
            return this._remoteAppURI;
        }
    }
}

