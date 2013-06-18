namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class PeerFlooderSimple : PeerFlooderBase<Message, UtilityInfo>
    {
        private const uint MaxBuckets = 5;
        private ListManager messageIds;

        internal PeerFlooderSimple(PeerNodeConfig config, PeerNeighborManager neighborManager) : base(config, neighborManager)
        {
            this.messageIds = new ListManager(5);
        }

        public override void EndFloodMessage(IAsyncResult result)
        {
            base.EndFloodMessage(result);
        }

        public bool IsNotSeenBefore(Message message, out byte[] id, out int cacheHit)
        {
            cacheHit = -1;
            id = PeerNodeImplementation.DefaultId;
            if (message is SecurityVerifiedMessage)
            {
                id = (message as SecurityVerifiedMessage).PrimarySignatureValue;
            }
            else
            {
                UniqueId id2 = PeerMessageHelpers.GetHeaderUniqueId(message.Headers, "MessageID", "http://schemas.microsoft.com/net/2006/05/peer");
                if ((id2 != null) && id2.IsGuid)
                {
                    id = new byte[0x10];
                    id2.TryGetGuid(id, 0);
                }
                else
                {
                    return false;
                }
            }
            cacheHit = this.messageIds.AddForLookup(id);
            return (cacheHit == -1);
        }

        public override void OnClose()
        {
            this.messageIds.Close();
        }

        public override IAsyncResult OnFloodedMessage(IPeerNeighbor neighbor, Message floodInfo, AsyncCallback callback, object state)
        {
            return base.OnFloodedMessage(neighbor, floodInfo, callback, state);
        }

        public override void OnOpen()
        {
        }

        public override void ProcessLinkUtility(IPeerNeighbor neighbor, UtilityInfo utilityInfo)
        {
            if (!PeerNeighborStateHelper.IsConnected(neighbor.State))
            {
                neighbor.Abort(PeerCloseReason.InvalidNeighbor, PeerCloseInitiator.LocalNode);
            }
            else
            {
                try
                {
                    UtilityExtension.ProcessLinkUtility(neighbor, utilityInfo);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (PeerFlooderBase<Message, UtilityInfo>.CloseNeighborIfKnownException(base.neighborManager, exception, neighbor) != null)
                    {
                        throw;
                    }
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
        }

        public override void RecordOutgoingMessage(byte[] id)
        {
            this.messageIds.AddForFlood(id);
        }

        public override bool ShouldProcess(Message message)
        {
            return message.Properties.ContainsKey("MessageVerified");
        }

        private class ListManager
        {
            private uint active;
            private readonly uint buckets;
            private bool disposed;
            private static readonly int InitialCount = 0x3e8;
            private static NonceCache.NonceCacheImpl.NonceKeyComparer keyComparer = new NonceCache.NonceCacheImpl.NonceKeyComparer();
            private IOThreadTimer messagePruningTimer;
            private const int NotFound = -1;
            private static readonly int PruningTimout = 0xea60;
            private Dictionary<byte[], bool>[] tables;
            private object thisLock;

            public ListManager(uint buckets)
            {
                if (buckets <= 1)
                {
                    throw Fx.AssertAndThrow("ListManager should be used atleast with 2 buckets");
                }
                this.buckets = buckets;
                this.tables = new Dictionary<byte[], bool>[buckets];
                for (uint i = 0; i < buckets; i++)
                {
                    this.tables[i] = this.NewCache(InitialCount);
                }
                this.messagePruningTimer = new IOThreadTimer(new Action<object>(this.OnTimeout), null, false);
                this.messagePruningTimer.Set(PruningTimout);
                this.active = 0;
                this.disposed = false;
                this.thisLock = new object();
            }

            public bool AddForFlood(byte[] key)
            {
                if (this.disposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerFlooderDisposed")));
                }
                lock (this.ThisLock)
                {
                    return this.UpdateFloodEntry(key);
                }
            }

            public int AddForLookup(byte[] key)
            {
                int num = -1;
                if (this.disposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerFlooderDisposed")));
                }
                lock (this.ThisLock)
                {
                    num = this.Contains(key);
                    if (num == -1)
                    {
                        this.tables[this.active].Add(key, false);
                    }
                    return num;
                }
            }

            internal void Close()
            {
                lock (this.ThisLock)
                {
                    if (!this.disposed)
                    {
                        this.messagePruningTimer.Cancel();
                        this.messagePruningTimer = null;
                        this.tables = null;
                        this.disposed = true;
                    }
                }
            }

            internal int Contains(byte[] key)
            {
                int num = -1;
                uint buckets = 0;
                buckets = this.buckets;
                while (buckets > 0)
                {
                    if (this.tables[(this.active + buckets) % this.buckets].ContainsKey(key))
                    {
                        num = (int) buckets;
                    }
                    buckets--;
                }
                if (num < 0)
                {
                    return num;
                }
                return (int) (((this.active + this.buckets) - buckets) % this.buckets);
            }

            private Dictionary<byte[], bool> NewCache(int capacity)
            {
                return new Dictionary<byte[], bool>(capacity, keyComparer);
            }

            private void OnTimeout(object state)
            {
                if (!this.disposed)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.disposed)
                        {
                            this.active = (this.active + 1) % this.buckets;
                            this.tables[this.active] = this.NewCache(this.tables[this.active].Count);
                            this.messagePruningTimer.Set(PruningTimout);
                        }
                    }
                }
            }

            internal bool UpdateFloodEntry(byte[] key)
            {
                bool flag = false;
                for (uint i = this.buckets; i > 0; i--)
                {
                    if (this.tables[(this.active + i) % this.buckets].TryGetValue(key, out flag))
                    {
                        if (!flag)
                        {
                            this.tables[(this.active + i) % this.buckets][key] = true;
                            return true;
                        }
                        return false;
                    }
                }
                this.tables[this.active].Add(key, true);
                return true;
            }

            private object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }
        }
    }
}

