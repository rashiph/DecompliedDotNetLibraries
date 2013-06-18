namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct QueryRange
    {
        internal int end;
        internal int start;
        internal QueryRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        internal int Count
        {
            get
            {
                return ((this.end - this.start) + 1);
            }
        }
        internal bool IsInRange(int point)
        {
            return ((this.start <= point) && (point <= this.end));
        }

        internal void Shift(int offset)
        {
            this.start += offset;
            this.end += offset;
        }
    }
}

