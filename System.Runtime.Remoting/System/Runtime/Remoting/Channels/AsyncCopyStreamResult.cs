namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;

    internal class AsyncCopyStreamResult : BasicAsyncResult
    {
        internal bool AsyncRead;
        internal bool AsyncWrite;
        internal byte[] Buffer;
        internal bool CloseSource;
        internal bool CloseTarget;
        internal Stream Source;
        internal Stream Target;

        internal AsyncCopyStreamResult(AsyncCallback callback, object state) : base(callback, state)
        {
        }

        internal override void CleanupOnComplete()
        {
            if (this.Buffer != null)
            {
                CoreChannel.BufferPool.ReturnBuffer(this.Buffer);
            }
            if (this.CloseSource)
            {
                this.Source.Close();
            }
            if (this.CloseTarget)
            {
                this.Target.Close();
            }
        }
    }
}

