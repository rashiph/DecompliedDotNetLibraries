namespace System.Runtime.InteropServices
{
    using System;

    [StructLayout(LayoutKind.Sequential), Obsolete("Use System.Runtime.InteropServices.ComTypes.FILETIME instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public struct FILETIME
    {
        public int dwLowDateTime;
        public int dwHighDateTime;
    }
}

