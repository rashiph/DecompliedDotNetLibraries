namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    internal sealed class RowBinding : DbBuffer
    {
        private IntPtr _accessorHandle;
        private readonly int _bindingCount;
        private readonly int _dataLength;
        private readonly int _emptyStringOffset;
        private bool _haveData;
        private readonly int _headerLength;
        private UnsafeNativeMethods.IAccessor _iaccessor;
        private readonly bool _needToReset;

        private RowBinding(int bindingCount, int headerLength, int dataLength, int length, bool needToReset) : base(length)
        {
            this._bindingCount = bindingCount;
            this._headerLength = headerLength;
            this._dataLength = dataLength;
            this._emptyStringOffset = length - 8;
            this._needToReset = needToReset;
        }

        internal static int AlignDataSize(int value)
        {
            return Math.Max(8, (value + 7) & -8);
        }

        internal int BindingCount()
        {
            return this._bindingCount;
        }

        internal void CloseFromConnection()
        {
            this._iaccessor = null;
            this._accessorHandle = ODB.DB_INVALID_HACCESSOR;
        }

        internal OleDbHResult CreateAccessor(UnsafeNativeMethods.IAccessor iaccessor, int flags, ColumnBinding[] bindings)
        {
            OleDbHResult result = OleDbHResult.S_OK;
            int[] rgStatus = new int[this.BindingCount()];
            this._iaccessor = iaccessor;
            Bid.Trace("<oledb.IAccessor.CreateAccessor|API|OLEDB>\n");
            result = iaccessor.CreateAccessor(flags, (IntPtr) rgStatus.Length, this, (IntPtr) this._dataLength, out this._accessorHandle, rgStatus);
            Bid.Trace("<oledb.IAccessor.CreateAccessor|API|OLEDB|RET> %08X{HRESULT}\n", result);
            for (int i = 0; i < rgStatus.Length; i++)
            {
                if (rgStatus[i] != 0)
                {
                    if (4 == flags)
                    {
                        throw ODB.BadStatus_ParamAcc(bindings[i].ColumnBindingOrdinal, (DBBindStatus) rgStatus[i]);
                    }
                    if (2 == flags)
                    {
                        throw ODB.BadStatusRowAccessor(bindings[i].ColumnBindingOrdinal, (DBBindStatus) rgStatus[i]);
                    }
                }
            }
            return result;
        }

        internal static RowBinding CreateBuffer(int bindingCount, int databuffersize, bool needToReset)
        {
            int headerLength = AlignDataSize(bindingCount * ODB.SizeOf_tagDBBINDING);
            return new RowBinding(bindingCount, headerLength, databuffersize, AlignDataSize(headerLength + databuffersize) + 8, needToReset);
        }

        internal IntPtr DangerousGetAccessorHandle()
        {
            return this._accessorHandle;
        }

        internal IntPtr DangerousGetDataPtr()
        {
            return ADP.IntPtrOffset(base.DangerousGetHandle(), this._headerLength);
        }

        internal IntPtr DangerousGetDataPtr(int valueOffset)
        {
            return ADP.IntPtrOffset(base.DangerousGetHandle(), valueOffset);
        }

        internal void Dispose()
        {
            int num;
            this.ResetValues();
            UnsafeNativeMethods.IAccessor accessor = this._iaccessor;
            IntPtr hAccessor = this._accessorHandle;
            this._iaccessor = null;
            this._accessorHandle = ODB.DB_INVALID_HACCESSOR;
            if (((ODB.DB_INVALID_HACCESSOR != hAccessor) && (accessor != null)) && (accessor.ReleaseAccessor(hAccessor, out num) < OleDbHResult.S_OK))
            {
                SafeNativeMethods.Wrapper.ClearErrorInfo();
            }
            base.Dispose();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static void FreeBstr(IntPtr buffer, int valueOffset)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                IntPtr bstr = Marshal.ReadIntPtr(buffer, valueOffset);
                IntPtr ptr = Marshal.ReadIntPtr(buffer, valueOffset + ADP.PtrSize);
                if ((ADP.PtrZero != bstr) && (bstr != ptr))
                {
                    SafeNativeMethods.SysFreeString(bstr);
                }
                if (ADP.PtrZero != ptr)
                {
                    SafeNativeMethods.SysFreeString(ptr);
                }
                Marshal.WriteIntPtr(buffer, valueOffset, ADP.PtrZero);
                Marshal.WriteIntPtr(buffer, valueOffset + ADP.PtrSize, ADP.PtrZero);
            }
        }

        private static void FreeChapter(IntPtr buffer, int valueOffset, object iaccessor)
        {
            UnsafeNativeMethods.IChapteredRowset rowset = iaccessor as UnsafeNativeMethods.IChapteredRowset;
            IntPtr ptr = SafeNativeMethods.InterlockedExchangePointer(ADP.IntPtrOffset(buffer, valueOffset), ADP.PtrZero);
            if (ODB.DB_NULL_HCHAPTER != ptr)
            {
                int num;
                Bid.Trace("<oledb.IChapteredRowset.ReleaseChapter|API|OLEDB> Chapter=%Id\n", ptr);
                OleDbHResult result = rowset.ReleaseChapter(ptr, out num);
                Bid.Trace("<oledb.IChapteredRowset.ReleaseChapter|API|OLEDB|RET> %08X{HRESULT}, RefCount=%d\n", result, num);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static void FreeCoTaskMem(IntPtr buffer, int valueOffset)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                IntPtr handle = Marshal.ReadIntPtr(buffer, valueOffset);
                IntPtr ptr2 = Marshal.ReadIntPtr(buffer, valueOffset + ADP.PtrSize);
                if ((ADP.PtrZero != handle) && (handle != ptr2))
                {
                    SafeNativeMethods.CoTaskMemFree(handle);
                }
                Marshal.WriteIntPtr(buffer, valueOffset, ADP.PtrZero);
                Marshal.WriteIntPtr(buffer, valueOffset + ADP.PtrSize, ADP.PtrZero);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static void FreePropVariant(IntPtr buffer, int valueOffset)
        {
            IntPtr ptr2 = ADP.IntPtrOffset(buffer, valueOffset);
            IntPtr ptr = ADP.IntPtrOffset(buffer, valueOffset + NativeOledbWrapper.SizeOfPROPVARIANT);
            bool flag = NativeOledbWrapper.MemoryCompare(ptr2, ptr, NativeOledbWrapper.SizeOfPROPVARIANT);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                SafeNativeMethods.PropVariantClear(ptr2);
                if (flag)
                {
                    SafeNativeMethods.PropVariantClear(ptr);
                }
                else
                {
                    SafeNativeMethods.ZeroMemory(ptr, (IntPtr) NativeOledbWrapper.SizeOfPROPVARIANT);
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static void FreeVariant(IntPtr buffer, int valueOffset)
        {
            IntPtr ptr2 = ADP.IntPtrOffset(buffer, valueOffset);
            IntPtr ptr = ADP.IntPtrOffset(buffer, valueOffset + ODB.SizeOf_Variant);
            bool flag = NativeOledbWrapper.MemoryCompare(ptr2, ptr, ODB.SizeOf_Variant);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                SafeNativeMethods.VariantClear(ptr2);
                if (flag)
                {
                    SafeNativeMethods.VariantClear(ptr);
                }
                else
                {
                    SafeNativeMethods.ZeroMemory(ptr, (IntPtr) ODB.SizeOf_Variant);
                }
            }
        }

        internal object GetVariantValue(int offset)
        {
            object objectForNativeVariant = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                objectForNativeVariant = Marshal.GetObjectForNativeVariant(ADP.IntPtrOffset(base.DangerousGetHandle(), offset));
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            if (objectForNativeVariant == null)
            {
                return DBNull.Value;
            }
            return objectForNativeVariant;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal IntPtr InterlockedExchangePointer(int offset)
        {
            IntPtr ptr;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                ptr = SafeNativeMethods.InterlockedExchangePointer(ADP.IntPtrOffset(base.DangerousGetHandle(), offset), IntPtr.Zero);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return ptr;
        }

        protected override bool ReleaseHandle()
        {
            this._iaccessor = null;
            if (this._needToReset && this._haveData)
            {
                IntPtr handle = base.handle;
                if (IntPtr.Zero != handle)
                {
                    this.ResetValues(handle, null);
                }
            }
            return base.ReleaseHandle();
        }

        internal void ResetValues()
        {
            if (this._needToReset && this._haveData)
            {
                lock (this)
                {
                    bool success = false;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        base.DangerousAddRef(ref success);
                        this.ResetValues(base.DangerousGetHandle(), this._iaccessor);
                    }
                    finally
                    {
                        if (success)
                        {
                            base.DangerousRelease();
                        }
                    }
                    return;
                }
            }
            this._haveData = false;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void ResetValues(IntPtr buffer, object iaccessor)
        {
            for (int i = 0; i < this._bindingCount; i++)
            {
                IntPtr ptr = ADP.IntPtrOffset(buffer, i * ODB.SizeOf_tagDBBINDING);
                int valueOffset = this._headerLength + Marshal.ReadIntPtr(ptr, ODB.OffsetOf_tagDBBINDING_obValue).ToInt32();
                switch (Marshal.ReadInt16(ptr, ODB.OffsetOf_tagDBBINDING_wType))
                {
                    case 8:
                        FreeBstr(buffer, valueOffset);
                        break;

                    case 12:
                        FreeVariant(buffer, valueOffset);
                        break;

                    case 0x88:
                        if (iaccessor != null)
                        {
                            FreeChapter(buffer, valueOffset, iaccessor);
                        }
                        break;

                    case 0x8a:
                        FreePropVariant(buffer, valueOffset);
                        break;

                    case 0x4080:
                    case 0x4082:
                        FreeCoTaskMem(buffer, valueOffset);
                        break;
                }
            }
            this._haveData = false;
        }

        internal ColumnBinding[] SetBindings(OleDbDataReader dataReader, Bindings bindings, int indexStart, int indexForAccessor, OleDbParameter[] parameters, tagDBBINDING[] dbbindings, bool ifIRowsetElseIRow)
        {
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr handle = base.DangerousGetHandle();
                for (int j = 0; j < dbbindings.Length; j++)
                {
                    IntPtr ptr = ADP.IntPtrOffset(handle, j * ODB.SizeOf_tagDBBINDING);
                    Marshal.StructureToPtr(dbbindings[j], ptr, false);
                }
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            ColumnBinding[] bindingArray = new ColumnBinding[dbbindings.Length];
            for (int i = 0; i < bindingArray.Length; i++)
            {
                int index = indexStart + i;
                OleDbParameter parameter = (parameters != null) ? parameters[index] : null;
                bindingArray[i] = new ColumnBinding(dataReader, index, indexForAccessor, i, parameter, this, bindings, dbbindings[i], this._headerLength, ifIRowsetElseIRow);
            }
            return bindingArray;
        }

        internal void SetBstrValue(int offset, string value)
        {
            IntPtr ptr;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    ptr = SafeNativeMethods.SysAllocStringLen(value, value.Length);
                    Marshal.WriteIntPtr(base.handle, offset, ptr);
                    Marshal.WriteIntPtr(base.handle, offset + ADP.PtrSize, ptr);
                }
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            if (IntPtr.Zero == ptr)
            {
                throw new OutOfMemoryException();
            }
        }

        internal void SetByRefValue(int offset, IntPtr pinnedValue)
        {
            if (ADP.PtrZero == pinnedValue)
            {
                pinnedValue = ADP.IntPtrOffset(base.handle, this._emptyStringOffset);
            }
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    Marshal.WriteIntPtr(base.handle, offset, pinnedValue);
                    Marshal.WriteIntPtr(base.handle, offset + ADP.PtrSize, pinnedValue);
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

        internal void SetVariantValue(int offset, object value)
        {
            IntPtr ptrZero = ADP.PtrZero;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                ptrZero = ADP.IntPtrOffset(base.DangerousGetHandle(), offset);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Marshal.GetNativeVariantForObject(value, ptrZero);
                }
                finally
                {
                    NativeOledbWrapper.MemoryCopy(ADP.IntPtrOffset(ptrZero, ODB.SizeOf_Variant), ptrZero, ODB.SizeOf_Variant);
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

        internal void StartDataBlock()
        {
            if (this._haveData)
            {
                this.ResetValues();
            }
            this._haveData = true;
        }
    }
}

