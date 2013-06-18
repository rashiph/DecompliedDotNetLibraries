namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    [ComVisible(true), Guid("432D76CE-8C9E-4eed-ADDD-91737F27A8CB")]
    public class DebugConvert : IDebugConvert, IDebugConvert2
    {
        public string BooleanToString(bool value)
        {
            return Microsoft.JScript.Convert.ToString(value);
        }

        public string ByteToString(byte value, int radix)
        {
            return System.Convert.ToString(value, radix);
        }

        public string DecimalToString(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public string DoubleToDateString(double value)
        {
            return DatePrototype.DateToString(value);
        }

        public string DoubleToString(double value)
        {
            return Microsoft.JScript.Convert.ToString(value);
        }

        public string GetErrorMessageForHR(int hr, IJSVsaEngine engine)
        {
            CultureInfo culture = null;
            VsaEngine engine2 = engine as VsaEngine;
            if (engine2 != null)
            {
                culture = engine2.ErrorCultureInfo;
            }
            if (((hr & 0xffff0000L) == 0x800a0000L) && Enum.IsDefined(typeof(JSError), hr & 0xffff))
            {
                int num = hr & 0xffff;
                return JScriptException.Localize(num.ToString(CultureInfo.InvariantCulture), culture);
            }
            int num2 = 0x177b;
            return JScriptException.Localize(num2.ToString(CultureInfo.InvariantCulture), "0x" + hr.ToString("X", CultureInfo.InvariantCulture), culture);
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        public object GetManagedCharObject(ushort i)
        {
            return (char) i;
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        public object GetManagedInt64Object(long i)
        {
            return i;
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        public object GetManagedObject(object value)
        {
            return value;
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        public object GetManagedUInt64Object(ulong i)
        {
            return i;
        }

        public string Int16ToString(short value, int radix)
        {
            return System.Convert.ToString((short) Microsoft.JScript.Convert.ToInteger((double) value), radix);
        }

        public string Int32ToString(int value, int radix)
        {
            return System.Convert.ToString((int) Microsoft.JScript.Convert.ToInteger((double) value), radix);
        }

        public string Int64ToString(long value, int radix)
        {
            return System.Convert.ToString((long) Microsoft.JScript.Convert.ToInteger((double) value), radix);
        }

        public string RegexpToString(string source, bool ignoreCase, bool global, bool multiline)
        {
            return RegExpConstructor.ob.Construct(source, ignoreCase, global, multiline).ToString();
        }

        public string SByteToString(sbyte value, int radix)
        {
            if (radix == 10)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
            return System.Convert.ToString((byte) value, radix);
        }

        public string SingleToString(float value)
        {
            return Microsoft.JScript.Convert.ToString((double) value);
        }

        public string StringToPrintable(string source)
        {
            int length = source.Length;
            StringBuilder builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                switch (source[i])
                {
                    case '"':
                        builder.Append("\"");
                        break;

                    case '\\':
                        builder.Append(@"\\");
                        break;

                    case '\b':
                        builder.Append(@"\b");
                        break;

                    case '\t':
                        builder.Append(@"\t");
                        break;

                    case '\n':
                        builder.Append(@"\n");
                        break;

                    case '\v':
                        builder.Append(@"\v");
                        break;

                    case '\f':
                        builder.Append(@"\f");
                        break;

                    case '\r':
                        builder.Append(@"\r");
                        break;

                    case '\0':
                        builder.Append(@"\0");
                        break;

                    default:
                        if (char.GetUnicodeCategory(source[i]) != UnicodeCategory.Control)
                        {
                            builder.Append(source[i]);
                        }
                        else
                        {
                            builder.Append(@"\u");
                            int num3 = source[i];
                            char[] chArray = new char[4];
                            for (int j = 0; j < 4; j++)
                            {
                                int num5 = num3 % 0x10;
                                if (num5 <= 9)
                                {
                                    chArray[3 - j] = (char) (0x30 + num5);
                                }
                                else
                                {
                                    chArray[3 - j] = (char) ((0x41 + num5) - 10);
                                }
                                num3 /= 0x10;
                            }
                            builder.Append(chArray);
                        }
                        break;
                }
            }
            return builder.ToString();
        }

        public object ToPrimitive(object value, TypeCode typeCode, bool truncationPermitted)
        {
            return Microsoft.JScript.Convert.Coerce2(value, typeCode, truncationPermitted);
        }

        public string UInt16ToString(ushort value, int radix)
        {
            if (radix == 10)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
            return System.Convert.ToString((short) Microsoft.JScript.Convert.ToInteger((double) value), radix);
        }

        public string UInt32ToString(uint value, int radix)
        {
            if (radix == 10)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
            return System.Convert.ToString((int) Microsoft.JScript.Convert.ToInteger((double) value), radix);
        }

        public string UInt64ToString(ulong value, int radix)
        {
            if (radix == 10)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
            return System.Convert.ToString((long) Microsoft.JScript.Convert.ToInteger((double) value), radix);
        }
    }
}

