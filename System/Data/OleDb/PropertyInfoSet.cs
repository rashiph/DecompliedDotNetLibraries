namespace System.Data.OleDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class PropertyInfoSet : SafeHandle
    {
        private IntPtr descBuffer;
        private int setCount;

        internal PropertyInfoSet(UnsafeNativeMethods.IDBProperties idbProperties, PropertyIDSet propIDSet) : base(IntPtr.Zero, true)
        {
            OleDbHResult result;
            int count = propIDSet.Count;
            Bid.Trace("<oledb.IDBProperties.GetPropertyInfo|API|OLEDB>\n");
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                result = idbProperties.GetPropertyInfo(count, propIDSet, out this.setCount, out this.handle, out this.descBuffer);
            }
            Bid.Trace("<oledb.IDBProperties.GetPropertyInfo|API|OLEDB|RET> %08X{HRESULT}\n", result);
            if ((OleDbHResult.S_OK <= result) && (ADP.PtrZero != base.handle))
            {
                SafeNativeMethods.Wrapper.ClearErrorInfo();
            }
        }

        internal static Type FromVtType(int vartype)
        {
            switch (((VarEnum) vartype))
            {
                case VarEnum.VT_EMPTY:
                    return null;

                case VarEnum.VT_NULL:
                    return typeof(DBNull);

                case VarEnum.VT_I2:
                    return typeof(short);

                case VarEnum.VT_I4:
                    return typeof(int);

                case VarEnum.VT_R4:
                    return typeof(float);

                case VarEnum.VT_R8:
                    return typeof(double);

                case VarEnum.VT_CY:
                    return typeof(decimal);

                case VarEnum.VT_DATE:
                    return typeof(DateTime);

                case VarEnum.VT_BSTR:
                    return typeof(string);

                case VarEnum.VT_DISPATCH:
                    return typeof(object);

                case VarEnum.VT_ERROR:
                    return typeof(int);

                case VarEnum.VT_BOOL:
                    return typeof(bool);

                case VarEnum.VT_VARIANT:
                    return typeof(object);

                case VarEnum.VT_UNKNOWN:
                    return typeof(object);

                case VarEnum.VT_DECIMAL:
                    return typeof(decimal);

                case VarEnum.VT_I1:
                    return typeof(sbyte);

                case VarEnum.VT_UI1:
                    return typeof(byte);

                case VarEnum.VT_UI2:
                    return typeof(ushort);

                case VarEnum.VT_UI4:
                    return typeof(uint);

                case VarEnum.VT_I8:
                    return typeof(long);

                case VarEnum.VT_UI8:
                    return typeof(ulong);

                case VarEnum.VT_INT:
                    return typeof(int);

                case VarEnum.VT_UINT:
                    return typeof(uint);
            }
            return typeof(object);
        }

        internal Dictionary<string, OleDbPropertyInfo> GetValues()
        {
            Dictionary<string, OleDbPropertyInfo> dictionary = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                if (!(ADP.PtrZero != base.handle))
                {
                    return dictionary;
                }
                dictionary = new Dictionary<string, OleDbPropertyInfo>(StringComparer.OrdinalIgnoreCase);
                IntPtr handle = base.handle;
                tagDBPROPINFO structure = new tagDBPROPINFO();
                tagDBPROPINFOSET gdbpropinfoset = new tagDBPROPINFOSET();
                int num2 = 0;
                while (num2 < this.setCount)
                {
                    Marshal.PtrToStructure(handle, gdbpropinfoset);
                    int cPropertyInfos = gdbpropinfoset.cPropertyInfos;
                    IntPtr rgPropertyInfos = gdbpropinfoset.rgPropertyInfos;
                    int num = 0;
                    while (num < cPropertyInfos)
                    {
                        Marshal.PtrToStructure(rgPropertyInfos, structure);
                        OleDbPropertyInfo info = new OleDbPropertyInfo {
                            _propertySet = gdbpropinfoset.guidPropertySet,
                            _propertyID = structure.dwPropertyID,
                            _flags = structure.dwFlags,
                            _vtype = structure.vtType,
                            _supportedValues = structure.vValue,
                            _description = structure.pwszDescription,
                            _lowercase = structure.pwszDescription.ToLower(CultureInfo.InvariantCulture),
                            _type = FromVtType(structure.vtType)
                        };
                        if (Bid.AdvancedOn)
                        {
                            Bid.Trace("<oledb.struct.OleDbPropertyInfo|INFO|ADV> \n");
                        }
                        dictionary[info._lowercase] = info;
                        num++;
                        rgPropertyInfos = ADP.IntPtrOffset(rgPropertyInfos, ODB.SizeOf_tagDBPROPINFO);
                    }
                    num2++;
                    handle = ADP.IntPtrOffset(handle, ODB.SizeOf_tagDBPROPINFOSET);
                }
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return dictionary;
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if (IntPtr.Zero != handle)
            {
                int setCount = this.setCount;
                for (int i = 0; i < setCount; i++)
                {
                    int ofs = i * ODB.SizeOf_tagDBPROPINFOSET;
                    IntPtr pbase = Marshal.ReadIntPtr(handle, ofs);
                    if (IntPtr.Zero != pbase)
                    {
                        int num4 = Marshal.ReadInt32(handle, ofs + ADP.PtrSize);
                        for (int j = 0; j < num4; j++)
                        {
                            SafeNativeMethods.VariantClear(ADP.IntPtrOffset(pbase, (j * ODB.SizeOf_tagDBPROPINFO) + ODB.OffsetOf_tagDBPROPINFO_Value));
                        }
                        SafeNativeMethods.CoTaskMemFree(pbase);
                    }
                }
                SafeNativeMethods.CoTaskMemFree(handle);
            }
            handle = this.descBuffer;
            this.descBuffer = IntPtr.Zero;
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
                return ((IntPtr.Zero == base.handle) && (IntPtr.Zero == this.descBuffer));
            }
        }
    }
}

