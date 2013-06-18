namespace System.Xml
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal class PrefixHandle
    {
        private XmlBufferReader bufferReader;
        private int length;
        private int offset;
        private static byte[] prefixBuffer = new byte[] { 
            0x61, 0x62, 0x63, 100, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 110, 0x6f, 0x70, 
            0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 120, 0x79, 0x7a
         };
        private static string[] prefixStrings = new string[] { 
            "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", 
            "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
         };
        private PrefixHandleType type;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PrefixHandle(XmlBufferReader bufferReader)
        {
            this.bufferReader = bufferReader;
        }

        public int CompareTo(PrefixHandle that)
        {
            return this.GetString().CompareTo(that.GetString());
        }

        public override bool Equals(object obj)
        {
            PrefixHandle objA = obj as PrefixHandle;
            if (object.ReferenceEquals(objA, null))
            {
                return false;
            }
            return (this == objA);
        }

        private bool Equals2(string prefix2)
        {
            PrefixHandleType type = this.type;
            if (type != PrefixHandleType.Buffer)
            {
                return (GetString(type) == prefix2);
            }
            return this.bufferReader.Equals2(this.offset, this.length, prefix2);
        }

        private bool Equals2(PrefixHandle prefix2)
        {
            PrefixHandleType type = this.type;
            PrefixHandleType type2 = prefix2.type;
            if (type != type2)
            {
                return false;
            }
            if (type != PrefixHandleType.Buffer)
            {
                return true;
            }
            if (this.bufferReader == prefix2.bufferReader)
            {
                return this.bufferReader.Equals2(this.offset, this.length, prefix2.offset, prefix2.length);
            }
            return this.bufferReader.Equals2(this.offset, this.length, prefix2.bufferReader, prefix2.offset, prefix2.length);
        }

        private bool Equals2(XmlDictionaryString prefix2)
        {
            return this.Equals2(prefix2.Value);
        }

        public static PrefixHandleType GetAlphaPrefix(int index)
        {
            return (PrefixHandleType) (1 + index);
        }

        public override int GetHashCode()
        {
            return this.GetString().GetHashCode();
        }

        public string GetString()
        {
            PrefixHandleType type = this.type;
            if (type != PrefixHandleType.Buffer)
            {
                return GetString(type);
            }
            return this.bufferReader.GetString(this.offset, this.length);
        }

        public static string GetString(PrefixHandleType type)
        {
            return prefixStrings[(int) type];
        }

        public string GetString(XmlNameTable nameTable)
        {
            PrefixHandleType type = this.type;
            if (type != PrefixHandleType.Buffer)
            {
                return GetString(type);
            }
            return this.bufferReader.GetString(this.offset, this.length, nameTable);
        }

        public byte[] GetString(out int offset, out int length)
        {
            PrefixHandleType type = this.type;
            if (type != PrefixHandleType.Buffer)
            {
                return GetString(type, out offset, out length);
            }
            offset = this.offset;
            length = this.length;
            return this.bufferReader.Buffer;
        }

        public static byte[] GetString(PrefixHandleType type, out int offset, out int length)
        {
            if (type == PrefixHandleType.Empty)
            {
                offset = 0;
                length = 0;
            }
            else
            {
                length = 1;
                offset = ((int) type) - 1;
            }
            return prefixBuffer;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool operator ==(PrefixHandle prefix1, string prefix2)
        {
            return prefix1.Equals2(prefix2);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool operator ==(PrefixHandle prefix1, PrefixHandle prefix2)
        {
            return prefix1.Equals2(prefix2);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool operator ==(PrefixHandle prefix1, XmlDictionaryString prefix2)
        {
            return prefix1.Equals2(prefix2);
        }

        public static bool operator !=(PrefixHandle prefix1, string prefix2)
        {
            return !prefix1.Equals2(prefix2);
        }

        public static bool operator !=(PrefixHandle prefix1, PrefixHandle prefix2)
        {
            return !prefix1.Equals2(prefix2);
        }

        public static bool operator !=(PrefixHandle prefix1, XmlDictionaryString prefix2)
        {
            return !prefix1.Equals2(prefix2);
        }

        public void SetValue(PrefixHandle prefix)
        {
            this.type = prefix.type;
            this.offset = prefix.offset;
            this.length = prefix.length;
        }

        public void SetValue(PrefixHandleType type)
        {
            this.type = type;
        }

        public void SetValue(int offset, int length)
        {
            if (length == 0)
            {
                this.SetValue(PrefixHandleType.Empty);
            }
            else
            {
                if (length == 1)
                {
                    byte @byte = this.bufferReader.GetByte(offset);
                    if ((@byte >= 0x61) && (@byte <= 0x7a))
                    {
                        this.SetValue(GetAlphaPrefix(@byte - 0x61));
                        return;
                    }
                }
                this.type = PrefixHandleType.Buffer;
                this.offset = offset;
                this.length = length;
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override string ToString()
        {
            return this.GetString();
        }

        public bool TryGetShortPrefix(out PrefixHandleType type)
        {
            type = this.type;
            return (type != PrefixHandleType.Buffer);
        }

        public bool IsEmpty
        {
            get
            {
                return (this.type == PrefixHandleType.Empty);
            }
        }

        public bool IsXml
        {
            get
            {
                if (this.type != PrefixHandleType.Buffer)
                {
                    return false;
                }
                if (this.length != 3)
                {
                    return false;
                }
                byte[] buffer = this.bufferReader.Buffer;
                int offset = this.offset;
                return (((buffer[offset] == 120) && (buffer[offset + 1] == 0x6d)) && (buffer[offset + 2] == 0x6c));
            }
        }

        public bool IsXmlns
        {
            get
            {
                if (this.type != PrefixHandleType.Buffer)
                {
                    return false;
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
    }
}

