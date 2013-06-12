namespace System.Runtime.InteropServices
{
    using System;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), Obsolete("Use System.Runtime.InteropServices.ComTypes.IDLDESC instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public struct IDLDESC
    {
        public int dwReserved;
        public IDLFLAG wIDLFlags;
    }
}

