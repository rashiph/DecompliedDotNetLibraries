namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;

    public abstract class StreamUpgradeAcceptor
    {
        protected StreamUpgradeAcceptor()
        {
        }

        public virtual Stream AcceptUpgrade(Stream stream)
        {
            return this.EndAcceptUpgrade(this.BeginAcceptUpgrade(stream, null, null));
        }

        public abstract IAsyncResult BeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state);
        public abstract bool CanUpgrade(string contentType);
        public abstract Stream EndAcceptUpgrade(IAsyncResult result);
    }
}

