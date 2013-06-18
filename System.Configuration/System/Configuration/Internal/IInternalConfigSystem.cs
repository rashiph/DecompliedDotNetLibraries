namespace System.Configuration.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public interface IInternalConfigSystem
    {
        object GetSection(string configKey);
        void RefreshConfig(string sectionName);

        bool SupportsUserConfig { get; }
    }
}

