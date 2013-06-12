namespace System.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public interface IConfigurationSystem
    {
        object GetConfig(string configKey);
        void Init();
    }
}

