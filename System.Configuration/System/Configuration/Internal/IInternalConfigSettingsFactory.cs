namespace System.Configuration.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public interface IInternalConfigSettingsFactory
    {
        void CompleteInit();
        void SetConfigurationSystem(IInternalConfigSystem internalConfigSystem, bool initComplete);
    }
}

