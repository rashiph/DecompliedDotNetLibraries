namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, Obsolete("Use System.Runtime.InteropServices.ComTypes.CALLCONV instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public enum CALLCONV
    {
        CC_CDECL = 1,
        CC_MACPASCAL = 3,
        CC_MAX = 9,
        CC_MPWCDECL = 7,
        CC_MPWPASCAL = 8,
        CC_MSCPASCAL = 2,
        CC_PASCAL = 2,
        CC_RESERVED = 5,
        CC_STDCALL = 4,
        CC_SYSCALL = 6
    }
}

