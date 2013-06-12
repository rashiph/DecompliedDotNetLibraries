namespace System.Xml
{
    using System;

    internal class ReadContentAsBinaryHelper
    {
        private Base64Decoder base64Decoder;
        private BinHexDecoder binHexDecoder;
        private bool canReadValueChunk;
        private const int ChunkSize = 0x100;
        private IncrementalReadDecoder decoder;
        private bool isEnd;
        private XmlReader reader;
        private State state;
        private char[] valueChunk;
        private int valueChunkLength;
        private int valueOffset;

        internal ReadContentAsBinaryHelper(XmlReader reader)
        {
            this.reader = reader;
            this.canReadValueChunk = reader.CanReadValueChunk;
            if (this.canReadValueChunk)
            {
                this.valueChunk = new char[0x100];
            }
        }

        internal static ReadContentAsBinaryHelper CreateOrReset(ReadContentAsBinaryHelper helper, XmlReader reader)
        {
            if (helper == null)
            {
                return new ReadContentAsBinaryHelper(reader);
            }
            helper.Reset();
            return helper;
        }

        internal void Finish()
        {
            if (this.state != State.None)
            {
                while (this.MoveToNextContentNode(true))
                {
                }
                if (this.state == State.InReadElementContent)
                {
                    if (this.reader.NodeType != XmlNodeType.EndElement)
                    {
                        throw new XmlException("Xml_InvalidNodeType", this.reader.NodeType.ToString(), this.reader as IXmlLineInfo);
                    }
                    this.reader.Read();
                }
            }
            this.Reset();
        }

        private bool Init()
        {
            if (!this.MoveToNextContentNode(false))
            {
                return false;
            }
            this.state = State.InReadContent;
            this.isEnd = false;
            return true;
        }

        private void InitBase64Decoder()
        {
            if (this.base64Decoder == null)
            {
                this.base64Decoder = new Base64Decoder();
            }
            else
            {
                this.base64Decoder.Reset();
            }
            this.decoder = this.base64Decoder;
        }

        private void InitBinHexDecoder()
        {
            if (this.binHexDecoder == null)
            {
                this.binHexDecoder = new BinHexDecoder();
            }
            else
            {
                this.binHexDecoder.Reset();
            }
            this.decoder = this.binHexDecoder;
        }

        private bool InitOnElement()
        {
            bool isEmptyElement = this.reader.IsEmptyElement;
            this.reader.Read();
            if (isEmptyElement)
            {
                return false;
            }
            if (!this.MoveToNextContentNode(false))
            {
                if (this.reader.NodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException("Xml_InvalidNodeType", this.reader.NodeType.ToString(), this.reader as IXmlLineInfo);
                }
                this.reader.Read();
                return false;
            }
            this.state = State.InReadElementContent;
            this.isEnd = false;
            return true;
        }

        private bool MoveToNextContentNode(bool moveIfOnContentNode)
        {
        Label_0000:
            switch (this.reader.NodeType)
            {
                case XmlNodeType.Attribute:
                    return !moveIfOnContentNode;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    if (moveIfOnContentNode)
                    {
                        goto Label_0078;
                    }
                    return true;

                case XmlNodeType.EntityReference:
                    if (!this.reader.CanResolveEntity)
                    {
                        break;
                    }
                    this.reader.ResolveEntity();
                    goto Label_0078;

                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.EndEntity:
                    goto Label_0078;
            }
            return false;
        Label_0078:
            moveIfOnContentNode = false;
            if (this.reader.Read())
            {
                goto Label_0000;
            }
            return false;
        }

        internal int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            switch (this.state)
            {
                case State.None:
                    if (!this.reader.CanReadContentAs())
                    {
                        throw this.reader.CreateReadContentAsException("ReadContentAsBase64");
                    }
                    if (!this.Init())
                    {
                        return 0;
                    }
                    break;

                case State.InReadContent:
                    if (this.decoder != this.base64Decoder)
                    {
                        break;
                    }
                    return this.ReadContentAsBinary(buffer, index, count);

                case State.InReadElementContent:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));

                default:
                    return 0;
            }
            this.InitBase64Decoder();
            return this.ReadContentAsBinary(buffer, index, count);
        }

        private int ReadContentAsBinary(byte[] buffer, int index, int count)
        {
            if (this.isEnd)
            {
                this.Reset();
                return 0;
            }
            this.decoder.SetNextOutputBuffer(buffer, index, count);
        Label_001E:
            if (this.canReadValueChunk)
            {
                while (true)
                {
                    if (this.valueOffset < this.valueChunkLength)
                    {
                        int num = this.decoder.Decode(this.valueChunk, this.valueOffset, this.valueChunkLength - this.valueOffset);
                        this.valueOffset += num;
                    }
                    if (this.decoder.IsFull)
                    {
                        return this.decoder.DecodedCount;
                    }
                    this.valueChunkLength = this.reader.ReadValueChunk(this.valueChunk, 0, 0x100);
                    if (this.valueChunkLength == 0)
                    {
                        goto Label_0104;
                    }
                    this.valueOffset = 0;
                }
            }
            string str = this.reader.Value;
            int num2 = this.decoder.Decode(str, this.valueOffset, str.Length - this.valueOffset);
            this.valueOffset += num2;
            if (this.decoder.IsFull)
            {
                return this.decoder.DecodedCount;
            }
        Label_0104:
            this.valueOffset = 0;
            if (this.MoveToNextContentNode(true))
            {
                goto Label_001E;
            }
            this.isEnd = true;
            return this.decoder.DecodedCount;
        }

        internal int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            switch (this.state)
            {
                case State.None:
                    if (!this.reader.CanReadContentAs())
                    {
                        throw this.reader.CreateReadContentAsException("ReadContentAsBinHex");
                    }
                    if (!this.Init())
                    {
                        return 0;
                    }
                    break;

                case State.InReadContent:
                    if (this.decoder != this.binHexDecoder)
                    {
                        break;
                    }
                    return this.ReadContentAsBinary(buffer, index, count);

                case State.InReadElementContent:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));

                default:
                    return 0;
            }
            this.InitBinHexDecoder();
            return this.ReadContentAsBinary(buffer, index, count);
        }

        internal int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            switch (this.state)
            {
                case State.None:
                    if (this.reader.NodeType != XmlNodeType.Element)
                    {
                        throw this.reader.CreateReadElementContentAsException("ReadElementContentAsBase64");
                    }
                    if (!this.InitOnElement())
                    {
                        return 0;
                    }
                    break;

                case State.InReadContent:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));

                case State.InReadElementContent:
                    if (this.decoder != this.base64Decoder)
                    {
                        break;
                    }
                    return this.ReadElementContentAsBinary(buffer, index, count);

                default:
                    return 0;
            }
            this.InitBase64Decoder();
            return this.ReadElementContentAsBinary(buffer, index, count);
        }

        private int ReadElementContentAsBinary(byte[] buffer, int index, int count)
        {
            if (count != 0)
            {
                int num = this.ReadContentAsBinary(buffer, index, count);
                if (num > 0)
                {
                    return num;
                }
                if (this.reader.NodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException("Xml_InvalidNodeType", this.reader.NodeType.ToString(), this.reader as IXmlLineInfo);
                }
                this.reader.Read();
                this.state = State.None;
            }
            return 0;
        }

        internal int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            switch (this.state)
            {
                case State.None:
                    if (this.reader.NodeType != XmlNodeType.Element)
                    {
                        throw this.reader.CreateReadElementContentAsException("ReadElementContentAsBinHex");
                    }
                    if (!this.InitOnElement())
                    {
                        return 0;
                    }
                    break;

                case State.InReadContent:
                    throw new InvalidOperationException(Res.GetString("Xml_MixingBinaryContentMethods"));

                case State.InReadElementContent:
                    if (this.decoder != this.binHexDecoder)
                    {
                        break;
                    }
                    return this.ReadElementContentAsBinary(buffer, index, count);

                default:
                    return 0;
            }
            this.InitBinHexDecoder();
            return this.ReadElementContentAsBinary(buffer, index, count);
        }

        internal void Reset()
        {
            this.state = State.None;
            this.isEnd = false;
            this.valueOffset = 0;
        }

        private enum State
        {
            None,
            InReadContent,
            InReadElementContent
        }
    }
}

