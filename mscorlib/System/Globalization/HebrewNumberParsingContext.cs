namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct HebrewNumberParsingContext
    {
        internal HebrewNumber.HS state;
        internal int result;
        public HebrewNumberParsingContext(int result)
        {
            this.state = HebrewNumber.HS.Start;
            this.result = result;
        }
    }
}

