namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;

    internal abstract class DetectEofStream : DelegatingStream
    {
        private bool isAtEof;

        protected DetectEofStream(Stream stream) : base(stream)
        {
            this.isAtEof = false;
        }

        public override int EndRead(IAsyncResult result)
        {
            int num = base.EndRead(result);
            if (num == 0)
            {
                this.ReceivedEof();
            }
            return num;
        }

        protected virtual void OnReceivedEof()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = base.Read(buffer, offset, count);
            if (num == 0)
            {
                this.ReceivedEof();
            }
            return num;
        }

        public override int ReadByte()
        {
            int num = base.ReadByte();
            if (num == -1)
            {
                this.ReceivedEof();
            }
            return num;
        }

        private void ReceivedEof()
        {
            if (!this.isAtEof)
            {
                this.isAtEof = true;
                this.OnReceivedEof();
            }
        }

        protected bool IsAtEof
        {
            get
            {
                return this.isAtEof;
            }
        }
    }
}

