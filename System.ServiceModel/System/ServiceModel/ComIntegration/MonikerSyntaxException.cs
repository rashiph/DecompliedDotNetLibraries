namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    internal class MonikerSyntaxException : COMException
    {
        internal MonikerSyntaxException(string message) : base(message, HR.MK_E_SYNTAX)
        {
        }
    }
}

