namespace System.Xml
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    internal sealed class XmlCanonicalWriter
    {
        private Attribute attribute;
        private int attributeCount;
        private Attribute[] attributes;
        private int depth;
        private Element element;
        private byte[] elementBuffer;
        private MemoryStream elementStream;
        private XmlUTF8NodeWriter elementWriter;
        private bool includeComments;
        private string[] inclusivePrefixes;
        private bool inStartElement;
        private static readonly bool[] isEscapedAttributeChar = new bool[] { 
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
            false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, 
            false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false
         };
        private static readonly bool[] isEscapedElementChar = new bool[] { 
            true, true, true, true, true, true, true, true, true, false, false, true, true, true, true, true, 
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
            false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 
            false, false, false, false, false, false, false, false, false, false, false, false, true, false, true, false
         };
        private const int maxBytesPerChar = 3;
        private Scope[] scopes;
        private XmlUTF8NodeWriter writer;
        private int xmlnsAttributeCount;
        private XmlnsAttribute[] xmlnsAttributes;
        private byte[] xmlnsBuffer;
        private const string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";
        private int xmlnsOffset;
        private int xmlnsStartOffset;

        private void AddAttribute(ref Attribute attribute)
        {
            if (this.attributes == null)
            {
                this.attributes = new Attribute[4];
            }
            else if (this.attributeCount == this.attributes.Length)
            {
                Attribute[] destinationArray = new Attribute[this.attributeCount * 2];
                Array.Copy(this.attributes, destinationArray, this.attributeCount);
                this.attributes = destinationArray;
            }
            this.attributes[this.attributeCount] = attribute;
            this.attributeCount++;
        }

        private void AddXmlnsAttribute(ref XmlnsAttribute xmlnsAttribute)
        {
            if (this.xmlnsAttributes == null)
            {
                this.xmlnsAttributes = new XmlnsAttribute[4];
            }
            else if (this.xmlnsAttributes.Length == this.xmlnsAttributeCount)
            {
                XmlnsAttribute[] destinationArray = new XmlnsAttribute[this.xmlnsAttributeCount * 2];
                Array.Copy(this.xmlnsAttributes, destinationArray, this.xmlnsAttributeCount);
                this.xmlnsAttributes = destinationArray;
            }
            if (((this.depth > 0) && (this.inclusivePrefixes != null)) && this.IsInclusivePrefix(ref xmlnsAttribute))
            {
                xmlnsAttribute.referred = true;
            }
            if (this.depth == 0)
            {
                this.xmlnsAttributes[this.xmlnsAttributeCount++] = xmlnsAttribute;
            }
            else
            {
                int xmlnsAttributeCount = this.scopes[this.depth - 1].xmlnsAttributeCount;
                bool flag = true;
                while (xmlnsAttributeCount < this.xmlnsAttributeCount)
                {
                    int num2 = this.Compare(ref xmlnsAttribute, ref this.xmlnsAttributes[xmlnsAttributeCount]);
                    if (num2 > 0)
                    {
                        xmlnsAttributeCount++;
                    }
                    else
                    {
                        if (num2 == 0)
                        {
                            this.xmlnsAttributes[xmlnsAttributeCount] = xmlnsAttribute;
                            flag = false;
                        }
                        break;
                    }
                }
                if (flag)
                {
                    Array.Copy(this.xmlnsAttributes, xmlnsAttributeCount, this.xmlnsAttributes, xmlnsAttributeCount + 1, this.xmlnsAttributeCount - xmlnsAttributeCount);
                    this.xmlnsAttributes[xmlnsAttributeCount] = xmlnsAttribute;
                    this.xmlnsAttributeCount++;
                }
            }
        }

        public void Close()
        {
            if (this.writer != null)
            {
                this.writer.Close();
            }
            if (this.elementWriter != null)
            {
                this.elementWriter.Close();
            }
            if ((this.elementStream != null) && (this.elementStream.Length > 0x200L))
            {
                this.elementStream = null;
            }
            this.elementBuffer = null;
            if ((this.scopes != null) && (this.scopes.Length > 0x10))
            {
                this.scopes = null;
            }
            if ((this.attributes != null) && (this.attributes.Length > 0x10))
            {
                this.attributes = null;
            }
            if ((this.xmlnsBuffer != null) && (this.xmlnsBuffer.Length > 0x400))
            {
                this.xmlnsAttributes = null;
                this.xmlnsBuffer = null;
            }
            this.inclusivePrefixes = null;
        }

        private int Compare(ref Attribute attribute1, ref Attribute attribute2)
        {
            int num = this.Compare(this.xmlnsBuffer, attribute1.nsOffset, attribute1.nsLength, attribute2.nsOffset, attribute2.nsLength);
            if (num == 0)
            {
                num = this.Compare(this.elementBuffer, attribute1.localNameOffset, attribute1.localNameLength, attribute2.localNameOffset, attribute2.localNameLength);
            }
            return num;
        }

        private int Compare(ref XmlnsAttribute xmlnsAttribute1, ref XmlnsAttribute xmlnsAttribute2)
        {
            return this.Compare(this.xmlnsBuffer, xmlnsAttribute1.prefixOffset, xmlnsAttribute1.prefixLength, xmlnsAttribute2.prefixOffset, xmlnsAttribute2.prefixLength);
        }

        private int Compare(byte[] buffer, int offset1, int length1, int offset2, int length2)
        {
            if (offset1 == offset2)
            {
                return (length1 - length2);
            }
            return this.Compare(buffer, offset1, length1, buffer, offset2, length2);
        }

        private int Compare(byte[] buffer1, int offset1, int length1, byte[] buffer2, int offset2, int length2)
        {
            int num = Math.Min(length1, length2);
            int num2 = 0;
            for (int i = 0; (i < num) && (num2 == 0); i++)
            {
                num2 = buffer1[offset1 + i] - buffer2[offset2 + i];
            }
            if (num2 == 0)
            {
                num2 = length1 - length2;
            }
            return num2;
        }

        private void EndElement()
        {
            this.depth--;
            this.xmlnsAttributeCount = this.scopes[this.depth].xmlnsAttributeCount;
            this.xmlnsOffset = this.scopes[this.depth].xmlnsOffset;
        }

        private void EnsureXmlnsBuffer(int byteCount)
        {
            if (this.xmlnsBuffer == null)
            {
                this.xmlnsBuffer = new byte[Math.Max(byteCount, 0x80)];
            }
            else if ((this.xmlnsOffset + byteCount) > this.xmlnsBuffer.Length)
            {
                byte[] dst = new byte[Math.Max((int) (this.xmlnsOffset + byteCount), (int) (this.xmlnsBuffer.Length * 2))];
                Buffer.BlockCopy(this.xmlnsBuffer, 0, dst, 0, this.xmlnsOffset);
                this.xmlnsBuffer = dst;
            }
        }

        private bool Equals(byte[] buffer1, int offset1, int length1, byte[] buffer2, int offset2, int length2)
        {
            if (length1 != length2)
            {
                return false;
            }
            for (int i = 0; i < length1; i++)
            {
                if (buffer1[offset1 + i] != buffer2[offset2 + i])
                {
                    return false;
                }
            }
            return true;
        }

        public void Flush()
        {
            this.ThrowIfClosed();
            this.writer.Flush();
        }

        private bool IsInclusivePrefix(ref XmlnsAttribute xmlnsAttribute)
        {
            for (int i = 0; i < this.inclusivePrefixes.Length; i++)
            {
                if ((this.inclusivePrefixes[i].Length == xmlnsAttribute.prefixLength) && (string.Compare(Encoding.UTF8.GetString(this.xmlnsBuffer, xmlnsAttribute.prefixOffset, xmlnsAttribute.prefixLength), this.inclusivePrefixes[i], StringComparison.Ordinal) == 0))
                {
                    return true;
                }
            }
            return false;
        }

        private void ResolvePrefix(ref Attribute attribute)
        {
            if (attribute.prefixLength != 0)
            {
                this.ResolvePrefix(attribute.prefixOffset, attribute.prefixLength, out attribute.nsOffset, out attribute.nsLength);
            }
        }

        private void ResolvePrefix(int prefixOffset, int prefixLength, out int nsOffset, out int nsLength)
        {
            int xmlnsAttributeCount = this.scopes[this.depth - 1].xmlnsAttributeCount;
            int index = this.xmlnsAttributeCount - 1;
            while (!this.Equals(this.elementBuffer, prefixOffset, prefixLength, this.xmlnsBuffer, this.xmlnsAttributes[index].prefixOffset, this.xmlnsAttributes[index].prefixLength))
            {
                index--;
            }
            nsOffset = this.xmlnsAttributes[index].nsOffset;
            nsLength = this.xmlnsAttributes[index].nsLength;
            if (index < xmlnsAttributeCount)
            {
                if (!this.xmlnsAttributes[index].referred)
                {
                    XmlnsAttribute xmlnsAttribute = this.xmlnsAttributes[index];
                    xmlnsAttribute.referred = true;
                    this.AddXmlnsAttribute(ref xmlnsAttribute);
                }
            }
            else
            {
                this.xmlnsAttributes[index].referred = true;
            }
        }

        private void ResolvePrefixes()
        {
            int num;
            int num2;
            this.ResolvePrefix(this.element.prefixOffset, this.element.prefixLength, out num, out num2);
            for (int i = 0; i < this.attributeCount; i++)
            {
                this.ResolvePrefix(ref this.attributes[i]);
            }
        }

        public void SetOutput(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (this.writer == null)
            {
                this.writer = new XmlUTF8NodeWriter(isEscapedAttributeChar, isEscapedElementChar);
            }
            this.writer.SetOutput(stream, false, null);
            if (this.elementStream == null)
            {
                this.elementStream = new MemoryStream();
            }
            if (this.elementWriter == null)
            {
                this.elementWriter = new XmlUTF8NodeWriter(isEscapedAttributeChar, isEscapedElementChar);
            }
            this.elementWriter.SetOutput(this.elementStream, false, null);
            if (this.xmlnsAttributes == null)
            {
                this.xmlnsAttributeCount = 0;
                this.xmlnsOffset = 0;
                this.WriteXmlnsAttribute("xml", "http://www.w3.org/XML/1998/namespace");
                this.WriteXmlnsAttribute("xmlns", "http://www.w3.org/2000/xmlns/");
                this.WriteXmlnsAttribute(string.Empty, string.Empty);
                this.xmlnsStartOffset = this.xmlnsOffset;
                for (int i = 0; i < 3; i++)
                {
                    this.xmlnsAttributes[i].referred = true;
                }
            }
            else
            {
                this.xmlnsAttributeCount = 3;
                this.xmlnsOffset = this.xmlnsStartOffset;
            }
            this.depth = 0;
            this.inStartElement = false;
            this.includeComments = includeComments;
            this.inclusivePrefixes = null;
            if (inclusivePrefixes != null)
            {
                this.inclusivePrefixes = new string[inclusivePrefixes.Length];
                for (int j = 0; j < inclusivePrefixes.Length; j++)
                {
                    if (inclusivePrefixes[j] == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.Runtime.Serialization.SR.GetString("InvalidInclusivePrefixListCollection"));
                    }
                    this.inclusivePrefixes[j] = inclusivePrefixes[j];
                }
            }
        }

        private void SortAttributes()
        {
            if (this.attributeCount < 0x10)
            {
                for (int i = 0; i < (this.attributeCount - 1); i++)
                {
                    int index = i;
                    for (int j = i + 1; j < this.attributeCount; j++)
                    {
                        if (this.Compare(ref this.attributes[j], ref this.attributes[index]) < 0)
                        {
                            index = j;
                        }
                    }
                    if (index != i)
                    {
                        Attribute attribute = this.attributes[i];
                        this.attributes[i] = this.attributes[index];
                        this.attributes[index] = attribute;
                    }
                }
            }
            else
            {
                new AttributeSorter(this).Sort();
            }
        }

        private void StartElement()
        {
            if (this.scopes == null)
            {
                this.scopes = new Scope[4];
            }
            else if (this.depth == this.scopes.Length)
            {
                Scope[] destinationArray = new Scope[this.depth * 2];
                Array.Copy(this.scopes, destinationArray, this.depth);
                this.scopes = destinationArray;
            }
            this.scopes[this.depth].xmlnsAttributeCount = this.xmlnsAttributeCount;
            this.scopes[this.depth].xmlnsOffset = this.xmlnsOffset;
            this.depth++;
            this.inStartElement = true;
            this.attributeCount = 0;
            this.elementStream.Position = 0L;
        }

        private void ThrowClosed()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().ToString()));
        }

        private void ThrowIfClosed()
        {
            if (this.writer == null)
            {
                this.ThrowClosed();
            }
        }

        public void WriteCharEntity(int ch)
        {
            this.ThrowIfClosed();
            if (ch <= 0xffff)
            {
                char[] chars = new char[] { (char) ch };
                this.WriteEscapedText(chars, 0, 1);
            }
            else
            {
                this.WriteText(ch);
            }
        }

        public void WriteComment(string value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            this.ThrowIfClosed();
            if (this.includeComments)
            {
                this.writer.WriteComment(value);
            }
        }

        public void WriteDeclaration()
        {
        }

        public void WriteEndAttribute()
        {
            this.ThrowIfClosed();
            this.elementWriter.WriteEndAttribute();
            this.attribute.length = this.elementWriter.Position - this.attribute.offset;
            this.AddAttribute(ref this.attribute);
        }

        public void WriteEndElement(string prefix, string localName)
        {
            if (prefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            this.ThrowIfClosed();
            this.writer.WriteEndElement(prefix, localName);
            this.EndElement();
        }

        public void WriteEndStartElement(bool isEmpty)
        {
            this.ThrowIfClosed();
            this.elementWriter.Flush();
            this.elementBuffer = this.elementStream.GetBuffer();
            this.inStartElement = false;
            this.ResolvePrefixes();
            this.writer.WriteStartElement(this.elementBuffer, this.element.prefixOffset, this.element.prefixLength, this.elementBuffer, this.element.localNameOffset, this.element.localNameLength);
            for (int i = this.scopes[this.depth - 1].xmlnsAttributeCount; i < this.xmlnsAttributeCount; i++)
            {
                int index = i - 1;
                bool flag = false;
                while (index >= 0)
                {
                    if (this.Equals(this.xmlnsBuffer, this.xmlnsAttributes[i].prefixOffset, this.xmlnsAttributes[i].prefixLength, this.xmlnsBuffer, this.xmlnsAttributes[index].prefixOffset, this.xmlnsAttributes[index].prefixLength))
                    {
                        if (!this.Equals(this.xmlnsBuffer, this.xmlnsAttributes[i].nsOffset, this.xmlnsAttributes[i].nsLength, this.xmlnsBuffer, this.xmlnsAttributes[index].nsOffset, this.xmlnsAttributes[index].nsLength))
                        {
                            break;
                        }
                        if (this.xmlnsAttributes[index].referred)
                        {
                            flag = true;
                            break;
                        }
                    }
                    index--;
                }
                if (!flag)
                {
                    this.WriteXmlnsAttribute(ref this.xmlnsAttributes[i]);
                }
            }
            if (this.attributeCount > 0)
            {
                if (this.attributeCount > 1)
                {
                    this.SortAttributes();
                }
                for (int j = 0; j < this.attributeCount; j++)
                {
                    this.writer.WriteText(this.elementBuffer, this.attributes[j].offset, this.attributes[j].length);
                }
            }
            this.writer.WriteEndStartElement(false);
            if (isEmpty)
            {
                this.writer.WriteEndElement(this.elementBuffer, this.element.prefixOffset, this.element.prefixLength, this.elementBuffer, this.element.localNameOffset, this.element.localNameLength);
                this.EndElement();
            }
            this.elementBuffer = null;
        }

        public void WriteEscapedText(string value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            this.ThrowIfClosed();
            if (this.depth > 0)
            {
                if (this.inStartElement)
                {
                    this.elementWriter.WriteEscapedText(value);
                }
                else
                {
                    this.writer.WriteEscapedText(value);
                }
            }
        }

        public void WriteEscapedText(byte[] chars, int offset, int count)
        {
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
            this.ThrowIfClosed();
            if (this.depth > 0)
            {
                if (this.inStartElement)
                {
                    this.elementWriter.WriteEscapedText(chars, offset, count);
                }
                else
                {
                    this.writer.WriteEscapedText(chars, offset, count);
                }
            }
        }

        public void WriteEscapedText(char[] chars, int offset, int count)
        {
            this.ThrowIfClosed();
            if (this.depth > 0)
            {
                if (this.inStartElement)
                {
                    this.elementWriter.WriteEscapedText(chars, offset, count);
                }
                else
                {
                    this.writer.WriteEscapedText(chars, offset, count);
                }
            }
        }

        public void WriteStartAttribute(string prefix, string localName)
        {
            if (prefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            this.ThrowIfClosed();
            this.attribute.offset = this.elementWriter.Position;
            this.attribute.length = 0;
            this.attribute.prefixOffset = this.attribute.offset + 1;
            this.attribute.prefixLength = Encoding.UTF8.GetByteCount(prefix);
            this.attribute.localNameOffset = (this.attribute.prefixOffset + this.attribute.prefixLength) + ((this.attribute.prefixLength != 0) ? 1 : 0);
            this.attribute.localNameLength = Encoding.UTF8.GetByteCount(localName);
            this.attribute.nsOffset = 0;
            this.attribute.nsLength = 0;
            this.elementWriter.WriteStartAttribute(prefix, localName);
        }

        public void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            if (prefixBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("prefixBuffer"));
            }
            if (prefixOffset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (prefixOffset > prefixBuffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { prefixBuffer.Length })));
            }
            if (prefixLength < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (prefixLength > (prefixBuffer.Length - prefixOffset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { prefixBuffer.Length - prefixOffset })));
            }
            if (localNameBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localNameBuffer"));
            }
            if (localNameOffset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameOffset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (localNameOffset > localNameBuffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameOffset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { localNameBuffer.Length })));
            }
            if (localNameLength < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameLength", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (localNameLength > (localNameBuffer.Length - localNameOffset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameLength", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { localNameBuffer.Length - localNameOffset })));
            }
            this.ThrowIfClosed();
            this.attribute.offset = this.elementWriter.Position;
            this.attribute.length = 0;
            this.attribute.prefixOffset = this.attribute.offset + 1;
            this.attribute.prefixLength = prefixLength;
            this.attribute.localNameOffset = (this.attribute.prefixOffset + prefixLength) + ((prefixLength != 0) ? 1 : 0);
            this.attribute.localNameLength = localNameLength;
            this.attribute.nsOffset = 0;
            this.attribute.nsLength = 0;
            this.elementWriter.WriteStartAttribute(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
        }

        public void WriteStartElement(string prefix, string localName)
        {
            if (prefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            this.ThrowIfClosed();
            bool flag = this.depth == 0;
            this.StartElement();
            this.element.prefixOffset = this.elementWriter.Position + 1;
            this.element.prefixLength = Encoding.UTF8.GetByteCount(prefix);
            this.element.localNameOffset = (this.element.prefixOffset + this.element.prefixLength) + ((this.element.prefixLength != 0) ? 1 : 0);
            this.element.localNameLength = Encoding.UTF8.GetByteCount(localName);
            this.elementWriter.WriteStartElement(prefix, localName);
            if (flag && (this.inclusivePrefixes != null))
            {
                for (int i = 0; i < this.scopes[0].xmlnsAttributeCount; i++)
                {
                    if (this.IsInclusivePrefix(ref this.xmlnsAttributes[i]))
                    {
                        XmlnsAttribute xmlnsAttribute = this.xmlnsAttributes[i];
                        this.AddXmlnsAttribute(ref xmlnsAttribute);
                    }
                }
            }
        }

        public void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            if (prefixBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("prefixBuffer"));
            }
            if (prefixOffset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (prefixOffset > prefixBuffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { prefixBuffer.Length })));
            }
            if (prefixLength < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (prefixLength > (prefixBuffer.Length - prefixOffset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { prefixBuffer.Length - prefixOffset })));
            }
            if (localNameBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localNameBuffer"));
            }
            if (localNameOffset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameOffset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (localNameOffset > localNameBuffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameOffset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { localNameBuffer.Length })));
            }
            if (localNameLength < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameLength", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (localNameLength > (localNameBuffer.Length - localNameOffset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameLength", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { localNameBuffer.Length - localNameOffset })));
            }
            this.ThrowIfClosed();
            bool flag = this.depth == 0;
            this.StartElement();
            this.element.prefixOffset = this.elementWriter.Position + 1;
            this.element.prefixLength = prefixLength;
            this.element.localNameOffset = (this.element.prefixOffset + prefixLength) + ((prefixLength != 0) ? 1 : 0);
            this.element.localNameLength = localNameLength;
            this.elementWriter.WriteStartElement(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
            if (flag && (this.inclusivePrefixes != null))
            {
                for (int i = 0; i < this.scopes[0].xmlnsAttributeCount; i++)
                {
                    if (this.IsInclusivePrefix(ref this.xmlnsAttributes[i]))
                    {
                        XmlnsAttribute xmlnsAttribute = this.xmlnsAttributes[i];
                        this.AddXmlnsAttribute(ref xmlnsAttribute);
                    }
                }
            }
        }

        public void WriteText(int ch)
        {
            this.ThrowIfClosed();
            if (this.inStartElement)
            {
                this.elementWriter.WriteText(ch);
            }
            else
            {
                this.writer.WriteText(ch);
            }
        }

        public void WriteText(string value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            }
            if (value.Length > 0)
            {
                if (this.inStartElement)
                {
                    this.elementWriter.WriteText(value);
                }
                else
                {
                    this.writer.WriteText(value);
                }
            }
        }

        public void WriteText(byte[] chars, int offset, int count)
        {
            this.ThrowIfClosed();
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
            if (this.inStartElement)
            {
                this.elementWriter.WriteText(chars, offset, count);
            }
            else
            {
                this.writer.WriteText(chars, offset, count);
            }
        }

        public void WriteText(char[] chars, int offset, int count)
        {
            this.ThrowIfClosed();
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
            if (this.inStartElement)
            {
                this.elementWriter.WriteText(chars, offset, count);
            }
            else
            {
                this.writer.WriteText(chars, offset, count);
            }
        }

        private void WriteXmlnsAttribute(ref XmlnsAttribute xmlnsAttribute)
        {
            if (xmlnsAttribute.referred)
            {
                this.writer.WriteXmlnsAttribute(this.xmlnsBuffer, xmlnsAttribute.prefixOffset, xmlnsAttribute.prefixLength, this.xmlnsBuffer, xmlnsAttribute.nsOffset, xmlnsAttribute.nsLength);
            }
        }

        public void WriteXmlnsAttribute(string prefix, string ns)
        {
            XmlnsAttribute attribute;
            if (prefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            this.ThrowIfClosed();
            if (prefix.Length > (0x7fffffff - ns.Length))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("ns", System.Runtime.Serialization.SR.GetString("CombinedPrefixNSLength", new object[] { 0x2aaaaaaa })));
            }
            int num = prefix.Length + ns.Length;
            if (num > 0x2aaaaaaa)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("ns", System.Runtime.Serialization.SR.GetString("CombinedPrefixNSLength", new object[] { 0x2aaaaaaa })));
            }
            this.EnsureXmlnsBuffer(num * 3);
            attribute.prefixOffset = this.xmlnsOffset;
            attribute.prefixLength = Encoding.UTF8.GetBytes(prefix, 0, prefix.Length, this.xmlnsBuffer, this.xmlnsOffset);
            this.xmlnsOffset += attribute.prefixLength;
            attribute.nsOffset = this.xmlnsOffset;
            attribute.nsLength = Encoding.UTF8.GetBytes(ns, 0, ns.Length, this.xmlnsBuffer, this.xmlnsOffset);
            this.xmlnsOffset += attribute.nsLength;
            attribute.referred = false;
            this.AddXmlnsAttribute(ref attribute);
        }

        public void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength)
        {
            XmlnsAttribute attribute;
            if (prefixBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("prefixBuffer"));
            }
            if (prefixOffset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (prefixOffset > prefixBuffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { prefixBuffer.Length })));
            }
            if (prefixLength < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (prefixLength > (prefixBuffer.Length - prefixOffset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { prefixBuffer.Length - prefixOffset })));
            }
            if (nsBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("nsBuffer"));
            }
            if (nsOffset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsOffset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (nsOffset > nsBuffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsOffset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { nsBuffer.Length })));
            }
            if (nsLength < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsLength", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (nsLength > (nsBuffer.Length - nsOffset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsLength", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { nsBuffer.Length - nsOffset })));
            }
            this.ThrowIfClosed();
            if (prefixLength > (0x7fffffff - nsLength))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsLength", System.Runtime.Serialization.SR.GetString("CombinedPrefixNSLength", new object[] { 0x7fffffff })));
            }
            this.EnsureXmlnsBuffer(prefixLength + nsLength);
            attribute.prefixOffset = this.xmlnsOffset;
            attribute.prefixLength = prefixLength;
            Buffer.BlockCopy(prefixBuffer, prefixOffset, this.xmlnsBuffer, this.xmlnsOffset, prefixLength);
            this.xmlnsOffset += prefixLength;
            attribute.nsOffset = this.xmlnsOffset;
            attribute.nsLength = nsLength;
            Buffer.BlockCopy(nsBuffer, nsOffset, this.xmlnsBuffer, this.xmlnsOffset, nsLength);
            this.xmlnsOffset += nsLength;
            attribute.referred = false;
            this.AddXmlnsAttribute(ref attribute);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Attribute
        {
            public int prefixOffset;
            public int prefixLength;
            public int localNameOffset;
            public int localNameLength;
            public int nsOffset;
            public int nsLength;
            public int offset;
            public int length;
        }

        private class AttributeSorter : IComparer
        {
            private XmlCanonicalWriter writer;

            public AttributeSorter(XmlCanonicalWriter writer)
            {
                this.writer = writer;
            }

            public int Compare(object obj1, object obj2)
            {
                int index = (int) obj1;
                int num2 = (int) obj2;
                return this.writer.Compare(ref this.writer.attributes[index], ref this.writer.attributes[num2]);
            }

            public void Sort()
            {
                object[] array = new object[this.writer.attributeCount];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = i;
                }
                Array.Sort(array, this);
                XmlCanonicalWriter.Attribute[] attributeArray = new XmlCanonicalWriter.Attribute[this.writer.attributes.Length];
                for (int j = 0; j < array.Length; j++)
                {
                    attributeArray[j] = this.writer.attributes[(int) array[j]];
                }
                this.writer.attributes = attributeArray;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Element
        {
            public int prefixOffset;
            public int prefixLength;
            public int localNameOffset;
            public int localNameLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Scope
        {
            public int xmlnsAttributeCount;
            public int xmlnsOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XmlnsAttribute
        {
            public int prefixOffset;
            public int prefixLength;
            public int nsOffset;
            public int nsLength;
            public bool referred;
        }
    }
}

