namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(false)]
    internal enum CSC_SxsConfig
    {
        CSC_NoSxs,
        CSC_InheritSxs,
        CSC_NewSxs
    }
}

