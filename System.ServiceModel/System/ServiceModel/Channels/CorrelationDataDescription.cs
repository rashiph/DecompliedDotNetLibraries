namespace System.ServiceModel.Channels
{
    using System;

    public abstract class CorrelationDataDescription
    {
        protected CorrelationDataDescription()
        {
        }

        public abstract bool IsDefault { get; }

        public abstract bool IsOptional { get; }

        public abstract bool KnownBeforeSend { get; }

        public abstract string Name { get; }

        public abstract bool ReceiveValue { get; }

        public abstract bool SendValue { get; }
    }
}

