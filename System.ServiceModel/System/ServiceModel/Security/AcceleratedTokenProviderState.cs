namespace System.ServiceModel.Security
{
    using System;

    internal class AcceleratedTokenProviderState : IssuanceTokenProviderState
    {
        private byte[] entropy;

        public AcceleratedTokenProviderState(byte[] value)
        {
            this.entropy = value;
        }

        public byte[] GetRequestorEntropy()
        {
            return this.entropy;
        }
    }
}

