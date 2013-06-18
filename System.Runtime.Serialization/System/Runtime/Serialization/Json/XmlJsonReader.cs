namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    internal class XmlJsonReader : XmlBaseReader, IXmlJsonReaderInitializer
    {
        private bool buffered;
        private byte[] charactersToSkipOnNextRead;
        private static byte[] charType = new byte[] { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 
            0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 3, 
            0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3
         };
        private JsonComplexTextMode complexTextMode = JsonComplexTextMode.None;
        private bool expectingFirstElementInNonPrimitiveChild;
        private int maxBytesPerRead;
        private const int MaxTextChunk = 0x800;
        private OnXmlDictionaryReaderClose onReaderClose;
        private bool readServerTypeElement;
        private int scopeDepth;
        private JsonNodeType[] scopes;

        private static int BreakText(byte[] buffer, int offset, int length)
        {
            if ((length > 0) && ((buffer[(offset + length) - 1] & 0x80) == 0x80))
            {
                int num = length;
                do
                {
                    length--;
                }
                while ((length > 0) && ((buffer[offset + length] & 0xc0) != 0xc0));
                if (length == 0)
                {
                    return num;
                }
                byte num2 = (byte) (buffer[offset + length] << 2);
                int num3 = 2;
                while ((num2 & 0x80) == 0x80)
                {
                    num2 = (byte) (num2 << 1);
                    num3++;
                    if (num3 > 4)
                    {
                        return num;
                    }
                }
                if ((length + num3) == num)
                {
                    return num;
                }
                if (length == 0)
                {
                    return num;
                }
            }
            return length;
        }

        private void BufferElement()
        {
            int offset = base.BufferReader.Offset;
            bool flag = false;
            byte num2 = 0;
            while (!flag)
            {
                int num3;
                int num4;
                byte[] buffer = base.BufferReader.GetBuffer(0x80, out num3, out num4);
                if ((num3 + 0x80) != num4)
                {
                    break;
                }
                for (int i = num3; (i < num4) && !flag; i++)
                {
                    byte num6 = buffer[i];
                    if (num6 == 0x5c)
                    {
                        i++;
                        if (i < num4)
                        {
                            continue;
                        }
                        break;
                    }
                    if (num2 == 0)
                    {
                        switch (num6)
                        {
                            case 0x27:
                            case 0x22:
                                num2 = num6;
                                break;
                        }
                        if (num6 == 0x3a)
                        {
                            flag = true;
                        }
                    }
                    else if (num6 == num2)
                    {
                        num2 = 0;
                    }
                }
                base.BufferReader.Advance(0x80);
            }
            base.BufferReader.Offset = offset;
        }

        internal static void CheckArray(Array array, int offset, int count)
        {
            if (array == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("array"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > array.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { array.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (array.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { array.Length - offset })));
            }
        }

        public override void Close()
        {
            base.Close();
            OnXmlDictionaryReaderClose onReaderClose = this.onReaderClose;
            this.onReaderClose = null;
            this.ResetState();
            if (onReaderClose != null)
            {
                try
                {
                    onReaderClose(this);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        private static int ComputeNumericalTextLength(byte[] buffer, int offset, int offsetMax)
        {
            int num = offset;
            while (offset < offsetMax)
            {
                byte ch = buffer[offset];
                if (((ch == 0x2c) || (ch == 0x7d)) || ((ch == 0x5d) || IsWhitespace(ch)))
                {
                    break;
                }
                offset++;
            }
            return (offset - num);
        }

        private static int ComputeQuotedTextLengthUntilEndQuote(byte[] buffer, int offset, int offsetMax, out bool escaped)
        {
            int num = offset;
            escaped = false;
            while (offset < offsetMax)
            {
                byte num2 = buffer[offset];
                if (num2 < 0x20)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("InvalidCharacterEncountered", new object[] { (char) num2 })));
                }
                if ((num2 == 0x5c) || (num2 == 0xef))
                {
                    escaped = true;
                    break;
                }
                if (num2 == 0x22)
                {
                    break;
                }
                offset++;
            }
            return (offset - num);
        }

        protected override XmlSigningNodeWriter CreateSigningNodeWriter()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("JsonMethodNotSupported", new object[] { "CreateSigningNodeWriter" })));
        }

        public override void EndCanonicalization()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        private void EnterJsonScope(JsonNodeType currentNodeType)
        {
            this.scopeDepth++;
            if (this.scopes == null)
            {
                this.scopes = new JsonNodeType[4];
            }
            else if (this.scopes.Length == this.scopeDepth)
            {
                JsonNodeType[] destinationArray = new JsonNodeType[this.scopeDepth * 2];
                Array.Copy(this.scopes, destinationArray, this.scopeDepth);
                this.scopes = destinationArray;
            }
            this.scopes[this.scopeDepth] = currentNodeType;
        }

        private JsonNodeType ExitJsonScope()
        {
            JsonNodeType type = this.scopes[this.scopeDepth];
            this.scopes[this.scopeDepth] = JsonNodeType.None;
            this.scopeDepth--;
            return type;
        }

        public override string GetAttribute(int index)
        {
            return this.UnescapeJsonString(base.GetAttribute(index));
        }

        public override string GetAttribute(string name)
        {
            if (name != "type")
            {
                return this.UnescapeJsonString(base.GetAttribute(name));
            }
            return base.GetAttribute(name);
        }

        public override string GetAttribute(string localName, string namespaceUri)
        {
            if (localName != "type")
            {
                return this.UnescapeJsonString(base.GetAttribute(localName, namespaceUri));
            }
            return base.GetAttribute(localName, namespaceUri);
        }

        public override string GetAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (XmlDictionaryString.GetString(localName) != "type")
            {
                return this.UnescapeJsonString(base.GetAttribute(localName, namespaceUri));
            }
            return base.GetAttribute(localName, namespaceUri);
        }

        private static bool IsWhitespace(byte ch)
        {
            if (((ch != 0x20) && (ch != 9)) && (ch != 10))
            {
                return (ch == 13);
            }
            return true;
        }

        private void MoveToEndElement()
        {
            this.ExitJsonScope();
            base.MoveToEndElement();
        }

        private void MoveToInitial(XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
        {
            base.MoveToInitial(quotas);
            this.maxBytesPerRead = quotas.MaxBytesPerRead;
            this.onReaderClose = onClose;
        }

        private void ParseAndSetLocalName()
        {
            XmlBaseReader.XmlElementNode elementNode = base.EnterScope();
            elementNode.NameOffset = base.BufferReader.Offset;
            do
            {
                if (base.BufferReader.GetByte() == 0x5c)
                {
                    this.ReadEscapedCharacter(false);
                }
                else
                {
                    this.ReadQuotedText(false);
                }
            }
            while (this.complexTextMode == JsonComplexTextMode.QuotedText);
            int num = base.BufferReader.Offset - 1;
            elementNode.LocalName.SetValue(elementNode.NameOffset, num - elementNode.NameOffset);
            elementNode.NameLength = num - elementNode.NameOffset;
            elementNode.Namespace.Uri.SetValue(elementNode.NameOffset, 0);
            elementNode.Prefix.SetValue(PrefixHandleType.Empty);
            elementNode.IsEmptyElement = false;
            elementNode.ExitScope = false;
            elementNode.BufferOffset = num;
            int @byte = base.BufferReader.GetByte(elementNode.NameOffset);
            if ((charType[@byte] & 1) == 0)
            {
                this.SetJsonNameWithMapping(elementNode);
            }
            else
            {
                int num3 = 0;
                for (int i = elementNode.NameOffset; num3 < elementNode.NameLength; i++)
                {
                    @byte = base.BufferReader.GetByte(i);
                    if (((charType[@byte] & 2) == 0) || (@byte >= 0x80))
                    {
                        this.SetJsonNameWithMapping(elementNode);
                        return;
                    }
                    num3++;
                }
            }
        }

        private static char ParseChar(string value, NumberStyles style)
        {
            char ch;
            int num = ParseInt(value, style);
            try
            {
                ch = Convert.ToChar(num);
            }
            catch (OverflowException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "char", exception));
            }
            return ch;
        }

        private static int ParseInt(string value, NumberStyles style)
        {
            int num;
            try
            {
                num = int.Parse(value, style, NumberFormatInfo.InvariantInfo);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int32", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int32", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int32", exception3));
            }
            return num;
        }

        private void ParseStartElement()
        {
            if (!this.buffered)
            {
                this.BufferElement();
            }
            this.expectingFirstElementInNonPrimitiveChild = false;
            byte @byte = base.BufferReader.GetByte();
            if (@byte == 0x22)
            {
                base.BufferReader.SkipByte();
                this.ParseAndSetLocalName();
                this.SkipWhitespaceInBufferReader();
                this.SkipExpectedByteInBufferReader(0x3a);
                this.SkipWhitespaceInBufferReader();
                if (base.BufferReader.GetByte() == 0x7b)
                {
                    base.BufferReader.SkipByte();
                    this.expectingFirstElementInNonPrimitiveChild = true;
                }
                this.ReadAttributes();
            }
            else
            {
                XmlExceptionHelper.ThrowTokenExpected(this, "\"", (char) @byte);
            }
        }

        public override bool Read()
        {
            byte @byte;
            if (base.Node.CanMoveToElement)
            {
                this.MoveToElement();
            }
            if (base.Node.ReadState == System.Xml.ReadState.Closed)
            {
                return false;
            }
            if (base.Node.ExitScope)
            {
                base.ExitScope();
            }
            if (!this.buffered)
            {
                base.BufferReader.SetWindow(base.ElementNode.BufferOffset, this.maxBytesPerRead);
            }
            if (!this.IsReadingComplexText)
            {
                this.SkipWhitespaceInBufferReader();
                if (this.TryGetByte(out @byte) && ((this.charactersToSkipOnNextRead[0] == @byte) || (this.charactersToSkipOnNextRead[1] == @byte)))
                {
                    base.BufferReader.SkipByte();
                    this.charactersToSkipOnNextRead[0] = 0;
                    this.charactersToSkipOnNextRead[1] = 0;
                }
                this.SkipWhitespaceInBufferReader();
                if ((this.TryGetByte(out @byte) && (@byte == 0x5d)) && this.IsReadingCollection)
                {
                    base.BufferReader.SkipByte();
                    this.SkipWhitespaceInBufferReader();
                    this.ExitJsonScope();
                }
                if (base.BufferReader.EndOfFile)
                {
                    if (this.scopeDepth > 0)
                    {
                        this.MoveToEndElement();
                        return true;
                    }
                    base.MoveToEndOfFile();
                    return false;
                }
            }
            @byte = base.BufferReader.GetByte();
            if (this.scopeDepth == 0)
            {
                this.ReadNonExistentElementName(StringHandleConstStringType.Root);
            }
            else
            {
                if (!this.IsReadingComplexText)
                {
                    if (this.IsReadingCollection)
                    {
                        this.ReadNonExistentElementName(StringHandleConstStringType.Item);
                        goto Label_062F;
                    }
                    switch (@byte)
                    {
                        case 0x5d:
                            base.BufferReader.SkipByte();
                            this.MoveToEndElement();
                            this.ExitJsonScope();
                            goto Label_062F;

                        case 0x7b:
                            base.BufferReader.SkipByte();
                            this.SkipWhitespaceInBufferReader();
                            @byte = base.BufferReader.GetByte();
                            if (@byte == 0x7d)
                            {
                                base.BufferReader.SkipByte();
                                this.SkipWhitespaceInBufferReader();
                                if (this.TryGetByte(out @byte))
                                {
                                    if (@byte == 0x2c)
                                    {
                                        base.BufferReader.SkipByte();
                                    }
                                }
                                else
                                {
                                    this.charactersToSkipOnNextRead[0] = 0x2c;
                                }
                                this.MoveToEndElement();
                            }
                            else
                            {
                                this.EnterJsonScope(JsonNodeType.Object);
                                this.ParseStartElement();
                            }
                            goto Label_062F;
                    }
                    if (@byte != 0x7d)
                    {
                        switch (@byte)
                        {
                            case 0x2c:
                                base.BufferReader.SkipByte();
                                this.MoveToEndElement();
                                goto Label_062F;

                            case 0x22:
                                if (this.readServerTypeElement)
                                {
                                    this.readServerTypeElement = false;
                                    this.EnterJsonScope(JsonNodeType.Object);
                                    this.ParseStartElement();
                                }
                                else if (base.Node.NodeType == XmlNodeType.Element)
                                {
                                    if (this.expectingFirstElementInNonPrimitiveChild)
                                    {
                                        this.EnterJsonScope(JsonNodeType.Object);
                                        this.ParseStartElement();
                                    }
                                    else
                                    {
                                        base.BufferReader.SkipByte();
                                        this.ReadQuotedText(true);
                                    }
                                }
                                else if (base.Node.NodeType == XmlNodeType.EndElement)
                                {
                                    this.EnterJsonScope(JsonNodeType.Element);
                                    this.ParseStartElement();
                                }
                                else
                                {
                                    XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncounteredUnexpectedCharacter", new object[] { '"' })));
                                }
                                goto Label_062F;

                            case 0x66:
                            {
                                int num2;
                                byte[] bytes = base.BufferReader.GetBuffer(5, out num2);
                                if (((bytes[num2 + 1] != 0x61) || (bytes[num2 + 2] != 0x6c)) || ((bytes[num2 + 3] != 0x73) || (bytes[num2 + 4] != 0x65)))
                                {
                                    XmlExceptionHelper.ThrowTokenExpected(this, "false", Encoding.UTF8.GetString(bytes, num2, 5));
                                }
                                base.BufferReader.Advance(5);
                                if (((this.TryGetByte(out @byte) && !IsWhitespace(@byte)) && ((@byte != 0x2c) && (@byte != 0x7d))) && (@byte != 0x5d))
                                {
                                    XmlExceptionHelper.ThrowTokenExpected(this, "false", Encoding.UTF8.GetString(bytes, num2, 4) + ((char) @byte));
                                }
                                base.MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, num2, 5);
                                goto Label_062F;
                            }
                            case 0x74:
                            {
                                int num3;
                                byte[] buffer = base.BufferReader.GetBuffer(4, out num3);
                                if (((buffer[num3 + 1] != 0x72) || (buffer[num3 + 2] != 0x75)) || (buffer[num3 + 3] != 0x65))
                                {
                                    XmlExceptionHelper.ThrowTokenExpected(this, "true", Encoding.UTF8.GetString(buffer, num3, 4));
                                }
                                base.BufferReader.Advance(4);
                                if (((this.TryGetByte(out @byte) && !IsWhitespace(@byte)) && ((@byte != 0x2c) && (@byte != 0x7d))) && (@byte != 0x5d))
                                {
                                    XmlExceptionHelper.ThrowTokenExpected(this, "true", Encoding.UTF8.GetString(buffer, num3, 4) + ((char) @byte));
                                }
                                base.MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, num3, 4);
                                goto Label_062F;
                            }
                            case 110:
                            {
                                int num4;
                                byte[] buffer3 = base.BufferReader.GetBuffer(4, out num4);
                                if (((buffer3[num4 + 1] != 0x75) || (buffer3[num4 + 2] != 0x6c)) || (buffer3[num4 + 3] != 0x6c))
                                {
                                    XmlExceptionHelper.ThrowTokenExpected(this, "null", Encoding.UTF8.GetString(buffer3, num4, 4));
                                }
                                base.BufferReader.Advance(4);
                                this.SkipWhitespaceInBufferReader();
                                if (this.TryGetByte(out @byte))
                                {
                                    if ((@byte == 0x2c) || (@byte == 0x7d))
                                    {
                                        base.BufferReader.SkipByte();
                                    }
                                    else if (@byte != 0x5d)
                                    {
                                        XmlExceptionHelper.ThrowTokenExpected(this, "null", Encoding.UTF8.GetString(buffer3, num4, 4) + ((char) @byte));
                                    }
                                }
                                else
                                {
                                    this.charactersToSkipOnNextRead[0] = 0x2c;
                                    this.charactersToSkipOnNextRead[1] = 0x7d;
                                }
                                this.MoveToEndElement();
                                goto Label_062F;
                            }
                        }
                        if (((@byte == 0x2d) || ((0x30 <= @byte) && (@byte <= 0x39))) || ((@byte == 0x49) || (@byte == 0x4e)))
                        {
                            this.ReadNumericalText();
                        }
                        else
                        {
                            XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncounteredUnexpectedCharacter", new object[] { (char) @byte })));
                        }
                        goto Label_062F;
                    }
                    base.BufferReader.SkipByte();
                    if (!this.expectingFirstElementInNonPrimitiveChild)
                    {
                        goto Label_02C4;
                    }
                    this.SkipWhitespaceInBufferReader();
                    @byte = base.BufferReader.GetByte();
                    switch (@byte)
                    {
                        case 0x2c:
                        case 0x7d:
                            base.BufferReader.SkipByte();
                            goto Label_02BD;
                    }
                    XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncounteredUnexpectedCharacter", new object[] { (char) @byte })));
                    goto Label_02BD;
                }
                switch (this.complexTextMode)
                {
                    case JsonComplexTextMode.QuotedText:
                        if (@byte != 0x5c)
                        {
                            this.ReadQuotedText(true);
                            break;
                        }
                        this.ReadEscapedCharacter(true);
                        break;

                    case JsonComplexTextMode.NumericalText:
                        this.ReadNumericalText();
                        break;

                    case JsonComplexTextMode.None:
                        XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncounteredUnexpectedCharacter", new object[] { (char) @byte })));
                        break;
                }
            }
            goto Label_062F;
        Label_02BD:
            this.expectingFirstElementInNonPrimitiveChild = false;
        Label_02C4:
            this.MoveToEndElement();
        Label_062F:
            return true;
        }

        private void ReadAttributes()
        {
            XmlBaseReader.XmlAttributeNode node = base.AddAttribute();
            node.LocalName.SetConstantValue(StringHandleConstStringType.Type);
            node.Namespace.Uri.SetValue(0, 0);
            node.Prefix.SetValue(PrefixHandleType.Empty);
            this.SkipWhitespaceInBufferReader();
            byte @byte = base.BufferReader.GetByte();
            switch (@byte)
            {
                case 0x7b:
                    node.Value.SetConstantValue(ValueHandleConstStringType.Object);
                    this.ReadServerTypeAttribute(false);
                    return;

                case 0x7d:
                    if (!this.expectingFirstElementInNonPrimitiveChild)
                    {
                        XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncounteredUnexpectedCharacter", new object[] { (char) @byte })));
                        return;
                    }
                    node.Value.SetConstantValue(ValueHandleConstStringType.Object);
                    return;

                case 0x74:
                case 0x66:
                    node.Value.SetConstantValue(ValueHandleConstStringType.Boolean);
                    return;

                case 110:
                    node.Value.SetConstantValue(ValueHandleConstStringType.Null);
                    return;

                case 0x22:
                    if (!this.expectingFirstElementInNonPrimitiveChild)
                    {
                        node.Value.SetConstantValue(ValueHandleConstStringType.String);
                        return;
                    }
                    node.Value.SetConstantValue(ValueHandleConstStringType.Object);
                    this.ReadServerTypeAttribute(true);
                    return;

                case 0x5b:
                    node.Value.SetConstantValue(ValueHandleConstStringType.Array);
                    base.BufferReader.SkipByte();
                    this.EnterJsonScope(JsonNodeType.Collection);
                    return;
            }
            if (((@byte == 0x2d) || ((@byte <= 0x39) && (@byte >= 0x30))) || ((@byte == 0x4e) || (@byte == 0x49)))
            {
                node.Value.SetConstantValue(ValueHandleConstStringType.Number);
            }
            else
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncounteredUnexpectedCharacter", new object[] { (char) @byte })));
            }
        }

        public override decimal ReadContentAsDecimal()
        {
            decimal num;
            string s = this.ReadContentAsString();
            try
            {
                num = decimal.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "decimal", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "decimal", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "decimal", exception3));
            }
            return num;
        }

        public override int ReadContentAsInt()
        {
            return ParseInt(this.ReadContentAsString(), NumberStyles.Float);
        }

        public override long ReadContentAsLong()
        {
            long num;
            string s = this.ReadContentAsString();
            try
            {
                num = long.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "Int64", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "Int64", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(s, "Int64", exception3));
            }
            return num;
        }

        private void ReadEscapedCharacter(bool moveToText)
        {
            base.BufferReader.SkipByte();
            char @byte = (char) base.BufferReader.GetByte();
            switch (@byte)
            {
                case '"':
                case '/':
                case '\\':
                    break;

                case 'r':
                    @byte = '\r';
                    break;

                case 't':
                    @byte = '\t';
                    break;

                case 'n':
                    @byte = '\n';
                    break;

                case 'b':
                    @byte = '\b';
                    break;

                case 'f':
                    @byte = '\f';
                    break;

                case 'u':
                {
                    int num;
                    base.BufferReader.SkipByte();
                    byte[] bytes = base.BufferReader.GetBuffer(5, out num);
                    string str = Encoding.UTF8.GetString(bytes, num, 4);
                    base.BufferReader.Advance(4);
                    int ch = ParseChar(str, NumberStyles.HexNumber);
                    if (char.IsHighSurrogate((char) ch) && (base.BufferReader.GetByte() == 0x5c))
                    {
                        base.BufferReader.SkipByte();
                        this.SkipExpectedByteInBufferReader(0x75);
                        bytes = base.BufferReader.GetBuffer(5, out num);
                        str = Encoding.UTF8.GetString(bytes, num, 4);
                        base.BufferReader.Advance(4);
                        char c = ParseChar(str, NumberStyles.HexNumber);
                        if (!char.IsLowSurrogate(c))
                        {
                            XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidLowSurrogate", new object[] { str })));
                        }
                        SurrogateChar ch3 = new SurrogateChar(c, (char) ch);
                        ch = ch3.Char;
                    }
                    if (bytes[num + 4] == 0x22)
                    {
                        base.BufferReader.SkipByte();
                        if (moveToText)
                        {
                            base.MoveToAtomicText().Value.SetCharValue(ch);
                        }
                        this.complexTextMode = JsonComplexTextMode.None;
                        return;
                    }
                    if (moveToText)
                    {
                        base.MoveToComplexText().Value.SetCharValue(ch);
                    }
                    this.complexTextMode = JsonComplexTextMode.QuotedText;
                    return;
                }
                default:
                    XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncounteredUnexpectedCharacter", new object[] { @byte })));
                    break;
            }
            base.BufferReader.SkipByte();
            if (base.BufferReader.GetByte() == 0x22)
            {
                base.BufferReader.SkipByte();
                if (moveToText)
                {
                    base.MoveToAtomicText().Value.SetCharValue(@byte);
                }
                this.complexTextMode = JsonComplexTextMode.None;
            }
            else
            {
                if (moveToText)
                {
                    base.MoveToComplexText().Value.SetCharValue(@byte);
                }
                this.complexTextMode = JsonComplexTextMode.QuotedText;
            }
        }

        private void ReadNonExistentElementName(StringHandleConstStringType elementName)
        {
            this.EnterJsonScope(JsonNodeType.Object);
            XmlBaseReader.XmlElementNode node = base.EnterScope();
            node.LocalName.SetConstantValue(elementName);
            node.Namespace.Uri.SetValue(node.NameOffset, 0);
            node.Prefix.SetValue(PrefixHandleType.Empty);
            node.BufferOffset = base.BufferReader.Offset;
            node.IsEmptyElement = false;
            node.ExitScope = false;
            this.ReadAttributes();
        }

        private int ReadNonFFFE()
        {
            int num;
            byte[] buffer = base.BufferReader.GetBuffer(3, out num);
            if ((buffer[num + 1] == 0xbf) && ((buffer[num + 2] == 190) || (buffer[num + 2] == 0xbf)))
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidFFFE")));
            }
            return 3;
        }

        private void ReadNumericalText()
        {
            int num;
            int num2;
            int num3;
            if (this.buffered)
            {
                num3 = ComputeNumericalTextLength(base.BufferReader.GetBuffer(out num, out num2), num, num2);
            }
            else
            {
                byte[] buffer = base.BufferReader.GetBuffer(0x800, out num, out num2);
                num3 = ComputeNumericalTextLength(buffer, num, num2);
                num3 = BreakText(buffer, num, num3);
            }
            base.BufferReader.Advance(num3);
            if (num <= (num2 - num3))
            {
                base.MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, num, num3);
                this.complexTextMode = JsonComplexTextMode.None;
            }
            else
            {
                base.MoveToComplexText().Value.SetValue(ValueHandleType.UTF8, num, num3);
                this.complexTextMode = JsonComplexTextMode.NumericalText;
            }
        }

        private void ReadQuotedText(bool moveToText)
        {
            int offset;
            int num2;
            int num3;
            bool flag;
            if (this.buffered)
            {
                num3 = ComputeQuotedTextLengthUntilEndQuote(base.BufferReader.GetBuffer(out offset, out num2), offset, num2, out flag);
            }
            else
            {
                byte[] buffer = base.BufferReader.GetBuffer(0x800, out offset, out num2);
                num3 = ComputeQuotedTextLengthUntilEndQuote(buffer, offset, num2, out flag);
                num3 = BreakText(buffer, offset, num3);
            }
            if (flag && (base.BufferReader.GetByte() == 0xef))
            {
                offset = base.BufferReader.Offset;
                num3 = this.ReadNonFFFE();
            }
            base.BufferReader.Advance(num3);
            if (!flag && (offset < (num2 - num3)))
            {
                if (moveToText)
                {
                    base.MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, offset, num3);
                }
                this.SkipExpectedByteInBufferReader(0x22);
                this.complexTextMode = JsonComplexTextMode.None;
            }
            else if ((num3 == 0) && flag)
            {
                this.ReadEscapedCharacter(moveToText);
            }
            else
            {
                if (moveToText)
                {
                    base.MoveToComplexText().Value.SetValue(ValueHandleType.UTF8, offset, num3);
                }
                this.complexTextMode = JsonComplexTextMode.QuotedText;
            }
        }

        private void ReadServerTypeAttribute(bool consumedObjectChar)
        {
            int num;
            int num2;
            int num3 = consumedObjectChar ? -1 : 0;
            byte[] buffer = base.BufferReader.GetBuffer(9 + num3, out num, out num2);
            if (((((((num + 9) + num3) <= num2) && (buffer[(num + num3) + 1] == 0x22)) && ((buffer[(num + num3) + 2] == 0x5f) && (buffer[(num + num3) + 3] == 0x5f))) && (((buffer[(num + num3) + 4] == 0x74) && (buffer[(num + num3) + 5] == 0x79)) && ((buffer[(num + num3) + 6] == 0x70) && (buffer[(num + num3) + 7] == 0x65)))) && (buffer[(num + num3) + 8] == 0x22))
            {
                XmlBaseReader.XmlAttributeNode node = base.AddAttribute();
                node.LocalName.SetValue((num + 2) + num3, 6);
                node.Namespace.Uri.SetValue(0, 0);
                node.Prefix.SetValue(PrefixHandleType.Empty);
                base.BufferReader.Advance(9 + num3);
                if (!this.buffered)
                {
                    this.BufferElement();
                }
                this.SkipWhitespaceInBufferReader();
                this.SkipExpectedByteInBufferReader(0x3a);
                this.SkipWhitespaceInBufferReader();
                this.SkipExpectedByteInBufferReader(0x22);
                buffer = base.BufferReader.GetBuffer(out num, out num2);
                do
                {
                    if (base.BufferReader.GetByte() == 0x5c)
                    {
                        this.ReadEscapedCharacter(false);
                    }
                    else
                    {
                        this.ReadQuotedText(false);
                    }
                }
                while (this.complexTextMode == JsonComplexTextMode.QuotedText);
                node.Value.SetValue(ValueHandleType.UTF8, num, (base.BufferReader.Offset - 1) - num);
                this.SkipWhitespaceInBufferReader();
                if (base.BufferReader.GetByte() == 0x2c)
                {
                    base.BufferReader.SkipByte();
                    this.readServerTypeElement = true;
                }
                else if (base.BufferReader.GetByte() == 0x7d)
                {
                    base.BufferReader.SkipByte();
                    this.readServerTypeElement = false;
                    this.expectingFirstElementInNonPrimitiveChild = false;
                }
                else
                {
                    this.readServerTypeElement = true;
                }
            }
        }

        public override int ReadValueAsBase64(byte[] buffer, int offset, int count)
        {
            if (!this.IsAttributeValue)
            {
                return base.ReadValueAsBase64(buffer, offset, count);
            }
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > buffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { buffer.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (buffer.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
            }
            return 0;
        }

        public override int ReadValueChunk(char[] chars, int offset, int count)
        {
            if (!this.IsAttributeValue)
            {
                return base.ReadValueChunk(chars, offset, count);
            }
            if (chars == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > chars.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { chars.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (chars.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { chars.Length - offset })));
            }
            string str = this.UnescapeJsonString(base.Node.ValueAsString);
            int num = Math.Min(count, str.Length);
            if (num > 0)
            {
                str.CopyTo(0, chars, offset, num);
                if (base.Node.QNameType == XmlBaseReader.QNameType.Xmlns)
                {
                    base.Node.Namespace.Uri.SetValue(0, 0);
                    return num;
                }
                base.Node.Value.SetValue(ValueHandleType.UTF8, 0, 0);
            }
            return num;
        }

        private void ResetState()
        {
            this.complexTextMode = JsonComplexTextMode.None;
            this.expectingFirstElementInNonPrimitiveChild = false;
            this.charactersToSkipOnNextRead = new byte[2];
            this.scopeDepth = 0;
            if ((this.scopes != null) && (this.scopes.Length > 0x19))
            {
                this.scopes = null;
            }
        }

        public void SetInput(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            this.MoveToInitial(quotas, onClose);
            stream = new JsonEncodingStreamWrapper(stream, encoding, true);
            base.BufferReader.SetBuffer(stream, null, null);
            this.buffered = false;
            this.ResetState();
        }

        public void SetInput(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > buffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("JsonOffsetExceedsBufferSize", new object[] { buffer.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (buffer.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("JsonSizeExceedsRemainingBufferSpace", new object[] { buffer.Length - offset })));
            }
            this.MoveToInitial(quotas, onClose);
            ArraySegment<byte> segment = JsonEncodingStreamWrapper.ProcessBuffer(buffer, offset, count, encoding);
            base.BufferReader.SetBuffer(segment.Array, segment.Offset, segment.Count, null, null);
            this.buffered = true;
            this.ResetState();
        }

        private void SetJsonNameWithMapping(XmlBaseReader.XmlElementNode elementNode)
        {
            XmlBaseReader.Namespace ns = base.AddNamespace();
            ns.Prefix.SetValue(PrefixHandleType.A);
            ns.Uri.SetConstantValue(StringHandleConstStringType.Item);
            base.AddXmlnsAttribute(ns);
            XmlBaseReader.XmlAttributeNode node = base.AddAttribute();
            node.LocalName.SetConstantValue(StringHandleConstStringType.Item);
            node.Namespace.Uri.SetValue(0, 0);
            node.Prefix.SetValue(PrefixHandleType.Empty);
            node.Value.SetValue(ValueHandleType.UTF8, elementNode.NameOffset, elementNode.NameLength);
            elementNode.NameLength = 0;
            elementNode.Prefix.SetValue(PrefixHandleType.A);
            elementNode.LocalName.SetConstantValue(StringHandleConstStringType.Item);
            elementNode.Namespace = ns;
        }

        private void SkipExpectedByteInBufferReader(byte characterToSkip)
        {
            if (base.BufferReader.GetByte() != characterToSkip)
            {
                XmlExceptionHelper.ThrowTokenExpected(this, ((char) characterToSkip).ToString(), (char) base.BufferReader.GetByte());
            }
            base.BufferReader.SkipByte();
        }

        private void SkipWhitespaceInBufferReader()
        {
            byte num;
            while (this.TryGetByte(out num) && IsWhitespace(num))
            {
                base.BufferReader.SkipByte();
            }
        }

        public override void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        private bool TryGetByte(out byte ch)
        {
            int num;
            int num2;
            byte[] buffer = base.BufferReader.GetBuffer(1, out num, out num2);
            if (num < num2)
            {
                ch = buffer[num];
                return true;
            }
            ch = 0;
            return false;
        }

        private string UnescapeJsonString(string val)
        {
            if (val == null)
            {
                return null;
            }
            StringBuilder builder = null;
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < val.Length; i++)
            {
                if (val[i] == '\\')
                {
                    i++;
                    if (builder == null)
                    {
                        builder = new StringBuilder();
                    }
                    builder.Append(val, startIndex, count);
                    if (i >= val.Length)
                    {
                        XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncounteredUnexpectedCharacter", new object[] { val[i] })));
                    }
                    switch (val[i])
                    {
                        case '/':
                        case '\\':
                        case '"':
                        case '\'':
                            builder.Append(val[i]);
                            break;

                        case 'b':
                            builder.Append('\b');
                            break;

                        case 'f':
                            builder.Append('\f');
                            break;

                        case 'r':
                            builder.Append('\r');
                            break;

                        case 't':
                            builder.Append('\t');
                            break;

                        case 'u':
                            if ((i + 3) >= val.Length)
                            {
                                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncounteredUnexpectedCharacter", new object[] { val[i] })));
                            }
                            builder.Append(ParseChar(val.Substring(i + 1, 4), NumberStyles.HexNumber));
                            i += 4;
                            break;

                        case 'n':
                            builder.Append('\n');
                            break;
                    }
                    startIndex = i + 1;
                    count = 0;
                    continue;
                }
                count++;
            }
            if (builder == null)
            {
                return val;
            }
            if (count > 0)
            {
                builder.Append(val, startIndex, count);
            }
            return builder.ToString();
        }

        public override bool CanCanonicalize
        {
            get
            {
                return false;
            }
        }

        private bool IsAttributeValue
        {
            get
            {
                if (base.Node.NodeType != XmlNodeType.Attribute)
                {
                    return (base.Node is XmlBaseReader.XmlAttributeTextNode);
                }
                return true;
            }
        }

        private bool IsReadingCollection
        {
            get
            {
                return ((this.scopeDepth > 0) && (this.scopes[this.scopeDepth] == JsonNodeType.Collection));
            }
        }

        private bool IsReadingComplexText
        {
            get
            {
                return (!base.Node.IsAtomicValue && (base.Node.NodeType == XmlNodeType.Text));
            }
        }

        public override string Value
        {
            get
            {
                if (this.IsAttributeValue && !this.IsLocalName("type"))
                {
                    return this.UnescapeJsonString(base.Value);
                }
                return base.Value;
            }
        }

        private static class CharType
        {
            public const byte FirstName = 1;
            public const byte Name = 2;
            public const byte None = 0;
        }

        private enum JsonComplexTextMode
        {
            QuotedText,
            NumericalText,
            None
        }
    }
}

