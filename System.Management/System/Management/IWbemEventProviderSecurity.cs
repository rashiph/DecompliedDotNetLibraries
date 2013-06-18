namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("631F7D96-D993-11D2-B339-00105A1F4AAF"), TypeLibType((short) 0x200), InterfaceType((short) 1)]
    internal interface IWbemEventProviderSecurity
    {
        [PreserveSig]
        int AccessCheck_([In, MarshalAs(UnmanagedType.LPWStr)] string wszQueryLanguage, [In, MarshalAs(UnmanagedType.LPWStr)] string wszQuery, [In] int lSidLength, [In] ref byte pSid);
    }
}

