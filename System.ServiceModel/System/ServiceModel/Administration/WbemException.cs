namespace System.ServiceModel.Administration
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal class WbemException : Win32Exception
    {
        internal WbemException(int hr) : base(hr)
        {
        }

        internal WbemException(WbemNative.WbemStatus hr) : base((int) hr)
        {
        }

        internal WbemException(int hr, string message) : base(hr, message)
        {
        }

        internal static void Throw(WbemNative.WbemStatus hr)
        {
            switch (hr)
            {
                case WbemNative.WbemStatus.WBEM_E_NOT_FOUND:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInstanceNotFoundException());

                case WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInvalidParameterException());

                case WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());

                case WbemNative.WbemStatus.WBEM_E_INVALID_METHOD:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInvalidMethodException());
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemException(hr));
        }

        internal static void ThrowIfFail(int hr)
        {
            if (hr < 0)
            {
                Throw((WbemNative.WbemStatus) hr);
            }
        }
    }
}

