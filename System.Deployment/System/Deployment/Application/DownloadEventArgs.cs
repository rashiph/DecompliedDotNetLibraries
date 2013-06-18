namespace System.Deployment.Application
{
    using System;

    internal class DownloadEventArgs : EventArgs
    {
        internal long _bytesCompleted;
        internal long _bytesTotal;
        internal object _cookie;
        internal string _fileLocalPath;
        internal Uri _fileResponseUri;
        internal int _filesCompleted;
        internal Uri _fileSourceUri;
        internal int _filesTotal;
        internal int _progress;

        public long BytesCompleted
        {
            get
            {
                return this._bytesCompleted;
            }
        }

        public long BytesTotal
        {
            get
            {
                return this._bytesTotal;
            }
        }

        internal object Cookie
        {
            get
            {
                return this._cookie;
            }
            set
            {
                this._cookie = value;
            }
        }

        internal string FileLocalPath
        {
            get
            {
                return this._fileLocalPath;
            }
            set
            {
                this._fileLocalPath = value;
            }
        }

        public Uri FileResponseUri
        {
            get
            {
                return this._fileResponseUri;
            }
        }

        public Uri FileSourceUri
        {
            get
            {
                return this._fileSourceUri;
            }
        }

        public int Progress
        {
            get
            {
                return this._progress;
            }
        }
    }
}

