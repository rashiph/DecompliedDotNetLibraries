namespace System.Management
{
    using System;

    public enum AuthenticationLevel
    {
        Call = 3,
        Connect = 2,
        Default = 0,
        None = 1,
        Packet = 4,
        PacketIntegrity = 5,
        PacketPrivacy = 6,
        Unchanged = -1
    }
}

