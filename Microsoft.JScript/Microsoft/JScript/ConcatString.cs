namespace Microsoft.JScript
{
    using System;
    using System.Text;

    internal sealed class ConcatString : IConvertible
    {
        private StringBuilder buf;
        private bool isOwner;
        private int length;

        internal ConcatString(ConcatString str1, string str2)
        {
            this.length = str1.length + str2.Length;
            if (str1.isOwner)
            {
                this.buf = str1.buf;
                str1.isOwner = false;
            }
            else
            {
                int capacity = this.length * 2;
                if (capacity < 0x100)
                {
                    capacity = 0x100;
                }
                this.buf = new StringBuilder(str1.ToString(), capacity);
            }
            this.buf.Append(str2);
            this.isOwner = true;
        }

        internal ConcatString(string str1, string str2)
        {
            this.length = str1.Length + str2.Length;
            int capacity = this.length * 2;
            if (capacity < 0x100)
            {
                capacity = 0x100;
            }
            this.buf = new StringBuilder(str1, capacity);
            this.buf.Append(str2);
            this.isOwner = true;
        }

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.String;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return this.ToIConvertible().ToBoolean(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return this.ToIConvertible().ToByte(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return this.ToIConvertible().ToChar(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return this.ToIConvertible().ToDateTime(provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return this.ToIConvertible().ToDecimal(provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return this.ToIConvertible().ToDouble(provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return this.ToIConvertible().ToInt16(provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return this.ToIConvertible().ToInt32(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return this.ToIConvertible().ToInt64(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return this.ToIConvertible().ToSByte(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return this.ToIConvertible().ToSingle(provider);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return this.ToIConvertible().ToString(provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return this.ToIConvertible().ToType(conversionType, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return this.ToIConvertible().ToUInt16(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return this.ToIConvertible().ToUInt32(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return this.ToIConvertible().ToUInt64(provider);
        }

        private IConvertible ToIConvertible()
        {
            return this.ToString();
        }

        public override string ToString()
        {
            return this.buf.ToString(0, this.length);
        }
    }
}

