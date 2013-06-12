namespace System.Web.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Web.Util;

    public abstract class BufferedWebEventProvider : WebEventProvider
    {
        private bool _buffer = true;
        private string _bufferMode;
        private WebEventBuffer _webEventBuffer;

        protected BufferedWebEventProvider()
        {
        }

        public override void Flush()
        {
            if (this._buffer)
            {
                this._webEventBuffer.Flush(0x7fffffff, FlushCallReason.StaticFlush);
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            ProviderUtil.GetAndRemoveBooleanAttribute(config, "buffer", name, ref this._buffer);
            if (this._buffer)
            {
                ProviderUtil.GetAndRemoveRequiredNonEmptyStringAttribute(config, "bufferMode", name, ref this._bufferMode);
                this._webEventBuffer = new WebEventBuffer(this, this._bufferMode, new WebEventBufferFlushCallback(this.ProcessEventFlush));
            }
            else
            {
                ProviderUtil.GetAndRemoveStringAttribute(config, "bufferMode", name, ref this._bufferMode);
            }
            base.Initialize(name, config);
            ProviderUtil.CheckUnrecognizedAttributes(config, name);
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            if (this._buffer)
            {
                this._webEventBuffer.AddEvent(eventRaised);
            }
            else
            {
                WebEventBufferFlushInfo flushInfo = new WebEventBufferFlushInfo(new WebBaseEventCollection(eventRaised), EventNotificationType.Unbuffered, 0, DateTime.MinValue, 0, 0);
                this.ProcessEventFlush(flushInfo);
            }
        }

        public abstract void ProcessEventFlush(WebEventBufferFlushInfo flushInfo);
        public override void Shutdown()
        {
            if (this._webEventBuffer != null)
            {
                this._webEventBuffer.Shutdown();
            }
        }

        public string BufferMode
        {
            get
            {
                return this._bufferMode;
            }
        }

        public bool UseBuffering
        {
            get
            {
                return this._buffer;
            }
        }
    }
}

