namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;

    internal abstract class PeerFlooderBase<TFloodContract, TLinkContract> : IFlooderForThrottle, IPeerFlooderContract<TFloodContract, TLinkContract> where TFloodContract: Message
    {
        protected PeerNodeConfig config;
        internal IPeerNodeMessageHandling messageHandler;
        private long messageSequence;
        protected PeerNeighborManager neighborManager;
        protected List<IPeerNeighbor> neighbors;
        public EventHandler OnMessageSentHandler;
        internal PeerThrottleHelper<TFloodContract, TLinkContract> quotaHelper;
        private object thisLock;

        public event EventHandler SlowNeighborKilled;

        public event EventHandler ThrottleReached;

        public event EventHandler ThrottleReleased;

        public PeerFlooderBase(PeerNodeConfig config, PeerNeighborManager neighborManager)
        {
            this.thisLock = new object();
            this.neighborManager = neighborManager;
            this.neighbors = new List<IPeerNeighbor>();
            this.config = config;
            this.neighbors = this.neighborManager.GetConnectedNeighbors();
            this.quotaHelper = new PeerThrottleHelper<TFloodContract, TLinkContract>(this, this.config.MaxPendingOutgoingCalls);
            this.OnMessageSentHandler = new EventHandler(this.OnMessageSent);
        }

        public virtual IAsyncResult BeginFloodEncodedMessage(byte[] id, MessageBuffer encodedMessage, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            this.RecordOutgoingMessage(id);
            SynchronizationContext currentSynchronizationContext = ThreadBehavior.GetCurrentSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(null);
            if (this.neighbors.Count == 0)
            {
                return new CompletedAsyncResult(callback, state);
            }
            try
            {
                result = this.FloodMessageToNeighbors(encodedMessage, timeout, callback, state, -1, null, null, this.OnMessageSentHandler);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(currentSynchronizationContext);
            }
            return result;
        }

        protected virtual IAsyncResult BeginFloodReceivedMessage(IPeerNeighbor sender, MessageBuffer messageBuffer, TimeSpan timeout, AsyncCallback callback, object state, int index, MessageHeader hopHeader)
        {
            this.quotaHelper.AcquireNoQueue();
            try
            {
                return this.FloodMessageToNeighbors(messageBuffer, timeout, callback, state, index, hopHeader, sender, this.OnMessageSentHandler);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!(exception is QuotaExceededException) && (!(exception is CommunicationException) || !(exception.InnerException is QuotaExceededException)))
                {
                    throw;
                }
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceError)
                {
                    PeerFlooderTraceRecord extendedData = new PeerFlooderTraceRecord(this.config.MeshId, sender.ListenAddress, exception);
                    TraceUtility.TraceEvent(TraceEventType.Error, 0x4004f, System.ServiceModel.SR.GetString("TraceCodePeerFlooderReceiveMessageQuotaExceeded"), extendedData, this, null);
                }
                return null;
            }
        }

        protected IAsyncResult BeginSendHelper(IPeerNeighbor neighbor, TimeSpan timeout, Message message, FloodAsyncResult fresult)
        {
            IAsyncResult result = null;
            IAsyncResult result2;
            bool flag = false;
            try
            {
                UtilityExtension.OnMessageSent(neighbor);
                result = neighbor.BeginSend(message, timeout, Fx.ThunkCallback(new AsyncCallback(fresult.OnSendComplete)), message);
                fresult.AddResult(result, neighbor);
                if (result.CompletedSynchronously)
                {
                    neighbor.EndSend(result);
                    UtilityExtension.OnEndSend(neighbor, fresult);
                }
                result2 = result;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    flag = true;
                    throw;
                }
                if (PeerFlooderBase<TFloodContract, TLinkContract>.CloseNeighborIfKnownException(this.neighborManager, exception, neighbor) != null)
                {
                    fresult.MarkEnd(false);
                    throw;
                }
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                result2 = null;
            }
            finally
            {
                if (((result == null) || result.CompletedSynchronously) && !flag)
                {
                    message.Close();
                }
            }
            return result2;
        }

        public void Close()
        {
            this.OnClose();
        }

        internal static Exception CloseNeighborIfKnownException(PeerNeighborManager neighborManager, Exception exception, IPeerNeighbor peer)
        {
            try
            {
                if (exception is ObjectDisposedException)
                {
                    return null;
                }
                if (((exception is CommunicationException) && !(exception.InnerException is QuotaExceededException)) || (((exception is TimeoutException) || (exception is InvalidOperationException)) || (exception is MessageSecurityException)))
                {
                    neighborManager.CloseNeighbor(peer, PeerCloseReason.InternalFailure, PeerCloseInitiator.LocalNode, exception);
                    return null;
                }
                return exception;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                return exception2;
            }
        }

        public static void EndFloodEncodedMessage(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                FloodAsyncResult result3 = result as FloodAsyncResult;
                if (result3 == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", System.ServiceModel.SR.GetString("InvalidAsyncResult"));
                }
                result3.End();
            }
        }

        public virtual void EndFloodMessage(IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                (result as FloodAsyncResult).End();
            }
        }

        public void EndFloodReceivedMessage(IAsyncResult result)
        {
        }

        public void FireDequeuedEvent()
        {
            this.FireEvent(this.ThrottleReleased);
        }

        private void FireEvent(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void FireKilledEvent()
        {
            this.FireEvent(this.SlowNeighborKilled);
        }

        public void FireReachedEvent()
        {
            this.FireEvent(this.ThrottleReached);
        }

        protected virtual IAsyncResult FloodMessageToNeighbors(MessageBuffer messageBuffer, TimeSpan timeout, AsyncCallback callback, object state, int index, MessageHeader hopHeader, IPeerNeighbor except, EventHandler OnMessageSentCallback)
        {
            Interlocked.Increment(ref this.messageSequence);
            FloodAsyncResult fresult = new FloodAsyncResult(this.neighborManager, timeout, callback, state);
            fresult.OnMessageSent += OnMessageSentCallback;
            foreach (IPeerNeighbor neighbor in this.Neighbors)
            {
                if (!neighbor.Equals(except) && PeerNeighborStateHelper.IsConnected(neighbor.State))
                {
                    Message message = messageBuffer.CreateMessage();
                    if (index != -1)
                    {
                        message.Headers.ReplaceAt(index, hopHeader);
                    }
                    if (PeerNeighborStateHelper.IsConnected(neighbor.State))
                    {
                        this.BeginSendHelper(neighbor, timeout, message, fresult);
                    }
                }
            }
            fresult.MarkEnd(true);
            return fresult;
        }

        private void KillSlowNeighbor()
        {
            IPeerNeighbor neighbor = this.neighborManager.SlowestNeighbor();
            if (neighbor != null)
            {
                neighbor.Abort(PeerCloseReason.NodeTooSlow, PeerCloseInitiator.LocalNode);
            }
        }

        public abstract void OnClose();
        public virtual IAsyncResult OnFloodedMessage(IPeerNeighbor neighbor, TFloodContract floodInfo, AsyncCallback callback, object state)
        {
            bool useful = false;
            MessageBuffer messageBuffer = null;
            Message message = null;
            int index = 0;
            ulong maxValue = ulong.MaxValue;
            MessageHeader hopHeader = null;
            bool flag2 = false;
            PeerMessageProperty property = null;
            IAsyncResult result = null;
            try
            {
                property = (PeerMessageProperty) floodInfo.Properties["PeerProperty"];
                if (!property.MessageVerified)
                {
                    if (property.CacheMiss > 2)
                    {
                        UtilityExtension.ReportCacheMiss(neighbor, property.CacheMiss);
                    }
                    result = new CompletedAsyncResult(callback, state);
                }
                else
                {
                    useful = true;
                    messageBuffer = floodInfo.CreateBufferedCopy((int) this.config.MaxReceivedMessageSize);
                    message = messageBuffer.CreateMessage();
                    Uri peerVia = property.PeerVia;
                    Uri peerTo = property.PeerTo;
                    message.Headers.To = message.Properties.Via = peerVia;
                    index = this.UpdateHopCount(message, out hopHeader, out maxValue);
                    PeerMessagePropagation localAndRemote = PeerMessagePropagation.LocalAndRemote;
                    if (property.SkipLocalChannels)
                    {
                        localAndRemote = PeerMessagePropagation.Remote;
                    }
                    else if (this.messageHandler.HasMessagePropagation)
                    {
                        using (Message message2 = messageBuffer.CreateMessage())
                        {
                            localAndRemote = this.messageHandler.DetermineMessagePropagation(message2, PeerMessageOrigination.Remote);
                        }
                    }
                    if (((localAndRemote & PeerMessagePropagation.Remote) != PeerMessagePropagation.None) && (maxValue == 0L))
                    {
                        localAndRemote &= ~PeerMessagePropagation.Remote;
                    }
                    if ((localAndRemote & PeerMessagePropagation.Remote) != PeerMessagePropagation.None)
                    {
                        result = this.BeginFloodReceivedMessage(neighbor, messageBuffer, PeerTransportConstants.ForwardTimeout, callback, state, index, hopHeader);
                    }
                    else
                    {
                        result = new CompletedAsyncResult(callback, state);
                    }
                    if ((localAndRemote & PeerMessagePropagation.Local) != PeerMessagePropagation.None)
                    {
                        this.messageHandler.HandleIncomingMessage(messageBuffer, localAndRemote, index, hopHeader, peerVia, peerTo);
                    }
                }
                UtilityExtension.UpdateLinkUtility(neighbor, useful);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    flag2 = true;
                    throw;
                }
                if (PeerFlooderBase<TFloodContract, TLinkContract>.CloseNeighborIfKnownException(this.neighborManager, exception, neighbor) != null)
                {
                    throw;
                }
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
            }
            finally
            {
                if (!flag2)
                {
                    if (message != null)
                    {
                        message.Close();
                    }
                    if (messageBuffer != null)
                    {
                        messageBuffer.Close();
                    }
                }
            }
            return result;
        }

        public void OnMessageSent(object sender, EventArgs args)
        {
            this.quotaHelper.ItemDequeued();
        }

        public virtual void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            this.neighbors = this.neighborManager.GetConnectedNeighbors();
        }

        public virtual void OnNeighborConnected(IPeerNeighbor neighbor)
        {
            this.neighbors = this.neighborManager.GetConnectedNeighbors();
        }

        public abstract void OnOpen();
        public void Open()
        {
            this.OnOpen();
        }

        public abstract void ProcessLinkUtility(IPeerNeighbor neighbor, TLinkContract utilityInfo);
        private void PruneNeighborCallback(IPeerNeighbor peer)
        {
            lock (this.ThisLock)
            {
                if (this.Neighbors.Count <= 1)
                {
                    return;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    string message = System.ServiceModel.SR.GetString("PeerThrottlePruning", new object[] { this.config.MeshId });
                    PeerThrottleTraceRecord extendedData = new PeerThrottleTraceRecord(this.config.MeshId, message);
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x4004f, System.ServiceModel.SR.GetString("TraceCodePeerFlooderReceiveMessageQuotaExceeded"), extendedData, this, null);
                }
            }
            try
            {
                peer.Abort(PeerCloseReason.NodeTooSlow, PeerCloseInitiator.LocalNode);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (PeerFlooderBase<TFloodContract, TLinkContract>.CloseNeighborIfKnownException(this.neighborManager, exception, peer) != null)
                {
                    throw;
                }
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
            }
        }

        public abstract void RecordOutgoingMessage(byte[] id);
        public abstract bool ShouldProcess(TFloodContract floodInfo);
        void IFlooderForThrottle.OnThrottleReached()
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                string message = System.ServiceModel.SR.GetString("PeerThrottleWaiting", new object[] { this.config.MeshId });
                PeerThrottleTraceRecord extendedData = new PeerThrottleTraceRecord(this.config.MeshId, message);
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4004f, System.ServiceModel.SR.GetString("TraceCodePeerFlooderReceiveMessageQuotaExceeded"), extendedData, this, null);
            }
            IPeerNeighbor neighbor = this.neighborManager.SlowestNeighbor();
            if (neighbor != null)
            {
                UtilityExtension utility = neighbor.Utility;
                if (neighbor.IsConnected && (utility != null))
                {
                    if (utility.PendingMessages > 0x20)
                    {
                        utility.BeginCheckPoint(new System.ServiceModel.Channels.UtilityExtension.PruneNeighborCallback(this.PruneNeighborCallback));
                    }
                    this.FireReachedEvent();
                }
            }
        }

        void IFlooderForThrottle.OnThrottleReleased()
        {
            this.FireDequeuedEvent();
        }

        private int UpdateHopCount(Message message, out MessageHeader hopHeader, out ulong currentValue)
        {
            int index = -1;
            currentValue = ulong.MaxValue;
            hopHeader = null;
            try
            {
                index = message.Headers.FindHeader("Hops", "http://schemas.microsoft.com/net/2006/05/peer/HopCount");
                if (index != -1)
                {
                    ulong num2;
                    currentValue = PeerMessageHelpers.GetHeaderULong(message.Headers, index);
                    currentValue = num2 = (ulong) (currentValue - 1L);
                    hopHeader = MessageHeader.CreateHeader("Hops", "http://schemas.microsoft.com/net/2006/05/peer/HopCount", num2, false);
                }
            }
            catch (MessageHeaderException exception)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
            }
            catch (CommunicationException exception2)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
            }
            catch (SerializationException exception3)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
            }
            catch (XmlException exception4)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Warning);
            }
            return index;
        }

        protected long MaxReceivedMessageSize
        {
            get
            {
                return this.config.MaxReceivedMessageSize;
            }
        }

        protected System.ServiceModel.Channels.MessageEncoder MessageEncoder
        {
            get
            {
                return this.config.MessageEncoder;
            }
        }

        protected List<IPeerNeighbor> Neighbors
        {
            get
            {
                return this.neighbors;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        public class PeerThrottleHelper
        {
            private IFlooderForThrottle flooder;
            private int outgoingEnqueuedCount;
            private int outgoingQuota;

            public PeerThrottleHelper(IFlooderForThrottle flooder, int outgoingLimit)
            {
                this.outgoingQuota = 0x80;
                this.outgoingQuota = outgoingLimit;
                this.flooder = flooder;
            }

            public void AcquireNoQueue()
            {
                if (Interlocked.Increment(ref this.outgoingEnqueuedCount) >= this.outgoingQuota)
                {
                    this.flooder.OnThrottleReached();
                }
            }

            public void ItemDequeued()
            {
                Interlocked.Decrement(ref this.outgoingEnqueuedCount);
            }
        }
    }
}

