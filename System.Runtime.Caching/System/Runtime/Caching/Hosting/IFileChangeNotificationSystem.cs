namespace System.Runtime.Caching.Hosting
{
    using System;
    using System.Runtime.Caching;
    using System.Runtime.InteropServices;

    public interface IFileChangeNotificationSystem
    {
        void StartMonitoring(string filePath, OnChangedCallback onChangedCallback, out object state, out DateTimeOffset lastWriteTime, out long fileSize);
        void StopMonitoring(string filePath, object state);
    }
}

