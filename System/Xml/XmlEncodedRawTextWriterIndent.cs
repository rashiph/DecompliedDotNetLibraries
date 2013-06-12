namespace System.Xml
{
    using System;
    using System.IO;

    internal class XmlEncodedRawTextWriterIndent : XmlEncodedRawTextWriter
    {
        protected ConformanceLevel conformanceLevel;
        protected string indentChars;
        protected int indentLevel;
        protected bool mixedContent;
        private BitStack mixedContentStack;
        protected bool newLineOnAttributes;

        public XmlEncodedRawTextWriterIndent(Stream stream, XmlWriterSettings settings) : base(stream, settings)
        {
            this.Init(settings);
        }

        public XmlEncodedRawTextWriterIndent(TextWriter writer, XmlWriterSettings settings) : base(writer, settings)
        {
            this.Init(settings);
        }

        private void Init(XmlWriterSettings settings)
        {
            this.indentLevel = 0;
            this.indentChars = settings.IndentChars;
            this.newLineOnAttributes = settings.NewLineOnAttributes;
            this.mixedContentStack = new BitStack();
            if (base.checkCharacters)
            {
                if (this.newLineOnAttributes)
                {
                    base.ValidateContentChars(this.indentChars, "IndentChars", true);
                    base.ValidateContentChars(base.newLineChars, "NewLineChars", true);
                }
                else
                {
                    base.ValidateContentChars(this.indentChars, "IndentChars", false);
                    if (base.newLineHandling != NewLineHandling.Replace)
                    {
                        base.ValidateContentChars(base.newLineChars, "NewLineChars", false);
                    }
                }
            }
        }

        internal override void OnRootElement(ConformanceLevel currentConformanceLevel)
        {
            this.conformanceLevel = currentConformanceLevel;
        }

        internal override void StartElementContent()
        {
            if ((this.indentLevel == 1) && (this.conformanceLevel == ConformanceLevel.Document))
            {
                this.mixedContent = false;
            }
            else
            {
                this.mixedContent = this.mixedContentStack.PeekBit();
            }
            base.StartElementContent();
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this.mixedContent = true;
            base.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.mixedContent = true;
            base.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.mixedContent = true;
            base.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.mixedContent = true;
            base.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            if (!this.mixedContent && (base.textPos != base.bufPos))
            {
                this.WriteIndent();
            }
            base.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            if (!this.mixedContent && (base.textPos != base.bufPos))
            {
                this.WriteIndent();
            }
            base.WriteDocType(name, pubid, sysid, subset);
        }

        internal override void WriteEndElement(string prefix, string localName, string ns)
        {
            this.indentLevel--;
            if ((!this.mixedContent && (base.contentPos != base.bufPos)) && (base.textPos != base.bufPos))
            {
                this.WriteIndent();
            }
            this.mixedContent = this.mixedContentStack.PopBit();
            base.WriteEndElement(prefix, localName, ns);
        }

        public override void WriteEntityRef(string name)
        {
            this.mixedContent = true;
            base.WriteEntityRef(name);
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns)
        {
            this.indentLevel--;
            if ((!this.mixedContent && (base.contentPos != base.bufPos)) && (base.textPos != base.bufPos))
            {
                this.WriteIndent();
            }
            this.mixedContent = this.mixedContentStack.PopBit();
            base.WriteFullEndElement(prefix, localName, ns);
        }

        private void WriteIndent()
        {
            base.RawText(base.newLineChars);
            for (int i = this.indentLevel; i > 0; i--)
            {
                base.RawText(this.indentChars);
            }
        }

        public override void WriteProcessingInstruction(string target, string text)
        {
            if (!this.mixedContent && (base.textPos != base.bufPos))
            {
                this.WriteIndent();
            }
            base.WriteProcessingInstruction(target, text);
        }

        public override void WriteRaw(string data)
        {
            this.mixedContent = true;
            base.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.mixedContent = true;
            base.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (this.newLineOnAttributes)
            {
                this.WriteIndent();
            }
            base.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (!this.mixedContent && (base.textPos != base.bufPos))
            {
                this.WriteIndent();
            }
            this.indentLevel++;
            this.mixedContentStack.PushBit(this.mixedContent);
            base.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            this.mixedContent = true;
            base.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.mixedContent = true;
            base.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            this.mixedContent = true;
            base.WriteWhitespace(ws);
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                XmlWriterSettings settings = base.Settings;
                settings.ReadOnly = false;
                settings.Indent = true;
                settings.IndentChars = this.indentChars;
                settings.NewLineOnAttributes = this.newLineOnAttributes;
                settings.ReadOnly = true;
                return settings;
            }
        }
    }
}

