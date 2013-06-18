namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("9AE62877-7544-4BB0-AA26-A13824659ED6"), InterfaceType((short) 1)]
    internal interface IWbemPathKeyList
    {
        [PreserveSig]
        int GetCount_(out uint puKeyCount);
        [PreserveSig]
        int SetKey_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] uint uFlags, [In] uint uCimType, [In] IntPtr pKeyVal);
        [PreserveSig]
        int SetKey2_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] uint uFlags, [In] uint uCimType, [In] ref object pKeyVal);
        [PreserveSig]
        int GetKey_([In] uint uKeyIx, [In] uint uFlags, [In, Out] ref uint puNameBufSize, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string pszKeyName, [In, Out] ref uint puKeyValBufSize, [In, Out] IntPtr pKeyVal, out uint puApparentCimType);
        [PreserveSig]
        int GetKey2_([In] uint uKeyIx, [In] uint uFlags, [In, Out] ref uint puNameBufSize, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string pszKeyName, [In, Out] ref object pKeyValue, out uint puApparentCimType);
        [PreserveSig]
        int RemoveKey_([In, MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] uint uFlags);
        [PreserveSig]
        int RemoveAllKeys_([In] uint uFlags);
        [PreserveSig]
        int MakeSingleton_([In] sbyte bSet);
        [PreserveSig]
        int GetInfo_([In] uint uRequestedInfo, out ulong puResponse);
        [PreserveSig]
        int GetText_([In] int lFlags, [In, Out] ref uint puBuffLength, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string pszText);
    }
}

