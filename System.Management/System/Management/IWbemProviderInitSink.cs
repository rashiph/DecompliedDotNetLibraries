namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("1BE41571-91DD-11D1-AEB2-00C04FB68820")]
    internal interface IWbemProviderInitSink
    {
        [PreserveSig]
        int SetStatus_([In] int lStatus, [In] int lFlags);
    }
}

