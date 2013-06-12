namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Position
    {
        public int symbol;
        public object particle;
        public Position(int symbol, object particle)
        {
            this.symbol = symbol;
            this.particle = particle;
        }
    }
}

