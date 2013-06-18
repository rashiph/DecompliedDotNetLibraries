namespace System.Runtime.Caching
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ExpiresEntryRef
    {
        private const uint ENTRY_MASK = 0xff;
        private const uint PAGE_MASK = 0xffffff00;
        private const int PAGE_SHIFT = 8;
        internal static readonly ExpiresEntryRef INVALID;
        private uint _ref;
        internal ExpiresEntryRef(int pageIndex, int entryIndex)
        {
            this._ref = (uint) ((pageIndex << 8) | (entryIndex & 0xff));
        }

        public override bool Equals(object value)
        {
            return ((value is ExpiresEntryRef) && (this._ref == ((ExpiresEntryRef) value)._ref));
        }

        public static bool operator !=(ExpiresEntryRef r1, ExpiresEntryRef r2)
        {
            return (r1._ref != r2._ref);
        }

        public static bool operator ==(ExpiresEntryRef r1, ExpiresEntryRef r2)
        {
            return (r1._ref == r2._ref);
        }

        public override int GetHashCode()
        {
            return (int) this._ref;
        }

        internal int PageIndex
        {
            get
            {
                return (int) (this._ref >> 8);
            }
        }
        internal int Index
        {
            get
            {
                return (((int) this._ref) & 0xff);
            }
        }
        internal bool IsInvalid
        {
            get
            {
                return (this._ref == 0);
            }
        }
        static ExpiresEntryRef()
        {
            INVALID = new ExpiresEntryRef(0, 0);
        }
    }
}

