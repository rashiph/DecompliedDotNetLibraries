namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SurrogateChar
    {
        public const int MinValue = 0x10000;
        public const int MaxValue = 0x10ffff;
        private char lowChar;
        private char highChar;
        private const char surHighMin = '\ud800';
        private const char surHighMax = '\udbff';
        private const char surLowMin = '\udc00';
        private const char surLowMax = '\udfff';
        public SurrogateChar(int ch)
        {
            if ((ch < 0x10000) || (ch > 0x10ffff))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlInvalidSurrogate", new object[] { ch.ToString("X", CultureInfo.InvariantCulture) }), "ch"));
            }
            this.lowChar = (char) (((ch - 0x10000) & 0x3ff) + 0xdc00);
            this.highChar = (char) ((((ch - 0x10000) >> 10) & 0x3ff) + 0xd800);
        }

        public SurrogateChar(char lowChar, char highChar)
        {
            if ((lowChar < 0xdc00) || (lowChar > 0xdfff))
            {
                object[] args = new object[] { ((int) lowChar).ToString("X", CultureInfo.InvariantCulture) };
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlInvalidLowSurrogate", args), "lowChar"));
            }
            if ((highChar < 0xd800) || (highChar > 0xdbff))
            {
                object[] objArray2 = new object[] { ((int) highChar).ToString("X", CultureInfo.InvariantCulture) };
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlInvalidHighSurrogate", objArray2), "highChar"));
            }
            this.lowChar = lowChar;
            this.highChar = highChar;
        }

        public char LowChar
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lowChar;
            }
        }
        public char HighChar
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.highChar;
            }
        }
        public int Char
        {
            get
            {
                return ((this.lowChar - 0xdc00) | (((this.highChar - 0xd800) << 10) + 0x10000));
            }
        }
    }
}

