namespace System.Security.Util
{
    using System;
    using System.IO;
    using System.Security;
    using System.Text;

    internal sealed class Parser
    {
        private SecurityDocument _doc;
        private Tokenizer _t;
        private const short c_additionaltexttag = 0x6300;
        private const short c_attributetag = 0x4200;
        private const short c_childrentag = 0x4400;
        private const short c_elementtag = 0x4100;
        private const short c_flag = 0x4000;
        private const short c_texttag = 0x4300;
        private const short c_wastedstringtag = 0x5000;

        internal Parser(StreamReader input) : this(new Tokenizer(input))
        {
        }

        private Parser(Tokenizer t)
        {
            this._t = t;
            this._doc = null;
            try
            {
                this.ParseContents();
            }
            finally
            {
                this._t.Recycle();
            }
        }

        internal Parser(string input) : this(new Tokenizer(input))
        {
        }

        internal Parser(char[] array) : this(new Tokenizer(array))
        {
        }

        internal Parser(byte[] array, Tokenizer.ByteTokenEncoding encoding) : this(new Tokenizer(array, encoding, 0))
        {
        }

        internal Parser(string input, string[] searchStrings, string[] replaceStrings) : this(new Tokenizer(input, searchStrings, replaceStrings))
        {
        }

        internal Parser(byte[] array, Tokenizer.ByteTokenEncoding encoding, int startIndex) : this(new Tokenizer(array, encoding, startIndex))
        {
        }

        private int DetermineFormat(TokenizerStream stream)
        {
            if ((stream.GetNextToken() != 0) || (stream.GetNextToken() != 5))
            {
                return 2;
            }
            this._t.GetTokens(stream, -1, true);
            stream.GoToPosition(2);
            bool flag = false;
            bool flag2 = false;
            for (short i = stream.GetNextToken(); (i != -1) && (i != 1); i = stream.GetNextToken())
            {
                switch (i)
                {
                    case 3:
                        if (!flag || !flag2)
                        {
                            break;
                        }
                        this._t.ChangeFormat(Encoding.GetEncoding(stream.GetNextString()));
                        return 0;

                    case 4:
                    {
                        flag = true;
                        continue;
                    }
                    default:
                        throw new XmlSyntaxException(this._t.LineNo, Environment.GetResourceString("XMLSyntax_UnexpectedEndOfFile"));
                }
                if (!flag)
                {
                    if (string.Compare(stream.GetNextString(), "encoding", StringComparison.Ordinal) == 0)
                    {
                        flag2 = true;
                    }
                }
                else
                {
                    flag = false;
                    flag2 = false;
                    stream.ThrowAwayNextString();
                }
            }
            return 0;
        }

