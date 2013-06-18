namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal sealed class ConnectAlgorithms : IConnectAlgorithms, IDisposable
    {
        private EventWaitHandle addNeighbor = new EventWaitHandle(true, EventResetMode.ManualReset);
        private PeerNodeConfig config;
        private bool disposed;
        private IPeerMaintainer maintainer;
        private EventWaitHandle maintainerClosed = new EventWaitHandle(false, EventResetMode.ManualReset);
        private Dictionary<Uri, PeerNodeAddress> nodeAddresses = new Dictionary<Uri, PeerNodeAddress>();
        private Dictionary<Uri, PeerNodeAddress> pendingConnectedNeighbor = new Dictionary<Uri, PeerNodeAddress>();
        private static Random random = new Random();
        private object thisLock = new object();
        private int wantedConnectionCount;
        private EventWaitHandle welcomeReceived = new EventWaitHandle(false, EventResetMode.ManualReset);

        public void Connect(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.addNeighbor.Set();
            List<IAsyncResult> list = new List<IAsyncResult>();
            List<WaitHandle> list2 = new List<WaitHandle>();
            while ((list.Count != 0) || (((this.nodeAddresses.Count != 0) || (this.pendingConnectedNeighbor.Count != 0)) && (this.maintainer.IsOpen && (this.maintainer.ConnectedNeighborCount < this.wantedConnectionCount))))
            {
                try
                {
                    list2.Clear();
                    foreach (IAsyncResult result in list)
                    {
                        list2.Add(result.AsyncWaitHandle);
                    }
                    list2.Add(this.welcomeReceived);
                    list2.Add(this.maintainerClosed);
                    list2.Add(this.addNeighbor);
                    int index = WaitHandle.WaitAny(list2.ToArray(), this.config.ConnectTimeout, false);
                    if (index == list.Count)
                    {
                        this.welcomeReceived.Reset();
                        continue;
                    }
                    if (index == (list.Count + 1))
                    {
                        this.maintainerClosed.Reset();
                        lock (this.ThisLock)
                        {
                            this.nodeAddresses.Clear();
                            continue;
                        }
                    }
                    if (index == (list.Count + 2))
                    {
                        if ((this.nodeAddresses.Count > 0) && ((this.pendingConnectedNeighbor.Count + this.maintainer.ConnectedNeighborCount) < this.wantedConnectionCount))
                        {
                            PeerNodeAddress address = null;
                            lock (this.ThisLock)
                            {
                                if ((this.nodeAddresses.Count == 0) || !this.maintainer.IsOpen)
                                {
                                    this.addNeighbor.Reset();
                                    continue;
                                }
                                int num2 = random.Next() % this.nodeAddresses.Count;
                                ICollection<Uri> keys = this.nodeAddresses.Keys;
                                int num3 = 0;
                                Uri key = null;
                                foreach (Uri uri2 in keys)
                                {
                                    if (num3++ == num2)
                                    {
                                        key = uri2;
                                        break;
                                    }
                                }
                                address = this.nodeAddresses[key];
                                this.nodeAddresses.Remove(key);
                            }
                            if ((this.maintainer.FindDuplicateNeighbor(address) == null) && !this.pendingConnectedNeighbor.ContainsKey(GetEndpointUri(address)))
                            {
                                lock (this.ThisLock)
                                {
                                    this.pendingConnectedNeighbor.Add(GetEndpointUri(address), address);
                                }
                                try
                                {
                                    if (this.maintainer.IsOpen)
                                    {
                                        if (DiagnosticUtility.ShouldTraceInformation)
                                        {
                                            PeerMaintainerTraceRecord extendedData = new PeerMaintainerTraceRecord(System.ServiceModel.SR.GetString("PeerMaintainerConnect", new object[] { address, this.config.MeshId }));
                                            TraceUtility.TraceEvent(TraceEventType.Information, 0x40051, System.ServiceModel.SR.GetString("TraceCodePeerMaintainerActivity"), extendedData, this, null);
                                        }
                                        IAsyncResult item = this.maintainer.BeginOpenNeighbor(address, helper.RemainingTime(), null, address);
                                        list.Add(item);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    if (Fx.IsFatal(exception))
                                    {
                                        throw;
                                    }
                                    if (DiagnosticUtility.ShouldTraceInformation)
                                    {
                                        PeerMaintainerTraceRecord record2 = new PeerMaintainerTraceRecord(System.ServiceModel.SR.GetString("PeerMaintainerConnectFailure", new object[] { address, this.config.MeshId, exception.Message }));
                                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40051, System.ServiceModel.SR.GetString("TraceCodePeerMaintainerActivity"), record2, this, null);
                                    }
                                    this.pendingConnectedNeighbor.Remove(GetEndpointUri(address));
                                    if (!(exception is ObjectDisposedException))
                                    {
                                        throw;
                                    }
                                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                                }
                            }
                        }
                        if ((this.nodeAddresses.Count == 0) || ((this.pendingConnectedNeighbor.Count + this.maintainer.ConnectedNeighborCount) == this.wantedConnectionCount))
                        {
                            this.addNeighbor.Reset();
                        }
                        continue;
                    }
                    if (index != 0x102)
                    {
                        IAsyncResult result3 = list[index];
                        list.RemoveAt(index);
                        try
                        {
                            this.maintainer.EndOpenNeighbor(result3);
                            continue;
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            this.pendingConnectedNeighbor.Remove(GetEndpointUri((PeerNodeAddress) result3.AsyncState));
                            throw;
                        }
                    }
                    this.pendingConnectedNeighbor.Clear();
                    list.Clear();
                    this.addNeighbor.Set();
                }
                catch (CommunicationException exception3)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    this.addNeighbor.Set();
                }
                catch (TimeoutException exception4)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                    this.addNeighbor.Set();
                }
            }
        }

        private static Uri GetEndpointUri(PeerNodeAddress address)
        {
            return address.EndpointAddress.Uri;
        }

        public void Initialize(IPeerMaintainer maintainer, PeerNodeConfig config, int wantedConnectionCount, Dictionary<EndpointAddress, Referral> referralCache)
        {
            this.maintainer = maintainer;
            this.config = config;
            this.wantedConnectionCount = wantedConnectionCount;
            this.UpdateEndpointsCollection(referralCache.Values);
            maintainer.NeighborClosed += new NeighborClosedHandler(this.OnNeighborClosed);
            maintainer.NeighborConnected += new NeighborConnectedHandler(this.OnNeighborConnected);
            maintainer.MaintainerClosed += new MaintainerClosedHandler(this.OnMaintainerClosed);
            maintainer.ReferralsAdded += new ReferralsAddedHandler(this.OnReferralsAdded);
        }

        private void OnMaintainerClosed()
        {
            if (!this.disposed)
            {
                lock (this.ThisLock)
                {
                    if (!this.disposed)
                    {
                        this.maintainerClosed.Set();
                    }
                }
            }
        }

        private void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            if (neighbor.ListenAddress != null)
            {
                Uri endpointUri = GetEndpointUri(neighbor.ListenAddress);
                if (!this.disposed)
                {
                    lock (this.ThisLock)
                    {
                        if ((!this.disposed && (endpointUri != null)) && this.pendingConnectedNeighbor.ContainsKey(endpointUri))
                        {
                            this.pendingConnectedNeighbor.Remove(endpointUri);
                            this.addNeighbor.Set();
                        }
                    }
                }
            }
        }

        private void OnNeighborConnected(IPeerNeighbor neighbor)
        {
            Uri endpointUri = GetEndpointUri(neighbor.ListenAddress);
            if (!this.disposed)
            {
                lock (this.ThisLock)
                {
                    if (!this.disposed)
                    {
                        if ((endpointUri != null) && this.pendingConnectedNeighbor.ContainsKey(endpointUri))
                        {
                            this.pendingConnectedNeighbor.Remove(endpointUri);
                        }
                        this.welcomeReceived.Set();
                    }
                }
            }
        }

        private void OnReferralsAdded(IList<Referral> referrals, IPeerNeighbor neighbor)
        {
            bool flag = false;
            foreach (Referral referral in referrals)
            {
                if (!this.disposed)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.disposed)
                        {
                            if (!this.maintainer.IsOpen)
                            {
                                return;
                            }
                            Uri endpointUri = GetEndpointUri(referral.Address);
                            if (((endpointUri != GetEndpointUri(this.maintainer.GetListenAddress())) && !this.nodeAddresses.ContainsKey(endpointUri)) && (!this.pendingConnectedNeighbor.ContainsKey(endpointUri) && (this.maintainer.FindDuplicateNeighbor(referral.Address) == null)))
                            {
                                this.nodeAddresses[endpointUri] = referral.Address;
                                flag = true;
                            }
                        }
                    }
                }
            }
            if (flag && (this.maintainer.ConnectedNeighborCount < this.wantedConnectionCount))
            {
                this.addNeighbor.Set();
            }
        }

        public void PruneConnections()
        {
            while ((this.maintainer.NonClosingNeighborCount > this.config.IdealNeighbors) && this.maintainer.IsOpen)
            {
                IPeerNeighbor leastUsefulNeighbor = this.maintainer.GetLeastUsefulNeighbor();
                if (leastUsefulNeighbor == null)
                {
                    return;
                }
                this.maintainer.CloseNeighbor(leastUsefulNeighbor, PeerCloseReason.NotUsefulNeighbor);
            }
        }

        void IDisposable.Dispose()
        {
            if (!this.disposed)
            {
                lock (this.ThisLock)
                {
                    if (!this.disposed)
                    {
                        this.disposed = true;
                        this.maintainer.ReferralsAdded -= new ReferralsAddedHandler(this.OnReferralsAdded);
                        this.maintainer.MaintainerClosed -= new MaintainerClosedHandler(this.OnMaintainerClosed);
                        this.maintainer.NeighborClosed -= new NeighborClosedHandler(this.OnNeighborClosed);
                        this.maintainer.NeighborConnected -= new NeighborConnectedHandler(this.OnNeighborConnected);
                        this.addNeighbor.Close();
                        this.maintainerClosed.Close();
                        this.welcomeReceived.Close();
                    }
                }
            }
        }

        public void UpdateEndpointsCollection(ICollection<Referral> src)
        {
            if (src != null)
            {
                lock (this.ThisLock)
                {
                    foreach (Referral referral in src)
                    {
                        this.UpdateEndpointsCollection(referral.Address);
                    }
                }
            }
        }

        private void UpdateEndpointsCollection(PeerNodeAddress address)
        {
            if (PeerValidateHelper.ValidNodeAddress(address))
            {
                Uri endpointUri = GetEndpointUri(address);
                if (!this.nodeAddresses.ContainsKey(endpointUri) && (endpointUri != GetEndpointUri(this.maintainer.GetListenAddress())))
                {
                    this.nodeAddresses[endpointUri] = address;
                }
            }
        }

        public void UpdateEndpointsCollection(ICollection<PeerNodeAddress> src)
        {
            if (src != null)
            {
                lock (this.ThisLock)
                {
                    foreach (PeerNodeAddress address in src)
                    {
                        this.UpdateEndpointsCollection(address);
                    }
                }
            }
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

