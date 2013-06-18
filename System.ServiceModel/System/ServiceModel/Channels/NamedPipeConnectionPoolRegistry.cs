namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class NamedPipeConnectionPoolRegistry : ConnectionPoolRegistry
    {
        protected override ConnectionPool CreatePool(IConnectionOrientedTransportChannelFactorySettings settings)
        {
            return new NamedPipeConnectionPool(settings);
        }

        private class NamedPipeConnectionPool : ConnectionPool
        {
            private NamedPipeConnectionPoolRegistry.PipeNameCache pipeNameCache;

            public NamedPipeConnectionPool(IConnectionOrientedTransportChannelFactorySettings settings) : base(settings, TimeSpan.MaxValue)
            {
                this.pipeNameCache = new NamedPipeConnectionPoolRegistry.PipeNameCache();
            }

            protected override CommunicationPool<string, IConnection>.EndpointConnectionPool CreateEndpointConnectionPool(string key)
            {
                return new NamedPipeEndpointConnectionPool(this, key);
            }

            protected override string GetPoolKey(EndpointAddress address, Uri via)
            {
                string pipeName;
                lock (base.ThisLock)
                {
                    if (!this.pipeNameCache.TryGetValue(via, out pipeName))
                    {
                        pipeName = PipeConnectionInitiator.GetPipeName(via);
                        this.pipeNameCache.Add(via, pipeName);
                    }
                }
                return pipeName;
            }

            protected override void OnClosed()
            {
                base.OnClosed();
                this.pipeNameCache.Clear();
            }

            private void OnConnectionAborted(string pipeName)
            {
                lock (base.ThisLock)
                {
                    this.pipeNameCache.Purge(pipeName);
                }
            }

            protected class NamedPipeEndpointConnectionPool : IdlingCommunicationPool<string, IConnection>.IdleTimeoutEndpointConnectionPool
            {
                private NamedPipeConnectionPoolRegistry.NamedPipeConnectionPool parent;

                public NamedPipeEndpointConnectionPool(NamedPipeConnectionPoolRegistry.NamedPipeConnectionPool parent, string key) : base(parent, key)
                {
                    this.parent = parent;
                }

                protected override void OnConnectionAborted()
                {
                    this.parent.OnConnectionAborted(base.Key);
                }
            }
        }

        private class PipeNameCache
        {
            private Dictionary<Uri, string> forwardTable = new Dictionary<Uri, string>();
            private Dictionary<string, ICollection<Uri>> reverseTable = new Dictionary<string, ICollection<Uri>>();

            public void Add(Uri uri, string pipeName)
            {
                ICollection<Uri> is2;
                this.forwardTable.Add(uri, pipeName);
                if (!this.reverseTable.TryGetValue(pipeName, out is2))
                {
                    is2 = new Collection<Uri>();
                    this.reverseTable.Add(pipeName, is2);
                }
                is2.Add(uri);
            }

            public void Clear()
            {
                this.forwardTable.Clear();
                this.reverseTable.Clear();
            }

            public void Purge(string pipeName)
            {
                ICollection<Uri> is2;
                if (this.reverseTable.TryGetValue(pipeName, out is2))
                {
                    this.reverseTable.Remove(pipeName);
                    foreach (Uri uri in is2)
                    {
                        this.forwardTable.Remove(uri);
                    }
                }
            }

            public bool TryGetValue(Uri uri, out string pipeName)
            {
                return this.forwardTable.TryGetValue(uri, out pipeName);
            }
        }
    }
}

