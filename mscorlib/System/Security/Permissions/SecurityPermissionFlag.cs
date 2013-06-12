namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum SecurityPermissionFlag
    {
        AllFlags = 0x3fff,
        Assertion = 1,
        BindingRedirects = 0x2000,
        ControlAppDomain = 0x400,
        ControlDomainPolicy = 0x100,
        ControlEvidence = 0x20,
        ControlPolicy = 0x40,
        ControlPrincipal = 0x200,
        ControlThread = 0x10,
        Execution = 8,
        Infrastructure = 0x1000,
        NoFlags = 0,
        RemotingConfiguration = 0x800,
        SerializationFormatter = 0x80,
        SkipVerification = 4,
        UnmanagedCode = 2
    }
}

