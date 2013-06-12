namespace System.Net.Cache
{
    using System;
    using System.IO;
    using System.Net;

    internal abstract class BaseWrapperStream : Stream, IRequestLifetimeTracker
    {
        private Stream m_WrappedStream;

        public BaseWrapperStream(Stream wrappedStream)
        {
            this.m_WrappedStream = wrappedStream;
        }

        public void TrackRequestLifetime(long requestStartTimestamp)
        {
            (this.m_WrappedStream as IRequestLifetimeTracker).TrackRequestLifetime(requestStartTimestamp);
        }

        protected Stream WrappedStream
        {
            get
            {
                return this.m_WrappedStream;
            }
        }
    }
}

