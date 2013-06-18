namespace System.Xml
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    internal class ValueHandle
    {
        private static System.Text.Base64Encoding base64Encoding;
        private XmlBufferReader bufferReader;
        private static string[] constStrings = new string[] { "string", "number", "array", "object", "boolean", "null" };
        private int length;
        private int offset;
        private ValueHandleType type;

        public ValueHandle(XmlBufferReader bufferReader)
        {
            this.bufferReader = bufferReader;
            this.type = ValueHandleType.Empty;
        }

        public bool Equals2(string str, bool checkLower)
        {
            if (this.type != ValueHandleType.UTF8)
            {
                return (this.GetString() == str);
            }
            if (this.length != str.Length)
            {
                return false;
            }
            byte[] buffer = this.bufferReader.Buffer;
            for (int i = 0; i < this.length; i++)
            {
                byte num2 = buffer[i + this.offset];
                if ((num2 != str[i]) && (!checkLower || (char.ToLowerInvariant((char) num2) != str[i])))
                {
                    return false;
                }
            }
            return true;
        }

        private void GetBase64(byte[] buffer, int offset, int count)
        {
            this.bufferReader.GetBase64(this.offset, buffer, offset, count);
        }

        private int GetChar()
        {
            return this.offset;
        }

        private string GetCharsText()
        {
            if ((this.length == 1) && (this.bufferReader.GetByte(this.offset) == 0x31))
            {
                return "1";
            }
            return this.bufferReader.GetString(this.offset, this.length);
        }

        private string GetCharText()
        {
            int num = this.GetChar();
            if (num > 0xffff)
            {
                SurrogateChar ch = new SurrogateChar(num);
                return new string(new char[] { ch.HighChar, ch.LowChar }, 0, 2);
            }
            char ch2 = (char) num;
            return ch2.ToString();
        }

        private decimal GetDecimal()
        {
            return this.bufferReader.GetDecimal(this.offset);
        }

        private XmlDictionaryString GetDictionaryString()
        {
            return this.bufferReader.GetDictionaryString(this.offset);
        }

        private double GetDouble()
        {
            return this.bufferReader.GetDouble(this.offset);
        }

        private string GetEscapedCharsText()
        {
            return this.bufferReader.GetEscapedString(this.offset, this.length);
        }

        private Guid GetGuid()
        {
            return this.bufferReader.GetGuid(this.offset);
        }

        private int GetInt16()
        {
            return this.bufferReader.GetInt16(this.offset);
        }

        private int GetInt32()
        {
            return this.bufferReader.GetInt32(this.offset);
        }

        private long GetInt64()
        {
            return this.bufferReader.GetInt64(this.offset);
        }

        private int GetInt8()
        {
            return this.bufferReader.GetInt8(this.offset);
        }

        private string GetQNameDictionaryText()
        {
            return (PrefixHandle.GetString(PrefixHandle.GetAlphaPrefix(this.length)) + ":" + this.bufferReader.GetDictionaryString(this.offset));
        }

        private float GetSingle()
        {
            return this.bufferReader.GetSingle(this.offset);
        }

        public string GetString()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.UTF8)
            {
                return this.GetCharsText();
            }
            switch (type)
            {
                case ValueHandleType.Empty:
                    return string.Empty;

                case ValueHandleType.True:
                    return "true";

                case ValueHandleType.False:
                    return "false";

                case ValueHandleType.Zero:
                    return "0";

                case ValueHandleType.One:
                    return "1";

                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    return XmlConverter.ToString(this.ToInt());

                case ValueHandleType.Int64:
                    return XmlConverter.ToString(this.GetInt64());

                case ValueHandleType.UInt64:
                    return XmlConverter.ToString(this.GetUInt64());

                case ValueHandleType.Single:
                    return XmlConverter.ToString(this.GetSingle());

                case ValueHandleType.Double:
                    return XmlConverter.ToString(this.GetDouble());

                case ValueHandleType.Decimal:
                    return XmlConverter.ToString(this.GetDecimal());

                case ValueHandleType.DateTime:
                    return XmlConverter.ToString(this.ToDateTime());

                case ValueHandleType.TimeSpan:
                    return XmlConverter.ToString(this.ToTimeSpan());

                case ValueHandleType.Guid:
                    return XmlConverter.ToString(this.ToGuid());

                case ValueHandleType.UniqueId:
                    return XmlConverter.ToString(this.ToUniqueId());

                case ValueHandleType.UTF8:
                    return this.GetCharsText();

                case ValueHandleType.EscapedUTF8:
                    return this.GetEscapedCharsText();

                case ValueHandleType.Base64:
                    return Base64Encoding.GetString(this.ToByteArray());

                case ValueHandleType.Dictionary:
                    return this.GetDictionaryString().Value;

                case ValueHandleType.List:
                    return XmlConverter.ToString(this.ToList());

                case ValueHandleType.Char:
                    return this.GetCharText();

                case ValueHandleType.Unicode:
                    return this.GetUnicodeCharsText();

                case ValueHandleType.QName:
                    return this.GetQNameDictionaryText();

                case ValueHandleType.ConstString:
                    return constStrings[this.offset];
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
        }

        private ulong GetUInt64()
        {
            return this.bufferReader.GetUInt64(this.offset);
        }

        private string GetUnicodeCharsText()
        {
            return this.bufferReader.GetUnicodeString(this.offset, this.length);
        }

        private UniqueId GetUniqueId()
        {
            return this.bufferReader.GetUniqueId(this.offset);
        }

        public bool IsWhitespace()
        {
            switch (this.type)
            {
                case ValueHandleType.True:
                case ValueHandleType.False:
                case ValueHandleType.Zero:
                case ValueHandleType.One:
                    return false;

                case ValueHandleType.UTF8:
                    return this.bufferReader.IsWhitespaceUTF8(this.offset, this.length);

                case ValueHandleType.EscapedUTF8:
                    return this.bufferReader.IsWhitespaceUTF8(this.offset, this.length);

                case ValueHandleType.Dictionary:
                    return this.bufferReader.IsWhitespaceKey(this.offset);

                case ValueHandleType.Char:
                {
                    int num = this.GetChar();
                    return ((num <= 0xffff) && XmlConverter.IsWhitespace((char) num));
                }
                case ValueHandleType.Unicode:
                    return this.bufferReader.IsWhitespaceUnicode(this.offset, this.length);

                case ValueHandleType.ConstString:
                    return (constStrings[this.offset].Length == 0);
            }
            return (this.length == 0);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SetCharValue(int ch)
        {
            this.SetValue(ValueHandleType.Char, ch, 0);
        }

        public void SetConstantValue(ValueHandleConstStringType constStringType)
        {
            this.type = ValueHandleType.ConstString;
            this.offset = (int) constStringType;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SetDictionaryValue(int key)
        {
            this.SetValue(ValueHandleType.Dictionary, key, 0);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SetQNameValue(int prefix, int key)
        {
            this.SetValue(ValueHandleType.QName, key, prefix);
        }

        public void SetValue(ValueHandleType type)
        {
            this.type = type;
        }

        public void SetValue(ValueHandleType type, int offset, int length)
        {
            this.type = type;
            this.offset = offset;
            this.length = length;
        }

        public void Sign(XmlSigningNodeWriter writer)
        {
            switch (this.type)
            {
                case ValueHandleType.Empty:
                    return;

                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    writer.WriteInt32Text(this.ToInt());
                    return;

                case ValueHandleType.Int64:
                    writer.WriteInt64Text(this.GetInt64());
                    return;

                case ValueHandleType.UInt64:
                    writer.WriteUInt64Text(this.GetUInt64());
                    return;

                case ValueHandleType.Single:
                    writer.WriteFloatText(this.GetSingle());
                    return;

                case ValueHandleType.Double:
                    writer.WriteDoubleText(this.GetDouble());
                    return;

                case ValueHandleType.Decimal:
                    writer.WriteDecimalText(this.GetDecimal());
                    return;

                case ValueHandleType.DateTime:
                    writer.WriteDateTimeText(this.ToDateTime());
                    return;

                case ValueHandleType.TimeSpan:
                    writer.WriteTimeSpanText(this.ToTimeSpan());
                    return;

                case ValueHandleType.Guid:
                    writer.WriteGuidText(this.ToGuid());
                    return;

                case ValueHandleType.UniqueId:
                    writer.WriteUniqueIdText(this.ToUniqueId());
                    return;

                case ValueHandleType.UTF8:
                    writer.WriteEscapedText(this.bufferReader.Buffer, this.offset, this.length);
                    return;

                case ValueHandleType.Base64:
                    writer.WriteBase64Text(this.bufferReader.Buffer, 0, this.bufferReader.Buffer, this.offset, this.length);
                    return;
            }
            writer.WriteEscapedText(this.GetString());
        }

        public bool ToBoolean()
        {
            ValueHandleType type = this.type;
            switch (type)
            {
                case ValueHandleType.False:
                    return false;

                case ValueHandleType.True:
                    return true;

                case ValueHandleType.UTF8:
                    return XmlConverter.ToBoolean(this.bufferReader.Buffer, this.offset, this.length);
            }
            if (type == ValueHandleType.Int8)
            {
                switch (this.GetInt8())
                {
                    case 0:
                        return false;

                    case 1:
                        return true;
                }
            }
            return XmlConverter.ToBoolean(this.GetString());
        }

        public byte[] ToByteArray()
        {
            byte[] buffer4;
            if (this.type == ValueHandleType.Base64)
            {
                byte[] buffer = new byte[this.length];
                this.GetBase64(buffer, 0, this.length);
                return buffer;
            }
            if ((this.type == ValueHandleType.UTF8) && ((this.length % 4) == 0))
            {
                try
                {
                    int num = (this.length / 4) * 3;
                    if ((this.length > 0) && (this.bufferReader.Buffer[(this.offset + this.length) - 1] == 0x3d))
                    {
                        num--;
                        if (this.bufferReader.Buffer[(this.offset + this.length) - 2] == 0x3d)
                        {
                            num--;
                        }
                    }
                    byte[] bytes = new byte[num];
                    int count = Base64Encoding.GetBytes(this.bufferReader.Buffer, this.offset, this.length, bytes, 0);
                    if (count != bytes.Length)
                    {
                        byte[] dst = new byte[count];
                        Buffer.BlockCopy(bytes, 0, dst, 0, count);
                        bytes = dst;
                    }
                    return bytes;
                }
                catch (FormatException)
                {
                }
            }
            try
            {
                buffer4 = Base64Encoding.GetBytes(XmlConverter.StripWhitespace(this.GetString()));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(exception.Message, exception.InnerException));
            }
            return buffer4;
        }

        public DateTime ToDateTime()
        {
            if (this.type == ValueHandleType.DateTime)
            {
                return XmlConverter.ToDateTime(this.GetInt64());
            }
            if (this.type == ValueHandleType.UTF8)
            {
                return XmlConverter.ToDateTime(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToDateTime(this.GetString());
        }

        public decimal ToDecimal()
        {
            ValueHandleType type = this.type;
            switch (type)
            {
                case ValueHandleType.Decimal:
                    return this.GetDecimal();

                case ValueHandleType.Zero:
                    return 0M;

                case ValueHandleType.One:
                    return 1M;
            }
            if ((type >= ValueHandleType.Int8) && (type <= ValueHandleType.Int64))
            {
                return this.ToLong();
            }
            if (type == ValueHandleType.UInt64)
            {
                return this.GetUInt64();
            }
            if (type == ValueHandleType.UTF8)
            {
                return XmlConverter.ToDecimal(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToDecimal(this.GetString());
        }

        public double ToDouble()
        {
            switch (this.type)
            {
                case ValueHandleType.Double:
                    return this.GetDouble();

                case ValueHandleType.Single:
                    return (double) this.GetSingle();

                case ValueHandleType.Zero:
                    return 0.0;

                case ValueHandleType.One:
                    return 1.0;

                case ValueHandleType.Int8:
                    return (double) this.GetInt8();

                case ValueHandleType.Int16:
                    return (double) this.GetInt16();

                case ValueHandleType.Int32:
                    return (double) this.GetInt32();

                case ValueHandleType.UTF8:
                    return XmlConverter.ToDouble(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToDouble(this.GetString());
        }

        public Guid ToGuid()
        {
            if (this.type == ValueHandleType.Guid)
            {
                return this.GetGuid();
            }
            if (this.type == ValueHandleType.UTF8)
            {
                return XmlConverter.ToGuid(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToGuid(this.GetString());
        }

        public int ToInt()
        {
            switch (this.type)
            {
                case ValueHandleType.Zero:
                    return 0;

                case ValueHandleType.One:
                    return 1;

                case ValueHandleType.Int8:
                    return this.GetInt8();

                case ValueHandleType.Int16:
                    return this.GetInt16();

                case ValueHandleType.Int32:
                    return this.GetInt32();

                case ValueHandleType.Int64:
                {
                    long num = this.GetInt64();
                    if ((num >= -2147483648L) && (num <= 0x7fffffffL))
                    {
                        return (int) num;
                    }
                    break;
                }
                case ValueHandleType.UInt64:
                {
                    ulong num2 = this.GetUInt64();
                    if (num2 <= 0x7fffffffL)
                    {
                        return (int) num2;
                    }
                    break;
                }
                case ValueHandleType.UTF8:
                    return XmlConverter.ToInt32(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToInt32(this.GetString());
        }

        public object[] ToList()
        {
            return this.bufferReader.GetList(this.offset, this.length);
        }

        public long ToLong()
        {
            switch (this.type)
            {
                case ValueHandleType.Zero:
                    return 0L;

                case ValueHandleType.One:
                    return 1L;

                case ValueHandleType.Int8:
                    return (long) this.GetInt8();

                case ValueHandleType.Int16:
                    return (long) this.GetInt16();

                case ValueHandleType.Int32:
                    return (long) this.GetInt32();

                case ValueHandleType.Int64:
                    return this.GetInt64();

                case ValueHandleType.UInt64:
                {
                    ulong num = this.GetUInt64();
                    if (num <= 0x7fffffffffffffffL)
                    {
                        return (long) num;
                    }
                    break;
                }
                case ValueHandleType.UTF8:
                    return XmlConverter.ToInt64(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToInt64(this.GetString());
        }

        public object ToObject()
        {
            switch (this.type)
            {
                case ValueHandleType.Empty:
                case ValueHandleType.UTF8:
                case ValueHandleType.EscapedUTF8:
                case ValueHandleType.Dictionary:
                case ValueHandleType.Char:
                case ValueHandleType.Unicode:
                case ValueHandleType.ConstString:
                    return this.ToString();

                case ValueHandleType.True:
                case ValueHandleType.False:
                    return this.ToBoolean();

                case ValueHandleType.Zero:
                case ValueHandleType.One:
                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    return this.ToInt();

                case ValueHandleType.Int64:
                    return this.ToLong();

                case ValueHandleType.UInt64:
                    return this.GetUInt64();

                case ValueHandleType.Single:
                    return this.ToSingle();

                case ValueHandleType.Double:
                    return this.ToDouble();

                case ValueHandleType.Decimal:
                    return this.ToDecimal();

                case ValueHandleType.DateTime:
                    return this.ToDateTime();

                case ValueHandleType.TimeSpan:
                    return this.ToTimeSpan();

                case ValueHandleType.Guid:
                    return this.ToGuid();

                case ValueHandleType.UniqueId:
                    return this.ToUniqueId();

                case ValueHandleType.Base64:
                    return this.ToByteArray();

                case ValueHandleType.List:
                    return this.ToList();
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
        }

        public float ToSingle()
        {
            switch (this.type)
            {
                case ValueHandleType.Single:
                    return this.GetSingle();

                case ValueHandleType.Double:
                {
                    double d = this.GetDouble();
                    if (((d >= -3.4028234663852886E+38) && (d <= 3.4028234663852886E+38)) || (double.IsInfinity(d) || double.IsNaN(d)))
                    {
                        return (float) d;
                    }
                    break;
                }
                case ValueHandleType.Zero:
                    return 0f;

                case ValueHandleType.One:
                    return 1f;

                case ValueHandleType.Int8:
                    return (float) this.GetInt8();

                case ValueHandleType.Int16:
                    return (float) this.GetInt16();

                case ValueHandleType.UTF8:
                    return XmlConverter.ToSingle(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToSingle(this.GetString());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override string ToString()
        {
            return this.GetString();
        }

        public TimeSpan ToTimeSpan()
        {
            if (this.type == ValueHandleType.TimeSpan)
            {
                return new TimeSpan(this.GetInt64());
            }
            if (this.type == ValueHandleType.UTF8)
            {
                return XmlConverter.ToTimeSpan(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToTimeSpan(this.GetString());
        }

        public Type ToType()
        {
            switch (this.type)
            {
                case ValueHandleType.Empty:
                case ValueHandleType.UTF8:
                case ValueHandleType.EscapedUTF8:
                case ValueHandleType.Dictionary:
                case ValueHandleType.Char:
                case ValueHandleType.Unicode:
                case ValueHandleType.QName:
                case ValueHandleType.ConstString:
                    return typeof(string);

                case ValueHandleType.True:
                case ValueHandleType.False:
                    return typeof(bool);

                case ValueHandleType.Zero:
                case ValueHandleType.One:
                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    return typeof(int);

                case ValueHandleType.Int64:
                    return typeof(long);

                case ValueHandleType.UInt64:
                    return typeof(ulong);

                case ValueHandleType.Single:
                    return typeof(float);

                case ValueHandleType.Double:
                    return typeof(double);

                case ValueHandleType.Decimal:
                    return typeof(decimal);

                case ValueHandleType.DateTime:
                    return typeof(DateTime);

                case ValueHandleType.TimeSpan:
                    return typeof(TimeSpan);

                case ValueHandleType.Guid:
                    return typeof(Guid);

                case ValueHandleType.UniqueId:
                    return typeof(UniqueId);

                case ValueHandleType.Base64:
                    return typeof(byte[]);

                case ValueHandleType.List:
                    return typeof(object[]);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
        }

        public ulong ToULong()
        {
            ValueHandleType type = this.type;
            switch (type)
            {
                case ValueHandleType.Zero:
                    return 0L;

                case ValueHandleType.One:
                    return 1L;
            }
            if ((type >= ValueHandleType.Int8) && (type <= ValueHandleType.Int64))
            {
                long num = this.ToLong();
                if (num >= 0L)
                {
                    return (ulong) num;
                }
            }
            if (type == ValueHandleType.UInt64)
            {
                return this.GetUInt64();
            }
            if (type == ValueHandleType.UTF8)
            {
                return XmlConverter.ToUInt64(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToUInt64(this.GetString());
        }

        public UniqueId ToUniqueId()
        {
            if (this.type == ValueHandleType.UniqueId)
            {
                return this.GetUniqueId();
            }
            if (this.type == ValueHandleType.UTF8)
            {
                return XmlConverter.ToUniqueId(this.bufferReader.Buffer, this.offset, this.length);
            }
            return XmlConverter.ToUniqueId(this.GetString());
        }

        public bool TryGetByteArrayLength(out int length)
        {
            if (this.type == ValueHandleType.Base64)
            {
                length = this.length;
                return true;
            }
            length = 0;
            return false;
        }

        public bool TryGetDictionaryString(out XmlDictionaryString value)
        {
            if (this.type == ValueHandleType.Dictionary)
            {
                value = this.GetDictionaryString();
                return true;
            }
            value = null;
            return false;
        }

        public bool TryReadBase64(byte[] buffer, int offset, int count, out int actual)
        {
            if (this.type == ValueHandleType.Base64)
            {
                actual = Math.Min(this.length, count);
                this.GetBase64(buffer, offset, actual);
                this.offset += actual;
                this.length -= actual;
                return true;
            }
            if (((this.type == ValueHandleType.UTF8) && (count >= 3)) && ((this.length % 4) == 0))
            {
                try
                {
                    int charCount = Math.Min((count / 3) * 4, this.length);
                    actual = Base64Encoding.GetBytes(this.bufferReader.Buffer, this.offset, charCount, buffer, offset);
                    this.offset += charCount;
                    this.length -= charCount;
                    return true;
                }
                catch (FormatException)
                {
                }
            }
            actual = 0;
            return false;
        }

        public bool TryReadChars(char[] chars, int offset, int count, out int actual)
        {
            if (this.type == ValueHandleType.Unicode)
            {
                return this.TryReadUnicodeChars(chars, offset, count, out actual);
            }
            if (this.type != ValueHandleType.UTF8)
            {
                actual = 0;
                return false;
            }
            int index = offset;
            int num2 = count;
            byte[] bytes = this.bufferReader.Buffer;
            int num3 = this.offset;
            int length = this.length;
        Label_006C:
            while ((num2 > 0) && (length > 0))
            {
                byte num5 = bytes[num3];
                if (num5 >= 0x80)
                {
                    break;
                }
                chars[index] = (char) num5;
                num3++;
                length--;
                index++;
                num2--;
            }
            if ((num2 != 0) && (length != 0))
            {
                int num6;
                int num7;
                UTF8Encoding encoding = new UTF8Encoding(false, true);
                try
                {
                    if ((num2 >= encoding.GetMaxCharCount(length)) || (num2 >= encoding.GetCharCount(bytes, num3, length)))
                    {
                        num7 = encoding.GetChars(bytes, num3, length, chars, index);
                        num6 = length;
                    }
                    else
                    {
                        System.Text.Decoder decoder = encoding.GetDecoder();
                        num6 = Math.Min(num2, length);
                        num7 = decoder.GetChars(bytes, num3, num6, chars, index);
                        while (num7 == 0)
                        {
                            num7 = decoder.GetChars(bytes, num3 + num6, 1, chars, index);
                            num6++;
                        }
                        num6 = encoding.GetByteCount(chars, index, num7);
                    }
                }
                catch (FormatException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateEncodingException(bytes, num3, length, exception));
                }
                num3 += num6;
                length -= num6;
                index += num7;
                num2 -= num7;
                goto Label_006C;
            }
            this.offset = num3;
            this.length = length;
            actual = count - num2;
            return true;
        }

        private bool TryReadUnicodeChars(char[] chars, int offset, int count, out int actual)
        {
            int num = Math.Min(count, this.length / 2);
            for (int i = 0; i < num; i++)
            {
                chars[offset + i] = (char) this.bufferReader.GetInt16(this.offset + (i * 2));
            }
            this.offset += num * 2;
            this.length -= num * 2;
            actual = num;
            return true;
        }

        private static System.Text.Base64Encoding Base64Encoding
        {
            get
            {
                if (base64Encoding == null)
                {
                    base64Encoding = new System.Text.Base64Encoding();
                }
                return base64Encoding;
            }
        }
    }
}

