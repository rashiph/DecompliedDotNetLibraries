namespace System.Runtime.Serialization
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class FormatterConverter : IFormatterConverter
    {
        public object Convert(object value, Type type)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        public object Convert(object value, TypeCode typeCode)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ChangeType(value, typeCode, CultureInfo.InvariantCulture);
        }

        public bool ToBoolean(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        public byte ToByte(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToByte(value, CultureInfo.InvariantCulture);
        }

        public char ToChar(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToChar(value, CultureInfo.InvariantCulture);
        }

        public DateTime ToDateTime(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        public decimal ToDecimal(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        public double ToDouble(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        public short ToInt16(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
        }

        public int ToInt32(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public long ToInt64(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }

        [CLSCompliant(false)]
        public sbyte ToSByte(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToSByte(value, CultureInfo.InvariantCulture);
        }

        public float ToSingle(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        public string ToString(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        [CLSCompliant(false)]
        public ushort ToUInt16(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToUInt16(value, CultureInfo.InvariantCulture);
        }

        [CLSCompliant(false)]
        public uint ToUInt32(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToUInt32(value, CultureInfo.InvariantCulture);
        }

        [CLSCompliant(false)]
        public ulong ToUInt64(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return System.Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        }
    }
}

