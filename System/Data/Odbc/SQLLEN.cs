namespace System.Data.Odbc
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SQLLEN
    {
        private IntPtr _value;
        internal SQLLEN(int value)
        {
            this._value = new IntPtr(value);
        }

        internal SQLLEN(long value)
        {
            this._value = new IntPtr((int) value);
        }

        internal SQLLEN(IntPtr value)
        {
            this._value = value;
        }

        public static implicit operator SQLLEN(int value)
        {
            return new SQLLEN(value);
        }

        public static explicit operator SQLLEN(long value)
        {
            return new SQLLEN(value);
        }

        public static implicit operator int(SQLLEN value)
        {
            return value._value.ToInt32();
        }

        public static explicit operator long(SQLLEN value)
        {
            return value._value.ToInt64();
        }

        public long ToInt64()
        {
            return this._value.ToInt64();
        }
    }
}

