namespace System.Runtime.Caching
{
    using System;
    using System.Collections.ObjectModel;

    public abstract class FileChangeMonitor : ChangeMonitor
    {
        protected FileChangeMonitor()
        {
        }

        public abstract ReadOnlyCollection<string> FilePaths { get; }

        public abstract DateTimeOffset LastModified { get; }
    }
}

