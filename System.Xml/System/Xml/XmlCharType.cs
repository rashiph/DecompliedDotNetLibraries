namespace System.Xml
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct XmlCharType
    {
        internal const int SurHighStart = 0xd800;
        internal const int SurHighEnd = 0xdbff;
        internal const int SurLowStart = 0xdc00;
        internal const int SurLowEnd = 0xdfff;
        internal const int SurMask = 0xfc00;
        internal const int fWhitespace = 1;
        internal const int fLetter = 2;
        internal const int fNCStartNameSC = 4;
        internal const int fNCNameSC = 8;
        internal const int fCharData = 0x10;
        internal const int fNCNameXml4e = 0x20;
        internal const int fText = 0x40;
        internal const int fAttrValue = 0x80;
        private const string s_PublicIdBitmap = "␀\0ﾻ꿿￿蟿￾߿";
        private const uint CharPropertiesSize = 0x10000;
        private static object s_Lock;
        private unsafe static byte* s_CharProperties;
        internal unsafe byte* charProperties;
        private static object StaticLock
        {
            get
            {
                if (s_Lock == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref s_Lock, obj2, null);
                }
                return s_Lock;
            }
        }
        private static unsafe void InitInstance()
        {
            lock (StaticLock)
            {
                if (s_CharProperties == null)
                {
                    UnmanagedMemoryStream manifestResourceStream = (UnmanagedMemoryStream) Assembly.GetExecutingAssembly().GetManifestResourceStream("XmlCharType.bin");
                    byte* positionPointer = manifestResourceStream.PositionPointer;
                    Thread.MemoryBarrier();
                    s_CharProperties = positionPointer;
                }
            }
        }

        private unsafe XmlCharType(byte* charProperties)
        {
            this.charProperties = charProperties;
        }

        public static XmlCharType Instance
        {
            get
            {
                if (s_CharProperties == null)
                {
                    InitInstance();
                }
                return new XmlCharType(s_CharProperties);
            }
        }
        public unsafe bool IsWhiteSpace(char ch)
        {
            return ((this.charProperties[ch] & 1) != 0);
        }

        public bool IsExtender(char ch)
        {
            return (ch == '\x00b7');
        }

        public unsafe bool IsNCNameSingleChar(char ch)
        {
            return ((this.charProperties[ch] & 8) != 0);
        }

        public unsafe bool IsStartNCNameSingleChar(char ch)
        {
            return ((this.charProperties[ch] & 4) != 0);
        }

        public bool IsNameSingleChar(char ch)
        {
            if (!this.IsNCNameSingleChar(ch))
            {
                return (ch == ':');
            }
            return true;
        }

        public bool IsStartNameSingleChar(char ch)
        {
            if (!this.IsStartNCNameSingleChar(ch))
            {
                return (ch == ':');
            }
            return true;
        }

        public unsafe bool IsCharData(char ch)
        {
            return ((this.charProperties[ch] & 0x10) != 0);
        }

        public bool IsPubidChar(char ch)
        {
            return ((ch < '\x0080') && (("␀\0ﾻ꿿￿蟿￾߿"[ch >> 4] & (((int) 1) << (ch & 15))) != 0));
        }

        internal unsafe bool IsTextChar(char ch)
        {
            return ((this.charProperties[ch] & 0x40) != 0);
        }

        internal unsafe bool IsAttributeValueChar(char ch)
        {
            return ((this.charProperties[ch] & 0x80) != 0);
        }

        public unsafe bool IsLetter(char ch)
        {
            return ((this.charProperties[ch] & 2) != 0);
        }

        public unsafe bool IsNCNameCharXml4e(char ch)
        {
            return ((this.charProperties[ch] & 0x20) != 0);
        }

        public bool IsStartNCNameCharXml4e(char ch)
        {
            if (!this.IsLetter(ch))
            {
                return (ch == '_');
            }
            return true;
        }

        public bool IsNameCharXml4e(char ch)
        {
            if (!this.IsNCNameCharXml4e(ch))
            {
                return (ch == ':');
            }
            return true;
        }

        public bool IsStartNameCharXml4e(char ch)
        {
            if (!this.IsStartNCNameCharXml4e(ch))
            {
                return (ch == ':');
            }
            return true;
        }

        public static bool IsDigit(char ch)
        {
            return InRange(ch, 0x30, 0x39);
        }

        public static bool IsHexDigit(char ch)
        {
            if (!InRange(ch, 0x30, 0x39) && !InRange(ch, 0x61, 0x66))
            {
                return InRange(ch, 0x41, 70);
            }
            return true;
        }

        internal static bool IsHighSurrogate(int ch)
        {
            return InRange(ch, 0xd800, 0xdbff);
        }

        internal static bool IsLowSurrogate(int ch)
        {
            return InRange(ch, 0xdc00, 0xdfff);
        }

        internal static bool IsSurrogate(int ch)
        {
            return InRange(ch, 0xd800, 0xdfff);
        }

        internal static int CombineSurrogateChar(int lowChar, int highChar)
        {
            return ((lowChar - 0xdc00) | (((highChar - 0xd800) << 10) + 0x10000));
        }

        internal static void SplitSurrogateChar(int combinedChar, out char lowChar, out char highChar)
        {
            int num = combinedChar - 0x10000;
            lowChar = (char) (0xdc00 + (num % 0x400));
            highChar = (char) (0xd800 + (num / 0x400));
        }

        internal bool IsOnlyWhitespace(string str)
        {
            return (this.IsOnlyWhitespaceWithPos(str) == -1);
        }

        internal unsafe int IsOnlyWhitespaceWithPos(string str)
        {
            if (str != null)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if ((this.charProperties[str[i]] & 1) == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal unsafe int IsOnlyCharData(string str)
        {
            if (str != null)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if ((this.charProperties[str[i]] & 0x10) == 0)
                    {
                        if ((((i + 1) >= str.Length) || !IsHighSurrogate(str[i])) || !IsLowSurrogate(str[i + 1]))
                        {
                            return i;
                        }
                        i++;
                    }
                }
            }
            return -1;
        }

        internal static bool IsOnlyDigits(string str, int startPos, int len)
        {
            for (int i = startPos; i < (startPos + len); i++)
            {
                if (!IsDigit(str[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsOnlyDigits(char[] chars, int startPos, int len)
        {
            for (int i = startPos; i < (startPos + len); i++)
            {
                if (!IsDigit(chars[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal int IsPublicId(string str)
        {
            if (str != null)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (!this.IsPubidChar(str[i]))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private static bool InRange(int value, int start, int end)
        {
            return ((value - start) <= (end - start));
        }
    }
}

