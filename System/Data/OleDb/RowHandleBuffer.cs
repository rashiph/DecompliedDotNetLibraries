namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;

    internal sealed class RowHandleBuffer : DbBuffer
    {
        internal RowHandleBuffer(IntPtr rowHandleFetchCount) : base(((int) rowHandleFetchCount) * ADP.PtrSize)
        {
        }

        internal IntPtr GetRowHandle(int index)
        {
            return base.ReadIntPtr(index * ADP.PtrSize);
        }
    }
}

