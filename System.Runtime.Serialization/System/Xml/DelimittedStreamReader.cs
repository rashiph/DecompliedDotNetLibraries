namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    internal class DelimittedStreamReader
    {
        private bool canGetNextStream = true;
        private DelimittedReadStream currentStream;
        private byte[] delimitter;
        private byte[] matchBuffer;
        private byte[] scratch;
        private BufferedReadStream stream;

        public DelimittedStreamReader(Stream stream)
        {
            this.stream = new BufferedReadStream(stream);
        }

        public void Close()
        {
            this.stream.Close();
        }

        private void Close(DelimittedReadStream caller)
        {
            if (this.currentStream == caller)
            {
                if (this.delimitter == null)
                {
                    this.stream.Close();
                }
                else
                {
                    if (this.scratch == null)
                    {
                        this.scratch = new byte[0x400];
                    }
                    while (this.Read(caller, this.scratch, 0, this.scratch.Length) != 0)
                    {
                    }
                }
                this.currentStream = null;
            }
        }

        public Stream GetNextStream(byte[] delimitter)
        {
            if (this.currentStream != null)
            {
                this.currentStream.Close();
                this.currentStream = null;
            }
            if (!this.canGetNextStream)
            {
                return null;
            }
            this.delimitter = delimitter;
            this.canGetNextStream = delimitter != null;
            this.currentStream = new DelimittedReadStream(this);
            return this.currentStream;
        }

        private MatchState MatchDelimitter(byte[] buffer, int start, int end)
        {
            if (this.delimitter.Length > (end - start))
            {
                for (int j = (end - start) - 1; j >= 1; j--)
                {
                    if (buffer[start + j] != this.delimitter[j])
                    {
                        return MatchState.False;
                    }
                }
                return MatchState.InsufficientData;
            }
            for (int i = this.delimitter.Length - 1; i >= 1; i--)
            {
                if (buffer[start + i] != this.delimitter[i])
                {
                    return MatchState.False;
                }
            }
            return MatchState.True;
        }

        private bool MatchRemainder(int start, int count)
        {
            if ((start + count) != this.delimitter.Length)
            {
                return false;
            }
            count--;
            while (count >= 0)
            {
                if (this.delimitter[start + count] != this.matchBuffer[count])
                {
                    return false;
                }
                count--;
            }
            return true;
        }

        private int ProcessRead(byte[] buffer, int offset, int read)
        {
            if (read != 0)
            {
                int index = offset;
                int end = offset + read;
                while (index < end)
                {
                    if (buffer[index] == this.delimitter[0])
                    {
                        switch (this.MatchDelimitter(buffer, index, end))
                        {
                            case MatchState.True:
                            {
                                int num3 = index - offset;
                                index += this.delimitter.Length;
                                this.stream.Push(buffer, index, end - index);
                                this.currentStream = null;
                                return num3;
                            }
                            case MatchState.InsufficientData:
                            {
                                int num4 = index - offset;
                                if (num4 <= 0)
                                {
                                    return -1;
                                }
                                this.stream.Push(buffer, index, end - index);
                                return num4;
                            }
                        }
                    }
                    index++;
                }
            }
            return read;
        }

        internal void Push(byte[] buffer, int offset, int count)
        {
            this.stream.Push(buffer, offset, count);
        }

        private int Read(DelimittedReadStream caller, byte[] buffer, int offset, int count)
        {
            if (this.currentStream != caller)
            {
                return 0;
            }
            int read = this.stream.Read(buffer, offset, count);
            if (read == 0)
            {
                this.canGetNextStream = false;
                this.currentStream = null;
                return read;
            }
            if (this.delimitter == null)
            {
                return read;
            }
            int num2 = this.ProcessRead(buffer, offset, read);
            if (num2 >= 0)
            {
                return num2;
            }
            if ((this.matchBuffer == null) || (this.matchBuffer.Length < (this.delimitter.Length - read)))
            {
                this.matchBuffer = new byte[this.delimitter.Length - read];
            }
            int num3 = this.stream.ReadBlock(this.matchBuffer, 0, this.delimitter.Length - read);
            if (this.MatchRemainder(read, num3))
            {
                this.currentStream = null;
                return 0;
            }
            this.stream.Push(this.matchBuffer, 0, num3);
            int index = 1;
            while (index < read)
            {
                if (buffer[index] == this.delimitter[0])
                {
                    break;
                }
                index++;
            }
            if (index < read)
            {
                this.stream.Push(buffer, offset + index, read - index);
            }
            return index;
        }

        private class DelimittedReadStream : Stream
        {
            private DelimittedStreamReader reader;

            public DelimittedReadStream(DelimittedStreamReader reader)
            {
                if (reader == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
                }
                this.reader = reader;
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("WriteNotSupportedOnStream", new object[] { base.GetType().FullName })));
            }

            public override void Close()
            {
                this.reader.Close(this);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("WriteNotSupportedOnStream", new object[] { base.GetType().FullName })));
            }

            public override void Flush()
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("WriteNotSupportedOnStream", new object[] { base.GetType().FullName })));
            }

            public override int Read(byte[] buffer, int offset, int count)
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
                return this.reader.Read(this, buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SeekNotSupportedOnStream", new object[] { base.GetType().FullName })));
            }

            public override void SetLength(long value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("WriteNotSupportedOnStream", new object[] { base.GetType().FullName })));
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("WriteNotSupportedOnStream", new object[] { base.GetType().FullName })));
            }

            public override bool CanRead
            {
                get
                {
                    return true;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return false;
                }
            }

            public override long Length
            {
                get
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SeekNotSupportedOnStream", new object[] { base.GetType().FullName })));
                }
            }

            public override long Position
            {
                get
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SeekNotSupportedOnStream", new object[] { base.GetType().FullName })));
                }
                set
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("SeekNotSupportedOnStream", new object[] { base.GetType().FullName })));
                }
            }
        }

        private enum MatchState
        {
            True,
            False,
            InsufficientData
        }
    }
}

