namespace System.Web.Caching
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct UsageEntryRef
    {
        private const uint ENTRY_MASK = 0xff;
        private const uint PAGE_MASK = 0xffffff00;
        private const int PAGE_SHIFT = 8;
        internal static readonly UsageEntryRef INVALID;
        private uint _ref;
        internal UsageEntryRef(int pageIndex, int entryIndex)
        {
            this._ref = (uint) ((pageIndex << 8) | (entryIndex & 0xff));
        }

        public override bool Equals(object value)
        {
            return ((value is UsageEntryRef) && (this._ref == ((UsageEntryRef) value)._ref));
        }

        public static bool operator ==(UsageEntryRef r1, UsageEntryRef r2)
        {
            return (r1._ref == r2._ref);
        }

        public static bool operator !=(UsageEntryRef r1, UsageEntryRef r2)
        {
            return (r1._ref != r2._ref);
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
        internal int Ref1Index
        {
            get
            {
                return (sbyte) (this._ref & 0xff);
            }
        }
        internal int Ref2Index
        {
            get
            {
                int num = (sbyte) (this._ref & 0xff);
                return -num;
            }
        }
        internal bool IsRef1
        {
            get
            {
                return (((sbyte) (this._ref & 0xff)) > 0);
            }
        }
        internal bool IsRef2
        {
            get
            {
                return (((sbyte) (this._ref & 0xff)) < 0);
            }
        }
        internal bool IsInvalid
        {
            get
            {
                return (this._ref == 0);
            }
        }
        static UsageEntryRef()
        {
            INVALID = new UsageEntryRef(0, 0);
        }
    }
}

