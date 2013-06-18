namespace System.Xml
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal class StringHandle
    {
        private XmlBufferReader bufferReader;
        private static string[] constStrings = new string[] { "type", "root", "item" };
        private int key;
        private int length;
        private int offset;
        private StringHandleType type;

        public StringHandle(XmlBufferReader bufferReader)
        {
            this.bufferReader = bufferReader;
            this.SetValue(0, 0);
        }

        public int CompareTo(StringHandle that)
        {
            if ((this.type == StringHandleType.UTF8) && (that.type == StringHandleType.UTF8))
            {
                return this.bufferReader.Compare(this.offset, this.length, that.offset, that.length);
            }
            return string.Compare(this.GetString(), that.GetString(), StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            StringHandle objA = obj as StringHandle;
            if (object.ReferenceEquals(objA, null))
            {
                return false;
            }
            return (this == objA);
        }

        private bool Equals2(string s2)
        {
            switch (this.type)
            {
                case StringHandleType.Dictionary:
                    return (this.bufferReader.GetDictionaryString(this.key).Value == s2);

                case StringHandleType.UTF8:
                    return this.bufferReader.Equals2(this.offset, this.length, s2);
            }
            return (this.GetString() == s2);
        }

        private bool Equals2(StringHandle s2)
        {
            switch (s2.type)
            {
                case StringHandleType.Dictionary:
                    return this.Equals2(s2.key, s2.bufferReader);

                case StringHandleType.UTF8:
                    return this.Equals2(s2.offset, s2.length, s2.bufferReader);
            }
            return this.Equals2(s2.GetString());
        }

        private bool Equals2(XmlDictionaryString xmlString2)
        {
            switch (this.type)
            {
                case StringHandleType.Dictionary:
                    return this.bufferReader.Equals2(this.key, xmlString2);

                case StringHandleType.UTF8:
                    return this.bufferReader.Equals2(this.offset, this.length, xmlString2.ToUTF8());
            }
            return (this.GetString() == xmlString2.Value);
        }

        private bool Equals2(int key2, XmlBufferReader bufferReader2)
        {
            switch (this.type)
            {
                case StringHandleType.Dictionary:
                    return this.bufferReader.Equals2(this.key, key2, bufferReader2);

                case StringHandleType.UTF8:
                    return this.bufferReader.Equals2(this.offset, this.length, bufferReader2.GetDictionaryString(key2).Value);
            }
            return (this.GetString() == this.bufferReader.GetDictionaryString(key2).Value);
        }

        private bool Equals2(int offset2, int length2, XmlBufferReader bufferReader2)
        {
            switch (this.type)
            {
                case StringHandleType.Dictionary:
                    return bufferReader2.Equals2(offset2, length2, this.bufferReader.GetDictionaryString(this.key).Value);

                case StringHandleType.UTF8:
                    return this.bufferReader.Equals2(this.offset, this.length, bufferReader2, offset2, length2);
            }
            return (this.GetString() == this.bufferReader.GetString(offset2, length2));
        }

        public override int GetHashCode()
        {
            return this.GetString().GetHashCode();
        }

        public string GetString()
        {
            switch (this.type)
            {
                case StringHandleType.UTF8:
                    return this.bufferReader.GetString(this.offset, this.length);

                case StringHandleType.Dictionary:
                    return this.bufferReader.GetDictionaryString(this.key).Value;

                case StringHandleType.ConstString:
                    return constStrings[this.key];
            }
            return this.bufferReader.GetEscapedString(this.offset, this.length);
        }

        public string GetString(XmlNameTable nameTable)
        {
            switch (this.type)
            {
                case StringHandleType.UTF8:
                    return this.bufferReader.GetString(this.offset, this.length, nameTable);

                case StringHandleType.Dictionary:
                    return nameTable.Add(this.bufferReader.GetDictionaryString(this.key).Value);

                case StringHandleType.ConstString:
                    return nameTable.Add(constStrings[this.key]);
            }
            return this.bufferReader.GetEscapedString(this.offset, this.length, nameTable);
        }

        public byte[] GetString(out int offset, out int length)
        {
            switch (this.type)
            {
                case StringHandleType.UTF8:
                    offset = this.offset;
                    length = this.length;
                    return this.bufferReader.Buffer;

                case StringHandleType.Dictionary:
                {
                    byte[] buffer = this.bufferReader.GetDictionaryString(this.key).ToUTF8();
                    offset = 0;
                    length = buffer.Length;
                    return buffer;
                }
                case StringHandleType.ConstString:
                {
                    byte[] buffer2 = XmlConverter.ToBytes(constStrings[this.key]);
                    offset = 0;
                    length = buffer2.Length;
                    return buffer2;
                }
            }
            byte[] buffer3 = XmlConverter.ToBytes(this.bufferReader.GetEscapedString(this.offset, this.length));
            offset = 0;
            length = buffer3.Length;
            return buffer3;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool operator ==(StringHandle s1, string s2)
        {
            return s1.Equals2(s2);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool operator ==(StringHandle s1, StringHandle s2)
        {
            return s1.Equals2(s2);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool operator ==(StringHandle s1, XmlDictionaryString xmlString2)
        {
            return s1.Equals2(xmlString2);
        }

        public static bool operator !=(StringHandle s1, string s2)
        {
            return !s1.Equals2(s2);
        }

        public static bool operator !=(StringHandle s1, StringHandle s2)
        {
            return !s1.Equals2(s2);
        }

        public static bool operator !=(StringHandle s1, XmlDictionaryString xmlString2)
        {
            return !s1.Equals2(xmlString2);
        }

        public void SetConstantValue(StringHandleConstStringType constStringType)
        {
            this.type = StringHandleType.ConstString;
            this.key = (int) constStringType;
        }

        public void SetValue(int key)
        {
            this.type = StringHandleType.Dictionary;
            this.key = key;
        }

        public void SetValue(StringHandle value)
        {
            this.type = value.type;
            this.key = value.key;
            this.offset = value.offset;
            this.length = value.length;
        }

        public void SetValue(int offset, int length)
        {
            this.type = StringHandleType.UTF8;
            this.offset = offset;
            this.length = length;
        }

        public void SetValue(int offset, int length, bool escaped)
        {
            this.type = escaped ? StringHandleType.EscapedUTF8 : StringHandleType.UTF8;
            this.offset = offset;
            this.length = length;
        }

        public void ToPrefixHandle(PrefixHandle prefix)
        {
            prefix.SetValue(this.offset, this.length);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override string ToString()
        {
            return this.GetString();
        }

        public bool TryGetDictionaryString(out XmlDictionaryString value)
        {
            if (this.type == StringHandleType.Dictionary)
            {
                value = this.bufferReader.GetDictionaryString(this.key);
                return true;
            }
            if (this.IsEmpty)
            {
                value = XmlDictionaryString.Empty;
                return true;
            }
            value = null;
            return false;
        }

        public bool IsEmpty
        {
            get
            {
                if (this.type == StringHandleType.UTF8)
                {
                    return (this.length == 0);
                }
                return this.Equals2(string.Empty);
            }
        }

        public bool IsXmlns
        {
            get
            {
                if (this.type != StringHandleType.UTF8)
                {
                    return this.Equals2("xmlns");
                }
                if (this.length != 5)
                {
                    return false;
                }
                byte[] buffer = this.bufferReader.Buffer;
                int offset = this.offset;
                return ((((buffer[offset] == 120) && (buffer[offset + 1] == 0x6d)) && ((buffer[offset + 2] == 0x6c) && (buffer[offset + 3] == 110))) && (buffer[offset + 4] == 0x73));
            }
        }

        private enum StringHandleType
        {
            Dictionary,
            UTF8,
            EscapedUTF8,
            ConstString
        }
    }
}

