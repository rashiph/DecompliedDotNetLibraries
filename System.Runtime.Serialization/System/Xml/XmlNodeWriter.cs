namespace System.Xml
{
    using System;
    using System.Runtime;
    using System.Text;

    internal abstract class XmlNodeWriter
    {
        private static XmlNodeWriter nullNodeWriter;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected XmlNodeWriter()
        {
        }

        public abstract void Close();
        public abstract void Flush();
        public abstract void WriteBase64Text(byte[] trailBuffer, int trailCount, byte[] buffer, int offset, int count);
        public abstract void WriteBoolText(bool value);
        public abstract void WriteCData(string text);
        public abstract void WriteCharEntity(int ch);
        public abstract void WriteComment(string text);
        public abstract void WriteDateTimeText(DateTime value);
        public abstract void WriteDecimalText(decimal value);
        public abstract void WriteDeclaration();
        public abstract void WriteDoubleText(double value);
        public abstract void WriteEndAttribute();
        public abstract void WriteEndElement(string prefix, string localName);
        public virtual void WriteEndElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            this.WriteEndElement(Encoding.UTF8.GetString(prefixBuffer, prefixOffset, prefixLength), Encoding.UTF8.GetString(localNameBuffer, localNameOffset, localNameLength));
        }

        public abstract void WriteEndListText();
        public abstract void WriteEndStartElement(bool isEmpty);
        public abstract void WriteEscapedText(string value);
        public abstract void WriteEscapedText(XmlDictionaryString value);
        public abstract void WriteEscapedText(byte[] buffer, int offset, int count);
        public abstract void WriteEscapedText(char[] chars, int offset, int count);
        public abstract void WriteFloatText(float value);
        public abstract void WriteGuidText(Guid value);
        public abstract void WriteInt32Text(int value);
        public abstract void WriteInt64Text(long value);
        public abstract void WriteListSeparator();
        public abstract void WriteQualifiedName(string prefix, XmlDictionaryString localName);
        public abstract void WriteStartAttribute(string prefix, string localName);
        public abstract void WriteStartAttribute(string prefix, XmlDictionaryString localName);
        public virtual void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            this.WriteStartAttribute(Encoding.UTF8.GetString(prefixBuffer, prefixOffset, prefixLength), Encoding.UTF8.GetString(localNameBuffer, localNameOffset, localNameLength));
        }

        public abstract void WriteStartElement(string prefix, string localName);
        public abstract void WriteStartElement(string prefix, XmlDictionaryString localName);
        public virtual void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            this.WriteStartElement(Encoding.UTF8.GetString(prefixBuffer, prefixOffset, prefixLength), Encoding.UTF8.GetString(localNameBuffer, localNameOffset, localNameLength));
        }

        public abstract void WriteStartListText();
        public abstract void WriteText(string value);
        public abstract void WriteText(XmlDictionaryString value);
        public abstract void WriteText(byte[] buffer, int offset, int count);
        public abstract void WriteText(char[] chars, int offset, int count);
        public abstract void WriteTimeSpanText(TimeSpan value);
        public abstract void WriteUInt64Text(ulong value);
        public abstract void WriteUniqueIdText(UniqueId value);
        public abstract void WriteXmlnsAttribute(string prefix, string ns);
        public abstract void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns);
        public virtual void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength)
        {
            this.WriteXmlnsAttribute(Encoding.UTF8.GetString(prefixBuffer, prefixOffset, prefixLength), Encoding.UTF8.GetString(nsBuffer, nsOffset, nsLength));
        }

        public static XmlNodeWriter Null
        {
            get
            {
                if (nullNodeWriter == null)
                {
                    nullNodeWriter = new XmlNullNodeWriter();
                }
                return nullNodeWriter;
            }
        }

        private class XmlNullNodeWriter : XmlNodeWriter
        {
            public override void Close()
            {
            }

            public override void Flush()
            {
            }

            public override void WriteBase64Text(byte[] trailBuffer, int trailCount, byte[] buffer, int offset, int count)
            {
            }

            public override void WriteBoolText(bool value)
            {
            }

            public override void WriteCData(string text)
            {
            }

            public override void WriteCharEntity(int ch)
            {
            }

            public override void WriteComment(string text)
            {
            }

            public override void WriteDateTimeText(DateTime value)
            {
            }

            public override void WriteDecimalText(decimal value)
            {
            }

            public override void WriteDeclaration()
            {
            }

            public override void WriteDoubleText(double value)
            {
            }

            public override void WriteEndAttribute()
            {
            }

            public override void WriteEndElement(string prefix, string localName)
            {
            }

            public override void WriteEndElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
            {
            }

            public override void WriteEndListText()
            {
            }

            public override void WriteEndStartElement(bool isEmpty)
            {
            }

            public override void WriteEscapedText(string value)
            {
            }

            public override void WriteEscapedText(XmlDictionaryString value)
            {
            }

            public override void WriteEscapedText(byte[] buffer, int offset, int count)
            {
            }

            public override void WriteEscapedText(char[] chars, int offset, int count)
            {
            }

            public override void WriteFloatText(float value)
            {
            }

            public override void WriteGuidText(Guid value)
            {
            }

            public override void WriteInt32Text(int value)
            {
            }

            public override void WriteInt64Text(long value)
            {
            }

            public override void WriteListSeparator()
            {
            }

            public override void WriteQualifiedName(string prefix, XmlDictionaryString localName)
            {
            }

            public override void WriteStartAttribute(string prefix, string localName)
            {
            }

            public override void WriteStartAttribute(string prefix, XmlDictionaryString localName)
            {
            }

            public override void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
            {
            }

            public override void WriteStartElement(string prefix, string localName)
            {
            }

            public override void WriteStartElement(string prefix, XmlDictionaryString localName)
            {
            }

            public override void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
            {
            }

            public override void WriteStartListText()
            {
            }

            public override void WriteText(string value)
            {
            }

            public override void WriteText(XmlDictionaryString value)
            {
            }

            public override void WriteText(byte[] buffer, int offset, int count)
            {
            }

            public override void WriteText(char[] chars, int offset, int count)
            {
            }

            public override void WriteTimeSpanText(TimeSpan value)
            {
            }

            public override void WriteUInt64Text(ulong value)
            {
            }

            public override void WriteUniqueIdText(UniqueId value)
            {
            }

            public override void WriteXmlnsAttribute(string prefix, string ns)
            {
            }

            public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
            {
            }

            public override void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength)
            {
            }
        }
    }
}

