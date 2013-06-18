namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;

    internal abstract class HttpDelayedAcceptStream : DetectEofStream
    {
        private HttpOutput httpOutput;

        protected HttpDelayedAcceptStream(Stream stream) : base(stream)
        {
        }

        public bool EnableDelayedAccept(HttpOutput output)
        {
            if (base.IsAtEof)
            {
                return false;
            }
            this.httpOutput = output;
            return true;
        }

        protected override void OnReceivedEof()
        {
            if (this.httpOutput != null)
            {
                this.httpOutput.Close();
            }
        }
    }
}

