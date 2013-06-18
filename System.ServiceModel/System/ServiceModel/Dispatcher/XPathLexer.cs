namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.ServiceModel;

    internal class XPathLexer
    {
        private char ch;
        private int currChar;
        private static Hashtable namedTypes = new Hashtable();
        private XPathTokenID previousID;
        private bool resolveKeywords;
        private XPathToken token;
        private int tokenStart;
        private string xpath;
        private int xpathLength;

        static XPathLexer()
        {
            namedTypes.Add("and", XPathTokenID.And);
            namedTypes.Add("or", XPathTokenID.Or);
            namedTypes.Add("mod", XPathTokenID.Mod);
            namedTypes.Add("div", XPathTokenID.Div);
            namedTypes.Add("ancestor", XPathTokenID.Ancestor);
            namedTypes.Add("ancestor-or-self", XPathTokenID.AncestorOrSelf);
            namedTypes.Add("attribute", XPathTokenID.Attribute);
            namedTypes.Add("child", XPathTokenID.Child);
            namedTypes.Add("descendant", XPathTokenID.Descendant);
            namedTypes.Add("descendant-or-self", XPathTokenID.DescendantOrSelf);
            namedTypes.Add("following", XPathTokenID.Following);
            namedTypes.Add("following-sibling", XPathTokenID.FollowingSibling);
            namedTypes.Add("namespace", XPathTokenID.Namespace);
            namedTypes.Add("parent", XPathTokenID.Parent);
            namedTypes.Add("preceding", XPathTokenID.Preceding);
            namedTypes.Add("preceding-sibling", XPathTokenID.PrecedingSibling);
            namedTypes.Add("self", XPathTokenID.Self);
            namedTypes.Add("comment", XPathTokenID.Comment);
            namedTypes.Add("text", XPathTokenID.Text);
            namedTypes.Add("processing-instruction", XPathTokenID.Processing);
            namedTypes.Add("node", XPathTokenID.Node);
        }

        internal XPathLexer(string xpath) : this(xpath, true)
        {
        }

        internal XPathLexer(string xpath, bool resolveKeywords)
        {
            this.resolveKeywords = resolveKeywords;
            this.xpath = string.Copy(xpath);
            this.xpathLength = this.xpath.Length;
            this.tokenStart = 0;
            this.currChar = 0;
            this.ch = '\0';
            this.previousID = XPathTokenID.Unknown;
            this.token = new XPathToken();
            this.ConsumeWhitespace();
        }

        private bool AdvanceChar()
        {
            if (this.currChar < this.xpathLength)
            {
                this.ch = this.xpath[this.currChar];
                this.currChar++;
                return true;
            }
            if (this.currChar == this.xpathLength)
            {
                this.currChar++;
                this.ch = '\0';
            }
            return false;
        }

        internal string ConsumedSubstring()
        {
            return this.xpath.Substring(0, this.tokenStart);
        }

        private void ConsumeToken()
        {
            this.tokenStart = this.currChar;
        }

        private void ConsumeWhitespace()
        {
            while (XPathCharTypes.IsWhitespace(this.PeekChar()))
            {
                this.AdvanceChar();
            }
            this.ConsumeToken();
        }

        private string CurrentSubstring()
        {
            return this.xpath.Substring(this.tokenStart, this.currChar - this.tokenStart);
        }

        private XPathTokenID GetAxisName(XPathParser.QName qname)
        {
            if (qname.Prefix.Length != 0)
            {
                this.ThrowError(QueryCompileError.InvalidAxisSpecifier, qname.Prefix + ":" + qname.Name);
            }
            XPathTokenID namedType = this.GetNamedType(qname.Name);
            if (this.resolveKeywords && ((namedType & XPathTokenID.Axis) == XPathTokenID.Unknown))
            {
                this.ThrowError(QueryCompileError.UnsupportedAxis, qname.Name);
            }
            return namedType;
        }

        private XPathTokenID GetNamedOperator(XPathParser.QName qname)
        {
            if (qname.Prefix.Length != 0)
            {
                this.ThrowError(QueryCompileError.InvalidOperatorName, qname.Prefix + ":" + qname.Name);
            }
            XPathTokenID namedType = this.GetNamedType(qname.Name);
            if (this.resolveKeywords && ((namedType & XPathTokenID.NamedOperator) == XPathTokenID.Unknown))
            {
                this.ThrowError(QueryCompileError.UnsupportedOperator, this.previousID.ToString() + "->" + qname.Name);
            }
            return namedType;
        }

        private XPathTokenID GetNamedType(string name)
        {
            if (this.resolveKeywords && namedTypes.ContainsKey(name))
            {
                return (XPathTokenID) namedTypes[name];
            }
            return XPathTokenID.Unknown;
        }

        private string GetNCName()
        {
            if (!XPathCharTypes.IsNCNameStart(this.PeekChar()))
            {
                return null;
            }
            this.AdvanceChar();
            while (XPathCharTypes.IsNCName(this.PeekChar()))
            {
                this.AdvanceChar();
            }
            string str = this.CurrentSubstring();
            this.ConsumeToken();
            return str;
        }

        private XPathTokenID GetNodeTypeOrFunction(XPathParser.QName qname)
        {
            XPathTokenID namedType = this.GetNamedType(qname.Name);
            if ((namedType & XPathTokenID.NodeType) == XPathTokenID.Unknown)
            {
                return XPathTokenID.Function;
            }
            if (qname.Prefix.Length > 0)
            {
                this.ThrowError(QueryCompileError.InvalidNodeType, qname.Prefix + ":" + qname.Name);
            }
            return namedType;
        }

        private XPathParser.QName GetQName()
        {
            string nCName = this.GetNCName();
            if (nCName == null)
            {
                return new XPathParser.QName(string.Empty, string.Empty);
            }
            if (nCName[0] == '$')
            {
                nCName = nCName.Substring(1);
            }
            if ((this.PeekChar() == ':') && XPathCharTypes.IsNCNameStart(this.PeekChar(2)))
            {
                this.AdvanceChar();
                this.ConsumeToken();
                return new XPathParser.QName(nCName, this.GetNCName());
            }
            return new XPathParser.QName(string.Empty, nCName);
        }

        private bool IsSpecialPrev()
        {
            return (((((this.previousID != XPathTokenID.Unknown) && (this.previousID != XPathTokenID.AtSign)) && ((this.previousID != XPathTokenID.DblColon) && (this.previousID != XPathTokenID.LParen))) && (((this.previousID != XPathTokenID.LBracket) && (this.previousID != XPathTokenID.Comma)) && ((this.previousID & XPathTokenID.Operator) == XPathTokenID.Unknown))) && ((this.previousID & XPathTokenID.NamedOperator) == XPathTokenID.Unknown));
        }

        internal bool MoveNext()
        {
            this.previousID = this.token.TokenID;
            if (!this.AdvanceChar())
            {
                return false;
            }
            if (XPathCharTypes.IsNCNameStart(this.ch))
            {
                this.TokenizeQName();
            }
            else if (XPathCharTypes.IsDigit(this.ch))
            {
                this.TokenizeNumber();
            }
            else
            {
                switch (this.ch)
                {
                    case '!':
                        if (this.PeekChar() != '=')
                        {
                            this.ThrowError(QueryCompileError.UnsupportedOperator, this.CurrentSubstring());
                        }
                        else
                        {
                            this.AdvanceChar();
                            this.token.Set(XPathTokenID.Neq);
                        }
                        goto Label_03FB;

                    case '"':
                        this.TokenizeLiteral('"');
                        goto Label_03FB;

                    case '$':
                    {
                        XPathParser.QName qName = this.GetQName();
                        if ((qName.Prefix.Length == 0) && (qName.Name.Length == 0))
                        {
                            this.AdvanceChar();
                            this.ThrowError(QueryCompileError.InvalidVariable, (this.ch == '\0') ? string.Empty : this.CurrentSubstring());
                        }
                        this.token.Set(XPathTokenID.Variable, qName);
                        goto Label_03FB;
                    }
                    case '\'':
                        this.TokenizeLiteral('\'');
                        goto Label_03FB;

                    case '(':
                        this.token.Set(XPathTokenID.LParen);
                        goto Label_03FB;

                    case ')':
                        this.token.Set(XPathTokenID.RParen);
                        goto Label_03FB;

                    case '*':
                        if (!this.IsSpecialPrev())
                        {
                            this.token.Set(XPathTokenID.Wildcard, new XPathParser.QName(string.Empty, QueryDataModel.Wildcard));
                        }
                        else
                        {
                            this.token.Set(XPathTokenID.Multiply);
                        }
                        goto Label_03FB;

                    case '+':
                        this.token.Set(XPathTokenID.Plus);
                        goto Label_03FB;

                    case ',':
                        this.token.Set(XPathTokenID.Comma);
                        goto Label_03FB;

                    case '-':
                        this.token.Set(XPathTokenID.Minus);
                        goto Label_03FB;

                    case '.':
                        if (this.PeekChar() != '.')
                        {
                            if (XPathCharTypes.IsDigit(this.PeekChar()))
                            {
                                this.TokenizeNumber();
                            }
                            else
                            {
                                this.token.Set(XPathTokenID.Period);
                            }
                        }
                        else
                        {
                            this.AdvanceChar();
                            this.token.Set(XPathTokenID.DblPeriod);
                        }
                        goto Label_03FB;

                    case '/':
                        if (this.PeekChar() != '/')
                        {
                            this.token.Set(XPathTokenID.Slash);
                        }
                        else
                        {
                            this.AdvanceChar();
                            this.token.Set(XPathTokenID.DblSlash);
                        }
                        goto Label_03FB;

                    case ':':
                        if (this.PeekChar() != ':')
                        {
                            this.ThrowError(QueryCompileError.UnexpectedToken, this.CurrentSubstring());
                        }
                        else
                        {
                            this.AdvanceChar();
                            this.token.Set(XPathTokenID.DblColon);
                        }
                        goto Label_03FB;

                    case '<':
                        if (this.PeekChar() != '=')
                        {
                            this.token.Set(XPathTokenID.Lt);
                        }
                        else
                        {
                            this.AdvanceChar();
                            this.token.Set(XPathTokenID.Lte);
                        }
                        goto Label_03FB;

                    case '=':
                        this.token.Set(XPathTokenID.Eq);
                        goto Label_03FB;

                    case '>':
                        if (this.PeekChar() != '=')
                        {
                            this.token.Set(XPathTokenID.Gt);
                        }
                        else
                        {
                            this.AdvanceChar();
                            this.token.Set(XPathTokenID.Gte);
                        }
                        goto Label_03FB;

                    case '@':
                        this.token.Set(XPathTokenID.AtSign);
                        goto Label_03FB;

                    case '[':
                        this.token.Set(XPathTokenID.LBracket);
                        goto Label_03FB;

                    case ']':
                        this.token.Set(XPathTokenID.RBracket);
                        goto Label_03FB;

                    case '|':
                        this.token.Set(XPathTokenID.Pipe);
                        goto Label_03FB;
                }
                this.token.Set(XPathTokenID.Unknown);
            }
        Label_03FB:
            this.ConsumeWhitespace();
            return true;
        }

        private char PeekChar()
        {
            return this.PeekChar(1);
        }

        private char PeekChar(int offset)
        {
            int num = (this.currChar + offset) - 1;
            if (num < this.xpathLength)
            {
                return this.xpath[num];
            }
            return '\0';
        }

        private void PutbackChar()
        {
            if (this.currChar > this.tokenStart)
            {
                this.currChar--;
            }
        }

        private void ThrowError(QueryCompileError err, string msg)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(err, msg));
        }

        private void TokenizeLiteral(char c)
        {
            this.ConsumeToken();
            this.AdvanceChar();
            while (this.ch != c)
            {
                if (this.ch == '\0')
                {
                    this.PutbackChar();
                    this.ThrowError(QueryCompileError.InvalidLiteral, this.CurrentSubstring());
                }
                this.AdvanceChar();
            }
            this.PutbackChar();
            this.token.Set(XPathTokenID.Literal, this.CurrentSubstring());
            this.AdvanceChar();
        }

        private void TokenizeNumber()
        {
            XPathTokenID integer = XPathTokenID.Integer;
            while (XPathCharTypes.IsDigit(this.ch))
            {
                this.AdvanceChar();
            }
            if (this.ch == '.')
            {
                this.AdvanceChar();
                if (XPathCharTypes.IsDigit(this.ch))
                {
                    integer = XPathTokenID.Decimal;
                    while (XPathCharTypes.IsDigit(this.ch))
                    {
                        this.AdvanceChar();
                    }
                }
            }
            this.PutbackChar();
            double number = QueryValueModel.Double(this.CurrentSubstring());
            this.token.Set(integer, number);
        }

        private void TokenizeQName()
        {
            while (XPathCharTypes.IsNCName(this.PeekChar()))
            {
                this.AdvanceChar();
            }
            string prefix = this.CurrentSubstring();
            XPathTokenID unknown = XPathTokenID.Unknown;
            XPathParser.QName qname = new XPathParser.QName("", "");
            if ((this.PeekChar() == ':') && (this.PeekChar(2) != ':'))
            {
                this.AdvanceChar();
                this.ConsumeToken();
                this.AdvanceChar();
                if (XPathCharTypes.IsNCNameStart(this.ch))
                {
                    while (XPathCharTypes.IsNCName(this.PeekChar()))
                    {
                        this.AdvanceChar();
                    }
                    unknown = XPathTokenID.NameTest;
                    qname = new XPathParser.QName(prefix, this.CurrentSubstring());
                }
                else if (this.ch == '*')
                {
                    unknown = XPathTokenID.NameWildcard;
                    qname = new XPathParser.QName(prefix, QueryDataModel.Wildcard);
                }
                else
                {
                    this.ThrowError(QueryCompileError.InvalidNCName, (this.ch == '\0') ? "" : this.CurrentSubstring());
                }
            }
            else
            {
                unknown = XPathTokenID.NameTest;
                qname = new XPathParser.QName(string.Empty, prefix);
            }
            this.ConsumeWhitespace();
            if (this.IsSpecialPrev())
            {
                this.token.Set(this.GetNamedOperator(qname));
            }
            else if (qname.Prefix.Length == 0)
            {
                if (this.PeekChar() == '(')
                {
                    unknown = this.GetNodeTypeOrFunction(qname);
                    if (unknown != XPathTokenID.Function)
                    {
                        this.token.Set(unknown);
                    }
                    else
                    {
                        this.token.Set(unknown, qname);
                    }
                }
                else if ((this.PeekChar() == ':') && (this.PeekChar(2) == ':'))
                {
                    this.token.Set(this.GetAxisName(qname));
                }
                else
                {
                    this.token.Set(unknown, qname);
                }
            }
            else
            {
                if (this.PeekChar() == '(')
                {
                    unknown = XPathTokenID.Function;
                }
                this.token.Set(unknown, qname);
            }
        }

        internal int FirstTokenChar
        {
            get
            {
                return this.tokenStart;
            }
        }

        internal XPathToken Token
        {
            get
            {
                return this.token;
            }
        }
    }
}

