namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;

    internal sealed class DualCoTaskMem : SafeHandle
    {
        private IntPtr handle2;

        private DualCoTaskMem() : base(IntPtr.Zero, true)
        {
            this.handle2 = IntPtr.Zero;
        }

        internal DualCoTaskMem(UnsafeNativeMethods.IColumnsRowset icolumnsRowset, out IntPtr cOptColumns, out OleDbHResult hr) : base(IntPtr.Zero, true)
        {
            Bid.Trace("<oledb.IColumnsRowset.GetAvailableColumns|API|OLEDB>\n");
            hr = icolumnsRowset.GetAvailableColumns(out cOptColumns, out this.handle);
            Bid.Trace("<oledb.IColumnsRowset.GetAvailableColumns|API|OLEDB|RET> %08X{HRESULT}\n", hr);
        }

        internal DualCoTaskMem(UnsafeNativeMethods.IColumnsInfo columnsInfo, out IntPtr columnCount, out IntPtr columnInfos, out OleDbHResult hr) : this()
        {
            Bid.Trace("<oledb.IColumnsInfo.GetColumnInfo|API|OLEDB>\n");
            hr = columnsInfo.GetColumnInfo(out columnCount, out this.handle, out this.handle2);
            columnInfos = base.handle;
            Bid.Trace("<oledb.IColumnsInfo.GetColumnInfo|API|OLEDB|RET> %08X{HRESULT}\n", hr);
        }

        internal DualCoTaskMem(UnsafeNativeMethods.IDBInfo dbInfo, int[] literals, out int literalCount, out IntPtr literalInfo, out OleDbHResult hr) : this()
        {
            int cLiterals = (literals != null) ? literals.Length : 0;
            Bid.Trace("<oledb.IDBInfo.GetLiteralInfo|API|OLEDB>\n");
            hr = dbInfo.GetLiteralInfo(cLiterals, literals, out literalCount, out this.handle, out this.handle2);
            literalInfo = base.handle;
            Bid.Trace("<oledb.IDBInfo.GetLiteralInfo|API|OLEDB|RET> %08X{HRESULT}\n", hr);
        }

        internal DualCoTaskMem(UnsafeNativeMethods.IDBSchemaRowset dbSchemaRowset, out int schemaCount, out IntPtr schemaGuids, out IntPtr schemaRestrictions, out OleDbHResult hr) : this()
        {
            Bid.Trace("<oledb.IDBSchemaRowset.GetSchemas|API|OLEDB>\n");
            hr = dbSchemaRowset.GetSchemas(out schemaCount, out this.handle, out this.handle2);
            schemaGuids = base.handle;
            schemaRestrictions = this.handle2;
            Bid.Trace("<oledb.IDBSchemaRowset.GetSchemas|API|OLEDB|RET> %08X{HRESULT}\n", hr);
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if (IntPtr.Zero != handle)
            {
                SafeNativeMethods.CoTaskMemFree(handle);
            }
            handle = this.handle2;
            this.handle2 = IntPtr.Zero;
            if (IntPtr.Zero != handle)
            {
                SafeNativeMethods.CoTaskMemFree(handle);
            }
            return true;
        }

        public override bool IsInvalid
        {
            get
            {
                return ((IntPtr.Zero == base.handle) && (IntPtr.Zero == this.handle2));
            }
        }
    }
}

