namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;

    internal class TimeoutStream : DelegatingStream
    {
        private TimeoutHelper timeoutHelper;

        public TimeoutStream(Stream stream, ref TimeoutHelper timeoutHelper) : base(stream)
        {
            if (!stream.CanTimeout)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("stream", System.ServiceModel.SR.GetString("StreamDoesNotSupportTimeout"));
            }
            this.timeoutHelper = timeoutHelper;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.UpdateReadTimeout();
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.UpdateWriteTimeout();
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.UpdateReadTimeout();
            return base.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            this.UpdateReadTimeout();
            return base.ReadByte();
        }

        private void UpdateReadTimeout()
        {
            this.ReadTimeout = TimeoutHelper.ToMilliseconds(this.timeoutHelper.RemainingTime());
        }

        private void UpdateWriteTimeout()
        {
            this.WriteTimeout = TimeoutHelper.ToMilliseconds(this.timeoutHelper.RemainingTime());
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.UpdateWriteTimeout();
            base.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            this.UpdateWriteTimeout();
            base.WriteByte(value);
        }
    }
}

