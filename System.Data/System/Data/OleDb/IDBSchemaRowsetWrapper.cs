namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IDBSchemaRowsetWrapper : IDisposable
    {
        private object _unknown;
        private UnsafeNativeMethods.IDBSchemaRowset _value;
        internal IDBSchemaRowsetWrapper(object unknown)
        {
            this._unknown = unknown;
            this._value = unknown as UnsafeNativeMethods.IDBSchemaRowset;
        }

        internal UnsafeNativeMethods.IDBSchemaRowset Value
        {
            get
            {
                return this._value;
            }
        }
        public void Dispose()
        {
            object o = this._unknown;
            this._unknown = null;
            this._value = null;
            if (o != null)
            {
                Marshal.ReleaseComObject(o);
            }
        }
    }
}

