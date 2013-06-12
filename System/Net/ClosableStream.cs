namespace System.Net
{
    using System;
    using System.IO;
    using System.Threading;

    internal class ClosableStream : DelegatedStream
    {
        private int closed;
        private EventHandler onClose;

        internal ClosableStream(Stream stream, EventHandler onClose) : base(stream)
        {
            this.onClose = onClose;
        }

        public override void Close()
        {
            if ((Interlocked.Increment(ref this.closed) == 1) && (this.onClose != null))
            {
                this.onClose(this, new EventArgs());
            }
        }
    }
}

