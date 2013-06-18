namespace System.IdentityModel
{
    using System;
    using System.IdentityModel.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;

    internal sealed class HashStream : Stream
    {
        private HashAlgorithm hash;
        private bool hashNeedsReset;
        private long length;
        private MemoryStream logStream;

        public HashStream(HashAlgorithm hash)
        {
            if (hash == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("hash");
            }
            this.Reset(hash);
        }

        public override void Flush()
        {
        }

        public void FlushHash()
        {
            this.hash.TransformFinalBlock(CryptoHelper.EmptyBuffer, 0, 0);
            if (DigestTraceRecordHelper.ShouldTraceDigest)
            {
                DigestTraceRecordHelper.TraceDigest(this.logStream, this.hash);
            }
        }

        public byte[] FlushHashAndGetValue()
        {
            this.FlushHash();
            return this.hash.Hash;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public void Reset()
        {
            if (this.hashNeedsReset)
            {
                this.hash.Initialize();
                this.hashNeedsReset = false;
            }
            this.length = 0L;
            if (DigestTraceRecordHelper.ShouldTraceDigest)
            {
                this.logStream = new MemoryStream();
            }
        }

        public void Reset(HashAlgorithm hash)
        {
            this.hash = hash;
            this.hashNeedsReset = false;
            this.length = 0L;
            if (DigestTraceRecordHelper.ShouldTraceDigest)
            {
                this.logStream = new MemoryStream();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public override void SetLength(long length)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.hash.TransformBlock(buffer, offset, count, buffer, offset);
            this.length += count;
            this.hashNeedsReset = true;
            if (DigestTraceRecordHelper.ShouldTraceDigest)
            {
                this.logStream.Write(buffer, offset, count);
            }
        }

        public override bool CanRead
        {
            get
            {
                return false;
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
                return true;
            }
        }

        public HashAlgorithm Hash
        {
            get
            {
                return this.hash;
            }
        }

        public override long Length
        {
            get
            {
                return this.length;
            }
        }

        public override long Position
        {
            get
            {
                return this.length;
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }
    }
}

