namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public class WebPartConnectionsEventArgs : EventArgs
    {
        private WebPartConnection _connection;
        private WebPart _consumer;
        private System.Web.UI.WebControls.WebParts.ConsumerConnectionPoint _consumerConnectionPoint;
        private WebPart _provider;
        private System.Web.UI.WebControls.WebParts.ProviderConnectionPoint _providerConnectionPoint;

        public WebPartConnectionsEventArgs(WebPart provider, System.Web.UI.WebControls.WebParts.ProviderConnectionPoint providerConnectionPoint, WebPart consumer, System.Web.UI.WebControls.WebParts.ConsumerConnectionPoint consumerConnectionPoint)
        {
            this._provider = provider;
            this._providerConnectionPoint = providerConnectionPoint;
            this._consumer = consumer;
            this._consumerConnectionPoint = consumerConnectionPoint;
        }

        public WebPartConnectionsEventArgs(WebPart provider, System.Web.UI.WebControls.WebParts.ProviderConnectionPoint providerConnectionPoint, WebPart consumer, System.Web.UI.WebControls.WebParts.ConsumerConnectionPoint consumerConnectionPoint, WebPartConnection connection) : this(provider, providerConnectionPoint, consumer, consumerConnectionPoint)
        {
            this._connection = connection;
        }

        public WebPartConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        public WebPart Consumer
        {
            get
            {
                return this._consumer;
            }
        }

        public System.Web.UI.WebControls.WebParts.ConsumerConnectionPoint ConsumerConnectionPoint
        {
            get
            {
                return this._consumerConnectionPoint;
            }
        }

        public WebPart Provider
        {
            get
            {
                return this._provider;
            }
        }

        public System.Web.UI.WebControls.WebParts.ProviderConnectionPoint ProviderConnectionPoint
        {
            get
            {
                return this._providerConnectionPoint;
            }
        }
    }
}

