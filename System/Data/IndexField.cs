namespace System.Data
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IndexField
    {
        public readonly DataColumn Column;
        public readonly bool IsDescending;
        internal IndexField(DataColumn column, bool isDescending)
        {
            this.Column = column;
            this.IsDescending = isDescending;
        }
    }
}

