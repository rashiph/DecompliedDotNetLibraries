namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    internal class XmlUTF8TextReader : XmlBaseReader, IXmlLineInfo, IXmlTextReaderInitializer
    {
        private bool buffered;
        private static byte[] charType = new byte[] { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0x6c, 0x6c, 0, 0, 0x44, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0x7c, 0x58, 0x48, 0x58, 0x58, 0x58, 0x40, 0x48, 0x58, 0x58, 0x58, 0x58, 0x58, 90, 90, 0x58, 
            90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 0x58, 0x58, 0x40, 0x58, 0x58, 0x58, 
            0x58, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x58, 0x58, 80, 0x58, 0x5b, 
            0x58, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x58, 0x58, 0x58, 0x58, 0x58, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 3, 
            0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b, 0x5b
         };
        private StringHandle localName;
        private int maxBytesPerRead;
        private const int MaxTextChunk = 0x800;
        private OnXmlDictionaryReaderClose onClose;
        private PrefixHandle prefix;
        private int[] rowOffsets;

        public XmlUTF8TextReader()
        {
            this.prefix = new PrefixHandle(base.BufferReader);
            this.localName = new StringHandle(base.BufferReader);
        }

        private int BreakText(byte[] buffer, int offset, int length)
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
                    if (num2 == 0)
                    {
                        switch (num6)
                        {
                            case 0x27:
                            case 0x22:
                                num2 = num6;
                                break;
                        }
                        if (num6 == 0x3e)
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

        public override void Close()
        {
            this.rowOffsets = null;
            base.Close();
            OnXmlDictionaryReaderClose onClose = this.onClose;
            this.onClose = null;
            if (onClose != null)
            {
                try
                {
                    onClose(this);
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

        protected override XmlSigningNodeWriter CreateSigningNodeWriter()
        {
            return new XmlSigningNodeWriter(true);
        }

        private void GetPosition(out int row, out int column)
        {
            if (this.rowOffsets == null)
            {
                this.rowOffsets = base.BufferReader.GetRows();
            }
            int offset = base.BufferReader.Offset;
            int index = 0;
            while ((index < (this.rowOffsets.Length - 1)) && (this.rowOffsets[index + 1] < offset))
            {
                index++;
            }
            row = index + 1;
            column = (offset - this.rowOffsets[index]) + 1;
        }

        public bool HasLineInfo()
        {
            return true;
        }

        private void MoveToInitial(XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
        {
            base.MoveToInitial(quotas);
            this.maxBytesPerRead = quotas.MaxBytesPerRead;
            this.onClose = onClose;
        }

        public override bool Read()
        {
            if (base.Node.ReadState == System.Xml.ReadState.Closed)
            {
                return false;
            }
            if (base.Node.CanMoveToElement)
            {
                this.MoveToElement();
            }
            base.SignNode();
            if (base.Node.ExitScope)
            {
                base.ExitScope();
            }
            if (!this.buffered)
            {
                base.BufferReader.SetWindow(base.ElementNode.BufferOffset, this.maxBytesPerRead);
            }
            if (base.BufferReader.EndOfFile)
            {
                base.MoveToEndOfFile();
                return false;
            }
            byte @byte = base.BufferReader.GetByte();
            if (@byte == 60)
            {
                base.BufferReader.SkipByte();
                @byte = base.BufferReader.GetByte();
                switch (@byte)
                {
                    case 0x2f:
                        this.ReadEndElement();
                        goto Label_025B;

                    case 0x21:
                        base.BufferReader.SkipByte();
                        if (base.BufferReader.GetByte() == 0x2d)
                        {
                            this.ReadComment();
                        }
                        else
                        {
                            if (base.OutsideRootElement)
                            {
                                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlCDATAInvalidAtTopLevel")));
                            }
                            this.ReadCData();
                        }
                        goto Label_025B;

                    case 0x3f:
                        this.ReadDeclaration();
                        break;
                }
                if (@byte == 0x3f)
                {
                    this.ReadDeclaration();
                }
                else
                {
                    this.ReadStartElement();
                }
            }
            else if ((charType[@byte] & 0x20) != 0)
            {
                this.ReadWhitespace();
            }
            else if (base.OutsideRootElement && (@byte != 13))
            {
                XmlExceptionHelper.ThrowInvalidRootData(this);
            }
            else if ((charType[@byte] & 8) != 0)
            {
                this.ReadText();
            }
            else if (@byte == 0x26)
            {
                this.ReadEscapedText();
            }
            else if (@byte == 13)
            {
                base.BufferReader.SkipByte();
                if (!base.BufferReader.EndOfFile && (base.BufferReader.GetByte() == 10))
                {
                    this.ReadWhitespace();
                }
                else
                {
                    base.MoveToComplexText().Value.SetCharValue(10);
                }
            }
            else if (@byte == 0x5d)
            {
                int num2;
                byte[] buffer = base.BufferReader.GetBuffer(3, out num2);
                if (((buffer[num2] == 0x5d) && (buffer[num2 + 1] == 0x5d)) && (buffer[num2 + 2] == 0x3e))
                {
                    XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlCloseCData")));
                }
                base.BufferReader.SkipByte();
                base.MoveToComplexText().Value.SetCharValue(0x5d);
            }
            else if (@byte == 0xef)
            {
                int offset = base.BufferReader.Offset;
                this.ReadNonFFFE();
                base.MoveToComplexText().Value.SetValue(ValueHandleType.UTF8, offset, 3);
            }
            else
            {
                XmlExceptionHelper.ThrowInvalidXml(this, @byte);
            }
        Label_025B:
            return true;
        }

        private void ReadAttributes()
        {
            int num5;
            int num6;
            byte[] buffer;
            XmlBaseReader.XmlAttributeNode node;
            int num = 0;
            if (this.buffered)
            {
                num = base.BufferReader.Offset;
            }
        Label_0016:
            this.ReadQualifiedName(this.prefix, this.localName);
            if (base.BufferReader.GetByte() != 0x3d)
            {
                this.SkipWhitespace();
                if (base.BufferReader.GetByte() != 0x3d)
                {
                    XmlExceptionHelper.ThrowTokenExpected(this, "=", (char) base.BufferReader.GetByte());
                }
            }
            base.BufferReader.SkipByte();
            byte @byte = base.BufferReader.GetByte();
            if ((@byte != 0x22) && (@byte != 0x27))
            {
                this.SkipWhitespace();
                @byte = base.BufferReader.GetByte();
                if ((@byte != 0x22) && (@byte != 0x27))
                {
                    XmlExceptionHelper.ThrowTokenExpected(this, "\"", (char) base.BufferReader.GetByte());
                }
            }
            base.BufferReader.SkipByte();
            bool escaped = false;
            int offset = base.BufferReader.Offset;
        Label_00CF:
            buffer = base.BufferReader.GetBuffer(out num5, out num6);
            int count = this.ReadAttributeText(buffer, num5, num6);
            base.BufferReader.Advance(count);
            byte index = base.BufferReader.GetByte();
            if (index != @byte)
            {
                if (index == 0x26)
                {
                    this.ReadCharRef();
                    escaped = true;
                }
                else if ((index == 0x27) || (index == 0x22))
                {
                    base.BufferReader.SkipByte();
                }
                else if (((index == 10) || (index == 13)) || (index == 9))
                {
                    base.BufferReader.SkipByte();
                    escaped = true;
                }
                else if (index == 0xef)
                {
                    this.ReadNonFFFE();
                }
                else
                {
                    XmlExceptionHelper.ThrowTokenExpected(this, ((char) @byte).ToString(), (char) index);
                }
                goto Label_00CF;
            }
            int length = base.BufferReader.Offset - offset;
            if (this.prefix.IsXmlns)
            {
                XmlBaseReader.Namespace ns = base.AddNamespace();
                this.localName.ToPrefixHandle(ns.Prefix);
                ns.Uri.SetValue(offset, length, escaped);
                node = base.AddXmlnsAttribute(ns);
            }
            else if (this.prefix.IsEmpty && this.localName.IsXmlns)
            {
                XmlBaseReader.Namespace namespace3 = base.AddNamespace();
                namespace3.Prefix.SetValue(PrefixHandleType.Empty);
                namespace3.Uri.SetValue(offset, length, escaped);
                node = base.AddXmlnsAttribute(namespace3);
            }
            else if (this.prefix.IsXml)
            {
                node = base.AddXmlAttribute();
                node.Prefix.SetValue(this.prefix);
                node.LocalName.SetValue(this.localName);
                node.Value.SetValue(escaped ? ValueHandleType.EscapedUTF8 : ValueHandleType.UTF8, offset, length);
                base.FixXmlAttribute(node);
            }
            else
            {
                node = base.AddAttribute();
                node.Prefix.SetValue(this.prefix);
                node.LocalName.SetValue(this.localName);
                node.Value.SetValue(escaped ? ValueHandleType.EscapedUTF8 : ValueHandleType.UTF8, offset, length);
            }
            node.QuoteChar = (char) @byte;
            base.BufferReader.SkipByte();
            index = base.BufferReader.GetByte();
            bool flag2 = false;
            while ((charType[index] & 4) != 0)
            {
                flag2 = true;
                base.BufferReader.SkipByte();
                index = base.BufferReader.GetByte();
            }
            switch (index)
            {
                case 0x3e:
                case 0x2f:
                case 0x3f:
                    if (this.buffered && ((base.BufferReader.Offset - num) > this.maxBytesPerRead))
                    {
                        XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(this, this.maxBytesPerRead);
                    }
                    base.ProcessAttributes();
                    return;
            }
            if (!flag2)
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlSpaceBetweenAttributes")));
            }
            goto Label_0016;
        }

        private int ReadAttributeText(byte[] buffer, int offset, int offsetMax)
        {
            byte[] charType = XmlUTF8TextReader.charType;
            int num = offset;
            while ((offset < offsetMax) && ((charType[buffer[offset]] & 0x10) != 0))
            {
                offset++;
            }
            return (offset - num);
        }

        private void ReadCData()
        {
            int num;
            byte[] bytes = base.BufferReader.GetBuffer(7, out num);
            if ((((bytes[num] != 0x5b) || (bytes[num + 1] != 0x43)) || ((bytes[num + 2] != 0x44) || (bytes[num + 3] != 0x41))) || (((bytes[num + 4] != 0x54) || (bytes[num + 5] != 0x41)) || (bytes[num + 6] != 0x5b)))
            {
                XmlExceptionHelper.ThrowTokenExpected(this, "[CDATA[", Encoding.UTF8.GetString(bytes, num, 7));
            }
            base.BufferReader.Advance(7);
            int offset = base.BufferReader.Offset;
        Label_007C:
            switch (base.BufferReader.GetByte())
            {
                case 0x5d:
                    bytes = base.BufferReader.GetBuffer(3, out num);
                    if (((bytes[num] == 0x5d) && (bytes[num + 1] == 0x5d)) && (bytes[num + 2] == 0x3e))
                    {
                        int length = base.BufferReader.Offset - offset;
                        base.MoveToCData().Value.SetValue(ValueHandleType.UTF8, offset, length);
                        base.BufferReader.Advance(3);
                        return;
                    }
                    base.BufferReader.SkipByte();
                    goto Label_007C;

                case 0xef:
                    this.ReadNonFFFE();
                    goto Label_007C;
            }
            base.BufferReader.SkipByte();
            goto Label_007C;
        }

        private int ReadCharRef()
        {
            int offset = base.BufferReader.Offset;
            base.BufferReader.SkipByte();
            while (base.BufferReader.GetByte() != 0x3b)
            {
                base.BufferReader.SkipByte();
            }
            base.BufferReader.SkipByte();
            int length = base.BufferReader.Offset - offset;
            base.BufferReader.Offset = offset;
            int charEntity = base.BufferReader.GetCharEntity(offset, length);
            base.BufferReader.Advance(length);
            return charEntity;
        }

        private void ReadComment()
        {
            base.BufferReader.SkipByte();
            if (base.BufferReader.GetByte() != 0x2d)
            {
                XmlExceptionHelper.ThrowTokenExpected(this, "--", (char) base.BufferReader.GetByte());
            }
            base.BufferReader.SkipByte();
            int offset = base.BufferReader.Offset;
            while (true)
            {
                byte @byte = base.BufferReader.GetByte();
                if (@byte == 0x2d)
                {
                    int num3;
                    byte[] buffer = base.BufferReader.GetBuffer(3, out num3);
                    if ((buffer[num3] == 0x2d) && (buffer[num3 + 1] == 0x2d))
                    {
                        if (buffer[num3 + 2] == 0x3e)
                        {
                            break;
                        }
                        XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidCommentChars")));
                    }
                    base.BufferReader.SkipByte();
                }
                else if ((charType[@byte] & 0x40) == 0)
                {
                    if (@byte == 0xef)
                    {
                        this.ReadNonFFFE();
                    }
                    else
                    {
                        XmlExceptionHelper.ThrowInvalidXml(this, @byte);
                    }
                }
                else
                {
                    base.BufferReader.SkipByte();
                }
            }
            int length = base.BufferReader.Offset - offset;
            base.MoveToComment().Value.SetValue(ValueHandleType.UTF8, offset, length);
            base.BufferReader.Advance(3);
        }

        private void ReadDeclaration()
        {
            int num;
            if (!this.buffered)
            {
                this.BufferElement();
            }
            byte[] bytes = base.BufferReader.GetBuffer(5, out num);
            if (((bytes[num] != 0x3f) || (bytes[num + 1] != 120)) || (((bytes[num + 2] != 0x6d) || (bytes[num + 3] != 0x6c)) || ((charType[bytes[num + 4]] & 4) == 0)))
            {
                XmlExceptionHelper.ThrowProcessingInstructionNotSupported(this);
            }
            if (base.Node.ReadState != System.Xml.ReadState.Initial)
            {
                XmlExceptionHelper.ThrowDeclarationNotFirst(this);
            }
            base.BufferReader.Advance(5);
            int offset = num + 1;
            int length = 3;
            int num4 = base.BufferReader.Offset;
            this.SkipWhitespace();
            this.ReadAttributes();
            int num5 = base.BufferReader.Offset - num4;
            while (num5 > 0)
            {
                byte @byte = base.BufferReader.GetByte((num4 + num5) - 1);
                if ((charType[@byte] & 4) == 0)
                {
                    break;
                }
                num5--;
            }
            bytes = base.BufferReader.GetBuffer(2, out num);
            if ((bytes[num] != 0x3f) || (bytes[num + 1] != 0x3e))
            {
                XmlExceptionHelper.ThrowTokenExpected(this, "?>", Encoding.UTF8.GetString(bytes, num, 2));
            }
            base.BufferReader.Advance(2);
            XmlBaseReader.XmlDeclarationNode node = base.MoveToDeclaration();
            node.LocalName.SetValue(offset, length);
            node.Value.SetValue(ValueHandleType.UTF8, num4, num5);
        }

        private void ReadEndElement()
        {
            int num3;
            base.BufferReader.SkipByte();
            XmlBaseReader.XmlElementNode elementNode = base.ElementNode;
            int nameOffset = elementNode.NameOffset;
            int nameLength = elementNode.NameLength;
            byte[] buffer = base.BufferReader.GetBuffer(nameLength, out num3);
            for (int i = 0; i < nameLength; i++)
            {
                if (buffer[num3 + i] != buffer[nameOffset + i])
                {
                    this.ReadQualifiedName(this.prefix, this.localName);
                    XmlExceptionHelper.ThrowTagMismatch(this, elementNode.Prefix.GetString(), elementNode.LocalName.GetString(), this.prefix.GetString(), this.localName.GetString());
                }
            }
            base.BufferReader.Advance(nameLength);
            if (base.BufferReader.GetByte() != 0x3e)
            {
                this.SkipWhitespace();
                if (base.BufferReader.GetByte() != 0x3e)
                {
                    XmlExceptionHelper.ThrowTokenExpected(this, ">", (char) base.BufferReader.GetByte());
                }
            }
            base.BufferReader.SkipByte();
            base.MoveToEndElement();
        }

        private void ReadEscapedText()
        {
            int index = this.ReadCharRef();
            if ((index < 0x100) && ((charType[index] & 4) != 0))
            {
                base.MoveToWhitespaceText().Value.SetCharValue(index);
            }
            else
            {
                base.MoveToComplexText().Value.SetCharValue(index);
            }
        }

        private void ReadNonFFFE()
        {
            int num;
            byte[] buffer = base.BufferReader.GetBuffer(3, out num);
            if ((buffer[num + 1] == 0xbf) && ((buffer[num + 2] == 190) || (buffer[num + 2] == 0xbf)))
            {
                XmlExceptionHelper.ThrowXmlException(this, new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidFFFE")));
            }
            base.BufferReader.Advance(3);
        }

        private void ReadQualifiedName(PrefixHandle prefix, StringHandle localName)
        {
            int num;
            int num2;
            byte[] buffer = base.BufferReader.GetBuffer(out num, out num2);
            int index = 0;
            int num4 = 0;
            int num5 = 0;
            int offset = num;
            if (num < num2)
            {
                index = buffer[num];
                num5 = index;
                if ((charType[index] & 1) == 0)
                {
                    num4 |= 0x80;
                }
                num4 |= index;
                num++;
                while (num < num2)
                {
                    index = buffer[num];
                    if ((charType[index] & 2) == 0)
                    {
                        break;
                    }
                    num4 |= index;
                    num++;
                }
            }
            else
            {
                num4 |= 0x80;
                index = 0;
            }
            if (index != 0x3a)
            {
                prefix.SetValue(PrefixHandleType.Empty);
                localName.SetValue(offset, num - offset);
                if (num4 >= 0x80)
                {
                    this.VerifyNCName(localName.GetString());
                }
            }
            else
            {
                int length = num - offset;
                if (((length == 1) && (num5 >= 0x61)) && (num5 <= 0x7a))
                {
                    prefix.SetValue(PrefixHandle.GetAlphaPrefix(num5 - 0x61));
                }
                else
                {
                    prefix.SetValue(offset, length);
                }
                num++;
                int num8 = num;
                if (num < num2)
                {
                    index = buffer[num];
                    if ((charType[index] & 1) == 0)
                    {
                        num4 |= 0x80;
                    }
                    num4 |= index;
                    num++;
                    while (num < num2)
                    {
                        index = buffer[num];
                        if ((charType[index] & 2) == 0)
                        {
                            break;
                        }
                        num4 |= index;
                        num++;
                    }
                }
                else
                {
                    num4 |= 0x80;
                    index = 0;
                }
                localName.SetValue(num8, num - num8);
                if (num4 >= 0x80)
                {
                    this.VerifyNCName(prefix.GetString());
                    this.VerifyNCName(localName.GetString());
                }
            }
            base.BufferReader.Advance(num - offset);
        }

        private void ReadStartElement()
        {
            if (!this.buffered)
            {
                this.BufferElement();
            }
            XmlBaseReader.XmlElementNode node = base.EnterScope();
            node.NameOffset = base.BufferReader.Offset;
            this.ReadQualifiedName(node.Prefix, node.LocalName);
            node.NameLength = base.BufferReader.Offset - node.NameOffset;
            byte @byte = base.BufferReader.GetByte();
            while ((charType[@byte] & 4) != 0)
            {
                base.BufferReader.SkipByte();
                @byte = base.BufferReader.GetByte();
            }
            if ((@byte != 0x3e) && (@byte != 0x2f))
            {
                this.ReadAttributes();
                @byte = base.BufferReader.GetByte();
            }
            node.Namespace = base.LookupNamespace(node.Prefix);
            bool flag = false;
            if (@byte == 0x2f)
            {
                flag = true;
                base.BufferReader.SkipByte();
            }
            node.IsEmptyElement = flag;
            node.ExitScope = flag;
            if (base.BufferReader.GetByte() != 0x3e)
            {
                XmlExceptionHelper.ThrowTokenExpected(this, ">", (char) base.BufferReader.GetByte());
            }
            base.BufferReader.SkipByte();
            node.BufferOffset = base.BufferReader.Offset;
        }

        private void ReadText()
        {
            byte[] buffer;
            int num;
            int num2;
            int num3;
            if (this.buffered)
            {
                buffer = base.BufferReader.GetBuffer(out num, out num2);
                num3 = this.ReadText(buffer, num, num2);
            }
            else
            {
                buffer = base.BufferReader.GetBuffer(0x800, out num, out num2);
                num3 = this.ReadText(buffer, num, num2);
                num3 = this.BreakText(buffer, num, num3);
            }
            base.BufferReader.Advance(num3);
            if (((num < ((num2 - 1) - num3)) && (buffer[num + num3] == 60)) && (buffer[(num + num3) + 1] != 0x21))
            {
                base.MoveToAtomicText().Value.SetValue(ValueHandleType.UTF8, num, num3);
            }
            else
            {
                base.MoveToComplexText().Value.SetValue(ValueHandleType.UTF8, num, num3);
            }
        }

        private int ReadText(byte[] buffer, int offset, int offsetMax)
        {
            byte[] charType = XmlUTF8TextReader.charType;
            int num = offset;
            while ((offset < offsetMax) && ((charType[buffer[offset]] & 8) != 0))
            {
                offset++;
            }
            return (offset - num);
        }

        private void ReadWhitespace()
        {
            byte[] buffer;
            int num;
            int num2;
            int num3;
            if (this.buffered)
            {
                buffer = base.BufferReader.GetBuffer(out num, out num2);
                num3 = this.ReadWhitespace(buffer, num, num2);
            }
            else
            {
                buffer = base.BufferReader.GetBuffer(0x800, out num, out num2);
                num3 = this.ReadWhitespace(buffer, num, num2);
                num3 = this.BreakText(buffer, num, num3);
            }
            base.BufferReader.Advance(num3);
            base.MoveToWhitespaceText().Value.SetValue(ValueHandleType.UTF8, num, num3);
        }

        private int ReadWhitespace(byte[] buffer, int offset, int offsetMax)
        {
            byte[] charType = XmlUTF8TextReader.charType;
            int num = offset;
            while ((offset < offsetMax) && ((charType[buffer[offset]] & 0x20) != 0))
            {
                offset++;
            }
            return (offset - num);
        }

        public void SetInput(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            this.MoveToInitial(quotas, onClose);
            stream = new EncodingStreamWrapper(stream, encoding);
            base.BufferReader.SetBuffer(stream, null, null);
            this.buffered = false;
        }

        public void SetInput(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose)
        {
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
            this.MoveToInitial(quotas, onClose);
            ArraySegment<byte> segment = EncodingStreamWrapper.ProcessBuffer(buffer, offset, count, encoding);
            base.BufferReader.SetBuffer(segment.Array, segment.Offset, segment.Count, null, null);
            this.buffered = true;
        }

        private void SkipWhitespace()
        {
            while (!base.BufferReader.EndOfFile && ((charType[base.BufferReader.GetByte()] & 4) != 0))
            {
                base.BufferReader.SkipByte();
            }
        }

        private void VerifyNCName(string s)
        {
            try
            {
                XmlConvert.VerifyNCName(s);
            }
            catch (XmlException exception)
            {
                XmlExceptionHelper.ThrowXmlException(this, exception);
            }
        }

        public int LineNumber
        {
            get
            {
                int num;
                int num2;
                this.GetPosition(out num, out num2);
                return num;
            }
        }

        public int LinePosition
        {
            get
            {
                int num;
                int num2;
                this.GetPosition(out num, out num2);
                return num2;
            }
        }

        private static class CharType
        {
            public const byte AttributeText = 0x10;
            public const byte Comment = 0x40;
            public const byte FirstName = 1;
            public const byte Name = 2;
            public const byte None = 0;
            public const byte SpecialWhitespace = 0x20;
            public const byte Text = 8;
            public const byte Whitespace = 4;
        }
    }
}

