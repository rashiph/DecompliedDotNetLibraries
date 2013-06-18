namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Text;

    public sealed class BerConverter
    {
        private BerConverter()
        {
        }

        public static object[] Decode(string format, byte[] value)
        {
            bool flag;
            object[] objArray = TryDecode(format, value, out flag);
            if (!flag)
            {
                throw new BerConversionException();
            }
            return objArray;
        }

        private static byte[] DecodingByteArrayHelper(BerSafeHandle berElement, char fmt, ref int error)
        {
            error = 0;
            IntPtr zero = IntPtr.Zero;
            berval structure = new berval();
            byte[] destination = null;
            error = Wldap32.ber_scanf_ptr(berElement, new string(fmt, 1), ref zero);
            try
            {
                if ((error == 0) && (zero != IntPtr.Zero))
                {
                    Marshal.PtrToStructure(zero, structure);
                    destination = new byte[structure.bv_len];
                    Marshal.Copy(structure.bv_val, destination, 0, structure.bv_len);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Wldap32.ber_bvfree(zero);
                }
            }
            return destination;
        }

        private static byte[][] DecodingMultiByteArrayHelper(BerSafeHandle berElement, char fmt, ref int error)
        {
            error = 0;
            IntPtr zero = IntPtr.Zero;
            int num = 0;
            ArrayList list = new ArrayList();
            IntPtr ptr = IntPtr.Zero;
            byte[][] bufferArray = null;
            try
            {
                error = Wldap32.ber_scanf_ptr(berElement, new string(fmt, 1), ref zero);
                if ((error != 0) || !(zero != IntPtr.Zero))
                {
                    return bufferArray;
                }
                for (ptr = Marshal.ReadIntPtr(zero); ptr != IntPtr.Zero; ptr = Marshal.ReadIntPtr(zero, num * Marshal.SizeOf(typeof(IntPtr))))
                {
                    berval structure = new berval();
                    Marshal.PtrToStructure(ptr, structure);
                    byte[] destination = new byte[structure.bv_len];
                    Marshal.Copy(structure.bv_val, destination, 0, structure.bv_len);
                    list.Add(destination);
                    num++;
                }
                bufferArray = new byte[list.Count][];
                for (int i = 0; i < list.Count; i++)
                {
                    bufferArray[i] = (byte[]) list[i];
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Wldap32.ber_bvecfree(zero);
                }
            }
            return bufferArray;
        }

        public static byte[] Encode(string format, params object[] value)
        {
            Utility.CheckOSVersion();
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] destination = null;
            if (value == null)
            {
                value = new object[0];
            }
            BerSafeHandle berElement = new BerSafeHandle();
            int index = 0;
            int num2 = 0;
            for (int i = 0; i < format.Length; i++)
            {
                char c = format[i];
                switch (c)
                {
                    case '{':
                    case '}':
                    case '[':
                    case ']':
                    case 'n':
                        num2 = Wldap32.ber_printf_emptyarg(berElement, new string(c, 1));
                        break;

                    case 't':
                    case 'i':
                    case 'e':
                        if (index >= value.Length)
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        if (!(value[index] is int))
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        num2 = Wldap32.ber_printf_int(berElement, new string(c, 1), (int) value[index]);
                        index++;
                        break;

                    case 'b':
                        if (index >= value.Length)
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        if (!(value[index] is bool))
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        num2 = Wldap32.ber_printf_int(berElement, new string(c, 1), ((bool) value[index]) ? 1 : 0);
                        index++;
                        break;

                    case 's':
                    {
                        if (index >= value.Length)
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        if ((value[index] != null) && !(value[index] is string))
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        byte[] tempValue = null;
                        if (value[index] != null)
                        {
                            tempValue = encoding.GetBytes((string) value[index]);
                        }
                        num2 = EncodingByteArrayHelper(berElement, tempValue, 'o');
                        index++;
                        break;
                    }
                    case 'o':
                    case 'X':
                    {
                        if (index >= value.Length)
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        if ((value[index] != null) && !(value[index] is byte[]))
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        byte[] buffer3 = (byte[]) value[index];
                        num2 = EncodingByteArrayHelper(berElement, buffer3, c);
                        index++;
                        break;
                    }
                    case 'v':
                    {
                        if (index >= value.Length)
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        if ((value[index] != null) && !(value[index] is string[]))
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        string[] strArray = (string[]) value[index];
                        byte[][] bufferArray = null;
                        if (strArray != null)
                        {
                            bufferArray = new byte[strArray.Length][];
                            for (int j = 0; j < strArray.Length; j++)
                            {
                                string s = strArray[j];
                                if (s == null)
                                {
                                    bufferArray[j] = null;
                                }
                                else
                                {
                                    bufferArray[j] = encoding.GetBytes(s);
                                }
                            }
                        }
                        num2 = EncodingMultiByteArrayHelper(berElement, bufferArray, 'V');
                        index++;
                        break;
                    }
                    default:
                    {
                        if (c != 'V')
                        {
                            throw new ArgumentException(Res.GetString("BerConverterUndefineChar"));
                        }
                        if (index >= value.Length)
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        if ((value[index] != null) && !(value[index] is byte[][]))
                        {
                            throw new ArgumentException(Res.GetString("BerConverterNotMatch"));
                        }
                        byte[][] bufferArray2 = (byte[][]) value[index];
                        num2 = EncodingMultiByteArrayHelper(berElement, bufferArray2, c);
                        index++;
                        break;
                    }
                }
                if (num2 == -1)
                {
                    throw new BerConversionException();
                }
            }
            berval structure = new berval();
            IntPtr zero = IntPtr.Zero;
            try
            {
                if (Wldap32.ber_flatten(berElement, ref zero) == -1)
                {
                    throw new BerConversionException();
                }
                if (zero != IntPtr.Zero)
                {
                    Marshal.PtrToStructure(zero, structure);
                }
                if ((structure == null) || (structure.bv_len == 0))
                {
                    return new byte[0];
                }
                destination = new byte[structure.bv_len];
                Marshal.Copy(structure.bv_val, destination, 0, structure.bv_len);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Wldap32.ber_bvfree(zero);
                }
            }
            return destination;
        }

        private static int EncodingByteArrayHelper(BerSafeHandle berElement, byte[] tempValue, char fmt)
        {
            if (tempValue != null)
            {
                IntPtr destination = Marshal.AllocHGlobal(tempValue.Length);
                Marshal.Copy(tempValue, 0, destination, tempValue.Length);
                HGlobalMemHandle handle = new HGlobalMemHandle(destination);
                return Wldap32.ber_printf_bytearray(berElement, new string(fmt, 1), handle, tempValue.Length);
            }
            return Wldap32.ber_printf_bytearray(berElement, new string(fmt, 1), new HGlobalMemHandle(IntPtr.Zero), 0);
        }

        private static int EncodingMultiByteArrayHelper(BerSafeHandle berElement, byte[][] tempValue, char fmt)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            SafeBerval[] bervalArray = null;
            int num = 0;
            try
            {
                if (tempValue != null)
                {
                    int index = 0;
                    zero = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(IntPtr)) * (tempValue.Length + 1)));
                    int cb = Marshal.SizeOf(typeof(SafeBerval));
                    bervalArray = new SafeBerval[tempValue.Length];
                    index = 0;
                    while (index < tempValue.Length)
                    {
                        byte[] source = tempValue[index];
                        bervalArray[index] = new SafeBerval();
                        if (source == null)
                        {
                            bervalArray[index].bv_len = 0;
                            bervalArray[index].bv_val = IntPtr.Zero;
                        }
                        else
                        {
                            bervalArray[index].bv_len = source.Length;
                            bervalArray[index].bv_val = Marshal.AllocHGlobal(source.Length);
                            Marshal.Copy(source, 0, bervalArray[index].bv_val, source.Length);
                        }
                        IntPtr ptr3 = Marshal.AllocHGlobal(cb);
                        Marshal.StructureToPtr(bervalArray[index], ptr3, false);
                        ptr = (IntPtr) (((long) zero) + (Marshal.SizeOf(typeof(IntPtr)) * index));
                        Marshal.WriteIntPtr(ptr, ptr3);
                        index++;
                    }
                    ptr = (IntPtr) (((long) zero) + (Marshal.SizeOf(typeof(IntPtr)) * index));
                    Marshal.WriteIntPtr(ptr, IntPtr.Zero);
                }
                num = Wldap32.ber_printf_berarray(berElement, new string(fmt, 1), zero);
                GC.KeepAlive(bervalArray);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    for (int i = 0; i < tempValue.Length; i++)
                    {
                        IntPtr hglobal = Marshal.ReadIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * i);
                        if (hglobal != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(hglobal);
                        }
                    }
                    Marshal.FreeHGlobal(zero);
                }
            }
            return num;
        }

        internal static object[] TryDecode(string format, byte[] value, out bool decodeSucceeded)
        {
            Utility.CheckOSVersion();
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            UTF8Encoding encoding = new UTF8Encoding(false, true);
            berval berval = new berval();
            ArrayList list = new ArrayList();
            BerSafeHandle berElement = null;
            object[] objArray = null;
            decodeSucceeded = false;
            if (value == null)
            {
                berval.bv_len = 0;
                berval.bv_val = IntPtr.Zero;
            }
            else
            {
                berval.bv_len = value.Length;
                berval.bv_val = Marshal.AllocHGlobal(value.Length);
                Marshal.Copy(value, 0, berval.bv_val, value.Length);
            }
            try
            {
                berElement = new BerSafeHandle(berval);
            }
            finally
            {
                if (berval.bv_val != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(berval.bv_val);
                }
            }
            int error = 0;
            for (int i = 0; i < format.Length; i++)
            {
                char c = format[i];
                switch (c)
                {
                    case '{':
                    case '}':
                    case '[':
                    case ']':
                    case 'n':
                    case 'x':
                        error = Wldap32.ber_scanf(berElement, new string(c, 1));
                        if (error == 0)
                        {
                        }
                        break;

                    case 'i':
                    case 'e':
                    case 'b':
                    {
                        int num3 = 0;
                        error = Wldap32.ber_scanf_int(berElement, new string(c, 1), ref num3);
                        if (error == 0)
                        {
                            if (c == 'b')
                            {
                                bool flag = false;
                                if (num3 == 0)
                                {
                                    flag = false;
                                }
                                else
                                {
                                    flag = true;
                                }
                                list.Add(flag);
                            }
                            else
                            {
                                list.Add(num3);
                            }
                        }
                        break;
                    }
                    case 'a':
                    {
                        byte[] bytes = DecodingByteArrayHelper(berElement, 'O', ref error);
                        if (error == 0)
                        {
                            string str = null;
                            if (bytes != null)
                            {
                                str = encoding.GetString(bytes);
                            }
                            list.Add(str);
                        }
                        break;
                    }
                    case 'O':
                    {
                        byte[] buffer2 = DecodingByteArrayHelper(berElement, c, ref error);
                        if (error == 0)
                        {
                            list.Add(buffer2);
                        }
                        break;
                    }
                    case 'B':
                    {
                        IntPtr zero = IntPtr.Zero;
                        int length = 0;
                        error = Wldap32.ber_scanf_bitstring(berElement, "B", ref zero, ref length);
                        if (error == 0)
                        {
                            byte[] destination = null;
                            if (zero != IntPtr.Zero)
                            {
                                destination = new byte[length];
                                Marshal.Copy(zero, destination, 0, length);
                            }
                            list.Add(destination);
                        }
                        break;
                    }
                    case 'v':
                    {
                        byte[][] bufferArray = null;
                        string[] strArray = null;
                        bufferArray = DecodingMultiByteArrayHelper(berElement, 'V', ref error);
                        if (error == 0)
                        {
                            if (bufferArray != null)
                            {
                                strArray = new string[bufferArray.Length];
                                for (int k = 0; k < bufferArray.Length; k++)
                                {
                                    if (bufferArray[k] == null)
                                    {
                                        strArray[k] = null;
                                    }
                                    else
                                    {
                                        strArray[k] = encoding.GetString(bufferArray[k]);
                                    }
                                }
                            }
                            list.Add(strArray);
                        }
                        break;
                    }
                    default:
                    {
                        if (c != 'V')
                        {
                            throw new ArgumentException(Res.GetString("BerConverterUndefineChar"));
                        }
                        byte[][] bufferArray2 = null;
                        bufferArray2 = DecodingMultiByteArrayHelper(berElement, c, ref error);
                        if (error == 0)
                        {
                            list.Add(bufferArray2);
                        }
                        break;
                    }
                }
                if (error != 0)
                {
                    return objArray;
                }
            }
            objArray = new object[list.Count];
            for (int j = 0; j < list.Count; j++)
            {
                objArray[j] = list[j];
            }
            decodeSucceeded = true;
            return objArray;
        }
    }
}

