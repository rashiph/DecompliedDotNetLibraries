namespace Microsoft.JScript
{
    using System;

    internal class ScannerException : Exception
    {
        internal JSError m_errorId;

        internal ScannerException(JSError errorId) : base(JScriptException.Localize("Scanner Exception", CultureInfo.CurrentUICulture))
        {
            this.m_errorId = errorId;
        }
    }
}

