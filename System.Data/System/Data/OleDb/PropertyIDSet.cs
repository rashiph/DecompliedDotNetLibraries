namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Runtime.InteropServices;

    internal sealed class PropertyIDSet : DbBuffer
    {
        private int _count;
        private static readonly int PropertyIDSetAndValueSize = (ODB.SizeOf_tagDBPROPIDSET + ADP.PtrSize);
        private static readonly int PropertyIDSetSize = ODB.SizeOf_tagDBPROPIDSET;

        internal PropertyIDSet(Guid[] propertySets) : base(PropertyIDSetSize * propertySets.Length)
        {
            this._count = propertySets.Length;
            for (int i = 0; i < propertySets.Length; i++)
            {
                IntPtr ptr = ADP.IntPtrOffset(base.handle, (i * PropertyIDSetSize) + ODB.OffsetOf_tagDBPROPIDSET_PropertySet);
                Marshal.StructureToPtr(propertySets[i], ptr, false);
            }
        }

        internal PropertyIDSet(Guid propertySet, int propertyID) : base(PropertyIDSetAndValueSize)
        {
            this._count = 1;
            IntPtr val = ADP.IntPtrOffset(base.handle, PropertyIDSetSize);
            Marshal.WriteIntPtr(base.handle, 0, val);
            Marshal.WriteInt32(base.handle, ADP.PtrSize, 1);
            val = ADP.IntPtrOffset(base.handle, ODB.OffsetOf_tagDBPROPIDSET_PropertySet);
            Marshal.StructureToPtr(propertySet, val, false);
            Marshal.WriteInt32(base.handle, PropertyIDSetSize, propertyID);
        }

        internal int Count
        {
            get
            {
                return this._count;
            }
        }
    }
}

