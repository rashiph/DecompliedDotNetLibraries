namespace System.ServiceModel.Channels
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal class PeerMaintainerBase<TConnectAlgorithms> : IPeerMaintainer where TConnectAlgorithms: IConnectAlgorithms, new()
    {
        private PeerNodeConfig config;
        private ConnectCallback<TConnectAlgorithms> connectCallback;
        private PeerFlooder flooder;
        private volatile bool isOpen;
        private volatile bool isRunningMaintenance;
        private IOThreadTimer maintainerTimer;
        private PeerNeighborManager neighborManager;
        private Dictionary<EndpointAddress, Referral> referralCache;
        private object thisLock;
        private PeerNodeTraceRecord traceRecord;

        public event MaintainerClosedHandler MaintainerClosed;

        public event NeighborClosedHandler NeighborClosed;

        public event NeighborConnectedHandler NeighborConnected;

        public event ReferralsAddedHandler ReferralsAdded;

        public PeerMaintainerBase(PeerNodeConfig config, PeerNeighborManager neighborManager, PeerFlooder flooder)
        {
            this.neighborManager = neighborManager;
            this.flooder = flooder;
            this.config = config;
            this.thisLock = new object();
            this.referralCache = new Dictionary<EndpointAddress, Referral>();
            this.maintainerTimer = new IOThreadTimer(new Action<object>(this.OnMaintainerTimer), this, false);
        }

        public bool AddReferrals(IList<Referral> referrals, IPeerNeighbor neighbor)
        {
            bool flag = true;
            bool canShareReferrals = false;
            try
            {
                canShareReferrals = this.config.Resolver.CanShareReferrals;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(System.ServiceModel.SR.GetString("ResolverException"), exception);
            }
            if ((referrals != null) && canShareReferrals)
            {
                foreach (Referral referral in referrals)
                {
                    if (((referral == null) || (referral.NodeId == 0L)) || (!PeerValidateHelper.ValidNodeAddress(referral.Address) || !PeerValidateHelper.ValidReferralNodeAddress(referral.Address)))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    lock (this.ThisLock)
                    {
                        foreach (Referral referral2 in referrals)
                        {
                            EndpointAddress endpointAddress = referral2.Address.EndpointAddress;
                            if ((this.referralCache.Count <= this.config.MaxReferralCacheSize) && !this.referralCache.ContainsKey(endpointAddress))
                            {
                                this.referralCache.Add(endpointAddress, referral2);
                            }
                        }
                    }
                    if (this.ReferralsAdded != null)
                    {
                        this.ReferralsAdded(referrals, neighbor);
                    }
                }
            }
            return flag;
        }

        public void Close()
        {
            lock (this.ThisLock)
            {
                this.isOpen = false;
            }
            this.maintainerTimer.Cancel();
            SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
            MaintainerClosedHandler maintainerClosed = this.MaintainerClosed;
            if (maintainerClosed != null)
            {
                maintainerClosed();
            }
        }

        public Referral[] GetReferrals()
        {
            Referral[] referralArray = null;
            bool canShareReferrals = false;
            try
            {
                canShareReferrals = this.config.Resolver.CanShareReferrals;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(System.ServiceModel.SR.GetString("ResolverException"), exception);
            }
            if (canShareReferrals)
            {
                List<IPeerNeighbor> connectedNeighbors = this.neighborManager.GetConnectedNeighbors();
                int num = Math.Min(this.config.MaxReferrals, connectedNeighbors.Count);
                referralArray = new Referral[num];
                for (int i = 0; i < num; i++)
                {
                    referralArray[i] = new Referral(connectedNeighbors[i].NodeId, connectedNeighbors[i].ListenAddress);
                }
                return referralArray;
            }
            return new Referral[0];
        }

        private void InitialConnection(object dummy)
        {
            if (this.isOpen)
            {
                bool flag = false;
                if (!this.isRunningMaintenance)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.isRunningMaintenance)
                        {
                            this.isRunningMaintenance = true;
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        PeerMaintainerTraceRecord extendedData = new PeerMaintainerTraceRecord(System.ServiceModel.SR.GetString("PeerMaintainerInitialConnect", new object[] { this.config.MeshId }));
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40051, System.ServiceModel.SR.GetString("TraceCodePeerMaintainerActivity"), extendedData, this, null);
                    }
                    TimeoutHelper helper = new TimeoutHelper(this.config.MaintainerTimeout);
                    Exception e = null;
                    try
                    {
                        this.maintainerTimer.Cancel();
                        using (IConnectAlgorithms algorithms = (default(TConnectAlgorithms) == null) ? ((IConnectAlgorithms) Activator.CreateInstance<TConnectAlgorithms>()) : ((IConnectAlgorithms) default(TConnectAlgorithms)))
                        {
                            algorithms.Initialize(this, this.config, this.config.MinNeighbors, this.referralCache);
                            if (this.referralCache.Count == 0)
                            {
                                ReadOnlyCollection<PeerNodeAddress> src = this.ResolveNewAddresses(helper.RemainingTime(), false);
                                algorithms.UpdateEndpointsCollection(src);
                            }
                            if (this.isOpen)
                            {
                                algorithms.Connect(helper.RemainingTime());
                            }
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        e = exception2;
                    }
                    if (this.isOpen)
                    {
                        try
                        {
                            lock (this.ThisLock)
                            {
                                if (this.isOpen)
                                {
                                    if (this.neighborManager.ConnectedNeighborCount < 1)
                                    {
                                        this.maintainerTimer.Set(this.config.MaintainerRetryInterval);
                                    }
                                    else
                                    {
                                        this.maintainerTimer.Set(this.config.MaintainerInterval);
                                    }
                                }
                            }
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                            if (e == null)
                            {
                                e = exception3;
                            }
                        }
                    }
                    lock (this.ThisLock)
                    {
                        this.isRunningMaintenance = false;
                    }
                    if (this.connectCallback != null)
                    {
                        this.connectCallback(e);
                    }
                }
            }
        }

        private void MaintainConnections(object dummy)
        {
            if (this.isOpen)
            {
                bool flag = false;
                if (!this.isRunningMaintenance)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.isRunningMaintenance)
                        {
                            this.isRunningMaintenance = true;
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        PeerMaintainerTraceRecord extendedData = new PeerMaintainerTraceRecord(System.ServiceModel.SR.GetString("PeerMaintainerStarting", new object[] { this.config.MeshId }));
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40051, System.ServiceModel.SR.GetString("TraceCodePeerMaintainerActivity"), extendedData, this, null);
                    }
                    TimeoutHelper helper = new TimeoutHelper(this.config.MaintainerTimeout);
                    try
                    {
                        this.maintainerTimer.Cancel();
                        int connectedNeighborCount = this.neighborManager.ConnectedNeighborCount;
                        if (connectedNeighborCount != this.config.IdealNeighbors)
                        {
                            using (IConnectAlgorithms algorithms = (default(TConnectAlgorithms) == null) ? ((IConnectAlgorithms) Activator.CreateInstance<TConnectAlgorithms>()) : ((IConnectAlgorithms) default(TConnectAlgorithms)))
                            {
                                algorithms.Initialize(this, this.config, this.config.IdealNeighbors, this.referralCache);
                                if (connectedNeighborCount > this.config.IdealNeighbors)
                                {
                                    if (DiagnosticUtility.ShouldTraceInformation)
                                    {
                                        PeerMaintainerTraceRecord record2 = new PeerMaintainerTraceRecord(System.ServiceModel.SR.GetString("PeerMaintainerPruneMode", new object[] { this.config.MeshId }));
                                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40051, System.ServiceModel.SR.GetString("TraceCodePeerMaintainerActivity"), record2, this, null);
                                    }
                                    algorithms.PruneConnections();
                                }
                                if (this.neighborManager.ConnectedNeighborCount < this.config.IdealNeighbors)
                                {
                                    if (this.referralCache.Count == 0)
                                    {
                                        ReadOnlyCollection<PeerNodeAddress> src = this.ResolveNewAddresses(helper.RemainingTime(), true);
                                        algorithms.UpdateEndpointsCollection(src);
                                    }
                                    if (DiagnosticUtility.ShouldTraceInformation)
                                    {
                                        PeerMaintainerTraceRecord record3 = new PeerMaintainerTraceRecord(System.ServiceModel.SR.GetString("PeerMaintainerConnectMode", new object[] { this.config.MeshId }));
                                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40051, System.ServiceModel.SR.GetString("TraceCodePeerMaintainerActivity"), record3, this, null);
                                    }
                                    algorithms.Connect(helper.RemainingTime());
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    finally
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            PeerMaintainerTraceRecord record4 = new PeerMaintainerTraceRecord("Maintainer cycle finish");
                            TraceUtility.TraceEvent(TraceEventType.Information, 0x40051, System.ServiceModel.SR.GetString("TraceCodePeerMaintainerActivity"), record4, this, null);
                        }
                    }
                    this.ResetMaintenance();
                }
            }
        }

        private void OnMaintainerTimer(object state)
        {
            ActionItem.Schedule(new Action<object>(this.MaintainConnections), null);
        }

        public virtual void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            if (this.isOpen)
            {
                lock (this.ThisLock)
                {
                    if ((neighbor != null) && (neighbor.ListenAddress != null))
                    {
                        EndpointAddress endpointAddress = neighbor.ListenAddress.EndpointAddress;
                    }
                    if ((this.isOpen && !this.isRunningMaintenance) && (this.neighborManager.ConnectedNeighborCount < this.config.MinNeighbors))
                    {
                        this.maintainerTimer.Set(0);
                    }
                }
            }
            NeighborClosedHandler neighborClosed = this.NeighborClosed;
            if (neighborClosed != null)
            {
                neighborClosed(neighbor);
            }
        }

        public virtual void OnNeighborConnected(IPeerNeighbor neighbor)
        {
            NeighborConnectedHandler neighborConnected = this.NeighborConnected;
            if (neighborConnected != null)
            {
                neighborConnected(neighbor);
            }
        }

        public void Open()
        {
            this.traceRecord = new PeerNodeTraceRecord(this.config.NodeId);
            if (!this.isRunningMaintenance)
            {
                lock (this.ThisLock)
                {
                    SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
                    this.isOpen = true;
                }
            }
        }

        public void PingAndRefresh(object state)
        {
            this.PingConnections();
            if (this.neighborManager.ConnectedNeighborCount < this.config.IdealNeighbors)
            {
                this.MaintainConnections(null);
            }
        }

        public void PingConnections()
        {
            this.neighborManager.PingNeighbors();
        }

        public void RefreshConnection()
        {
            if (this.isOpen)
            {
                bool flag = false;
                if (!this.isRunningMaintenance)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.isRunningMaintenance)
                        {
                            this.isRunningMaintenance = true;
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    try
                    {
                        TimeoutHelper helper = new TimeoutHelper(this.config.MaintainerTimeout);
                        this.maintainerTimer.Cancel();
                        using (IConnectAlgorithms algorithms = (default(TConnectAlgorithms) == null) ? ((IConnectAlgorithms) Activator.CreateInstance<TConnectAlgorithms>()) : ((IConnectAlgorithms) default(TConnectAlgorithms)))
                        {
                            ReadOnlyCollection<PeerNodeAddress> src = this.ResolveNewAddresses(helper.RemainingTime(), true);
                            algorithms.Initialize(this, this.config, this.neighborManager.ConnectedNeighborCount + 1, new Dictionary<EndpointAddress, Referral>());
                            if ((src.Count > 0) && this.isOpen)
                            {
                                algorithms.UpdateEndpointsCollection(src);
                                algorithms.Connect(helper.RemainingTime());
                            }
                        }
                    }
                    finally
                    {
                        this.ResetMaintenance();
                    }
                }
            }
        }

        private void ResetMaintenance()
        {
            if (this.isOpen)
            {
                lock (this.ThisLock)
                {
                    if (this.isOpen)
                    {
                        try
                        {
                            this.maintainerTimer.Set(this.config.MaintainerInterval);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                    }
                }
            }
            lock (this.ThisLock)
            {
                this.isRunningMaintenance = false;
            }
        }

        private ReadOnlyCollection<PeerNodeAddress> ResolveNewAddresses(TimeSpan timeLeft, bool retryResolve)
        {
            TimeoutHelper helper = new TimeoutHelper(timeLeft);
            Dictionary<string, PeerNodeAddress> dictionary = new Dictionary<string, PeerNodeAddress>();
            List<PeerNodeAddress> list = new List<PeerNodeAddress>();
            PeerNodeAddress listenAddress = this.config.GetListenAddress(true);
            dictionary.Add(listenAddress.ServicePath, listenAddress);
            int num = retryResolve ? 2 : 1;
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                PeerMaintainerTraceRecord extendedData = new PeerMaintainerTraceRecord("Resolving");
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40051, System.ServiceModel.SR.GetString("TraceCodePeerMaintainerActivity"), extendedData, this, null);
            }
            for (int i = 0; ((i < num) && (list.Count < this.config.MaxResolveAddresses)) && (this.isOpen && (helper.RemainingTime() > TimeSpan.Zero)); i++)
            {
                ReadOnlyCollection<PeerNodeAddress> onlys;
                try
                {
                    onlys = this.config.Resolver.Resolve(this.config.MeshId, this.config.MaxResolveAddresses, helper.RemainingTime());
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        PeerMaintainerTraceRecord record2 = new PeerMaintainerTraceRecord("Resolve exception " + exception.Message);
                        TraceUtility.TraceEvent(TraceEventType.Information, 0x40051, System.ServiceModel.SR.GetString("TraceCodePeerMaintainerActivity"), record2, this, null);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("ResolverException"), exception));
                }
                if (onlys != null)
                {
                    foreach (PeerNodeAddress address2 in onlys)
                    {
                        if (!dictionary.ContainsKey(address2.ServicePath))
                        {
                            dictionary.Add(address2.ServicePath, address2);
                            if (((IPeerMaintainer) this).FindDuplicateNeighbor(address2) == null)
                            {
                                list.Add(address2);
                            }
                        }
                    }
                }
            }
            return new ReadOnlyCollection<PeerNodeAddress>(list);
        }

        public void ScheduleConnect(ConnectCallback<TConnectAlgorithms> connectCallback)
        {
            this.connectCallback = connectCallback;
            ActionItem.Schedule(new Action<object>(this.InitialConnection), null);
        }

        IAsyncResult IPeerMaintainer.BeginOpenNeighbor(PeerNodeAddress address, TimeSpan timeout, AsyncCallback callback, object asyncState)
        {
            lock (this.ThisLock)
            {
                EndpointAddress endpointAddress = address.EndpointAddress;
                if (this.referralCache.ContainsKey(endpointAddress))
                {
                    this.referralCache.Remove(endpointAddress);
                }
            }
            return this.neighborManager.BeginOpenNeighbor(address, timeout, callback, asyncState);
        }

        void IPeerMaintainer.CloseNeighbor(IPeerNeighbor neighbor, PeerCloseReason closeReason)
        {
            this.neighborManager.CloseNeighbor(neighbor, closeReason, PeerCloseInitiator.LocalNode);
        }

        IPeerNeighbor IPeerMaintainer.EndOpenNeighbor(IAsyncResult result)
        {
            return this.neighborManager.EndOpenNeighbor(result);
        }

        IPeerNeighbor IPeerMaintainer.FindDuplicateNeighbor(PeerNodeAddress address)
        {
            return this.neighborManager.FindDuplicateNeighbor(address);
        }

        IPeerNeighbor IPeerMaintainer.GetLeastUsefulNeighbor()
        {
            IPeerNeighbor neighbor = null;
            uint maxValue = uint.MaxValue;
            foreach (IPeerNeighbor neighbor2 in this.neighborManager.GetConnectedNeighbors())
            {
                UtilityExtension extension = neighbor2.Extensions.Find<UtilityExtension>();
                if (((extension != null) && extension.IsAccurate) && ((extension.LinkUtility < maxValue) && !neighbor2.IsClosing))
                {
                    maxValue = extension.LinkUtility;
                    neighbor = neighbor2;
                }
            }
            return neighbor;
        }

        PeerNodeAddress IPeerMaintainer.GetListenAddress()
        {
            return this.config.GetListenAddress(true);
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if ((e.Mode == PowerModes.Resume) && this.isOpen)
            {
                ActionItem.Schedule(new Action<object>(this.PingAndRefresh), null);
            }
        }

        int IPeerMaintainer.ConnectedNeighborCount
        {
            get
            {
                return this.neighborManager.ConnectedNeighborCount;
            }
        }

        bool IPeerMaintainer.IsOpen
        {
            get
            {
                return this.isOpen;
            }
        }

        int IPeerMaintainer.NonClosingNeighborCount
        {
            get
            {
                return this.neighborManager.NonClosingNeighborCount;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public delegate void ConnectCallback(Exception e);
    }
}

