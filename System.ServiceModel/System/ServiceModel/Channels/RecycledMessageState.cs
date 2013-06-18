namespace System.ServiceModel.Channels
{
    using System;

    internal class RecycledMessageState
    {
        private System.ServiceModel.Channels.HeaderInfoCache headerInfoCache;
        private MessageHeaders recycledHeaders;
        private MessageProperties recycledProperties;
        private System.ServiceModel.Channels.UriCache uriCache;

        public void ReturnHeaders(MessageHeaders headers)
        {
            if (headers.CanRecycle)
            {
                headers.Recycle(this.HeaderInfoCache);
                this.recycledHeaders = headers;
            }
        }

        public void ReturnProperties(MessageProperties properties)
        {
            if (properties.CanRecycle)
            {
                properties.Recycle();
                this.recycledProperties = properties;
            }
        }

        public MessageHeaders TakeHeaders()
        {
            MessageHeaders recycledHeaders = this.recycledHeaders;
            this.recycledHeaders = null;
            return recycledHeaders;
        }

        public MessageProperties TakeProperties()
        {
            MessageProperties recycledProperties = this.recycledProperties;
            this.recycledProperties = null;
            return recycledProperties;
        }

        public System.ServiceModel.Channels.HeaderInfoCache HeaderInfoCache
        {
            get
            {
                if (this.headerInfoCache == null)
                {
                    this.headerInfoCache = new System.ServiceModel.Channels.HeaderInfoCache();
                }
                return this.headerInfoCache;
            }
        }

        public System.ServiceModel.Channels.UriCache UriCache
        {
            get
            {
                if (this.uriCache == null)
                {
                    this.uriCache = new System.ServiceModel.Channels.UriCache();
                }
                return this.uriCache;
            }
        }
    }
}

