namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    public class InfocardInteractiveChannelInitializer : IInteractiveChannelInitializer
    {
        private System.ServiceModel.Channels.Binding binding;
        private ClientCredentials credentials;

        public InfocardInteractiveChannelInitializer(ClientCredentials credentials, System.ServiceModel.Channels.Binding binding)
        {
            this.credentials = credentials;
            this.binding = binding;
        }

        public virtual IAsyncResult BeginDisplayInitializationUI(IClientChannel channel, AsyncCallback callback, object state)
        {
            return new GetTokenUIAsyncResult(this.binding, channel, this.credentials, callback, state);
        }

        public virtual void EndDisplayInitializationUI(IAsyncResult result)
        {
            GetTokenUIAsyncResult.End(result);
        }

        public System.ServiceModel.Channels.Binding Binding
        {
            get
            {
                return this.binding;
            }
        }
    }
}

