namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3BC15AF2-736C-477E-9E51-238AF8667DCC"), InterfaceType((short) 1)]
    internal interface IWbemPath
    {
        [PreserveSig]
        int SetText_([In] uint uMode, [In, MarshalAs(UnmanagedType.LPWStr)] string pszPath);
        [PreserveSig]
        int GetText_([In] int lFlags, [In, Out] ref uint puBuffLength, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string pszText);
        [PreserveSig]
        int GetInfo_([In] uint uRequestedInfo, out ulong puResponse);
        [PreserveSig]
        int SetServer_([In, MarshalAs(UnmanagedType.LPWStr)] string Name);
        [PreserveSig]
        int GetServer_([In, Out] ref uint puNameBufLength, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string pName);
        [PreserveSig]
        int GetNamespaceCount_(out uint puCount);
        [PreserveSig]
        int SetNamespaceAt_([In] uint uIndex, [In, MarshalAs(UnmanagedType.LPWStr)] string pszName);
        [PreserveSig]
        int GetNamespaceAt_([In] uint uIndex, [In, Out] ref uint puNameBufLength, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string pName);
        [PreserveSig]
        int RemoveNamespaceAt_([In] uint uIndex);
        [PreserveSig]
        int RemoveAllNamespaces_();
        [PreserveSig]
        int GetScopeCount_(out uint puCount);
        [PreserveSig]
        int SetScope_([In] uint uIndex, [In, MarshalAs(UnmanagedType.LPWStr)] string pszClass);
        [PreserveSig]
        int SetScopeFromText_([In] uint uIndex, [In, MarshalAs(UnmanagedType.LPWStr)] string pszText);
        [PreserveSig]
        int GetScope_([In] uint uIndex, [In, Out] ref uint puClassNameBufSize, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string pszClass, [MarshalAs(UnmanagedType.Interface)] out IWbemPathKeyList pKeyList);
        [PreserveSig]
        int GetScopeAsText_([In] uint uIndex, [In, Out] ref uint puTextBufSize, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string pszText);
        [PreserveSig]
        int RemoveScope_([In] uint uIndex);
        [PreserveSig]
        int RemoveAllScopes_();
        [PreserveSig]
        int SetClassName_([In, MarshalAs(UnmanagedType.LPWStr)] string Name);
        [PreserveSig]
        int GetClassName_([In, Out] ref uint puBuffLength, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string pszName);
        [PreserveSig]
        int GetKeyList_([MarshalAs(UnmanagedType.Interface)] out IWbemPathKeyList pOut);
        [PreserveSig]
        int CreateClassPart_([In] int lFlags, [In, MarshalAs(UnmanagedType.LPWStr)] string Name);
        [PreserveSig]
        int DeleteClassPart_([In] int lFlags);
        [PreserveSig]
        int IsRelative_([In, MarshalAs(UnmanagedType.LPWStr)] string wszMachine, [In, MarshalAs(UnmanagedType.LPWStr)] string wszNamespace);
        [PreserveSig]
        int IsRelativeOrChild_([In, MarshalAs(UnmanagedType.LPWStr)] string wszMachine, [In, MarshalAs(UnmanagedType.LPWStr)] string wszNamespace, [In] int lFlags);
        [PreserveSig]
        int IsLocal_([In, MarshalAs(UnmanagedType.LPWStr)] string wszMachine);
        [PreserveSig]
        int IsSameClassName_([In, MarshalAs(UnmanagedType.LPWStr)] string wszClass);
    }
}

