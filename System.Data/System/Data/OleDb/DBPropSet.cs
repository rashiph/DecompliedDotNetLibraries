namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class DBPropSet : SafeHandle
    {
        private Exception lastErrorFromProvider;
        private readonly int propertySetCount;

        private DBPropSet() : base(IntPtr.Zero, true)
        {
            this.propertySetCount = 0;
        }

        internal DBPropSet(int propertysetCount) : this()
        {
            this.propertySetCount = propertysetCount;
            IntPtr cb = (IntPtr) (propertysetCount * ODB.SizeOf_tagDBPROPSET);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                base.handle = SafeNativeMethods.CoTaskMemAlloc(cb);
                if (ADP.PtrZero != base.handle)
                {
                    SafeNativeMethods.ZeroMemory(base.handle, cb);
                }
            }
            if (ADP.PtrZero == base.handle)
            {
                throw new OutOfMemoryException();
            }
        }

        internal DBPropSet(UnsafeNativeMethods.ICommandProperties properties, PropertyIDSet propidset, out OleDbHResult hr) : this()
        {
            int cPropertyIDSets = 0;
            if (propidset != null)
            {
                cPropertyIDSets = propidset.Count;
            }
            Bid.Trace("<oledb.ICommandProperties.GetProperties|API|OLEDB>\n");
            hr = properties.GetProperties(cPropertyIDSets, propidset, out this.propertySetCount, out this.handle);
            Bid.Trace("<oledb.ICommandProperties.GetProperties|API|OLEDB|RET> %08X{HRESULT}\n", hr);
            if (hr < OleDbHResult.S_OK)
            {
                this.SetLastErrorInfo(hr);
            }
        }

        internal DBPropSet(UnsafeNativeMethods.IDBProperties properties, PropertyIDSet propidset, out OleDbHResult hr) : this()
        {
            int cPropertyIDSets = 0;
            if (propidset != null)
            {
                cPropertyIDSets = propidset.Count;
            }
            Bid.Trace("<oledb.IDBProperties.GetProperties|API|OLEDB>\n");
            hr = properties.GetProperties(cPropertyIDSets, propidset, out this.propertySetCount, out this.handle);
            Bid.Trace("<oledb.IDBProperties.GetProperties|API|OLEDB|RET> %08X{HRESULT}\n", hr);
            if (hr < OleDbHResult.S_OK)
            {
                this.SetLastErrorInfo(hr);
            }
        }

        internal DBPropSet(UnsafeNativeMethods.IRowsetInfo properties, PropertyIDSet propidset, out OleDbHResult hr) : this()
        {
            int cPropertyIDSets = 0;
            if (propidset != null)
            {
                cPropertyIDSets = propidset.Count;
            }
            Bid.Trace("<oledb.IRowsetInfo.GetProperties|API|OLEDB>\n");
            hr = properties.GetProperties(cPropertyIDSets, propidset, out this.propertySetCount, out this.handle);
            Bid.Trace("<oledb.IRowsetInfo.GetProperties|API|OLEDB|RET> %08X{HRESULT}\n", hr);
            if (hr < OleDbHResult.S_OK)
            {
                this.SetLastErrorInfo(hr);
            }
        }

        internal static DBPropSet CreateProperty(Guid propertySet, int propertyId, bool required, object value)
        {
            tagDBPROP gdbprop = new tagDBPROP(propertyId, required, value);
            DBPropSet set = new DBPropSet(1);
            set.SetPropertySet(0, propertySet, new tagDBPROP[] { gdbprop });
            return set;
        }

        internal tagDBPROP[] GetPropertySet(int index, out Guid propertyset)
        {
            if ((index < 0) || (this.PropertySetCount <= index))
            {
                if (this.lastErrorFromProvider != null)
                {
                    throw ADP.InternalError(ADP.InternalErrorCode.InvalidBuffer, this.lastErrorFromProvider);
                }
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidBuffer);
            }
            tagDBPROPSET structure = new tagDBPROPSET();
            tagDBPROP[] gdbpropArray = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                Marshal.PtrToStructure(ADP.IntPtrOffset(base.DangerousGetHandle(), index * ODB.SizeOf_tagDBPROPSET), structure);
                propertyset = structure.guidPropertySet;
                gdbpropArray = new tagDBPROP[structure.cProperties];
                for (int i = 0; i < gdbpropArray.Length; i++)
                {
                    gdbpropArray[i] = new tagDBPROP();
                    Marshal.PtrToStructure(ADP.IntPtrOffset(structure.rgProperties, i * ODB.SizeOf_tagDBPROP), gdbpropArray[i]);
                }
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return gdbpropArray;
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if (ADP.PtrZero != handle)
            {
                int propertySetCount = this.propertySetCount;
                int num3 = 0;
                for (int i = 0; num3 < propertySetCount; i += ODB.SizeOf_tagDBPROPSET)
                {
                    IntPtr pbase = Marshal.ReadIntPtr(handle, i);
                    if (ADP.PtrZero != pbase)
                    {
                        int num4 = Marshal.ReadInt32(handle, i + ADP.PtrSize);
                        IntPtr pObject = ADP.IntPtrOffset(pbase, ODB.OffsetOf_tagDBPROP_Value);
                        int num2 = 0;
                        while (num2 < num4)
                        {
                            SafeNativeMethods.VariantClear(pObject);
                            num2++;
                            pObject = ADP.IntPtrOffset(pObject, ODB.SizeOf_tagDBPROP);
                        }
                        SafeNativeMethods.CoTaskMemFree(pbase);
                    }
                    num3++;
                }
                SafeNativeMethods.CoTaskMemFree(handle);
            }
            return true;
        }

        private void SetLastErrorInfo(OleDbHResult lastErrorHr)
        {
            UnsafeNativeMethods.IErrorInfo ppIErrorInfo = null;
            string message = string.Empty;
            if ((UnsafeNativeMethods.GetErrorInfo(0, out ppIErrorInfo) == OleDbHResult.S_OK) && (ppIErrorInfo != null))
            {
                ODB.GetErrorDescription(ppIErrorInfo, lastErrorHr, out message);
            }
            this.lastErrorFromProvider = new COMException(message, (int) lastErrorHr);
        }

        internal void SetPropertySet(int index, Guid propertySet, tagDBPROP[] properties)
        {
            if ((index < 0) || (this.PropertySetCount <= index))
            {
                if (this.lastErrorFromProvider != null)
                {
                    throw ADP.InternalError(ADP.InternalErrorCode.InvalidBuffer, this.lastErrorFromProvider);
                }
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidBuffer);
            }
            IntPtr cb = (IntPtr) (properties.Length * ODB.SizeOf_tagDBPROP);
            tagDBPROPSET structure = new tagDBPROPSET(properties.Length, propertySet);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr ptr = ADP.IntPtrOffset(base.DangerousGetHandle(), index * ODB.SizeOf_tagDBPROPSET);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    structure.rgProperties = SafeNativeMethods.CoTaskMemAlloc(cb);
                    if (ADP.PtrZero != structure.rgProperties)
                    {
                        SafeNativeMethods.ZeroMemory(structure.rgProperties, cb);
                        Marshal.StructureToPtr(structure, ptr, false);
                    }
                }
                if (ADP.PtrZero == structure.rgProperties)
                {
                    throw new OutOfMemoryException();
                }
                for (int i = 0; i < properties.Length; i++)
                {
                    IntPtr ptr2 = ADP.IntPtrOffset(structure.rgProperties, i * ODB.SizeOf_tagDBPROP);
                    Marshal.StructureToPtr(properties[i], ptr2, false);
                }
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }

        internal int PropertySetCount
        {
            get
            {
                return this.propertySetCount;
            }
        }
    }
}

