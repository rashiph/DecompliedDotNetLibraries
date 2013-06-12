namespace System.Web
{
    using System;

    internal sealed class FileMonitorTarget
    {
        private int _refs;
        internal readonly string Alias;
        internal readonly FileChangeEventHandler Callback;
        internal readonly DateTime UtcStartMonitoring;

        internal FileMonitorTarget(FileChangeEventHandler callback, string alias)
        {
            this.Callback = callback;
            this.Alias = alias;
            this.UtcStartMonitoring = DateTime.UtcNow;
            this._refs = 1;
        }

        internal int AddRef()
        {
            this._refs++;
            return this._refs;
        }

        internal int Release()
        {
            this._refs--;
            return this._refs;
        }
    }
}

