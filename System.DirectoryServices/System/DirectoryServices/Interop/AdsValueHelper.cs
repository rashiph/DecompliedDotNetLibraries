namespace System.DirectoryServices.Interop
{
    using System;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class AdsValueHelper
    {
        public AdsValue adsvalue;
        private GCHandle pinnedHandle;

        public AdsValueHelper(AdsValue adsvalue)
        {
            this.adsvalue = adsvalue;
        }

        public AdsValueHelper(object managedValue)
        {
            AdsType adsTypeForManagedType = this.GetAdsTypeForManagedType(managedValue.GetType());
            this.SetValue(managedValue, adsTypeForManagedType);
        }

        public AdsValueHelper(object managedValue, AdsType adsType)
        {
            this.SetValue(managedValue, adsType);
        }

        ~AdsValueHelper()
        {
            if (this.pinnedHandle.IsAllocated)
            {
                this.pinnedHandle.Free();
            }
        }

        private AdsType GetAdsTypeForManagedType(Type type)
        {
            if (type == typeof(int))
            {
                return AdsType.ADSTYPE_INTEGER;
            }
            if (type == typeof(long))
            {
                return AdsType.ADSTYPE_LARGE_INTEGER;
            }
            if (type == typeof(bool))
            {
                return AdsType.ADSTYPE_BOOLEAN;
            }
            return AdsType.ADSTYPE_UNKNOWN;
        }

        public AdsValue GetStruct()
        {
            return this.adsvalue;
        }

        public object GetValue()
        {
            switch (this.adsvalue.dwType)
            {
                case 0:
                    throw new InvalidOperationException(Res.GetString("DSConvertTypeInvalid"));

                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 12:
                    return Marshal.PtrToStringUni(this.adsvalue.pointer.value);

                case 6:
                    return (this.adsvalue.generic.a != 0);

                case 7:
                    return this.adsvalue.generic.a;

                case 8:
                case 11:
                case 0x19:
                {
                    int length = this.adsvalue.octetString.length;
                    byte[] destination = new byte[length];
                    Marshal.Copy(this.adsvalue.octetString.value, destination, 0, length);
                    return destination;
                }
                case 9:
                {
                    SystemTime time = new SystemTime {
                        wYear = LowOfInt(this.adsvalue.generic.a),
                        wMonth = HighOfInt(this.adsvalue.generic.a),
                        wDayOfWeek = LowOfInt(this.adsvalue.generic.b),
                        wDay = HighOfInt(this.adsvalue.generic.b),
                        wHour = LowOfInt(this.adsvalue.generic.c),
                        wMinute = HighOfInt(this.adsvalue.generic.c),
                        wSecond = LowOfInt(this.adsvalue.generic.d),
                        wMilliseconds = HighOfInt(this.adsvalue.generic.d)
                    };
                    return new DateTime(time.wYear, time.wMonth, time.wDay, time.wHour, time.wMinute, time.wSecond, time.wMilliseconds);
                }
                case 10:
                    return this.LowInt64;

                case 13:
                case 14:
                case 15:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 20:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x1a:
                    return new NotImplementedException(Res.GetString("DSAdsvalueTypeNYI", new object[] { "0x" + Convert.ToString(this.adsvalue.dwType, 0x10) }));

                case 0x1b:
                {
                    DnWithBinary structure = new DnWithBinary();
                    Marshal.PtrToStructure(this.adsvalue.pointer.value, structure);
                    byte[] buffer = new byte[structure.dwLength];
                    Marshal.Copy(structure.lpBinaryValue, buffer, 0, structure.dwLength);
                    StringBuilder builder = new StringBuilder();
                    StringBuilder builder2 = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        string str = buffer[i].ToString("X", CultureInfo.InvariantCulture);
                        if (str.Length == 1)
                        {
                            builder2.Append("0");
                        }
                        builder2.Append(str);
                    }
                    builder.Append("B:");
                    builder.Append(builder2.Length);
                    builder.Append(":");
                    builder.Append(builder2.ToString());
                    builder.Append(":");
                    builder.Append(Marshal.PtrToStringUni(structure.pszDNString));
                    return builder.ToString();
                }
                case 0x1c:
                {
                    DnWithString str2 = new DnWithString();
                    Marshal.PtrToStructure(this.adsvalue.pointer.value, str2);
                    string str3 = Marshal.PtrToStringUni(str2.pszStringValue);
                    if (str3 == null)
                    {
                        str3 = "";
                    }
                    StringBuilder builder3 = new StringBuilder();
                    builder3.Append("S:");
                    builder3.Append(str3.Length);
                    builder3.Append(":");
                    builder3.Append(str3);
                    builder3.Append(":");
                    builder3.Append(Marshal.PtrToStringUni(str2.pszDNString));
                    return builder3.ToString();
                }
            }
            return new ArgumentException(Res.GetString("DSConvertFailed", new object[] { "0x" + Convert.ToString(this.LowInt64, 0x10), "0x" + Convert.ToString(this.adsvalue.dwType, 0x10) }));
        }

        public object GetVlvValue()
        {
            AdsVLV structure = new AdsVLV();
            Marshal.PtrToStructure(this.adsvalue.octetString.value, structure);
            byte[] destination = null;
            if ((structure.contextID != IntPtr.Zero) && (structure.contextIDlength != 0))
            {
                destination = new byte[structure.contextIDlength];
                Marshal.Copy(structure.contextID, destination, 0, structure.contextIDlength);
            }
            DirectoryVirtualListView view = new DirectoryVirtualListView {
                Offset = structure.offset,
                ApproximateTotal = structure.contentCount
            };
            DirectoryVirtualListViewContext context = new DirectoryVirtualListViewContext(destination);
            view.DirectoryVirtualListViewContext = context;
            return view;
        }

        private static ushort HighOfInt(int i)
        {
            return (ushort) ((i >> 0x10) & 0xffff);
        }

        private static ushort LowOfInt(int i)
        {
            return (ushort) (i & 0xffff);
        }

        private void SetValue(object managedValue, AdsType adsType)
        {
            this.adsvalue = new AdsValue();
            this.adsvalue.dwType = (int) adsType;
            switch (adsType)
            {
                case AdsType.ADSTYPE_CASE_IGNORE_STRING:
                    this.pinnedHandle = GCHandle.Alloc(managedValue, GCHandleType.Pinned);
                    this.adsvalue.pointer.value = this.pinnedHandle.AddrOfPinnedObject();
                    return;

                case AdsType.ADSTYPE_BOOLEAN:
                    if (!((bool) managedValue))
                    {
                        this.LowInt64 = 0L;
                        return;
                    }
                    this.LowInt64 = -1L;
                    return;

                case AdsType.ADSTYPE_INTEGER:
                    this.adsvalue.generic.a = (int) managedValue;
                    this.adsvalue.generic.b = 0;
                    return;

                case AdsType.ADSTYPE_LARGE_INTEGER:
                    this.LowInt64 = (long) managedValue;
                    return;

                case AdsType.ADSTYPE_PROV_SPECIFIC:
                {
                    byte[] buffer = (byte[]) managedValue;
                    this.adsvalue.octetString.length = buffer.Length;
                    this.pinnedHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    this.adsvalue.octetString.value = this.pinnedHandle.AddrOfPinnedObject();
                    return;
                }
            }
            throw new NotImplementedException(Res.GetString("DSAdsvalueTypeNYI", new object[] { "0x" + Convert.ToString((int) adsType, 0x10) }));
        }

        public long LowInt64
        {
            get
            {
                return (((long) ((ulong) this.adsvalue.generic.a)) + (this.adsvalue.generic.b << 0x20));
            }
            set
            {
                this.adsvalue.generic.a = (int) (((ulong) value) & 0xffffffffL);
                this.adsvalue.generic.b = (int) (value >> 0x20);
            }
        }
    }
}

