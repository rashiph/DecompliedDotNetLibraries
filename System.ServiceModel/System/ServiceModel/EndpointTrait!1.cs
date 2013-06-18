namespace System.ServiceModel
{
    using System;

    internal sealed class EndpointTrait<TChannel> where TChannel: class
    {
        private InstanceContext callbackInstance;
        private string endpointConfigurationName;
        private EndpointAddress remoteAddress;

        public EndpointTrait(string endpointConfigurationName, EndpointAddress remoteAddress, InstanceContext callbackInstance)
        {
            this.endpointConfigurationName = endpointConfigurationName;
            this.remoteAddress = remoteAddress;
            this.callbackInstance = callbackInstance;
        }

        public ChannelFactory<TChannel> CreateChannelFactory()
        {
            if (this.callbackInstance != null)
            {
                return this.CreateDuplexFactory();
            }
            return this.CreateSimplexFactory();
        }

        private DuplexChannelFactory<TChannel> CreateDuplexFactory()
        {
            if (this.remoteAddress != null)
            {
                return new DuplexChannelFactory<TChannel>(this.callbackInstance, this.endpointConfigurationName, this.remoteAddress);
            }
            return new DuplexChannelFactory<TChannel>(this.callbackInstance, this.endpointConfigurationName);
        }

        private ChannelFactory<TChannel> CreateSimplexFactory()
        {
            if (this.remoteAddress != null)
            {
                return new ChannelFactory<TChannel>(this.endpointConfigurationName, this.remoteAddress);
            }
            return new ChannelFactory<TChannel>(this.endpointConfigurationName);
        }

        public override bool Equals(object obj)
        {
            EndpointTrait<TChannel> trait = obj as EndpointTrait<TChannel>;
            if (trait == null)
            {
                return false;
            }
            if (!object.ReferenceEquals(this.callbackInstance, trait.callbackInstance))
            {
                return false;
            }
            if (string.CompareOrdinal(this.endpointConfigurationName, trait.endpointConfigurationName) != 0)
            {
                return false;
            }
            if (this.remoteAddress != trait.remoteAddress)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int num = 0;
            if (this.callbackInstance != null)
            {
                num ^= this.callbackInstance.GetHashCode();
            }
            num ^= this.endpointConfigurationName.GetHashCode();
            if (this.remoteAddress != null)
            {
                num ^= this.remoteAddress.GetHashCode();
            }
            return num;
        }
    }
}

