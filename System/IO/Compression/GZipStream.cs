namespace System.IO.Compression
{
    using System;
    using System.IO;
    using System.Security.Permissions;

    public class GZipStream : Stream
    {
        private DeflateStream deflateStream;

        public GZipStream(Stream stream, CompressionMode mode) : this(stream, mode, false)
        {
        }

        public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
        {
            this.deflateStream = new DeflateStream(stream, mode, leaveOpen);
            if (mode == CompressionMode.Compress)
            {
                IFileFormatWriter writer = new GZipFormatter();
                this.deflateStream.SetFileFormatWriter(writer);
            }
            else
            {
                IFileFormatReader reader = new GZipDecoder();
                this.deflateStream.SetFileFormatReader(reader);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            if (this.deflateStream == null)
            {
                throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
            }
            return this.deflateStream.BeginRead(array, offset, count, asyncCallback, asyncState);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            if (this.deflateStream == null)
            {
                throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
            }
            return this.deflateStream.BeginWrite(array, offset, count, asyncCallback, asyncState);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this.deflateStream != null))
                {
                    this.deflateStream.Close();
                }
                this.deflateStream = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (this.deflateStream == null)
            {
                throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
            }
            return this.deflateStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (this.deflateStream == null)
            {
                throw new InvalidOperationException(SR.GetString("ObjectDisposed_StreamClosed"));
            }
            this.deflateStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            if (this.deflateStream == null)
            {
                throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
            }
            this.deflateStream.Flush();
        }

        public override int Read(byte[] array, int offset, int count)
        {
            if (this.deflateStream == null)
            {
                throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
            }
            return this.deflateStream.Read(array, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("NotSupported"));
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.GetString("NotSupported"));
        }

        public override void Write(byte[] array, int offset, int count)
        {
            if (this.deflateStream == null)
            {
                throw new ObjectDisposedException(null, SR.GetString("ObjectDisposed_StreamClosed"));
            }
            this.deflateStream.Write(array, offset, count);
        }

        public Stream BaseStream
        {
            get
            {
                if (this.deflateStream != null)
                {
                    return this.deflateStream.BaseStream;
                }
                return null;
            }
        }

        public override bool CanRead
        {
            get
            {
                if (this.deflateStream == null)
                {
                    return false;
                }
                return this.deflateStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (this.deflateStream == null)
                {
                    return false;
                }
                return this.deflateStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (this.deflateStream == null)
                {
                    return false;
                }
                return this.deflateStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException(SR.GetString("NotSupported"));
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException(SR.GetString("NotSupported"));
            }
            set
            {
                throw new NotSupportedException(SR.GetString("NotSupported"));
            }
        }
    }
}

