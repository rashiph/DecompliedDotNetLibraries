namespace System.ServiceModel.Administration
{
    using System;

    internal class WbemInvalidMethodException : WbemException
    {
        internal WbemInvalidMethodException() : base(WbemNative.WbemStatus.WBEM_E_INVALID_METHOD)
        {
        }
    }
}

