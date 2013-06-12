namespace System.Security.Util
{
    using System;
    using System.IO;
    using System.Security;
    using System.Text;

    internal sealed class Tokenizer
    {
        private byte[] _inBytes;
        private char[] _inChars;
        private int _inIndex;
        private int _inNestedIndex;
        private int _inNestedSize;
        private string _inNestedString;
        private int _inProcessingTag;
        private int _inSavedCharacter;
        private int _inSize;
        private string _inString;
        private ITokenReader _inTokenReader;
        private TokenSource _inTokenSource;
        private StringMaker _maker;
        private string[] _replaceStrings;
        private string[] _searchStrings;
        internal const byte bang = 6;
        internal const byte bra = 0;
        internal const byte cstr = 3;
        internal const byte dash = 7;
        internal const byte equals = 4;
        internal const int intBang = 0x21;
        internal const int intCloseBracket = 0x3e;
        internal const int intCR = 13;
        internal const int intDash = 0x2d;
        internal const int intEquals = 0x3d;
        internal const int intLF = 10;
        internal const int intOpenBracket = 60;
        internal const int intQuest = 0x3f;
        internal const int intQuote = 0x22;
        internal const int intSlash = 0x2f;
        internal const int intSpace = 0x20;
        internal const int intTab = 9;
        internal const byte ket = 1;
        public int LineNo;
        internal const byte quest = 5;
        internal const byte slash = 2;

        internal Tokenizer(string input)
        {
            this.BasicInitialization();
            this._inString = input;
            this._inSize = input.Length;
            this._inTokenSource = TokenSource.String;
        }

        internal Tokenizer(char[] array)
        {
            this.BasicInitialization();
            this._inChars = array;
            this._inSize = array.Length;
            this._inTokenSource = TokenSource.CharArray;
        }

        internal Tokenizer(StreamReader input)
        {
            this.BasicInitialization();
            this._inTokenReader = new StreamTokenReader(input);
        }

        internal Tokenizer(string input, string[] searchStrings, string[] replaceStrings)
        {
            this.BasicInitialization();
            this._inString = input;
            this._inSize = this._inString.Length;
            this._inTokenSource = TokenSource.NestedStrings;
            this._searchStrings = searchStrings;
            this._replaceStrings = replaceStrings;
        }

        internal Tokenizer(byte[] array, ByteTokenEncoding encoding, int startIndex)
        {
            this.BasicInitialization();
            this._inBytes = array;
            this._inSize = array.Length;
            this._inIndex = startIndex;
            switch (encoding)
            {
                case ByteTokenEncoding.UnicodeTokens:
                    this._inTokenSource = TokenSource.UnicodeByteArray;
                    return;

                case ByteTokenEncoding.UTF8Tokens:
                    this._inTokenSource = TokenSource.UTF8ByteArray;
                    return;

                case ByteTokenEncoding.ByteTokens:
                    this._inTokenSource = TokenSource.ASCIIByteArray;
                    return;
            }
            throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) encoding }));
        }

        internal void BasicInitialization()
        {
            this.LineNo = 1;
            this._inProcessingTag = 0;
            this._inSavedCharacter = -1;
            this._inIndex = 0;
            this._inSize = 0;
            this._inNestedSize = 0;
            this._inNestedIndex = 0;
            this._inTokenSource = TokenSource.Other;
            this._maker = SharedStatics.GetSharedStringMaker();
        }

        internal void ChangeFormat(Encoding encoding)
        {
            Stream baseStream;
            if (encoding != null)
            {
                switch (this._inTokenSource)
                {
                    case TokenSource.UnicodeByteArray:
                    case TokenSource.UTF8ByteArray:
                    case TokenSource.ASCIIByteArray:
                        if (encoding != Encoding.Unicode)
                        {
                            if (encoding == Encoding.UTF8)
                            {
                                this._inTokenSource = TokenSource.UTF8ByteArray;
                                return;
                            }
                            if (encoding != Encoding.ASCII)
                            {
                                goto Label_005B;
                            }
                            this._inTokenSource = TokenSource.ASCIIByteArray;
                            break;
                        }
                        this._inTokenSource = TokenSource.UnicodeByteArray;
                        return;

                    case TokenSource.CharArray:
                    case TokenSource.String:
                    case TokenSource.NestedStrings:
                        break;

                    default:
                        goto Label_005B;
                }
            }
            return;
        Label_005B:
            baseStream = null;
            switch (this._inTokenSource)
            {
                case TokenSource.UnicodeByteArray:
                case TokenSource.UTF8ByteArray:
                case TokenSource.ASCIIByteArray:
                    baseStream = new MemoryStream(this._inBytes, this._inIndex, this._inSize - this._inIndex);
                    break;

                case TokenSource.CharArray:
                case TokenSource.String:
                case TokenSource.NestedStrings:
                    return;

                default:
                {
                    StreamTokenReader reader = this._inTokenReader as StreamTokenReader;
                    if (reader == null)
                    {
                        return;
                    }
                    baseStream = reader._in.BaseStream;
                    string s = new string(' ', reader.NumCharEncountered);
                    baseStream.Position = reader._in.CurrentEncoding.GetByteCount(s);
                    break;
                }
            }
            this._inTokenReader = new StreamTokenReader(new StreamReader(baseStream, encoding));
            this._inTokenSource = TokenSource.Other;
        }

        private string GetStringToken()
        {
            return this._maker.MakeString();
        }

        internal void GetTokens(TokenizerStream stream, int maxNum, bool endAfterKet)
        {
            while ((maxNum == -1) || (stream.GetTokenCount() < maxNum))
            {
                int num = -1;
                int num3 = 0;
                bool flag = false;
                bool flag2 = false;
                StringMaker maker = this._maker;
                maker._outStringBuilder = null;
                maker._outIndex = 0;
            Label_0026:
                if (this._inSavedCharacter != -1)
                {
                    num = this._inSavedCharacter;
                    this._inSavedCharacter = -1;
                }
                else
                {
                    switch (this._inTokenSource)
                    {
                        case TokenSource.UnicodeByteArray:
                            if ((this._inIndex + 1) < this._inSize)
                            {
                                break;
                            }
                            stream.AddToken(-1);
                            return;

                        case TokenSource.UTF8ByteArray:
                            if (this._inIndex < this._inSize)
                            {
                                goto Label_00CF;
                            }
                            stream.AddToken(-1);
                            return;

                        case TokenSource.ASCIIByteArray:
                            if (this._inIndex < this._inSize)
                            {
                                goto Label_023C;
                            }
                            stream.AddToken(-1);
                            return;

                        case TokenSource.CharArray:
                            if (this._inIndex < this._inSize)
                            {
                                goto Label_0272;
                            }
                            stream.AddToken(-1);
                            return;

                        case TokenSource.String:
                            if (this._inIndex < this._inSize)
                            {
                                goto Label_02A8;
                            }
                            stream.AddToken(-1);
                            return;

                        case TokenSource.NestedStrings:
                            if (this._inNestedSize == 0)
                            {
                                goto Label_030D;
                            }
                            if (this._inNestedIndex >= this._inNestedSize)
                            {
                                goto Label_0306;
                            }
                            num = this._inNestedString[this._inNestedIndex++];
                            goto Label_0402;

                        default:
                            num = this._inTokenReader.Read();
                            if (num == -1)
                            {
                                stream.AddToken(-1);
                                return;
                            }
                            goto Label_0402;
                    }
                    num = (this._inBytes[this._inIndex + 1] << 8) + this._inBytes[this._inIndex];
                    this._inIndex += 2;
                }
                goto Label_0402;
            Label_00CF:
                num = this._inBytes[this._inIndex++];
                if ((num & 0x80) != 0)
                {
                    switch (((num & 240) >> 4))
                    {
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            throw new XmlSyntaxException(this.LineNo);

                        case 12:
                        case 13:
                            num &= 0x1f;
                            num3 = 2;
                            break;

                        case 14:
                            num &= 15;
                            num3 = 3;
                            break;

                        case 15:
                            throw new XmlSyntaxException(this.LineNo);
                    }
                    if (this._inIndex >= this._inSize)
                    {
                        throw new XmlSyntaxException(this.LineNo, Environment.GetResourceString("XMLSyntax_UnexpectedEndOfFile"));
                    }
                    byte num2 = this._inBytes[this._inIndex++];
                    if ((num2 & 0xc0) != 0x80)
                    {
                        throw new XmlSyntaxException(this.LineNo);
                    }
                    num = (num << 6) | (num2 & 0x3f);
                    if (num3 != 2)
                    {
                        if (this._inIndex >= this._inSize)
                        {
                            throw new XmlSyntaxException(this.LineNo, Environment.GetResourceString("XMLSyntax_UnexpectedEndOfFile"));
                        }
                        num2 = this._inBytes[this._inIndex++];
                        if ((num2 & 0xc0) != 0x80)
                        {
                            throw new XmlSyntaxException(this.LineNo);
                        }
                        num = (num << 6) | (num2 & 0x3f);
                    }
                }
                goto Label_0402;
            Label_023C:
                num = this._inBytes[this._inIndex++];
                goto Label_0402;
            Label_0272:
                num = this._inChars[this._inIndex++];
                goto Label_0402;
            Label_02A8:
                num = this._inString[this._inIndex++];
                goto Label_0402;
            Label_0306:
                this._inNestedSize = 0;
            Label_030D:
                if (this._inIndex >= this._inSize)
                {
                    stream.AddToken(-1);
                    return;
                }
                num = this._inString[this._inIndex++];
                if (num == 0x7b)
                {
                    for (int i = 0; i < this._searchStrings.Length; i++)
                    {
                        if (string.Compare(this._searchStrings[i], 0, this._inString, this._inIndex - 1, this._searchStrings[i].Length, StringComparison.Ordinal) == 0)
                        {
                            this._inNestedString = this._replaceStrings[i];
                            this._inNestedSize = this._inNestedString.Length;
                            this._inNestedIndex = 1;
                            num = this._inNestedString[0];
                            this._inIndex += this._searchStrings[i].Length - 1;
                            break;
                        }
                    }
                }
            Label_0402:
                if (!flag)
                {
                    switch (num)
                    {
                        case 9:
                        case 13:
                        case 0x20:
                            goto Label_0026;

                        case 10:
                            this.LineNo++;
                            goto Label_0026;

                        case 0x21:
                        {
                            if (this._inProcessingTag == 0)
                            {
                                break;
                            }
                            stream.AddToken(6);
                            continue;
                        }
                        case 0x22:
                            flag = true;
                            flag2 = true;
                            goto Label_0026;

                        case 0x2d:
                        {
                            if (this._inProcessingTag == 0)
                            {
                                break;
                            }
                            stream.AddToken(7);
                            continue;
                        }
                        case 0x2f:
                        {
                            if (this._inProcessingTag == 0)
                            {
                                break;
                            }
                            stream.AddToken(2);
                            continue;
                        }
                        case 60:
                        {
                            this._inProcessingTag++;
                            stream.AddToken(0);
                            continue;
                        }
                        case 0x3d:
                        {
                            stream.AddToken(4);
                            continue;
                        }
                        case 0x3e:
                            this._inProcessingTag--;
                            stream.AddToken(1);
                            if (!endAfterKet)
                            {
                                continue;
                            }
                            return;

                        case 0x3f:
                        {
                            if (this._inProcessingTag == 0)
                            {
                                break;
                            }
                            stream.AddToken(5);
                            continue;
                        }
                    }
                }
                else
                {
                    switch (num)
                    {
                        case 60:
                        {
                            if (flag2)
                            {
                                break;
                            }
                            this._inSavedCharacter = num;
                            stream.AddToken(3);
                            stream.AddString(this.GetStringToken());
                            continue;
                        }
                        case 0x3d:
                        case 0x3e:
                        case 0x2f:
                        {
                            if (flag2 || (this._inProcessingTag == 0))
                            {
                                break;
                            }
                            this._inSavedCharacter = num;
                            stream.AddToken(3);
                            stream.AddString(this.GetStringToken());
                            continue;
                        }
                        case 9:
                        case 13:
                        case 0x20:
                        {
                            if (flag2)
                            {
                                break;
                            }
                            stream.AddToken(3);
                            stream.AddString(this.GetStringToken());
                            continue;
                        }
                        case 10:
                            goto Label_0629;

                        case 0x22:
                        {
                            if (!flag2)
                            {
                                break;
                            }
                            stream.AddToken(3);
                            stream.AddString(this.GetStringToken());
                            continue;
                        }
                    }
                }
                goto Label_0650;
            Label_0629:
                this.LineNo++;
                if (!flag2)
                {
                    stream.AddToken(3);
                    stream.AddString(this.GetStringToken());
                    continue;
                }
            Label_0650:
                flag = true;
                if (maker._outIndex < 0x200)
                {
                    maker._outChars[maker._outIndex++] = (char) num;
                }
                else
                {
                    if (maker._outStringBuilder == null)
                    {
                        maker._outStringBuilder = new StringBuilder();
                    }
                    maker._outStringBuilder.Append(maker._outChars, 0, 0x200);
                    maker._outChars[0] = (char) num;
                    maker._outIndex = 1;
                }
                goto Label_0026;
            }
        }

        public void Recycle()
        {
            SharedStatics.ReleaseSharedStringMaker(ref this._maker);
        }

        internal enum ByteTokenEncoding
        {
            UnicodeTokens,
            UTF8Tokens,
            ByteTokens
        }

        internal interface ITokenReader
        {
            int Read();
        }

        internal class StreamTokenReader : Tokenizer.ITokenReader
        {
            internal StreamReader _in;
            internal int _numCharRead;

            internal StreamTokenReader(StreamReader input)
            {
                this._in = input;
                this._numCharRead = 0;
            }

            public virtual int Read()
            {
                int num = this._in.Read();
                if (num != -1)
                {
                    this._numCharRead++;
                }
                return num;
            }

            internal int NumCharEncountered
            {
                get
                {
                    return this._numCharRead;
                }
            }
        }

        [Serializable]
        internal sealed class StringMaker
        {
            public char[] _outChars;
            public int _outIndex;
            public StringBuilder _outStringBuilder;
            private string[] aStrings;
            private uint cStringsMax = 0x800;
            private uint cStringsUsed = 0;
            public const int outMaxSize = 0x200;

            public StringMaker()
            {
                this.aStrings = new string[this.cStringsMax];
                this._outChars = new char[0x200];
            }

            private bool CompareStringAndChars(string str, char[] a, int l)
            {
                if (str.Length != l)
                {
                    return false;
                }
                for (int i = 0; i < l; i++)
                {
                    if (a[i] != str[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            private static uint HashCharArray(char[] a, int l)
            {
                uint num = 0;
                for (int i = 0; i < l; i++)
                {
                    num = ((num << 3) ^ a[i]) ^ (num >> 0x1d);
                }
                return num;
            }

            private static uint HashString(string str)
            {
                uint num = 0;
                int length = str.Length;
                for (int i = 0; i < length; i++)
                {
                    num = ((num << 3) ^ str[i]) ^ (num >> 0x1d);
                }
                return num;
            }

            public string MakeString()
            {
                uint num;
                string str;
                char[] a = this._outChars;
                int l = this._outIndex;
                if (this._outStringBuilder != null)
                {
                    this._outStringBuilder.Append(this._outChars, 0, this._outIndex);
                    return this._outStringBuilder.ToString();
                }
                if (this.cStringsUsed > ((this.cStringsMax / 4) * 3))
                {
                    uint num3 = this.cStringsMax * 2;
                    string[] strArray = new string[num3];
                    for (int i = 0; i < this.cStringsMax; i++)
                    {
                        if (this.aStrings[i] != null)
                        {
                            num = HashString(this.aStrings[i]) % num3;
                            while (strArray[num] != null)
                            {
                                if (++num >= num3)
                                {
                                    num = 0;
                                }
                            }
                            strArray[num] = this.aStrings[i];
                        }
                    }
                    this.cStringsMax = num3;
                    this.aStrings = strArray;
                }
                num = HashCharArray(a, l) % this.cStringsMax;
                while ((str = this.aStrings[num]) != null)
                {
                    if (this.CompareStringAndChars(str, a, l))
                    {
                        return str;
                    }
                    if (++num >= this.cStringsMax)
                    {
                        num = 0;
                    }
                }
                str = new string(a, 0, l);
                this.aStrings[num] = str;
                this.cStringsUsed++;
                return str;
            }
        }

        private enum TokenSource
        {
            UnicodeByteArray,
            UTF8ByteArray,
            ASCIIByteArray,
            CharArray,
            String,
            NestedStrings,
            Other
        }
    }
}

