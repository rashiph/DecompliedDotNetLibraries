namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LineInfo
    {
        internal int lineNo;
        internal int linePos;
        public LineInfo(int lineNo, int linePos)
        {
            this.lineNo = lineNo;
            this.linePos = linePos;
        }

        public void Set(int lineNo, int linePos)
        {
            this.lineNo = lineNo;
            this.linePos = linePos;
        }
    }
}

