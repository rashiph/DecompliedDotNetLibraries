namespace System.Configuration.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public interface IInternalConfigRecord
    {
        object GetLkgSection(string configKey);
        object GetSection(string configKey);
        void RefreshSection(string configKey);
        void Remove();
        void ThrowIfInitErrors();

        string ConfigPath { get; }

        bool HasInitErrors { get; }

        string StreamName { get; }
    }
}

