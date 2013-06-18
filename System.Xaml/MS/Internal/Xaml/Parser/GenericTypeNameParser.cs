namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xaml;
    using System.Xaml.Schema;

    internal class GenericTypeNameParser
    {
        private string _inputText;
        private Func<string, string> _prefixResolver;
        private GenericTypeNameScanner _scanner;
        private Stack<TypeNameFrame> _stack;

        public GenericTypeNameParser(Func<string, string> prefixResolver)
        {
            this._prefixResolver = prefixResolver;
        }

        private void Callout_EndOfType()
        {
            TypeNameFrame frame = this._stack.Pop();
            XamlTypeName item = new XamlTypeName(frame.Namespace, frame.Name, frame.TypeArgs);
            frame = this._stack.Peek();
            if (frame.TypeArgs == null)
            {
                frame.AllocateTypeArgs();
            }
            frame.TypeArgs.Add(item);
        }

        private void Callout_FoundName(string prefix, string name)
        {
            TypeNameFrame item = new TypeNameFrame {
                Name = name
            };
            string str = this._prefixResolver(prefix);
            if (str == null)
            {
                throw new TypeNameParserException(System.Xaml.SR.Get("PrefixNotFound", new object[] { prefix }));
            }
            item.Namespace = str;
            this._stack.Push(item);
        }

        private void Callout_Subscript(string subscript)
        {
            TypeNameFrame local1 = this._stack.Peek();
            local1.Name = local1.Name + subscript;
        }

        private XamlTypeName CollectNameFromStack()
        {
            if (this._stack.Count != 1)
            {
                throw new TypeNameParserException(System.Xaml.SR.Get("InvalidTypeString", new object[] { this._inputText }));
            }
            TypeNameFrame frame = this._stack.Peek();
            if (frame.TypeArgs.Count != 1)
            {
                throw new TypeNameParserException(System.Xaml.SR.Get("InvalidTypeString", new object[] { this._inputText }));
            }
            return frame.TypeArgs[0];
        }

        private IList<XamlTypeName> CollectNameListFromStack()
        {
            if (this._stack.Count != 1)
            {
                throw new TypeNameParserException(System.Xaml.SR.Get("InvalidTypeString", new object[] { this._inputText }));
            }
            return this._stack.Peek().TypeArgs;
        }

        private void P_NameListExt()
        {
            this._scanner.Read();
            this.P_XamlTypeName();
        }

        private void P_RepeatingSubscript()
        {
            do
            {
                this.Callout_Subscript(this._scanner.MultiCharTokenText);
                this._scanner.Read();
            }
            while (this._scanner.Token == GenericTypeNameScannerToken.SUBSCRIPT);
        }

        private void P_SimpleTypeName()
        {
            string prefix = string.Empty;
            string multiCharTokenText = this._scanner.MultiCharTokenText;
            this._scanner.Read();
            if (this._scanner.Token == GenericTypeNameScannerToken.COLON)
            {
                prefix = multiCharTokenText;
                this._scanner.Read();
                if (this._scanner.Token != GenericTypeNameScannerToken.NAME)
                {
                    this.ThrowOnBadInput();
                }
                multiCharTokenText = this._scanner.MultiCharTokenText;
                this._scanner.Read();
            }
            this.Callout_FoundName(prefix, multiCharTokenText);
        }

        private void P_TypeParameters()
        {
            this._scanner.Read();
            this.P_XamlTypeNameList();
            if (this._scanner.Token != GenericTypeNameScannerToken.CLOSE)
            {
                this.ThrowOnBadInput();
            }
            this._scanner.Read();
        }

        private void P_XamlTypeName()
        {
            if (this._scanner.Token != GenericTypeNameScannerToken.NAME)
            {
                this.ThrowOnBadInput();
            }
            this.P_SimpleTypeName();
            if (this._scanner.Token == GenericTypeNameScannerToken.OPEN)
            {
                this.P_TypeParameters();
            }
            if (this._scanner.Token == GenericTypeNameScannerToken.SUBSCRIPT)
            {
                this.P_RepeatingSubscript();
            }
            this.Callout_EndOfType();
        }

        private void P_XamlTypeNameList()
        {
            this.P_XamlTypeName();
            while (this._scanner.Token == GenericTypeNameScannerToken.COMMA)
            {
                this.P_NameListExt();
            }
        }

        public static XamlTypeName ParseIfTrivalName(string text, Func<string, string> prefixResolver, out string error)
        {
            string str;
            string str2;
            int index = text.IndexOf('(');
            int num2 = text.IndexOf('[');
            if ((index != -1) || (num2 != -1))
            {
                error = string.Empty;
                return null;
            }
            error = string.Empty;
            if (!XamlQualifiedName.Parse(text, out str, out str2))
            {
                error = System.Xaml.SR.Get("InvalidTypeString", new object[] { text });
                return null;
            }
            string str3 = prefixResolver(str);
            if (string.IsNullOrEmpty(str3))
            {
                error = System.Xaml.SR.Get("PrefixNotFound", new object[] { str });
                return null;
            }
            return new XamlTypeName(str3, str2);
        }

        public IList<XamlTypeName> ParseList(string text, out string error)
        {
            this._scanner = new GenericTypeNameScanner(text);
            this._inputText = text;
            this.StartStack();
            error = string.Empty;
            try
            {
                this._scanner.Read();
                this.P_XamlTypeNameList();
                if (this._scanner.Token != GenericTypeNameScannerToken.NONE)
                {
                    this.ThrowOnBadInput();
                }
            }
            catch (TypeNameParserException exception)
            {
                error = exception.Message;
            }
            IList<XamlTypeName> list = null;
            if (string.IsNullOrEmpty(error))
            {
                list = this.CollectNameListFromStack();
            }
            return list;
        }

        public XamlTypeName ParseName(string text, out string error)
        {
            error = string.Empty;
            this._scanner = new GenericTypeNameScanner(text);
            this._inputText = text;
            this.StartStack();
            try
            {
                this._scanner.Read();
                this.P_XamlTypeName();
                if (this._scanner.Token != GenericTypeNameScannerToken.NONE)
                {
                    this.ThrowOnBadInput();
                }
            }
            catch (TypeNameParserException exception)
            {
                error = exception.Message;
            }
            XamlTypeName name = null;
            if (string.IsNullOrEmpty(error))
            {
                name = this.CollectNameFromStack();
            }
            return name;
        }

        private void StartStack()
        {
            this._stack = new Stack<TypeNameFrame>();
            TypeNameFrame item = new TypeNameFrame();
            this._stack.Push(item);
        }

        private void ThrowOnBadInput()
        {
            throw new TypeNameParserException(System.Xaml.SR.Get("InvalidCharInTypeName", new object[] { this._scanner.ErrorCurrentChar, this._inputText }));
        }

        [Serializable]
        private class TypeNameParserException : Exception
        {
            public TypeNameParserException(string message) : base(message)
            {
            }

            protected TypeNameParserException(SerializationInfo si, StreamingContext sc) : base(si, sc)
            {
            }
        }
    }
}

