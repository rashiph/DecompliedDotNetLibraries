namespace System.ServiceModel.Administration
{
    using System;

    internal class WbemInvalidParameterException : WbemException
    {
        internal WbemInvalidParameterException() : base(WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER)
        {
        }

        internal WbemInvalidParameterException(string name) : base(-2147217400, name)
        {
        }
    }
}

