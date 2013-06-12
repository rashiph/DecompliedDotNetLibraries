namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Globalization;
    using System.Xml;
    using System.Xml.XPath;

    internal sealed class XPathScanner
    {
        private bool canBeFunction;
        private char currentChar;
        private LexKind kind;
        private string name;
        private double numberValue = double.NaN;
        private string prefix;
        private string stringValue;
        private XmlCharType xmlCharType = XmlCharType.Instance;
        private string xpathExpr;
        private int xpathExprIndex;

        public XPathScanner(string xpathExpr)
        {
            if (xpathExpr == null)
            {
                throw XPathException.Create("Xp_ExprExpected", string.Empty);
            }
            this.xpathExpr = xpathExpr;
            this.NextChar();
            this.NextLex();
        }

        private bool NextChar()
        {
            if (this.xpathExprIndex < this.xpathExpr.Length)
            {
                this.currentChar = this.xpathExpr[this.xpathExprIndex++];
                return true;
            }
            this.currentChar = '\0';
            return false;
        }

        public bool NextLex()
        {
            this.SkipSpace();
            switch (this.CurerntChar)
            {
                case '[':
                case ']':
                case '|':
                case '#':
                case '$':
                case '(':
                case ')':
                case '*':
                case '+':
                case ',':
                case '-':
                case '=':
                case '@':
                    this.kind = (LexKind) Convert.ToInt32(this.CurerntChar, CultureInfo.InvariantCulture);
                    this.NextChar();
                    break;

                case '!':
                    this.kind = LexKind.Bang;
                    this.NextChar();
                    if (this.CurerntChar == '=')
                    {
                        this.kind = LexKind.Ne;
                        this.NextChar();
                    }
                    break;

                case '"':
                case '\'':
                    this.kind = LexKind.String;
                    this.stringValue = this.ScanString();
                    break;

                case '.':
                    this.kind = LexKind.Dot;
                    this.NextChar();
                    if (this.CurerntChar != '.')
                    {
                        if (XmlCharType.IsDigit(this.CurerntChar))
                        {
                            this.kind = LexKind.Number;
                            this.numberValue = this.ScanFraction();
                        }
                    }
                    else
                    {
                        this.kind = LexKind.DotDot;
                        this.NextChar();
                    }
                    break;

                case '/':
                    this.kind = LexKind.Slash;
                    this.NextChar();
                    if (this.CurerntChar == '/')
                    {
                        this.kind = LexKind.SlashSlash;
                        this.NextChar();
                    }
                    break;

                case '<':
                    this.kind = LexKind.Lt;
                    this.NextChar();
                    if (this.CurerntChar == '=')
                    {
                        this.kind = LexKind.Le;
                        this.NextChar();
                    }
                    break;

                case '>':
                    this.kind = LexKind.Gt;
                    this.NextChar();
                    if (this.CurerntChar == '=')
                    {
                        this.kind = LexKind.Ge;
                        this.NextChar();
                    }
                    break;

                case '\0':
                    this.kind = LexKind.Eof;
                    return false;

                default:
                    if (XmlCharType.IsDigit(this.CurerntChar))
                    {
                        this.kind = LexKind.Number;
                        this.numberValue = this.ScanNumber();
                    }
                    else
                    {
                        if (!this.xmlCharType.IsStartNCNameSingleChar(this.CurerntChar))
                        {
                            throw XPathException.Create("Xp_InvalidToken", this.SourceText);
                        }
                        this.kind = LexKind.Name;
                        this.name = this.ScanName();
                        this.prefix = string.Empty;
                        if (this.CurerntChar == ':')
                        {
                            this.NextChar();
                            if (this.CurerntChar != ':')
                            {
                                this.prefix = this.name;
                                if (this.CurerntChar != '*')
                                {
                                    if (!this.xmlCharType.IsStartNCNameSingleChar(this.CurerntChar))
                                    {
                                        throw XPathException.Create("Xp_InvalidName", this.SourceText);
                                    }
                                    this.name = this.ScanName();
                                }
                                else
                                {
                                    this.NextChar();
                                    this.name = "*";
                                }
                            }
                            else
                            {
                                this.NextChar();
                                this.kind = LexKind.Axe;
                            }
                        }
                        else
                        {
                            this.SkipSpace();
                            if (this.CurerntChar == ':')
                            {
                                this.NextChar();
                                if (this.CurerntChar != ':')
                                {
                                    throw XPathException.Create("Xp_InvalidName", this.SourceText);
                                }
                                this.NextChar();
                                this.kind = LexKind.Axe;
                            }
                        }
                        this.SkipSpace();
                        this.canBeFunction = this.CurerntChar == '(';
                    }
                    break;
            }
            return true;
        }

        private double ScanFraction()
        {
            int startIndex = this.xpathExprIndex - 2;
            int length = 1;
            while (XmlCharType.IsDigit(this.CurerntChar))
            {
                this.NextChar();
                length++;
            }
            return XmlConvert.ToXPathDouble(this.xpathExpr.Substring(startIndex, length));
        }

        private string ScanName()
        {
            int startIndex = this.xpathExprIndex - 1;
            int length = 0;
            while (true)
            {
                if (!this.xmlCharType.IsNCNameSingleChar(this.CurerntChar))
                {
                    break;
                }
                this.NextChar();
                length++;
            }
            return this.xpathExpr.Substring(startIndex, length);
        }

        private double ScanNumber()
        {
            int startIndex = this.xpathExprIndex - 1;
            int length = 0;
            while (XmlCharType.IsDigit(this.CurerntChar))
            {
                this.NextChar();
                length++;
            }
            if (this.CurerntChar == '.')
            {
                this.NextChar();
                length++;
                while (XmlCharType.IsDigit(this.CurerntChar))
                {
                    this.NextChar();
                    length++;
                }
            }
            return XmlConvert.ToXPathDouble(this.xpathExpr.Substring(startIndex, length));
        }

        private string ScanString()
        {
            char curerntChar = this.CurerntChar;
            this.NextChar();
            int startIndex = this.xpathExprIndex - 1;
            int length = 0;
            while (this.CurerntChar != curerntChar)
            {
                if (!this.NextChar())
                {
                    throw XPathException.Create("Xp_UnclosedString");
                }
                length++;
            }
            this.NextChar();
            return this.xpathExpr.Substring(startIndex, length);
        }

        private void SkipSpace()
        {
            while (this.xmlCharType.IsWhiteSpace(this.CurerntChar) && this.NextChar())
            {
            }
        }

        public bool CanBeFunction
        {
            get
            {
                return this.canBeFunction;
            }
        }

        private char CurerntChar
        {
            get
            {
                return this.currentChar;
            }
        }

        public LexKind Kind
        {
            get
            {
                return this.kind;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public double NumberValue
        {
            get
            {
                return this.numberValue;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public string SourceText
        {
            get
            {
                return this.xpathExpr;
            }
        }

        public string StringValue
        {
            get
            {
                return this.stringValue;
            }
        }

        public enum LexKind
        {
            And = 0x41,
            Apos = 0x27,
            At = 0x40,
            Axe = 0x61,
            Bang = 0x21,
            Comma = 0x2c,
            Dollar = 0x24,
            Dot = 0x2e,
            DotDot = 0x44,
            Eof = 0x45,
            Eq = 0x3d,
            Ge = 0x47,
            Gt = 0x3e,
            LBracket = 0x5b,
            Le = 0x4c,
            LParens = 40,
            Lt = 60,
            Minus = 0x2d,
            Name = 110,
            Ne = 0x4e,
            Number = 100,
            Or = 0x4f,
            Plus = 0x2b,
            Quote = 0x22,
            RBracket = 0x5d,
            RParens = 0x29,
            Slash = 0x2f,
            SlashSlash = 0x53,
            Star = 0x2a,
            String = 0x73,
            Union = 0x7c
        }
    }
}

