namespace System.IO
{
    using System;

    public class FileSystemEventArgs : EventArgs
    {
        private WatcherChangeTypes changeType;
        private string fullPath;
        private string name;

        public FileSystemEventArgs(WatcherChangeTypes changeType, string directory, string name)
        {
            this.changeType = changeType;
            this.name = name;
            if (!directory.EndsWith(@"\", StringComparison.Ordinal))
            {
                directory = directory + @"\";
            }
            this.fullPath = directory + name;
        }

        public WatcherChangeTypes ChangeType
        {
            get
            {
                return this.changeType;
            }
        }

        public string FullPath
        {
            get
            {
                return this.fullPath;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