        private void GetRequiredSizes(TokenizerStream stream, ref int index)
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            int num = 1;
            SecurityElementType regular = SecurityElementType.Regular;
            string nextString = null;
            bool flag5 = false;
            bool flag6 = false;
            int num2 = 0;
            do
            {
                short nextToken = stream.GetNextToken();
                while (nextToken != -1)
                {
                    switch ((nextToken & 0xff))
                    {
                        case 0:
                            flag4 = true;
                            flag6 = false;
                            nextToken = stream.GetNextToken();
                            if (nextToken != 2)
                            {
                                goto Label_01BD;
                            }
                            stream.TagLastToken(0x4400);
                            while (true)
                            {
                                nextToken = stream.GetNextToken();
                                if (nextToken != 3)
                                {
                                    break;
                                }
                                stream.ThrowAwayNextString();
                                stream.TagLastToken(0x5000);
                            }
                            if (nextToken == -1)
                            {
                                throw new XmlSyntaxException(this._t.LineNo, Environment.GetResourceString("XMLSyntax_UnexpectedEndOfFile"));
                            }
                            if (nextToken != 1)
                            {
                                throw new XmlSyntaxException(this._t.LineNo, Environment.GetResourceString("XMLSyntax_ExpectedCloseBracket"));
                            }
                            flag4 = false;
                            index++;
                            flag6 = false;
                            num--;
                            flag = true;
                            goto Label_03BD;

                        case 1:
                            if (!flag4)
                            {
                                throw new XmlSyntaxException(this._t.LineNo, Environment.GetResourceString("XMLSyntax_UnexpectedCloseBracket"));
                            }
                            flag4 = false;
                            goto Label_03C8;

                        case 2:
                            nextToken = stream.GetNextToken();
                            if (nextToken != 1)
                            {
                                throw new XmlSyntaxException(this._t.LineNo, Environment.GetResourceString("XMLSyntax_ExpectedCloseBracket"));
                            }
                            stream.TagLastToken(0x4400);
                            index++;
                            num--;
                            flag6 = false;
                            flag = true;
                            goto Label_03BD;

                        case 3:
                            if (!flag4)
                            {
                                goto Label_00D1;
                            }
                            if (regular != SecurityElementType.Comment)
                            {
                                break;
                            }
                            stream.ThrowAwayNextString();
                            stream.TagLastToken(0x5000);
                            goto Label_03BD;

                        case 4:
                            flag5 = true;
                            goto Label_03BD;

                        case 5:
                            if ((!flag4 || (regular != SecurityElementType.Format)) || (num2 != 1))
                            {
                                throw new XmlSyntaxException(this._t.LineNo);
                            }
                            nextToken = stream.GetNextToken();
                            if (nextToken != 1)
                            {
                                throw new XmlSyntaxException(this._t.LineNo, Environment.GetResourceString("XMLSyntax_ExpectedCloseBracket"));
                            }
                            stream.TagLastToken(0x4400);
                            index++;
                            num--;
                            flag6 = false;
                            flag = true;
                            goto Label_03BD;

                        default:
                            throw new XmlSyntaxException(this._t.LineNo);
                    }
                    if (nextString == null)
                    {
                        nextString = stream.GetNextString();
                    }
                    else
                    {
                        if (!flag5)
                        {
                            throw new XmlSyntaxException(this._t.LineNo);
                        }
                        stream.TagLastToken(0x4200);
                        index += (SecurityDocument.EncodedStringSize(nextString) + SecurityDocument.EncodedStringSize(stream.GetNextString())) + 1;
                        nextString = null;
                        flag5 = false;
                    }
                    goto Label_03BD;
                Label_00D1:
                    if (flag6)
                    {
                        stream.TagLastToken(0x6300);
                        index += SecurityDocument.EncodedStringSize(stream.GetNextString()) + SecurityDocument.EncodedStringSize(" ");
                    }
                    else
                    {
                        stream.TagLastToken(0x4300);
                        index += SecurityDocument.EncodedStringSize(stream.GetNextString()) + 1;
                        flag6 = true;
                    }
                    goto Label_03BD;
                Label_01BD:
                    if (nextToken == 3)
                    {
                        flag3 = true;
                        stream.TagLastToken(0x4100);
                        index += SecurityDocument.EncodedStringSize(stream.GetNextString()) + 1;
                        if (regular != SecurityElementType.Regular)
                        {
                            throw new XmlSyntaxException(this._t.LineNo);
                        }
                        flag = true;
                        num++;
                    }
                    else if (nextToken == 6)
                    {
                        num2 = 1;
                        do
                        {
                            nextToken = stream.GetNextToken();
                            switch (nextToken)
                            {
                                case 0:
                                    num2++;
                                    break;

                                case 1:
                                    num2--;
                                    break;

                                case 3:
                                    stream.ThrowAwayNextString();
                                    stream.TagLastToken(0x5000);
                                    break;
                            }
                        }
                        while (num2 > 0);
                        flag4 = false;
                        flag6 = false;
                        flag = true;
                    }
                    else
                    {
                        if (nextToken != 5)
                        {
                            throw new XmlSyntaxException(this._t.LineNo, Environment.GetResourceString("XMLSyntax_ExpectedSlashOrString"));
                        }
                        nextToken = stream.GetNextToken();
                        if (nextToken != 3)
                        {
                            throw new XmlSyntaxException(this._t.LineNo);
                        }
                        flag3 = true;
                        regular = SecurityElementType.Format;
                        stream.TagLastToken(0x4100);
                        index += SecurityDocument.EncodedStringSize(stream.GetNextString()) + 1;
                        num2 = 1;
                        num++;
                        flag = true;
                    }
                Label_03BD:
                    if (flag)
                    {
                        flag = false;
                        flag2 = false;
                        break;
                    }
                    flag2 = true;
                Label_03C8:
                    nextToken = stream.GetNextToken();
                }
                if (flag2)
                {
                    index++;
                    num--;
                    flag6 = false;
                }
                else if ((nextToken == -1) && ((num != 1) || !flag3))
                {
                    throw new XmlSyntaxException(this._t.LineNo, Environment.GetResourceString("XMLSyntax_UnexpectedEndOfFile"));
                }
            }
            while (num > 1);
        }

        internal SecurityElement GetTopElement()
        {
            if (!this.ParsedSuccessfully())
            {
                throw new XmlSyntaxException(this._t.LineNo);
            }
            return this._doc.GetRootElement();
        }

        private void ParseContents()
        {
            TokenizerStream stream = new TokenizerStream();
            this._t.GetTokens(stream, 2, false);
            stream.Reset();
            int position = this.DetermineFormat(stream);
            stream.GoToPosition(position);
            this._t.GetTokens(stream, -1, false);
            stream.Reset();
            int index = 0;
            this.GetRequiredSizes(stream, ref index);
            this._doc = new SecurityDocument(index);
            int num4 = 0;
            stream.Reset();
            for (short i = stream.GetNextFullToken(); i != -1; i = stream.GetNextFullToken())
            {
                if ((i & 0x4000) == 0x4000)
                {
                    switch (((short) (i & 0xff00)))
                    {
                        case 0x4400:
                        {
                            this._doc.AddToken(4, ref num4);
                            continue;
                        }
                        case 0x5000:
                        {
                            stream.ThrowAwayNextString();
                            continue;
                        }
                        case 0x6300:
                        {
                            this._doc.AppendString(" ", ref num4);
                            this._doc.AppendString(stream.GetNextString(), ref num4);
                            continue;
                        }
                        case 0x4100:
                        {
                            this._doc.AddToken(1, ref num4);
                            this._doc.AddString(stream.GetNextString(), ref num4);
                            continue;
                        }
                        case 0x4200:
                        {
                            this._doc.AddToken(2, ref num4);
                            this._doc.AddString(stream.GetNextString(), ref num4);
                            this._doc.AddString(stream.GetNextString(), ref num4);
                            continue;
                        }
                        case 0x4300:
                        {
                            this._doc.AddToken(3, ref num4);
                            this._doc.AddString(stream.GetNextString(), ref num4);
                            continue;
                        }
                    }
                    throw new XmlSyntaxException();
                }
            }
        }

        internal bool ParsedSuccessfully()
        {
            return true;
        }
    }
}

