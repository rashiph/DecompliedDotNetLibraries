namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class LSA_FOREST_TRUST_COLLISION_RECORD
    {
        public int Index;
        public ForestTrustCollisionType Type;
        public int Flags;
        public LSA_UNICODE_STRING Name;
    }
}

