namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("261a6983-c35d-4d0d-aa5b-7867259e77bc")]
    internal interface IIdentityAuthority
    {
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IDefinitionIdentity TextToDefinition([In] uint Flags, [In, MarshalAs(UnmanagedType.LPWStr)] string Identity);
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IReferenceIdentity TextToReference([In] uint Flags, [In, MarshalAs(UnmanagedType.LPWStr)] string Identity);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [SecurityCritical]
        string DefinitionToText([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity DefinitionIdentity);
        [SecurityCritical]
        uint DefinitionToTextBuffer([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity DefinitionIdentity, [In] uint BufferSize, [Out, MarshalAs(UnmanagedType.LPArray)] char[] Buffer);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [SecurityCritical]
        string ReferenceToText([In] uint Flags, [In] System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentity);
        [SecurityCritical]
        uint ReferenceToTextBuffer([In] uint Flags, [In] System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentity, [In] uint BufferSize, [Out, MarshalAs(UnmanagedType.LPArray)] char[] Buffer);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical]
        bool AreDefinitionsEqual([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity Definition1, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity Definition2);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical]
        bool AreReferencesEqual([In] uint Flags, [In] System.Deployment.Internal.Isolation.IReferenceIdentity Reference1, [In] System.Deployment.Internal.Isolation.IReferenceIdentity Reference2);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical]
        bool AreTextualDefinitionsEqual([In] uint Flags, [In, MarshalAs(UnmanagedType.LPWStr)] string IdentityLeft, [In, MarshalAs(UnmanagedType.LPWStr)] string IdentityRight);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical]
        bool AreTextualReferencesEqual([In] uint Flags, [In, MarshalAs(UnmanagedType.LPWStr)] string IdentityLeft, [In, MarshalAs(UnmanagedType.LPWStr)] string IdentityRight);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical]
        bool DoesDefinitionMatchReference([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity DefinitionIdentity, [In] System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentity);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical]
        bool DoesTextualDefinitionMatchTextualReference([In] uint Flags, [In, MarshalAs(UnmanagedType.LPWStr)] string Definition, [In, MarshalAs(UnmanagedType.LPWStr)] string Reference);
        [SecurityCritical]
        ulong HashReference([In] uint Flags, [In] System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentity);
        [SecurityCritical]
        ulong HashDefinition([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity DefinitionIdentity);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [SecurityCritical]
        string GenerateDefinitionKey([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity DefinitionIdentity);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [SecurityCritical]
        string GenerateReferenceKey([In] uint Flags, [In] System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentity);
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IDefinitionIdentity CreateDefinition();
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IReferenceIdentity CreateReference();
    }
}

