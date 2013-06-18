namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Runtime.InteropServices;

    [Guid("DC3691BC-F188-4b67-8338-326671E0F3F6"), ComVisible(true)]
    public interface IVsaFullErrorInfo : IJSVsaError
    {
        int EndLine { get; }
    }
}

