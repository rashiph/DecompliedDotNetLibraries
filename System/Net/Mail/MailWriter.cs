namespace System.Net.Mail
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Net.Mime;

    internal class MailWriter : BaseWriter
    {
        private BufferBuilder bufferBuilder;
        private Stream contentStream;
        private static byte[] CRLF = new byte[] { 13, 10 };
        private bool isInContent;
        private int lineLength;
        private EventHandler onCloseHandler;
        private static AsyncCallback onWrite = new AsyncCallback(MailWriter.OnWrite);
        private Stream stream;
        private static int writerDefaultLineLength = 0x4c;

        internal MailWriter(Stream stream) : this(stream, writerDefaultLineLength)
        {
        }

        internal MailWriter(Stream stream, int lineLength)
        {
            this.bufferBuilder = new BufferBuilder();
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (lineLength < 0)
            {
                throw new ArgumentOutOfRangeException("lineLength");
            }
            this.stream = stream;
            this.lineLength = lineLength;
            this.onCloseHandler = new EventHandler(this.OnClose);
        }

        internal override IAsyncResult BeginGetContentStream(AsyncCallback callback, object state)
        {
            return this.BeginGetContentStream(ContentTransferEncoding.SevenBit, callback, state);
        }

        internal IAsyncResult BeginGetContentStream(ContentTransferEncoding contentTransferEncoding, AsyncCallback callback, object state)
        {
            MultiAsyncResult multiResult = new MultiAsyncResult(this, callback, state);
            Stream contentStream = this.GetContentStream(contentTransferEncoding, multiResult);
            if (!(multiResult.Result is Exception))
            {
                multiResult.Result = contentStream;
            }
            multiResult.CompleteSequence();
            return multiResult;
        }

        internal override void Close()
        {
            this.stream.Write(CRLF, 0, 2);
            this.stream.Close();
        }

        internal override Stream EndGetContentStream(IAsyncResult result)
        {
            object obj2 = MultiAsyncResult.End(result);
            if (obj2 is Exception)
            {
                throw ((Exception) obj2);
            }
            return (Stream) obj2;
        }

        private void Flush(MultiAsyncResult multiResult)
        {
            if (this.bufferBuilder.Length > 0)
            {
                if (multiResult != null)
                {
                    multiResult.Enter();
                    IAsyncResult asyncResult = this.stream.BeginWrite(this.bufferBuilder.GetBuffer(), 0, this.bufferBuilder.Length, onWrite, multiResult);
                    if (asyncResult.CompletedSynchronously)
                    {
                        this.stream.EndWrite(asyncResult);
                        multiResult.Leave();
                    }
                }
                else
                {
                    this.stream.Write(this.bufferBuilder.GetBuffer(), 0, this.bufferBuilder.Length);
                }
                this.bufferBuilder.Reset();
            }
        }

        internal override Stream GetContentStream()
        {
            return this.GetContentStream(ContentTransferEncoding.SevenBit);
        }

        internal Stream GetContentStream(ContentTransferEncoding contentTransferEncoding)
        {
            return this.GetContentStream(contentTransferEncoding, null);
        }

        private Stream GetContentStream(ContentTransferEncoding contentTransferEncoding, MultiAsyncResult multiResult)
        {
            if (this.isInContent)
            {
                throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
            }
            this.isInContent = true;
            this.bufferBuilder.Append(CRLF);
            this.Flush(multiResult);
            Stream stream = this.stream;
            if (contentTransferEncoding == ContentTransferEncoding.SevenBit)
            {
                stream = new SevenBitStream(stream);
            }
            else if (contentTransferEncoding == ContentTransferEncoding.QuotedPrintable)
            {
                stream = new QuotedPrintableStream(stream, this.lineLength);
            }
            else if (contentTransferEncoding == ContentTransferEncoding.Base64)
            {
                stream = new Base64Stream(stream, this.lineLength);
            }
            ClosableStream stream2 = new ClosableStream(stream, this.onCloseHandler);
            this.contentStream = stream2;
            return stream2;
        }

        private void OnClose(object sender, EventArgs args)
        {
            this.contentStream.Flush();
            this.contentStream = null;
        }

        private static void OnWrite(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult asyncState = (MultiAsyncResult) result.AsyncState;
                MailWriter context = (MailWriter) asyncState.Context;
                try
                {
                    context.stream.EndWrite(result);
                    asyncState.Leave();
                }
                catch (Exception exception)
                {
                    asyncState.Leave(exception);
                }
            }
        }

        private void WriteAndFold(string value)
        {
            if ((value.Length < writerDefaultLineLength) || value.Contains("\r\n"))
            {
                this.bufferBuilder.Append(value);
            }
            else
            {
                int offset = 0;
                int length = value.Length;
                while ((length - offset) > writerDefaultLineLength)
                {
                    int num3 = value.LastIndexOf(' ', (offset + writerDefaultLineLength) - 1, writerDefaultLineLength - 1);
                    if (num3 > -1)
                    {
                        this.bufferBuilder.Append(value, offset, num3 - offset);
                        this.bufferBuilder.Append(CRLF);
                        offset = num3;
                    }
                    else
                    {
                        this.bufferBuilder.Append(value, offset, writerDefaultLineLength);
                        offset += writerDefaultLineLength;
                    }
                }
                if (offset < length)
                {
                    this.bufferBuilder.Append(value, offset, length - offset);
                }
            }
        }

        internal override void WriteHeader(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (this.isInContent)
            {
                throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
            }
            this.bufferBuilder.Append(name);
            this.bufferBuilder.Append(": ");
            this.WriteAndFold(value);
            this.bufferBuilder.Append(CRLF);
        }

        internal override void WriteHeaders(NameValueCollection headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            if (this.isInContent)
            {
                throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
            }
            foreach (string str in headers)
            {
                foreach (string str2 in headers.GetValues(str))
                {
                    this.WriteHeader(str, str2);
                }
            }
        }
    }
}

