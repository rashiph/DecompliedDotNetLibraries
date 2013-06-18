namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal class Luid
    {
        internal int lowPart;
        internal int highPart;
        internal Luid()
        {
        }

        public int LowPart
        {
            get
            {
                return this.lowPart;
            }
        }
        public int HighPart
        {
            get
            {
                return this.highPart;
            }
        }
    }
}

