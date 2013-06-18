namespace System.ServiceModel.Administration
{
    using System;

    internal class WbemNotSupportedException : WbemException
    {
        internal WbemNotSupportedException() : base(WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED)
        {
        }
    }
}

