namespace MS.Internal.Xaml.Parser
{
    using MS.Internal.Xaml.Context;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xaml;

    internal class MePullParser
    {
        private string _brokenRule;
        private XamlParserContext _context;
        private string _originalText;
        private MeScanner _tokenizer;

        public MePullParser(XamlParserContext stack)
        {
            this._context = stack;
        }

        private bool Expect(MeTokenType token, string ruleString)
        {
            if (this._tokenizer.Token != token)
            {
                this.SetBrokenRuleString(ruleString);
                return false;
            }
            return true;
        }

        private XamlNode Logic_EndMember()
        {
            this._context.CurrentMember = null;
            return new XamlNode(XamlNodeType.EndMember);
        }

        private XamlNode Logic_EndObject()
        {
            this._context.PopScope();
            return new XamlNode(XamlNodeType.EndObject);
        }

        private XamlNode Logic_EndPositionalParameters()
        {
            XamlType currentType = this._context.CurrentType;
            this._context.CurrentArgCount = 0;
            this._context.CurrentMember = null;
            return new XamlNode(XamlNodeType.EndMember);
        }

        private XamlNode Logic_StartElement(XamlType xamlType, string xamlNamespace)
        {
            this._context.PushScope();
            this._context.CurrentType = xamlType;
            this._context.CurrentTypeNamespace = xamlNamespace;
            return new XamlNode(XamlNodeType.StartObject, xamlType);
        }

        private XamlNode Logic_StartMember()
        {
            XamlMember tokenProperty = this._tokenizer.TokenProperty;
            this._context.CurrentMember = tokenProperty;
            return new XamlNode(XamlNodeType.StartMember, tokenProperty);
        }

        private XamlNode Logic_StartPositionalParameters()
        {
            this._context.CurrentMember = XamlLanguage.PositionalParameters;
            return new XamlNode(XamlNodeType.StartMember, XamlLanguage.PositionalParameters);
        }

        private XamlNode Logic_Text()
        {
            return new XamlNode(XamlNodeType.Value, this._tokenizer.TokenText);
        }

        private void NextToken()
        {
            this._tokenizer.Read();
        }

        private IEnumerable<XamlNode> P_Arguments(Found f)
        {
            Found iteratorVariable0 = new Found();
            switch (this._tokenizer.Token)
            {
                case MeTokenType.PropertyName:
                {
                    IEnumerator<XamlNode> enumerator = this.P_NamedArgs(iteratorVariable0).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        XamlNode current = enumerator.Current;
                        yield return current;
                    }
                    f.found = iteratorVariable0.found;
                    break;
                }
                case MeTokenType.Close:
                    break;

                default:
                    this.SetBrokenRuleString("Arguments ::= @ (PositionalArgs ( ',' NamedArgs)?) | NamedArgs");
                    break;
                    foreach (XamlNode iteratorVariable1 in this.P_PositionalArgs(iteratorVariable0))
                    {
                        yield return iteratorVariable1;
                    }
                    f.found = iteratorVariable0.found;
                    if (f.found && (this._context.CurrentArgCount > 0))
                    {
                        yield return this.Logic_EndPositionalParameters();
                    }
                    while (this._tokenizer.Token == MeTokenType.Comma)
                    {
                        this.NextToken();
                        foreach (XamlNode iteratorVariable2 in this.P_NamedArgs(iteratorVariable0))
                        {
                            yield return iteratorVariable2;
                        }
                    }
                    break;
            }
        }

