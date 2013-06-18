namespace System.Configuration.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public interface IConfigurationManagerHelper
    {
        void EnsureNetConfigLoaded();
    }
}

