namespace System.Net.Security
{
    using System;
    using System.IO;

    public abstract class AuthenticatedStream : Stream
    {
        private Stream _InnerStream;
        private bool _LeaveStreamOpen;

        protected AuthenticatedStream(Stream innerStream, bool leaveInnerStreamOpen)
        {
            if ((innerStream == null) || (innerStream == Stream.Null))
            {
                throw new ArgumentNullException("innerStream");
            }
            if (!innerStream.CanRead || !innerStream.CanWrite)
            {
                throw new ArgumentException(SR.GetString("net_io_must_be_rw_stream"), "innerStream");
            }
            this._InnerStream = innerStream;
            this._LeaveStreamOpen = leaveInnerStreamOpen;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this._LeaveStreamOpen)
                    {
                        this._InnerStream.Flush();
                    }
                    else
                    {
                        this._InnerStream.Close();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected Stream InnerStream
        {
            get
            {
                return this._InnerStream;
            }
        }

        public abstract bool IsAuthenticated { get; }

        public abstract bool IsEncrypted { get; }

        public abstract bool IsMutuallyAuthenticated { get; }

        public abstract bool IsServer { get; }

        public abstract bool IsSigned { get; }

        public bool LeaveInnerStreamOpen
        {
            get
            {
                return this._LeaveStreamOpen;
            }
        }
    }
}

