namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class GENERIC_MAPPING
    {
        internal uint genericRead;
        internal uint genericWrite;
        internal uint genericExecute;
        internal uint genericAll;
    }
}