        private IEnumerable<XamlNode> P_MarkupExtension(Found f)
        {
            if (this.Expect(MeTokenType.Open, "MarkupExtension ::= @'{' Expr '}'"))
            {
                this.NextToken();
                if (this._tokenizer.Token != MeTokenType.TypeName)
                {
                    this.SetBrokenRuleString("MarkupExtension ::= '{' @TYPENAME (Arguments)? '}'");
                }
                else
                {
                    XamlType tokenType = this._tokenizer.TokenType;
                    yield return this.Logic_StartElement(tokenType, this._tokenizer.Namespace);
                    this.NextToken();
                    Found iteratorVariable1 = new Found();
                    switch (this._tokenizer.Token)
                    {
                        case MeTokenType.PropertyName:
                        case MeTokenType.String:
                        case MeTokenType.QuotedMarkupExtension:
                        case MeTokenType.Open:
                        {
                            IEnumerator<XamlNode> enumerator = this.P_Arguments(iteratorVariable1).GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                XamlNode current = enumerator.Current;
                                yield return current;
                            }
                            break;
                        }
                        default:
                            this.SetBrokenRuleString("MarkupExtension ::= '{' TYPENAME @(Arguments)? '}'");
                            break;
                            yield return this.Logic_EndObject();
                            this.NextToken();
                            f.found = true;
                            break;
                    }
                    if (iteratorVariable1.found && this.Expect(MeTokenType.Close, "MarkupExtension ::= '{' TYPENAME (Arguments)? @'}'"))
                    {
                        yield return this.Logic_EndObject();
                        f.found = true;
                        this.NextToken();
                    }
                }
            }
        }

        private IEnumerable<XamlNode> P_NamedArg(Found f)
        {
            Found iteratorVariable0 = new Found();
            if (this._tokenizer.Token == MeTokenType.PropertyName)
            {
                XamlMember tokenProperty = this._tokenizer.TokenProperty;
                yield return this.Logic_StartMember();
                this.NextToken();
                this.Expect(MeTokenType.EqualSign, "NamedArg ::= PROPERTYNAME @'=' Value");
                this.NextToken();
                switch (this._tokenizer.Token)
                {
                    case MeTokenType.PropertyName:
                        string str;
                        if (this._context.CurrentMember == null)
                        {
                            str = System.Xaml.SR.Get("MissingComma1", new object[] { this._tokenizer.TokenText });
                        }
                        else
                        {
                            str = System.Xaml.SR.Get("MissingComma2", new object[] { this._context.CurrentMember.Name, this._tokenizer.TokenText });
                        }
                        throw new XamlParseException(this._tokenizer, str);

                    case MeTokenType.QuotedMarkupExtension:
                    {
                        MePullParser iteratorVariable1 = new MePullParser(this._context);
                        foreach (XamlNode iteratorVariable2 in iteratorVariable1.Parse(this._tokenizer.TokenText, this.LineNumber, this.LinePosition))
                        {
                            yield return iteratorVariable2;
                        }
                        f.found = true;
                        this.NextToken();
                        break;
                    }
                    case MeTokenType.Open:
                    {
                        IEnumerator<XamlNode> enumerator = this.P_Value(iteratorVariable0).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            XamlNode current = enumerator.Current;
                            yield return current;
                        }
                        f.found = iteratorVariable0.found;
                        break;
                    }
                    default:
                        this.SetBrokenRuleString("NamedArg ::= PROPERTYNAME '=' @(STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)");
                        break;
                        yield return this.Logic_Text();
                        f.found = true;
                        this.NextToken();
                        break;
                }
                yield return this.Logic_EndMember();
            }
        }

        private IEnumerable<XamlNode> P_NamedArgs(Found f)
        {
            Found iteratorVariable0 = new Found();
            if (this._tokenizer.Token == MeTokenType.PropertyName)
            {
                foreach (XamlNode iteratorVariable1 in this.P_NamedArg(iteratorVariable0))
                {
                    yield return iteratorVariable1;
                }
                f.found = iteratorVariable0.found;
                while (this._tokenizer.Token == MeTokenType.Comma)
                {
                    this.NextToken();
                    foreach (XamlNode iteratorVariable2 in this.P_NamedArg(iteratorVariable0))
                    {
                        yield return iteratorVariable2;
                    }
                }
            }
            else
            {
                this.SetBrokenRuleString("NamedArgs ::= @NamedArg ( ',' NamedArg )*");
            }
        }

        private IEnumerable<XamlNode> P_PositionalArgs(Found f)
        {
            Found iteratorVariable0 = new Found();
            switch (this._tokenizer.Token)
            {
                case MeTokenType.PropertyName:
                {
                    if (this._context.CurrentArgCount > 0)
                    {
                        yield return this.Logic_EndPositionalParameters();
                    }
                    IEnumerator<XamlNode> enumerator = this.P_NamedArg(iteratorVariable0).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        XamlNode current = enumerator.Current;
                        yield return current;
                    }
                    if (!iteratorVariable0.found)
                    {
                        this.SetBrokenRuleString("PositionalArgs ::= (Value (',' PositionalArgs)?) | @ NamedArg");
                    }
                    f.found = iteratorVariable0.found;
                    break;
                }
                default:
                {
                    int num2;
                    this.SetBrokenRuleString("PositionalArgs ::= @ (Value (',' PositionalArgs)?) | NamedArg");
                    break;
                    this._context.CurrentArgCount = (num2 = this._context.CurrentArgCount) + 1;
                    if (num2 == 0)
                    {
                        yield return this.Logic_StartPositionalParameters();
                    }
                    IEnumerator<XamlNode> iteratorVariable5 = this.P_Value(iteratorVariable0).GetEnumerator();
                    while (iteratorVariable5.MoveNext())
                    {
                        XamlNode iteratorVariable1 = iteratorVariable5.Current;
                        yield return iteratorVariable1;
                    }
                    if (!iteratorVariable0.found)
                    {
                        this.SetBrokenRuleString("PositionalArgs ::= (NamedArg | (@Value (',' PositionalArgs)?)");
                    }
                    else
                    {
                        f.found = iteratorVariable0.found;
                        if (this._tokenizer.Token == MeTokenType.Comma)
                        {
                            Found iteratorVariable2 = new Found();
                            this.NextToken();
                            foreach (XamlNode iteratorVariable3 in this.P_PositionalArgs(iteratorVariable2))
                            {
                                yield return iteratorVariable3;
                            }
                            if (!iteratorVariable2.found)
                            {
                                this.SetBrokenRuleString("PositionalArgs ::= (Value (',' @ PositionalArgs)?) | NamedArg");
                            }
                        }
                    }
                    break;
                }
            }
        }

        private IEnumerable<XamlNode> P_Value(Found f)
        {
            Found iteratorVariable0 = new Found();
            switch (this._tokenizer.Token)
            {
                case MeTokenType.QuotedMarkupExtension:
                {
                    MePullParser iteratorVariable1 = new MePullParser(this._context);
                    foreach (XamlNode iteratorVariable2 in iteratorVariable1.Parse(this._tokenizer.TokenText, this.LineNumber, this.LinePosition))
                    {
                        yield return iteratorVariable2;
                    }
                    f.found = true;
                    this.NextToken();
                    break;
                }
                case MeTokenType.Open:
                {
                    IEnumerator<XamlNode> enumerator = this.P_MarkupExtension(iteratorVariable0).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        XamlNode current = enumerator.Current;
                        yield return current;
                    }
                    f.found = iteratorVariable0.found;
                    break;
                }
                default:
                    break;
                    yield return this.Logic_Text();
                    f.found = true;
                    this.NextToken();
                    break;
            }
        }

        public IEnumerable<XamlNode> Parse(string text, int lineNumber, int linePosition)
        {
            this._tokenizer = new MeScanner(this._context, text, lineNumber, linePosition);
            this._originalText = text;
            Found f = new Found();
            this.NextToken();
            foreach (XamlNode iteratorVariable1 in this.P_MarkupExtension(f))
            {
                yield return iteratorVariable1;
            }
            if (!f.found)
            {
                string message = this._brokenRule;
                this._brokenRule = null;
                throw new XamlParseException(this._tokenizer, message);
            }
            if (this._tokenizer.Token != MeTokenType.None)
            {
                throw new XamlParseException(this._tokenizer, System.Xaml.SR.Get("UnexpectedTokenAfterME"));
            }
            if (this._tokenizer.HasTrailingWhitespace)
            {
                throw new XamlParseException(this._tokenizer, System.Xaml.SR.Get("WhitespaceAfterME"));
            }
        }

        private void SetBrokenRuleString(string ruleString)
        {
            if (string.IsNullOrEmpty(this._brokenRule))
            {
                this._brokenRule = System.Xaml.SR.Get("UnexpectedToken", new object[] { this._tokenizer.Token, ruleString, this._originalText });
            }
        }

        private int LineNumber
        {
            get
            {
                return this._tokenizer.LineNumber;
            }
        }

        private int LinePosition
        {
            get
            {
                return this._tokenizer.LinePosition;
            }
        }








        [DebuggerDisplay("{found}")]
        private class Found
        {
            public bool found;
        }
    }
}

