namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal class PeerConnector : IPeerConnectorContract
    {
        private PeerNodeConfig config;
        private TypedMessageConverter connectInfoMessageConverter;
        private TypedMessageConverter disconnectInfoMessageConverter;
        private PeerMaintainer maintainer;
        private PeerNeighborManager neighborManager;
        private TypedMessageConverter refuseInfoMessageConverter;
        private State state;
        private object thisLock = new object();
        private Dictionary<IPeerNeighbor, IOThreadTimer> timerTable;
        private TypedMessageConverter welcomeInfoMessageConverter;

        public PeerConnector(PeerNodeConfig config, PeerNeighborManager neighborManager, PeerMaintainer maintainer)
        {
            this.config = config;
            this.neighborManager = neighborManager;
            this.maintainer = maintainer;
            this.timerTable = new Dictionary<IPeerNeighbor, IOThreadTimer>();
            this.state = State.Created;
        }

        private bool AddTimer(IPeerNeighbor neighbor)
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                if ((this.state == State.Opened) && (neighbor.State == PeerNeighborState.Connecting))
                {
                    IOThreadTimer timer = new IOThreadTimer(new Action<object>(this.OnConnectTimeout), neighbor, true);
                    timer.Set(this.config.ConnectTimeout);
                    this.timerTable.Add(neighbor, timer);
                    flag = true;
                }
            }
            return flag;
        }

        private void CleanupOnConnectFailure(IPeerNeighbor neighbor, PeerCloseReason reason, Exception exception)
        {
            if (this.RemoveTimer(neighbor))
            {
                this.neighborManager.CloseNeighbor(neighbor, reason, PeerCloseInitiator.LocalNode, exception);
            }
        }

        public void Close()
        {
            Dictionary<IPeerNeighbor, IOThreadTimer> timerTable;
            lock (this.ThisLock)
            {
                timerTable = this.timerTable;
                this.timerTable = null;
                this.state = State.Closed;
            }
            if (timerTable != null)
            {
                foreach (IOThreadTimer timer in timerTable.Values)
                {
                    timer.Cancel();
                }
            }
        }

        public void Closing()
        {
            lock (this.ThisLock)
            {
                this.state = State.Closing;
            }
        }

        private void CompleteTerminateMessageProcessing(IPeerNeighbor neighbor, PeerCloseReason closeReason, IList<Referral> referrals)
        {
            if (neighbor.TrySetState(PeerNeighborState.Disconnected))
            {
                this.neighborManager.CloseNeighbor(neighbor, closeReason, PeerCloseInitiator.RemoteNode);
            }
            else if (neighbor.State < PeerNeighborState.Disconnected)
            {
                throw Fx.AssertAndThrow("Unexpected neighbor state");
            }
            this.maintainer.AddReferrals(referrals, neighbor);
        }

        public void Connect(IPeerNeighbor neighbor, ConnectInfo connectInfo)
        {
            if (this.state == State.Opened)
            {
                PeerCloseReason none = PeerCloseReason.None;
                if ((neighbor.IsInitiator || !connectInfo.HasBody()) || ((neighbor.State != PeerNeighborState.Connecting) && (neighbor.State != PeerNeighborState.Closed)))
                {
                    none = PeerCloseReason.InvalidNeighbor;
                }
                else if (this.RemoveTimer(neighbor))
                {
                    if (this.neighborManager.ConnectedNeighborCount >= this.config.MaxNeighbors)
                    {
                        none = PeerCloseReason.NodeBusy;
                    }
                    else if (!PeerValidateHelper.ValidNodeAddress(connectInfo.Address))
                    {
                        none = PeerCloseReason.InvalidNeighbor;
                    }
                    else
                    {
                        PeerCloseReason reason2;
                        IPeerNeighbor neighbor2;
                        string action = "http://schemas.microsoft.com/net/2006/05/peer/Refuse";
                        this.ValidateNeighbor(neighbor, connectInfo.NodeId, out neighbor2, out reason2, out action);
                        if (neighbor != neighbor2)
                        {
                            this.SendWelcome(neighbor);
                            try
                            {
                                neighbor.ListenAddress = connectInfo.Address;
                            }
                            catch (ObjectDisposedException exception)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                            }
                            if (!neighbor.TrySetState(PeerNeighborState.Connected) && (neighbor.State < PeerNeighborState.Disconnecting))
                            {
                                throw Fx.AssertAndThrow("Neighbor state expected to be >= Disconnecting; it is " + neighbor.State.ToString());
                            }
                            if (neighbor2 != null)
                            {
                                this.SendTerminatingMessage(neighbor2, action, reason2);
                                this.neighborManager.CloseNeighbor(neighbor2, reason2, PeerCloseInitiator.LocalNode);
                            }
                        }
                        else
                        {
                            none = reason2;
                        }
                    }
                }
                if (none != PeerCloseReason.None)
                {
                    this.SendTerminatingMessage(neighbor, "http://schemas.microsoft.com/net/2006/05/peer/Refuse", none);
                    this.neighborManager.CloseNeighbor(neighbor, none, PeerCloseInitiator.LocalNode);
                }
            }
        }

        public void Disconnect(IPeerNeighbor neighbor, DisconnectInfo disconnectInfo)
        {
            if (this.state == State.Opened)
            {
                PeerCloseReason invalidNeighbor = PeerCloseReason.InvalidNeighbor;
                IList<Referral> referrals = null;
                if ((disconnectInfo.HasBody() && (neighbor.State >= PeerNeighborState.Connected)) && PeerConnectorHelper.IsDefined(disconnectInfo.Reason))
                {
                    invalidNeighbor = (PeerCloseReason) disconnectInfo.Reason;
                    referrals = disconnectInfo.Referrals;
                }
                this.CompleteTerminateMessageProcessing(neighbor, invalidNeighbor, referrals);
            }
        }

        private void OnConnectFailure(IPeerNeighbor neighbor, PeerCloseReason reason, Exception exception)
        {
            this.CleanupOnConnectFailure(neighbor, reason, exception);
        }

        private void OnConnectTimeout(object asyncState)
        {
            this.CleanupOnConnectFailure((IPeerNeighbor) asyncState, PeerCloseReason.ConnectTimedOut, null);
        }

        public void OnNeighborAuthenticated(IPeerNeighbor neighbor)
        {
            if (this.state == State.Created)
            {
                throw Fx.AssertAndThrow("Connector not expected to be in Created state");
            }
            if (!PeerNeighborStateHelper.IsAuthenticatedOrClosed(neighbor.State))
            {
                throw Fx.AssertAndThrow(string.Format(CultureInfo.InvariantCulture, "Neighbor state expected to be Authenticated or Closed, actual state: {0}", new object[] { neighbor.State }));
            }
            if (!neighbor.TrySetState(PeerNeighborState.Connecting))
            {
                if (neighbor.State < PeerNeighborState.Faulted)
                {
                    throw Fx.AssertAndThrow(string.Format(CultureInfo.InvariantCulture, "Neighbor state expected to be Faulted or Closed, actual state: {0}", new object[] { neighbor.State }));
                }
            }
            else if (this.AddTimer(neighbor) && neighbor.IsInitiator)
            {
                if (this.neighborManager.ConnectedNeighborCount < this.config.MaxNeighbors)
                {
                    this.SendConnect(neighbor);
                }
                else
                {
                    this.neighborManager.CloseNeighbor(neighbor, PeerCloseReason.NodeBusy, PeerCloseInitiator.LocalNode);
                }
            }
        }

        public void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            this.RemoveTimer(neighbor);
        }

        public void OnNeighborClosing(IPeerNeighbor neighbor, PeerCloseReason closeReason)
        {
            if (neighbor.IsConnected)
            {
                this.SendTerminatingMessage(neighbor, "http://schemas.microsoft.com/net/2006/05/peer/Disconnect", closeReason);
            }
        }

        public void Open()
        {
            lock (this.ThisLock)
            {
                if (this.state != State.Created)
                {
                    throw Fx.AssertAndThrow("Connector expected to be in Created state");
                }
                this.state = State.Opened;
            }
        }

        public void Refuse(IPeerNeighbor neighbor, RefuseInfo refuseInfo)
        {
            if (this.state == State.Opened)
            {
                PeerCloseReason invalidNeighbor = PeerCloseReason.InvalidNeighbor;
                IList<Referral> referrals = null;
                if ((refuseInfo.HasBody() && neighbor.IsInitiator) && ((neighbor.State == PeerNeighborState.Connecting) || (neighbor.State == PeerNeighborState.Closed)))
                {
                    this.RemoveTimer(neighbor);
                    if (PeerConnectorHelper.IsDefined(refuseInfo.Reason))
                    {
                        invalidNeighbor = (PeerCloseReason) refuseInfo.Reason;
                        referrals = refuseInfo.Referrals;
                    }
                }
                this.CompleteTerminateMessageProcessing(neighbor, invalidNeighbor, referrals);
            }
        }

        private bool RemoveTimer(IPeerNeighbor neighbor)
        {
            IOThreadTimer timer = null;
            bool flag = false;
            lock (this.ThisLock)
            {
                if ((this.state == State.Opened) && this.timerTable.TryGetValue(neighbor, out timer))
                {
                    flag = this.timerTable.Remove(neighbor);
                }
            }
            if (timer != null)
            {
                timer.Cancel();
                if (!flag)
                {
                    throw Fx.AssertAndThrow("Neighbor key should have beeen removed from the table");
                }
            }
            return flag;
        }

        private void SendConnect(IPeerNeighbor neighbor)
        {
            if ((neighbor.State == PeerNeighborState.Connecting) && (this.state == State.Opened))
            {
                PeerNodeAddress listenAddress = this.config.GetListenAddress(true);
                if (listenAddress != null)
                {
                    ConnectInfo typedMessage = new ConnectInfo(this.config.NodeId, listenAddress);
                    Message message = this.ConnectInfoMessageConverter.ToMessage(typedMessage, MessageVersion.Soap12WSAddressing10);
                    this.SendMessageToNeighbor(neighbor, message, new PeerMessageHelpers.CleanupCallback(this.OnConnectFailure));
                }
            }
        }

        private void SendMessageToNeighbor(IPeerNeighbor neighbor, Message message, PeerMessageHelpers.CleanupCallback cleanupCallback)
        {
            bool flag = false;
            try
            {
                neighbor.Send(message);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    flag = true;
                    throw;
                }
                if ((!(exception is CommunicationException) && !(exception is QuotaExceededException)) && (!(exception is ObjectDisposedException) && !(exception is TimeoutException)))
                {
                    throw;
                }
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                if (cleanupCallback != null)
                {
                    cleanupCallback(neighbor, PeerCloseReason.InternalFailure, exception);
                }
            }
            finally
            {
                if (!flag)
                {
                    message.Close();
                }
            }
        }

        private void SendTerminatingMessage(IPeerNeighbor neighbor, string action, PeerCloseReason closeReason)
        {
            if ((this.state == State.Opened) && (closeReason != PeerCloseReason.InvalidNeighbor))
            {
                if (neighbor.TrySetState(PeerNeighborState.Disconnecting))
                {
                    Message message;
                    Referral[] referrals = this.maintainer.GetReferrals();
                    if (action == "http://schemas.microsoft.com/net/2006/05/peer/Disconnect")
                    {
                        DisconnectInfo typedMessage = new DisconnectInfo((DisconnectReason) closeReason, referrals);
                        message = this.DisconnectInfoMessageConverter.ToMessage(typedMessage, MessageVersion.Soap12WSAddressing10);
                    }
                    else
                    {
                        RefuseInfo info2 = new RefuseInfo((RefuseReason) closeReason, referrals);
                        message = this.RefuseInfoMessageConverter.ToMessage(info2, MessageVersion.Soap12WSAddressing10);
                    }
                    this.SendMessageToNeighbor(neighbor, message, null);
                }
                else if (neighbor.State < PeerNeighborState.Disconnecting)
                {
                    throw Fx.AssertAndThrow("Neighbor state expected to be >= Disconnecting; it is " + neighbor.State.ToString());
                }
            }
        }

        private void SendWelcome(IPeerNeighbor neighbor)
        {
            if (this.state == State.Opened)
            {
                Referral[] referrals = this.maintainer.GetReferrals();
                WelcomeInfo typedMessage = new WelcomeInfo(this.config.NodeId, referrals);
                Message message = this.WelcomeInfoMessageConverter.ToMessage(typedMessage, MessageVersion.Soap12WSAddressing10);
                this.SendMessageToNeighbor(neighbor, message, new PeerMessageHelpers.CleanupCallback(this.OnConnectFailure));
            }
        }

        private void ValidateNeighbor(IPeerNeighbor neighbor, ulong neighborNodeId, out IPeerNeighbor neighborToClose, out PeerCloseReason closeReason, out string action)
        {
            neighborToClose = null;
            closeReason = PeerCloseReason.None;
            action = null;
            if (neighborNodeId == 0L)
            {
                neighborToClose = neighbor;
                closeReason = PeerCloseReason.InvalidNeighbor;
            }
            else if (neighborNodeId == this.config.NodeId)
            {
                neighborToClose = neighbor;
                closeReason = PeerCloseReason.DuplicateNodeId;
            }
            else
            {
                try
                {
                    neighbor.NodeId = neighborNodeId;
                }
                catch (ObjectDisposedException exception)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    return;
                }
                IPeerNeighbor peer = this.neighborManager.FindDuplicateNeighbor(neighborNodeId, neighbor);
                if ((peer != null) && this.neighborManager.PingNeighbor(peer))
                {
                    closeReason = PeerCloseReason.DuplicateNeighbor;
                    if (neighbor.IsInitiator == peer.IsInitiator)
                    {
                        neighborToClose = neighbor;
                    }
                    else if (this.config.NodeId > neighborNodeId)
                    {
                        neighborToClose = neighbor.IsInitiator ? neighbor : peer;
                    }
                    else
                    {
                        neighborToClose = neighbor.IsInitiator ? peer : neighbor;
                    }
                }
            }
            if ((neighborToClose != null) && (neighborToClose != neighbor))
            {
                if (neighborToClose.State == PeerNeighborState.Connected)
                {
                    action = "http://schemas.microsoft.com/net/2006/05/peer/Disconnect";
                }
                else if (!neighborToClose.IsInitiator && (neighborToClose.State == PeerNeighborState.Connecting))
                {
                    action = "http://schemas.microsoft.com/net/2006/05/peer/Refuse";
                }
            }
        }

        public void Welcome(IPeerNeighbor neighbor, WelcomeInfo welcomeInfo)
        {
            if (this.state == State.Opened)
            {
                PeerCloseReason none = PeerCloseReason.None;
                if ((!neighbor.IsInitiator || !welcomeInfo.HasBody()) || ((neighbor.State != PeerNeighborState.Connecting) && (neighbor.State != PeerNeighborState.Closed)))
                {
                    none = PeerCloseReason.InvalidNeighbor;
                }
                else if (this.RemoveTimer(neighbor))
                {
                    PeerCloseReason reason2;
                    IPeerNeighbor neighbor2;
                    string action = "http://schemas.microsoft.com/net/2006/05/peer/Refuse";
                    this.ValidateNeighbor(neighbor, welcomeInfo.NodeId, out neighbor2, out reason2, out action);
                    if (neighbor != neighbor2)
                    {
                        if (this.maintainer.AddReferrals(welcomeInfo.Referrals, neighbor))
                        {
                            if (!neighbor.TrySetState(PeerNeighborState.Connected) && (neighbor.State < PeerNeighborState.Faulted))
                            {
                                throw Fx.AssertAndThrow("Neighbor state expected to be >= Faulted; it is " + neighbor.State.ToString());
                            }
                            if (neighbor2 != null)
                            {
                                this.SendTerminatingMessage(neighbor2, action, reason2);
                                this.neighborManager.CloseNeighbor(neighbor2, reason2, PeerCloseInitiator.LocalNode);
                            }
                        }
                        else
                        {
                            none = PeerCloseReason.InvalidNeighbor;
                        }
                    }
                    else
                    {
                        none = reason2;
                    }
                }
                if (none != PeerCloseReason.None)
                {
                    this.SendTerminatingMessage(neighbor, "http://schemas.microsoft.com/net/2006/05/peer/Disconnect", none);
                    this.neighborManager.CloseNeighbor(neighbor, none, PeerCloseInitiator.LocalNode);
                }
            }
        }

        internal TypedMessageConverter ConnectInfoMessageConverter
        {
            get
            {
                if (this.connectInfoMessageConverter == null)
                {
                    this.connectInfoMessageConverter = TypedMessageConverter.Create(typeof(ConnectInfo), "http://schemas.microsoft.com/net/2006/05/peer/Connect");
                }
                return this.connectInfoMessageConverter;
            }
        }

        internal TypedMessageConverter DisconnectInfoMessageConverter
        {
            get
            {
                if (this.disconnectInfoMessageConverter == null)
                {
                    this.disconnectInfoMessageConverter = TypedMessageConverter.Create(typeof(DisconnectInfo), "http://schemas.microsoft.com/net/2006/05/peer/Disconnect");
                }
                return this.disconnectInfoMessageConverter;
            }
        }

        internal TypedMessageConverter RefuseInfoMessageConverter
        {
            get
            {
                if (this.refuseInfoMessageConverter == null)
                {
                    this.refuseInfoMessageConverter = TypedMessageConverter.Create(typeof(RefuseInfo), "http://schemas.microsoft.com/net/2006/05/peer/Refuse");
                }
                return this.refuseInfoMessageConverter;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        internal TypedMessageConverter WelcomeInfoMessageConverter
        {
            get
            {
                if (this.welcomeInfoMessageConverter == null)
                {
                    this.welcomeInfoMessageConverter = TypedMessageConverter.Create(typeof(WelcomeInfo), "http://schemas.microsoft.com/net/2006/05/peer/Welcome");
                }
                return this.welcomeInfoMessageConverter;
            }
        }

        private enum State
        {
            Created,
            Opened,
            Closed,
            Closing
        }
    }
}

