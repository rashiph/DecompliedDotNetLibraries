namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    internal class MimeMessageReader
    {
        private static byte[] CRLFCRLF = new byte[] { 13, 10, 13, 10 };
        private bool getContentStreamCalled;
        private MimeHeaderReader mimeHeaderReader;
        private DelimittedStreamReader reader;

        public MimeMessageReader(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            this.reader = new DelimittedStreamReader(stream);
            this.mimeHeaderReader = new MimeHeaderReader(this.reader.GetNextStream(CRLFCRLF));
        }

        public Stream GetContentStream()
        {
            if (this.getContentStreamCalled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("MimeMessageGetContentStreamCalledAlready")));
            }
            this.mimeHeaderReader.Close();
            Stream nextStream = this.reader.GetNextStream(null);
            this.getContentStreamCalled = true;
            return nextStream;
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
    }
}

