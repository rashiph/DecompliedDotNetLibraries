namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class XmlBuffer
    {
        private byte[] buffer;
        private BufferState bufferState;
        private int offset;
        private XmlDictionaryReaderQuotas quotas;
        private List<Section> sections;
        private BufferedOutputStream stream;
        private XmlDictionaryWriter writer;

        public XmlBuffer(int maxBufferSize)
        {
            if (maxBufferSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize", maxBufferSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            int initialSize = Math.Min(0x200, maxBufferSize);
            this.stream = new BufferManagerOutputStream("XmlBufferQuotaExceeded", initialSize, maxBufferSize, BufferManager.CreateBufferManager(0L, 0x7fffffff));
            this.sections = new List<Section>(1);
        }

        public void Close()
        {
            int num;
            if (this.bufferState != BufferState.Created)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidStateException());
            }
            this.bufferState = BufferState.Reading;
            this.buffer = this.stream.ToArray(out num);
            this.writer = null;
            this.stream = null;
        }

        public void CloseSection()
        {
            if (this.bufferState != BufferState.Writing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidStateException());
            }
            this.writer.Close();
            this.bufferState = BufferState.Created;
            int size = ((int) this.stream.Length) - this.offset;
            this.sections.Add(new Section(this.offset, size, this.quotas));
            this.offset += size;
        }

        private Exception CreateInvalidStateException()
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("XmlBufferInInvalidState"));
        }

        public XmlDictionaryReader GetReader(int sectionIndex)
        {
            if (this.bufferState != BufferState.Reading)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidStateException());
            }
            Section section = this.sections[sectionIndex];
            XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(this.buffer, section.Offset, section.Size, XD.Dictionary, section.Quotas, null, null);
            reader.MoveToContent();
            return reader;
        }

        public XmlDictionaryWriter OpenSection(XmlDictionaryReaderQuotas quotas)
        {
            if (this.bufferState != BufferState.Created)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidStateException());
            }
            this.bufferState = BufferState.Writing;
            this.quotas = new XmlDictionaryReaderQuotas();
            quotas.CopyTo(this.quotas);
            if (this.writer == null)
            {
                this.writer = XmlDictionaryWriter.CreateBinaryWriter(this.stream, XD.Dictionary, null, true);
            }
            else
            {
                ((IXmlBinaryWriterInitializer) this.writer).SetOutput(this.stream, XD.Dictionary, null, true);
            }
            return this.writer;
        }

        public void WriteTo(int sectionIndex, XmlWriter writer)
        {
            if (this.bufferState != BufferState.Reading)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateInvalidStateException());
            }
            XmlDictionaryReader reader = this.GetReader(sectionIndex);
            try
            {
                writer.WriteNode(reader, false);
            }
            finally
            {
                reader.Close();
            }
        }

        public int BufferSize
        {
            get
            {
                return this.buffer.Length;
            }
        }

        public int SectionCount
        {
            get
            {
                return this.sections.Count;
            }
        }

        private enum BufferState
        {
            Created,
            Writing,
            Reading
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Section
        {
            private int offset;
            private int size;
            private XmlDictionaryReaderQuotas quotas;
            public Section(int offset, int size, XmlDictionaryReaderQuotas quotas)
            {
                this.offset = offset;
                this.size = size;
                this.quotas = quotas;
            }

            public int Offset
            {
                get
                {
                    return this.offset;
                }
            }
            public int Size
            {
                get
                {
                    return this.size;
                }
            }
            public XmlDictionaryReaderQuotas Quotas
            {
                get
                {
                    return this.quotas;
                }
            }
        }
    }
}

