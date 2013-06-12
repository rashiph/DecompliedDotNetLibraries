namespace System.Net.Mime
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Mail;

    internal class MimePart : MimeBasePart, IDisposable
    {
        private const int maxBufferSize = 0x4400;
        private AsyncCallback readCallback;
        private System.IO.Stream stream;
        private bool streamSet;
        private bool streamUsedOnce;
        private AsyncCallback writeCallback;

        internal MimePart()
        {
        }

        internal override IAsyncResult BeginSend(BaseWriter writer, AsyncCallback callback, object state)
        {
            writer.WriteHeaders(base.Headers);
            MimeBasePart.MimePartAsyncResult result = new MimeBasePart.MimePartAsyncResult(this, state, callback);
            MimePartContext context = new MimePartContext(writer, result);
            this.ResetStream();
            this.streamUsedOnce = true;
            IAsyncResult result2 = writer.BeginGetContentStream(new AsyncCallback(this.ContentStreamCallback), context);
            if (result2.CompletedSynchronously)
            {
                this.ContentStreamCallbackHandler(result2);
            }
            return result;
        }

        internal void Complete(IAsyncResult result, Exception e)
        {
            MimePartContext asyncState = (MimePartContext) result.AsyncState;
            if (asyncState.completed)
            {
                throw e;
            }
            try
            {
                if (asyncState.outputStream != null)
                {
                    asyncState.outputStream.Close();
                }
            }
            catch (Exception exception)
            {
                if (e == null)
                {
                    e = exception;
                }
            }
            asyncState.completed = true;
            asyncState.result.InvokeCallback(e);
        }

        internal void ContentStreamCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((MimePartContext) result.AsyncState).completedSynchronously = false;
                try
                {
                    this.ContentStreamCallbackHandler(result);
                }
                catch (Exception exception)
                {
                    this.Complete(result, exception);
                }
            }
        }

        internal void ContentStreamCallbackHandler(IAsyncResult result)
        {
            MimePartContext asyncState = (MimePartContext) result.AsyncState;
            System.IO.Stream stream = asyncState.writer.EndGetContentStream(result);
            asyncState.outputStream = this.GetEncodedStream(stream);
            this.readCallback = new AsyncCallback(this.ReadCallback);
            this.writeCallback = new AsyncCallback(this.WriteCallback);
            IAsyncResult result2 = this.Stream.BeginRead(asyncState.buffer, 0, asyncState.buffer.Length, this.readCallback, asyncState);
            if (result2.CompletedSynchronously)
            {
                this.ReadCallbackHandler(result2);
            }
        }

        public void Dispose()
        {
            if (this.stream != null)
            {
                this.stream.Close();
            }
        }

        internal System.IO.Stream GetEncodedStream(System.IO.Stream stream)
        {
            System.IO.Stream stream2 = stream;
            if (this.TransferEncoding == System.Net.Mime.TransferEncoding.Base64)
            {
                return new Base64Stream(stream2, new Base64WriteStateInfo());
            }
            if (this.TransferEncoding == System.Net.Mime.TransferEncoding.QuotedPrintable)
            {
                return new QuotedPrintableStream(stream2, true);
            }
            if (this.TransferEncoding == System.Net.Mime.TransferEncoding.SevenBit)
            {
                stream2 = new SevenBitStream(stream2);
            }
            return stream2;
        }

        internal void ReadCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((MimePartContext) result.AsyncState).completedSynchronously = false;
                try
                {
                    this.ReadCallbackHandler(result);
                }
                catch (Exception exception)
                {
                    this.Complete(result, exception);
                }
            }
        }

        internal void ReadCallbackHandler(IAsyncResult result)
        {
            MimePartContext asyncState = (MimePartContext) result.AsyncState;
            asyncState.bytesLeft = this.Stream.EndRead(result);
            if (asyncState.bytesLeft > 0)
            {
                IAsyncResult result2 = asyncState.outputStream.BeginWrite(asyncState.buffer, 0, asyncState.bytesLeft, this.writeCallback, asyncState);
                if (result2.CompletedSynchronously)
                {
                    this.WriteCallbackHandler(result2);
                }
            }
            else
            {
                this.Complete(result, null);
            }
        }

        internal void ResetStream()
        {
            if (this.streamUsedOnce)
            {
                if (!this.Stream.CanSeek)
                {
                    throw new InvalidOperationException(SR.GetString("MimePartCantResetStream"));
                }
                this.Stream.Seek(0L, SeekOrigin.Begin);
                this.streamUsedOnce = false;
            }
        }

        internal override void Send(BaseWriter writer)
        {
            if (this.Stream != null)
            {
                int num;
                byte[] buffer = new byte[0x4400];
                writer.WriteHeaders(base.Headers);
                System.IO.Stream contentStream = writer.GetContentStream();
                contentStream = this.GetEncodedStream(contentStream);
                this.ResetStream();
                this.streamUsedOnce = true;
                while ((num = this.Stream.Read(buffer, 0, 0x4400)) > 0)
                {
                    contentStream.Write(buffer, 0, num);
                }
                contentStream.Close();
            }
        }

        internal void SetContent(System.IO.Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (this.streamSet)
            {
                this.stream.Close();
                this.stream = null;
                this.streamSet = false;
            }
            this.stream = stream;
            this.streamSet = true;
            this.streamUsedOnce = false;
            this.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
        }

        internal void SetContent(System.IO.Stream stream, ContentType contentType)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            base.contentType = contentType;
            this.SetContent(stream);
        }

        internal void SetContent(System.IO.Stream stream, string name, string mimeType)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if ((mimeType != null) && (mimeType != string.Empty))
            {
                base.contentType = new ContentType(mimeType);
            }
            if ((name != null) && (name != string.Empty))
            {
                base.ContentType.Name = name;
            }
            this.SetContent(stream);
        }

        internal void WriteCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((MimePartContext) result.AsyncState).completedSynchronously = false;
                try
                {
                    this.WriteCallbackHandler(result);
                }
                catch (Exception exception)
                {
                    this.Complete(result, exception);
                }
            }
        }

        internal void WriteCallbackHandler(IAsyncResult result)
        {
            MimePartContext asyncState = (MimePartContext) result.AsyncState;
            asyncState.outputStream.EndWrite(result);
            IAsyncResult result2 = this.Stream.BeginRead(asyncState.buffer, 0, asyncState.buffer.Length, this.readCallback, asyncState);
            if (result2.CompletedSynchronously)
            {
                this.ReadCallbackHandler(result2);
            }
        }

        internal System.Net.Mime.ContentDisposition ContentDisposition
        {
            get
            {
                return base.contentDisposition;
            }
            set
            {
                base.contentDisposition = value;
                if (value == null)
                {
                    ((HeaderCollection) base.Headers).InternalRemove(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition));
                }
                else
                {
                    base.contentDisposition.PersistIfNeeded((HeaderCollection) base.Headers, true);
                }
            }
        }

        internal System.IO.Stream Stream
        {
            get
            {
                return this.stream;
            }
        }

        internal System.Net.Mime.TransferEncoding TransferEncoding
        {
            get
            {
                if (base.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)].Equals("base64", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Net.Mime.TransferEncoding.Base64;
                }
                if (base.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)].Equals("quoted-printable", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Net.Mime.TransferEncoding.QuotedPrintable;
                }
                if (base.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)].Equals("7bit", StringComparison.OrdinalIgnoreCase))
                {
                    return System.Net.Mime.TransferEncoding.SevenBit;
                }
                return System.Net.Mime.TransferEncoding.Unknown;
            }
            set
            {
                if (value == System.Net.Mime.TransferEncoding.Base64)
                {
                    base.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)] = "base64";
                }
                else if (value == System.Net.Mime.TransferEncoding.QuotedPrintable)
                {
                    base.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)] = "quoted-printable";
                }
                else
                {
                    if (value != System.Net.Mime.TransferEncoding.SevenBit)
                    {
                        throw new NotSupportedException(SR.GetString("MimeTransferEncodingNotSupported", new object[] { value }));
                    }
                    base.Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)] = "7bit";
                }
            }
        }

        internal class MimePartContext
        {
            internal byte[] buffer;
            internal int bytesLeft;
            internal bool completed;
            internal bool completedSynchronously = true;
            internal Stream outputStream;
            internal LazyAsyncResult result;
            internal BaseWriter writer;

            internal MimePartContext(BaseWriter writer, LazyAsyncResult result)
            {
                this.writer = writer;
                this.result = result;
                this.buffer = new byte[0x4400];
            }
        }
    }
}

