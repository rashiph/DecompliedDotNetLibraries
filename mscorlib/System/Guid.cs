namespace System
{
    using Microsoft.Win32;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct Guid : IFormattable, IComparable, IComparable<Guid>, IEquatable<Guid>
    {
        public static readonly Guid Empty;
        private int _a;
        private short _b;
        private short _c;
        private byte _d;
        private byte _e;
        private byte _f;
        private byte _g;
        private byte _h;
        private byte _i;
        private byte _j;
        private byte _k;
        public Guid(byte[] b)
        {
            if (b == null)
            {
                throw new ArgumentNullException("b");
            }
            if (b.Length != 0x10)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_GuidArrayCtor", new object[] { "16" }));
            }
            this._a = (((b[3] << 0x18) | (b[2] << 0x10)) | (b[1] << 8)) | b[0];
            this._b = (short) ((b[5] << 8) | b[4]);
            this._c = (short) ((b[7] << 8) | b[6]);
            this._d = b[8];
            this._e = b[9];
            this._f = b[10];
            this._g = b[11];
            this._h = b[12];
            this._i = b[13];
            this._j = b[14];
            this._k = b[15];
        }

        [CLSCompliant(false)]
        public Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            this._a = (int) a;
            this._b = (short) b;
            this._c = (short) c;
            this._d = d;
            this._e = e;
            this._f = f;
            this._g = g;
            this._h = h;
            this._i = i;
            this._j = j;
            this._k = k;
        }

        public Guid(int a, short b, short c, byte[] d)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d");
            }
            if (d.Length != 8)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_GuidArrayCtor", new object[] { "8" }));
            }
            this._a = a;
            this._b = b;
            this._c = c;
            this._d = d[0];
            this._e = d[1];
            this._f = d[2];
            this._g = d[3];
            this._h = d[4];
            this._i = d[5];
            this._j = d[6];
            this._k = d[7];
        }

        public Guid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            this._a = a;
            this._b = b;
            this._c = c;
            this._d = d;
            this._e = e;
            this._f = f;
            this._g = g;
            this._h = h;
            this._i = i;
            this._j = j;
            this._k = k;
        }

        public Guid(string g)
        {
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }
            this = Empty;
            GuidResult result = new GuidResult();
            result.Init(GuidParseThrowStyle.All);
            if (!TryParseGuid(g, GuidStyles.Any, ref result))
            {
                throw result.GetGuidParseException();
            }
            this = result.parsedGuid;
        }

        public static Guid Parse(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            GuidResult result = new GuidResult();
            result.Init(GuidParseThrowStyle.AllButOverflow);
            if (!TryParseGuid(input, GuidStyles.Any, ref result))
            {
                throw result.GetGuidParseException();
            }
            return result.parsedGuid;
        }

        public static bool TryParse(string input, out Guid result)
        {
            GuidResult result2 = new GuidResult();
            result2.Init(GuidParseThrowStyle.None);
            if (TryParseGuid(input, GuidStyles.Any, ref result2))
            {
                result = result2.parsedGuid;
                return true;
            }
            result = Empty;
            return false;
        }

        public static Guid ParseExact(string input, string format)
        {
            GuidStyles digitFormat;
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            if (format.Length != 1)
            {
                throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
            }
            char ch = format[0];
            switch (ch)
            {
                case 'D':
                case 'd':
                    digitFormat = GuidStyles.DigitFormat;
                    break;

                case 'N':
                case 'n':
                    digitFormat = GuidStyles.None;
                    break;

                case 'B':
                case 'b':
                    digitFormat = GuidStyles.BraceFormat;
                    break;

                case 'P':
                case 'p':
                    digitFormat = GuidStyles.ParenthesisFormat;
                    break;

                default:
                    if ((ch != 'X') && (ch != 'x'))
                    {
                        throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
                    }
                    digitFormat = GuidStyles.HexFormat;
                    break;
            }
            GuidResult result = new GuidResult();
            result.Init(GuidParseThrowStyle.AllButOverflow);
            if (!TryParseGuid(input, digitFormat, ref result))
            {
                throw result.GetGuidParseException();
            }
            return result.parsedGuid;
        }

        public static bool TryParseExact(string input, string format, out Guid result)
        {
            GuidStyles digitFormat;
            if ((format == null) || (format.Length != 1))
            {
                result = Empty;
                return false;
            }
            switch (format[0])
            {
                case 'D':
                case 'd':
                    digitFormat = GuidStyles.DigitFormat;
                    break;

                case 'N':
                case 'n':
                    digitFormat = GuidStyles.None;
                    break;

                case 'B':
                case 'b':
                    digitFormat = GuidStyles.BraceFormat;
                    break;

                case 'P':
                case 'p':
                    digitFormat = GuidStyles.ParenthesisFormat;
                    break;

                case 'X':
                case 'x':
                    digitFormat = GuidStyles.HexFormat;
                    break;

                default:
                    result = Empty;
                    return false;
            }
            GuidResult result2 = new GuidResult();
            result2.Init(GuidParseThrowStyle.None);
            if (TryParseGuid(input, digitFormat, ref result2))
            {
                result = result2.parsedGuid;
                return true;
            }
            result = Empty;
            return false;
        }

        private static bool TryParseGuid(string g, GuidStyles flags, ref GuidResult result)
        {
            if (g == null)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }
            string guidString = g.Trim();
            if (guidString.Length == 0)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }
            bool flag = guidString.IndexOf('-', 0) >= 0;
            if (flag)
            {
                if ((flags & (GuidStyles.DigitFormat | GuidStyles.AllowDashes)) == GuidStyles.None)
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                    return false;
                }
            }
            else if ((flags & GuidStyles.DigitFormat) != GuidStyles.None)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }
            bool flag2 = guidString.IndexOf('{', 0) >= 0;
            if (flag2)
            {
                if ((flags & (GuidStyles.RequireBraces | GuidStyles.AllowBraces)) == GuidStyles.None)
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                    return false;
                }
            }
            else if ((flags & GuidStyles.RequireBraces) != GuidStyles.None)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }
            if (guidString.IndexOf('(', 0) >= 0)
            {
                if ((flags & (GuidStyles.RequireParenthesis | GuidStyles.AllowParenthesis)) == GuidStyles.None)
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                    return false;
                }
            }
            else if ((flags & GuidStyles.RequireParenthesis) != GuidStyles.None)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidUnrecognized");
                return false;
            }
            try
            {
                if (flag)
                {
                    return TryParseGuidWithDashes(guidString, ref result);
                }
                if (flag2)
                {
                    return TryParseGuidWithHexPrefix(guidString, ref result);
                }
                return TryParseGuidWithNoStyle(guidString, ref result);
            }
            catch (IndexOutOfRangeException exception)
            {
                result.SetFailure(ParseFailureKind.FormatWithInnerException, "Format_GuidUnrecognized", null, null, exception);
                return false;
            }
            catch (ArgumentException exception2)
            {
                result.SetFailure(ParseFailureKind.FormatWithInnerException, "Format_GuidUnrecognized", null, null, exception2);
                return false;
            }
        }

        private static bool TryParseGuidWithHexPrefix(string guidString, ref GuidResult result)
        {
            int startIndex = 0;
            int length = 0;
            guidString = EatAllWhitespace(guidString);
            if (string.IsNullOrEmpty(guidString) || (guidString[0] != '{'))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidBrace");
                return false;
            }
            if (!IsHexPrefix(guidString, 1))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidHexPrefix", "{0xdddddddd, etc}");
                return false;
            }
            startIndex = 3;
            length = guidString.IndexOf(',', startIndex) - startIndex;
            if (length <= 0)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidComma");
                return false;
            }
            if (!StringToInt(guidString.Substring(startIndex, length), -1, 0x1000, out result.parsedGuid._a, ref result))
            {
                return false;
            }
            if (!IsHexPrefix(guidString, (startIndex + length) + 1))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidHexPrefix", "{0xdddddddd, 0xdddd, etc}");
                return false;
            }
            startIndex = (startIndex + length) + 3;
            length = guidString.IndexOf(',', startIndex) - startIndex;
            if (length <= 0)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidComma");
                return false;
            }
            if (!StringToShort(guidString.Substring(startIndex, length), -1, 0x1000, out result.parsedGuid._b, ref result))
            {
                return false;
            }
            if (!IsHexPrefix(guidString, (startIndex + length) + 1))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidHexPrefix", "{0xdddddddd, 0xdddd, 0xdddd, etc}");
                return false;
            }
            startIndex = (startIndex + length) + 3;
            length = guidString.IndexOf(',', startIndex) - startIndex;
            if (length <= 0)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidComma");
                return false;
            }
            if (!StringToShort(guidString.Substring(startIndex, length), -1, 0x1000, out result.parsedGuid._c, ref result))
            {
                return false;
            }
            if ((guidString.Length <= ((startIndex + length) + 1)) || (guidString[(startIndex + length) + 1] != '{'))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidBrace");
                return false;
            }
            length++;
            byte[] buffer = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                if (!IsHexPrefix(guidString, (startIndex + length) + 1))
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidHexPrefix", "{... { ... 0xdd, ...}}");
                    return false;
                }
                startIndex = (startIndex + length) + 3;
                if (i < 7)
                {
                    length = guidString.IndexOf(',', startIndex) - startIndex;
                    if (length <= 0)
                    {
                        result.SetFailure(ParseFailureKind.Format, "Format_GuidComma");
                        return false;
                    }
                }
                else
                {
                    length = guidString.IndexOf('}', startIndex) - startIndex;
                    if (length <= 0)
                    {
                        result.SetFailure(ParseFailureKind.Format, "Format_GuidBraceAfterLastNumber");
                        return false;
                    }
                }
                uint num4 = (uint) Convert.ToInt32(guidString.Substring(startIndex, length), 0x10);
                if (num4 > 0xff)
                {
                    result.SetFailure(ParseFailureKind.Format, "Overflow_Byte");
                    return false;
                }
                buffer[i] = (byte) num4;
            }
            result.parsedGuid._d = buffer[0];
            result.parsedGuid._e = buffer[1];
            result.parsedGuid._f = buffer[2];
            result.parsedGuid._g = buffer[3];
            result.parsedGuid._h = buffer[4];
            result.parsedGuid._i = buffer[5];
            result.parsedGuid._j = buffer[6];
            result.parsedGuid._k = buffer[7];
            if ((((startIndex + length) + 1) >= guidString.Length) || (guidString[(startIndex + length) + 1] != '}'))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidEndBrace");
                return false;
            }
            if (((startIndex + length) + 1) != (guidString.Length - 1))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_ExtraJunkAtEnd");
                return false;
            }
            return true;
        }

        private static bool TryParseGuidWithNoStyle(string guidString, ref GuidResult result)
        {
            int num2;
            long num3;
            int startIndex = 0;
            int parsePos = 0;
            if (guidString.Length != 0x20)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                return false;
            }
            for (int i = 0; i < guidString.Length; i++)
            {
                char c = guidString[i];
                if ((c < '0') || (c > '9'))
                {
                    char ch2 = char.ToUpper(c, CultureInfo.InvariantCulture);
                    if ((ch2 < 'A') || (ch2 > 'F'))
                    {
                        result.SetFailure(ParseFailureKind.Format, "Format_GuidInvalidChar");
                        return false;
                    }
                }
            }
            if (!StringToInt(guidString.Substring(startIndex, 8), -1, 0x1000, out result.parsedGuid._a, ref result))
            {
                return false;
            }
            startIndex += 8;
            if (!StringToShort(guidString.Substring(startIndex, 4), -1, 0x1000, out result.parsedGuid._b, ref result))
            {
                return false;
            }
            startIndex += 4;
            if (!StringToShort(guidString.Substring(startIndex, 4), -1, 0x1000, out result.parsedGuid._c, ref result))
            {
                return false;
            }
            startIndex += 4;
            if (!StringToInt(guidString.Substring(startIndex, 4), -1, 0x1000, out num2, ref result))
            {
                return false;
            }
            startIndex += 4;
            parsePos = startIndex;
            if (!StringToLong(guidString, ref parsePos, startIndex, out num3, ref result))
            {
                return false;
            }
            if ((parsePos - startIndex) != 12)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                return false;
            }
            result.parsedGuid._d = (byte) (num2 >> 8);
            result.parsedGuid._e = (byte) num2;
            num2 = (int) (num3 >> 0x20);
            result.parsedGuid._f = (byte) (num2 >> 8);
            result.parsedGuid._g = (byte) num2;
            num2 = (int) num3;
            result.parsedGuid._h = (byte) (num2 >> 0x18);
            result.parsedGuid._i = (byte) (num2 >> 0x10);
            result.parsedGuid._j = (byte) (num2 >> 8);
            result.parsedGuid._k = (byte) num2;
            return true;
        }

        private static bool TryParseGuidWithDashes(string guidString, ref GuidResult result)
        {
            int num2;
            long num3;
            int num = 0;
            int parsePos = 0;
            if (guidString[0] == '{')
            {
                if ((guidString.Length != 0x26) || (guidString[0x25] != '}'))
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                    return false;
                }
                num = 1;
            }
            else if (guidString[0] == '(')
            {
                if ((guidString.Length != 0x26) || (guidString[0x25] != ')'))
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                    return false;
                }
                num = 1;
            }
            else if (guidString.Length != 0x24)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                return false;
            }
            if (((guidString[8 + num] != '-') || (guidString[13 + num] != '-')) || ((guidString[0x12 + num] != '-') || (guidString[0x17 + num] != '-')))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidDashes");
                return false;
            }
            parsePos = num;
            if (!StringToInt(guidString, ref parsePos, 8, 0x2000, out num2, ref result))
            {
                return false;
            }
            result.parsedGuid._a = num2;
            parsePos++;
            if (!StringToInt(guidString, ref parsePos, 4, 0x2000, out num2, ref result))
            {
                return false;
            }
            result.parsedGuid._b = (short) num2;
            parsePos++;
            if (!StringToInt(guidString, ref parsePos, 4, 0x2000, out num2, ref result))
            {
                return false;
            }
            result.parsedGuid._c = (short) num2;
            parsePos++;
            if (!StringToInt(guidString, ref parsePos, 4, 0x2000, out num2, ref result))
            {
                return false;
            }
            parsePos++;
            num = parsePos;
            if (!StringToLong(guidString, ref parsePos, 0x2000, out num3, ref result))
            {
                return false;
            }
            if ((parsePos - num) != 12)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_GuidInvLen");
                return false;
            }
            result.parsedGuid._d = (byte) (num2 >> 8);
            result.parsedGuid._e = (byte) num2;
            num2 = (int) (num3 >> 0x20);
            result.parsedGuid._f = (byte) (num2 >> 8);
            result.parsedGuid._g = (byte) num2;
            num2 = (int) num3;
            result.parsedGuid._h = (byte) (num2 >> 0x18);
            result.parsedGuid._i = (byte) (num2 >> 0x10);
            result.parsedGuid._j = (byte) (num2 >> 8);
            result.parsedGuid._k = (byte) num2;
            return true;
        }

        [SecuritySafeCritical]
        private static unsafe bool StringToShort(string str, int requiredLength, int flags, out short result, ref GuidResult parseResult)
        {
            return StringToShort(str, (int*) null, requiredLength, flags, out result, ref parseResult);
        }

        [SecuritySafeCritical]
        private static unsafe bool StringToShort(string str, ref int parsePos, int requiredLength, int flags, out short result, ref GuidResult parseResult)
        {
            fixed (int* numRef = ((int*) parsePos))
            {
                return StringToShort(str, numRef, requiredLength, flags, out result, ref parseResult);
            }
        }

        [SecurityCritical]
        private static unsafe bool StringToShort(string str, int* parsePos, int requiredLength, int flags, out short result, ref GuidResult parseResult)
        {
            int num;
            result = 0;
            bool flag = StringToInt(str, parsePos, requiredLength, flags, out num, ref parseResult);
            result = (short) num;
            return flag;
        }

        [SecuritySafeCritical]
        private static unsafe bool StringToInt(string str, int requiredLength, int flags, out int result, ref GuidResult parseResult)
        {
            return StringToInt(str, (int*) null, requiredLength, flags, out result, ref parseResult);
        }

        [SecuritySafeCritical]
        private static unsafe bool StringToInt(string str, ref int parsePos, int requiredLength, int flags, out int result, ref GuidResult parseResult)
        {
            fixed (int* numRef = ((int*) parsePos))
            {
                return StringToInt(str, numRef, requiredLength, flags, out result, ref parseResult);
            }
        }

        [SecurityCritical]
        private static unsafe bool StringToInt(string str, int* parsePos, int requiredLength, int flags, out int result, ref GuidResult parseResult)
        {
            result = 0;
            int num = (parsePos == null) ? 0 : parsePos[0];
            try
            {
                result = ParseNumbers.StringToInt(str, 0x10, flags, parsePos);
            }
            catch (OverflowException exception)
            {
                if (parseResult.throwStyle == GuidParseThrowStyle.All)
                {
                    throw;
                }
                if (parseResult.throwStyle == GuidParseThrowStyle.AllButOverflow)
                {
                    throw new FormatException(Environment.GetResourceString("Format_GuidUnrecognized"), exception);
                }
                parseResult.SetFailure(exception);
                return false;
            }
            catch (Exception exception2)
            {
                if (parseResult.throwStyle != GuidParseThrowStyle.None)
                {
                    throw;
                }
                parseResult.SetFailure(exception2);
                return false;
            }
            if (((requiredLength != -1) && (parsePos != null)) && ((parsePos[0] - num) != requiredLength))
            {
                parseResult.SetFailure(ParseFailureKind.Format, "Format_GuidInvalidChar");
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        private static unsafe bool StringToLong(string str, int flags, out long result, ref GuidResult parseResult)
        {
            return StringToLong(str, (int*) null, flags, out result, ref parseResult);
        }

        [SecuritySafeCritical]
        private static unsafe bool StringToLong(string str, ref int parsePos, int flags, out long result, ref GuidResult parseResult)
        {
            fixed (int* numRef = ((int*) parsePos))
            {
                return StringToLong(str, numRef, flags, out result, ref parseResult);
            }
        }

        [SecuritySafeCritical]
        private static unsafe bool StringToLong(string str, int* parsePos, int flags, out long result, ref GuidResult parseResult)
        {
            result = 0L;
            try
            {
                result = ParseNumbers.StringToLong(str, 0x10, flags, parsePos);
            }
            catch (OverflowException exception)
            {
                if (parseResult.throwStyle == GuidParseThrowStyle.All)
                {
                    throw;
                }
                if (parseResult.throwStyle == GuidParseThrowStyle.AllButOverflow)
                {
                    throw new FormatException(Environment.GetResourceString("Format_GuidUnrecognized"), exception);
                }
                parseResult.SetFailure(exception);
                return false;
            }
            catch (Exception exception2)
            {
                if (parseResult.throwStyle != GuidParseThrowStyle.None)
                {
                    throw;
                }
                parseResult.SetFailure(exception2);
                return false;
            }
            return true;
        }

        private static string EatAllWhitespace(string str)
        {
            int length = 0;
            char[] chArray = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (!char.IsWhiteSpace(c))
                {
                    chArray[length++] = c;
                }
            }
            return new string(chArray, 0, length);
        }

        private static bool IsHexPrefix(string str, int i)
        {
            return (((str.Length > (i + 1)) && (str[i] == '0')) && (char.ToLower(str[i + 1], CultureInfo.InvariantCulture) == 'x'));
        }

        public byte[] ToByteArray()
        {
            return new byte[] { ((byte) this._a), ((byte) (this._a >> 8)), ((byte) (this._a >> 0x10)), ((byte) (this._a >> 0x18)), ((byte) this._b), ((byte) (this._b >> 8)), ((byte) this._c), ((byte) (this._c >> 8)), this._d, this._e, this._f, this._g, this._h, this._i, this._j, this._k };
        }

        public override string ToString()
        {
            return this.ToString("D", null);
        }

        public override int GetHashCode()
        {
            return ((this._a ^ ((this._b << 0x10) | ((ushort) this._c))) ^ ((this._f << 0x18) | this._k));
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !(o is Guid))
            {
                return false;
            }
            Guid guid = (Guid) o;
            if (guid._a != this._a)
            {
                return false;
            }
            if (guid._b != this._b)
            {
                return false;
            }
            if (guid._c != this._c)
            {
                return false;
            }
            if (guid._d != this._d)
            {
                return false;
            }
            if (guid._e != this._e)
            {
                return false;
            }
            if (guid._f != this._f)
            {
                return false;
            }
            if (guid._g != this._g)
            {
                return false;
            }
            if (guid._h != this._h)
            {
                return false;
            }
            if (guid._i != this._i)
            {
                return false;
            }
            if (guid._j != this._j)
            {
                return false;
            }
            if (guid._k != this._k)
            {
                return false;
            }
            return true;
        }

        public bool Equals(Guid g)
        {
            if (g._a != this._a)
            {
                return false;
            }
            if (g._b != this._b)
            {
                return false;
            }
            if (g._c != this._c)
            {
                return false;
            }
            if (g._d != this._d)
            {
                return false;
            }
            if (g._e != this._e)
            {
                return false;
            }
            if (g._f != this._f)
            {
                return false;
            }
            if (g._g != this._g)
            {
                return false;
            }
            if (g._h != this._h)
            {
                return false;
            }
            if (g._i != this._i)
            {
                return false;
            }
            if (g._j != this._j)
            {
                return false;
            }
            if (g._k != this._k)
            {
                return false;
            }
            return true;
        }

        private int GetResult(uint me, uint them)
        {
            if (me < them)
            {
                return -1;
            }
            return 1;
        }

        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is Guid))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeGuid"));
            }
            Guid guid = (Guid) value;
            if (guid._a != this._a)
            {
                return this.GetResult((uint) this._a, (uint) guid._a);
            }
            if (guid._b != this._b)
            {
                return this.GetResult((uint) this._b, (uint) guid._b);
            }
            if (guid._c != this._c)
            {
                return this.GetResult((uint) this._c, (uint) guid._c);
            }
            if (guid._d != this._d)
            {
                return this.GetResult(this._d, guid._d);
            }
            if (guid._e != this._e)
            {
                return this.GetResult(this._e, guid._e);
            }
            if (guid._f != this._f)
            {
                return this.GetResult(this._f, guid._f);
            }
            if (guid._g != this._g)
            {
                return this.GetResult(this._g, guid._g);
            }
            if (guid._h != this._h)
            {
                return this.GetResult(this._h, guid._h);
            }
            if (guid._i != this._i)
            {
                return this.GetResult(this._i, guid._i);
            }
            if (guid._j != this._j)
            {
                return this.GetResult(this._j, guid._j);
            }
            if (guid._k != this._k)
            {
                return this.GetResult(this._k, guid._k);
            }
            return 0;
        }

        public int CompareTo(Guid value)
        {
            if (value._a != this._a)
            {
                return this.GetResult((uint) this._a, (uint) value._a);
            }
            if (value._b != this._b)
            {
                return this.GetResult((uint) this._b, (uint) value._b);
            }
            if (value._c != this._c)
            {
                return this.GetResult((uint) this._c, (uint) value._c);
            }
            if (value._d != this._d)
            {
                return this.GetResult(this._d, value._d);
            }
            if (value._e != this._e)
            {
                return this.GetResult(this._e, value._e);
            }
            if (value._f != this._f)
            {
                return this.GetResult(this._f, value._f);
            }
            if (value._g != this._g)
            {
                return this.GetResult(this._g, value._g);
            }
            if (value._h != this._h)
            {
                return this.GetResult(this._h, value._h);
            }
            if (value._i != this._i)
            {
                return this.GetResult(this._i, value._i);
            }
            if (value._j != this._j)
            {
                return this.GetResult(this._j, value._j);
            }
            if (value._k != this._k)
            {
                return this.GetResult(this._k, value._k);
            }
            return 0;
        }

        public static bool operator ==(Guid a, Guid b)
        {
            if (a._a != b._a)
            {
                return false;
            }
            if (a._b != b._b)
            {
                return false;
            }
            if (a._c != b._c)
            {
                return false;
            }
            if (a._d != b._d)
            {
                return false;
            }
            if (a._e != b._e)
            {
                return false;
            }
            if (a._f != b._f)
            {
                return false;
            }
            if (a._g != b._g)
            {
                return false;
            }
            if (a._h != b._h)
            {
                return false;
            }
            if (a._i != b._i)
            {
                return false;
            }
            if (a._j != b._j)
            {
                return false;
            }
            if (a._k != b._k)
            {
                return false;
            }
            return true;
        }

        public static bool operator !=(Guid a, Guid b)
        {
            return !(a == b);
        }

        [SecuritySafeCritical]
        public static Guid NewGuid()
        {
            Guid guid;
            Marshal.ThrowExceptionForHR(Win32Native.CoCreateGuid(out guid), new IntPtr(-1));
            return guid;
        }

        public string ToString(string format)
        {
            return this.ToString(format, null);
        }

        private static char HexToChar(int a)
        {
            a &= 15;
            return ((a > 9) ? ((char) ((a - 10) + 0x61)) : ((char) (a + 0x30)));
        }

        private static int HexsToChars(char[] guidChars, int offset, int a, int b)
        {
            return HexsToChars(guidChars, offset, a, b, false);
        }

        private static int HexsToChars(char[] guidChars, int offset, int a, int b, bool hex)
        {
            if (hex)
            {
                guidChars[offset++] = '0';
                guidChars[offset++] = 'x';
            }
            guidChars[offset++] = HexToChar(a >> 4);
            guidChars[offset++] = HexToChar(a);
            if (hex)
            {
                guidChars[offset++] = ',';
                guidChars[offset++] = '0';
                guidChars[offset++] = 'x';
            }
            guidChars[offset++] = HexToChar(b >> 4);
            guidChars[offset++] = HexToChar(b);
            return offset;
        }

        public string ToString(string format, IFormatProvider provider)
        {
            char[] chArray;
            if ((format == null) || (format.Length == 0))
            {
                format = "D";
            }
            int offset = 0;
            int length = 0x26;
            bool flag = true;
            bool flag2 = false;
            if (format.Length != 1)
            {
                throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
            }
            char ch = format[0];
            switch (ch)
            {
                case 'D':
                case 'd':
                    chArray = new char[0x24];
                    length = 0x24;
                    break;

                case 'N':
                case 'n':
                    chArray = new char[0x20];
                    length = 0x20;
                    flag = false;
                    break;

                case 'B':
                case 'b':
                    chArray = new char[0x26];
                    chArray[offset++] = '{';
                    chArray[0x25] = '}';
                    break;

                case 'P':
                case 'p':
                    chArray = new char[0x26];
                    chArray[offset++] = '(';
                    chArray[0x25] = ')';
                    break;

                default:
                    if ((ch != 'X') && (ch != 'x'))
                    {
                        throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
                    }
                    chArray = new char[0x44];
                    chArray[offset++] = '{';
                    chArray[0x43] = '}';
                    length = 0x44;
                    flag = false;
                    flag2 = true;
                    break;
            }
            if (flag2)
            {
                chArray[offset++] = '0';
                chArray[offset++] = 'x';
                offset = HexsToChars(chArray, offset, this._a >> 0x18, this._a >> 0x10);
                offset = HexsToChars(chArray, offset, this._a >> 8, this._a);
                chArray[offset++] = ',';
                chArray[offset++] = '0';
                chArray[offset++] = 'x';
                offset = HexsToChars(chArray, offset, this._b >> 8, this._b);
                chArray[offset++] = ',';
                chArray[offset++] = '0';
                chArray[offset++] = 'x';
                offset = HexsToChars(chArray, offset, this._c >> 8, this._c);
                chArray[offset++] = ',';
                chArray[offset++] = '{';
                offset = HexsToChars(chArray, offset, this._d, this._e, true);
                chArray[offset++] = ',';
                offset = HexsToChars(chArray, offset, this._f, this._g, true);
                chArray[offset++] = ',';
                offset = HexsToChars(chArray, offset, this._h, this._i, true);
                chArray[offset++] = ',';
                offset = HexsToChars(chArray, offset, this._j, this._k, true);
                chArray[offset++] = '}';
            }
            else
            {
                offset = HexsToChars(chArray, offset, this._a >> 0x18, this._a >> 0x10);
                offset = HexsToChars(chArray, offset, this._a >> 8, this._a);
                if (flag)
                {
                    chArray[offset++] = '-';
                }
                offset = HexsToChars(chArray, offset, this._b >> 8, this._b);
                if (flag)
                {
                    chArray[offset++] = '-';
                }
                offset = HexsToChars(chArray, offset, this._c >> 8, this._c);
                if (flag)
                {
                    chArray[offset++] = '-';
                }
                offset = HexsToChars(chArray, offset, this._d, this._e);
                if (flag)
                {
                    chArray[offset++] = '-';
                }
                offset = HexsToChars(chArray, offset, this._f, this._g);
                offset = HexsToChars(chArray, offset, this._h, this._i);
                offset = HexsToChars(chArray, offset, this._j, this._k);
            }
            return new string(chArray, 0, length);
        }

        static Guid()
        {
            Empty = new Guid();
        }
        private enum GuidParseThrowStyle
        {
            None,
            All,
            AllButOverflow
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GuidResult
        {
            internal Guid parsedGuid;
            internal Guid.GuidParseThrowStyle throwStyle;
            internal Guid.ParseFailureKind m_failure;
            internal string m_failureMessageID;
            internal object m_failureMessageFormatArgument;
            internal string m_failureArgumentName;
            internal Exception m_innerException;
            internal void Init(Guid.GuidParseThrowStyle canThrow)
            {
                this.parsedGuid = Guid.Empty;
                this.throwStyle = canThrow;
            }

            internal void SetFailure(Exception nativeException)
            {
                this.m_failure = Guid.ParseFailureKind.NativeException;
                this.m_innerException = nativeException;
            }

            internal void SetFailure(Guid.ParseFailureKind failure, string failureMessageID)
            {
                this.SetFailure(failure, failureMessageID, null, null, null);
            }

            internal void SetFailure(Guid.ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument)
            {
                this.SetFailure(failure, failureMessageID, failureMessageFormatArgument, null, null);
            }

            internal void SetFailure(Guid.ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument, string failureArgumentName, Exception innerException)
            {
                this.m_failure = failure;
                this.m_failureMessageID = failureMessageID;
                this.m_failureMessageFormatArgument = failureMessageFormatArgument;
                this.m_failureArgumentName = failureArgumentName;
                this.m_innerException = innerException;
                if (this.throwStyle != Guid.GuidParseThrowStyle.None)
                {
                    throw this.GetGuidParseException();
                }
            }

            internal Exception GetGuidParseException()
            {
                switch (this.m_failure)
                {
                    case Guid.ParseFailureKind.ArgumentNull:
                        return new ArgumentNullException(this.m_failureArgumentName, Environment.GetResourceString(this.m_failureMessageID));

                    case Guid.ParseFailureKind.Format:
                        return new FormatException(Environment.GetResourceString(this.m_failureMessageID));

                    case Guid.ParseFailureKind.FormatWithParameter:
                        return new FormatException(Environment.GetResourceString(this.m_failureMessageID, new object[] { this.m_failureMessageFormatArgument }));

                    case Guid.ParseFailureKind.NativeException:
                        return this.m_innerException;

                    case Guid.ParseFailureKind.FormatWithInnerException:
                        return new FormatException(Environment.GetResourceString(this.m_failureMessageID), this.m_innerException);
                }
                return new FormatException(Environment.GetResourceString("Format_GuidUnrecognized"));
            }
        }

        [Flags]
        private enum GuidStyles
        {
            AllowBraces = 2,
            AllowDashes = 4,
            AllowHexPrefix = 8,
            AllowParenthesis = 1,
            Any = 15,
            BraceFormat = 0x60,
            DigitFormat = 0x40,
            HexFormat = 160,
            None = 0,
            NumberFormat = 0,
            ParenthesisFormat = 80,
            RequireBraces = 0x20,
            RequireDashes = 0x40,
            RequireHexPrefix = 0x80,
            RequireParenthesis = 0x10
        }

        private enum ParseFailureKind
        {
            None,
            ArgumentNull,
            Format,
            FormatWithParameter,
            NativeException,
            FormatWithInnerException
        }
    }
}

