namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        private const string ADVAPI32 = "advapi32.dll";
        internal const int BufferTooSmall = 0x7a;
        internal const int ThreadTokenAllAccess = 0xf01ff;

        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern IntPtr GetSidIdentifierAuthority(IntPtr sidPointer);
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern IntPtr GetSidSubAuthority(IntPtr sidPointer, int count);
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern IntPtr GetSidSubAuthorityCount(IntPtr sidPointer);
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool GetTokenInformation(IntPtr tokenHandle, int tokenInformationClass, IntPtr sidAndAttributesPointer, int tokenInformationLength, ref int returnLength);
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool IsValidSid(IntPtr sidPointer);

        internal enum TokenInformationClass
        {
            TokenDefaultDacl = 6,
            TokenGroups = 2,
            TokenGroupsAndPrivileges = 13,
            TokenImpersonationLevel = 9,
            TokenOwner = 4,
            TokenPrimaryGroup = 5,
            TokenPrivileges = 3,
            TokenRestrictedSids = 11,
            TokenSandBoxInert = 15,
            TokenSessionId = 12,
            TokenSessionReference = 14,
            TokenSource = 7,
            TokenStatistics = 10,
            TokenType = 8,
            TokenUser = 1
        }
    }
}

