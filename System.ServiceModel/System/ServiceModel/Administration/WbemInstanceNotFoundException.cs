namespace System.ServiceModel.Administration
{
    using System;

    internal class WbemInstanceNotFoundException : WbemException
    {
        internal WbemInstanceNotFoundException() : base(WbemNative.WbemStatus.WBEM_E_NOT_FOUND)
        {
        }
    }
}

