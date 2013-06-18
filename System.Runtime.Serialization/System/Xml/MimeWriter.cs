namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;

    internal class MimeWriter
    {
        private byte[] boundaryBytes;
        private BufferedWrite bufferedWrite;
        private Stream contentStream;
        private MimeWriterState state;
        private Stream stream;

        internal MimeWriter(Stream stream, string boundary)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (boundary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("boundary");
            }
            this.stream = stream;
            this.boundaryBytes = GetBoundaryBytes(boundary);
            this.state = MimeWriterState.Start;
            this.bufferedWrite = new BufferedWrite();
        }

        internal void Close()
        {
            if (this.state == MimeWriterState.Closed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("MimeWriterInvalidStateForClose", new object[] { this.state.ToString() })));
            }
            this.state = MimeWriterState.Closed;
            if (this.contentStream != null)
            {
                this.contentStream.Flush();
                this.contentStream = null;
            }
            this.bufferedWrite.Write(this.boundaryBytes);
            this.bufferedWrite.Write(MimeGlobals.DASHDASH);
            this.bufferedWrite.Write(MimeGlobals.CRLF);
            this.Flush();
        }

        private void Flush()
        {
            if (this.bufferedWrite.Length > 0)
            {
                this.stream.Write(this.bufferedWrite.GetBuffer(), 0, this.bufferedWrite.Length);
                this.bufferedWrite.Reset();
            }
        }

        internal static byte[] GetBoundaryBytes(string boundary)
        {
            byte[] bytes = new byte[boundary.Length + MimeGlobals.BoundaryPrefix.Length];
            for (int i = 0; i < MimeGlobals.BoundaryPrefix.Length; i++)
            {
                bytes[i] = MimeGlobals.BoundaryPrefix[i];
            }
            Encoding.ASCII.GetBytes(boundary, 0, boundary.Length, bytes, MimeGlobals.BoundaryPrefix.Length);
            return bytes;
        }

        internal int GetBoundarySize()
        {
            return this.boundaryBytes.Length;
        }

        internal Stream GetContentStream()
        {
            switch (this.state)
            {
                case MimeWriterState.Content:
                case MimeWriterState.Closed:
                case MimeWriterState.Start:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("MimeWriterInvalidStateForContent", new object[] { this.state.ToString() })));
            }
            this.state = MimeWriterState.Content;
            this.bufferedWrite.Write(MimeGlobals.CRLF);
            this.Flush();
            this.contentStream = this.stream;
            return this.contentStream;
        }

        internal static int GetHeaderSize(string name, string value, int maxSizeInBytes)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            int offset = XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, 0, MimeGlobals.COLONSPACE.Length + MimeGlobals.CRLF.Length);
            offset += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, offset, name.Length);
            return (offset + XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, offset, value.Length));
        }

        internal void StartPart()
        {
            switch (this.state)
            {
                case MimeWriterState.StartPart:
                case MimeWriterState.Closed:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("MimeWriterInvalidStateForStartPart", new object[] { this.state.ToString() })));
            }
            this.state = MimeWriterState.StartPart;
            if (this.contentStream != null)
            {
                this.contentStream.Flush();
                this.contentStream = null;
            }
            this.bufferedWrite.Write(this.boundaryBytes);
            this.bufferedWrite.Write(MimeGlobals.CRLF);
        }

        internal void StartPreface()
        {
            if (this.state != MimeWriterState.Start)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("MimeWriterInvalidStateForStartPreface", new object[] { this.state.ToString() })));
            }
            this.state = MimeWriterState.StartPreface;
        }

        internal void WriteHeader(string name, string value)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            switch (this.state)
            {
                case MimeWriterState.Content:
                case MimeWriterState.Closed:
                case MimeWriterState.Start:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("MimeWriterInvalidStateForHeader", new object[] { this.state.ToString() })));
            }
            this.state = MimeWriterState.Header;
            this.bufferedWrite.Write(name);
            this.bufferedWrite.Write(MimeGlobals.COLONSPACE);
            this.bufferedWrite.Write(value);
            this.bufferedWrite.Write(MimeGlobals.CRLF);
        }

        internal MimeWriterState WriteState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.state;
            }
        }
    }
}

