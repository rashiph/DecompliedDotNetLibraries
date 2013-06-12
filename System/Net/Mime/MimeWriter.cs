namespace System.Net.Mime
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Text;

    internal class MimeWriter : BaseWriter
    {
        private byte[] boundaryBytes;
        private BufferBuilder bufferBuilder;
        private Stream contentStream;
        private static byte[] CRLF = new byte[] { 13, 10 };
        private static byte[] DASHDASH = new byte[] { 0x2d, 0x2d };
        private static int DefaultLineLength = 0x4e;
        private bool isInContent;
        private int lineLength;
        private EventHandler onCloseHandler;
        private static AsyncCallback onWrite = new AsyncCallback(MimeWriter.OnWrite);
        private string preface;
        private Stream stream;
        private bool writeBoundary;

        internal MimeWriter(Stream stream, string boundary) : this(stream, boundary, null, DefaultLineLength)
        {
        }

        internal MimeWriter(Stream stream, string boundary, string preface, int lineLength)
        {
            this.bufferBuilder = new BufferBuilder();
            this.writeBoundary = true;
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (boundary == null)
            {
                throw new ArgumentNullException("boundary");
            }
            if (lineLength < 40)
            {
                throw new ArgumentOutOfRangeException("lineLength", lineLength, SR.GetString("MailWriterLineLengthTooSmall"));
            }
            this.stream = stream;
            this.lineLength = lineLength;
            this.onCloseHandler = new EventHandler(this.OnClose);
            this.boundaryBytes = Encoding.ASCII.GetBytes(boundary);
            this.preface = preface;
        }

        internal IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            MultiAsyncResult multiResult = new MultiAsyncResult(this, callback, state);
            this.Close(multiResult);
            multiResult.CompleteSequence();
            return multiResult;
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

        private void CheckBoundary()
        {
            if (this.preface != null)
            {
                this.bufferBuilder.Append(this.preface);
                this.preface = null;
            }
            if (this.writeBoundary)
            {
                this.bufferBuilder.Append(CRLF);
                this.bufferBuilder.Append(DASHDASH);
                this.bufferBuilder.Append(this.boundaryBytes);
                this.bufferBuilder.Append(CRLF);
                this.writeBoundary = false;
            }
        }

        internal override void Close()
        {
            this.Close(null);
            this.stream.Close();
        }

        private void Close(MultiAsyncResult multiResult)
        {
            this.bufferBuilder.Append(CRLF);
            this.bufferBuilder.Append(DASHDASH);
            this.bufferBuilder.Append(this.boundaryBytes);
            this.bufferBuilder.Append(DASHDASH);
            this.bufferBuilder.Append(CRLF);
            this.Flush(multiResult);
        }

        internal void EndClose(IAsyncResult result)
        {
            MultiAsyncResult.End(result);
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
            if (this.isInContent)
            {
                throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
            }
            this.isInContent = true;
            return this.GetContentStream(contentTransferEncoding, null);
        }

        private Stream GetContentStream(ContentTransferEncoding contentTransferEncoding, MultiAsyncResult multiResult)
        {
            this.CheckBoundary();
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
            if (this.contentStream == sender)
            {
                this.contentStream.Flush();
                this.contentStream = null;
                this.writeBoundary = true;
                this.isInContent = false;
            }
        }

        private static void OnWrite(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult asyncState = (MultiAsyncResult) result.AsyncState;
                MimeWriter context = (MimeWriter) asyncState.Context;
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

        private void WriteAndFold(string value, int startLength)
        {
            int num = 0;
            int num2 = 0;
            int offset = 0;
        Label_0006:
            if (num == value.Length)
            {
                if ((num - offset) > 0)
                {
                    this.bufferBuilder.Append(value, offset, num - offset);
                    return;
                }
            }
            else
            {
                if ((value[num] == ' ') || (value[num] == '\t'))
                {
                    if ((num - offset) >= (this.lineLength - startLength))
                    {
                        startLength = 0;
                        if (num2 == offset)
                        {
                            num2 = num;
                        }
                        this.bufferBuilder.Append(value, offset, num2 - offset);
                        this.bufferBuilder.Append(CRLF);
                        offset = num2;
                    }
                    num2 = num;
                }
                num++;
                goto Label_0006;
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
            this.CheckBoundary();
            this.bufferBuilder.Append(name);
            this.bufferBuilder.Append(": ");
            this.WriteAndFold(value, name.Length + 2);
            this.bufferBuilder.Append(CRLF);
        }

        internal override void WriteHeaders(NameValueCollection headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            foreach (string str in headers)
            {
                this.WriteHeader(str, headers[str]);
            }
        }
    }
}

