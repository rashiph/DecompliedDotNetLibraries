namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;

    internal class MimeReader
    {
        private byte[] boundaryBytes;
        private string content;
        private static byte[] CRLFCRLF = new byte[] { 13, 10, 13, 10 };
        private Stream currentStream;
        private MimeHeaderReader mimeHeaderReader;
        private DelimittedStreamReader reader;
        private byte[] scratch = new byte[2];

        public MimeReader(Stream stream, string boundary)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (boundary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("boundary");
            }
            this.reader = new DelimittedStreamReader(stream);
            this.boundaryBytes = MimeWriter.GetBoundaryBytes(boundary);
            this.reader.Push(this.boundaryBytes, 0, 2);
        }

        private int BlockRead(Stream stream, byte[] buffer, int offset, int count)
        {
            int num = 0;
            do
            {
                int num2 = stream.Read(buffer, offset + num, count - num);
                if (num2 == 0)
                {
                    return num;
                }
                num += num2;
            }
            while (num < count);
            return num;
        }

        public void Close()
        {
            this.reader.Close();
        }

        public Stream GetContentStream()
        {
            this.mimeHeaderReader.Close();
            return this.reader.GetNextStream(this.boundaryBytes);
        }

        public MimeHeaders ReadHeaders(int maxBuffer, ref int remaining)
        {
            MimeHeaders headers = new MimeHeaders();
            while (this.mimeHeaderReader.Read(maxBuffer, ref remaining))
            {
                headers.Add(this.mimeHeaderReader.Name, this.mimeHeaderReader.Value, ref remaining);
            }
            return headers;
        }

        public bool ReadNextPart()
        {
            string preface = this.Preface;
            if (this.currentStream != null)
            {
                this.currentStream.Close();
                this.currentStream = null;
            }
            Stream nextStream = this.reader.GetNextStream(CRLFCRLF);
            if (nextStream == null)
            {
                return false;
            }
            if (this.BlockRead(nextStream, this.scratch, 0, 2) == 2)
            {
                if ((this.scratch[0] == 13) && (this.scratch[1] == 10))
                {
                    if (this.mimeHeaderReader == null)
                    {
                        this.mimeHeaderReader = new MimeHeaderReader(nextStream);
                    }
                    else
                    {
                        this.mimeHeaderReader.Reset(nextStream);
                    }
                    return true;
                }
                if (((this.scratch[0] == 0x2d) && (this.scratch[1] == 0x2d)) && ((this.BlockRead(nextStream, this.scratch, 0, 2) < 2) || ((this.scratch[0] == 13) && (this.scratch[1] == 10))))
                {
                    return false;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("MimeReaderTruncated")));
        }

        public string Preface
        {
            get
            {
                if (this.content == null)
                {
                    Stream nextStream = this.reader.GetNextStream(this.boundaryBytes);
                    this.content = new StreamReader(nextStream, Encoding.ASCII, false, 0x100).ReadToEnd();
                    nextStream.Close();
                    if (this.content == null)
                    {
                        this.content = string.Empty;
                    }
                }
                return this.content;
            }
        }
    }
}

