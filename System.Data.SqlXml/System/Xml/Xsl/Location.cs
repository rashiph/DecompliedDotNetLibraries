namespace System.Xml.Xsl
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("({Line},{Pos})")]
    internal struct Location
    {
        private ulong value;
        public int Line
        {
            get
            {
                return (int) (this.value >> 0x20);
            }
        }
        public int Pos
        {
            get
            {
                return (int) this.value;
            }
        }
        public Location(int line, int pos)
        {
            this.value = (line << 0x20) | ((ulong) pos);
        }

        public Location(Location that)
        {
            this.value = that.value;
        }

        public bool LessOrEqual(Location that)
        {
            return (this.value <= that.value);
        }
    }
}

