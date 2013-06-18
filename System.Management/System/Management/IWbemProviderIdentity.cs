namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("631F7D97-D993-11D2-B339-00105A1F4AAF"), TypeLibType((short) 0x200)]
    internal interface IWbemProviderIdentity
    {
        [PreserveSig]
        int SetRegistrationObject_([In] int lFlags, [In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pProvReg);
    }
}

