namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading;

    internal class UtilityExtension : IExtension<IPeerNeighbor>
    {
        public const int AcceptableMissDistance = 2;
        private IOThreadTimer ackTimer;
        private int checkPointPendingSends;
        private int expectedClearance;
        private uint inTotal;
        private uint inUseful;
        private bool isMonitoring;
        private uint linkUtility;
        private const uint linkUtilityIncrement = 0x80;
        private const uint maxLinkUtility = 0x1000;
        private TypedMessageConverter messageConverter;
        private const int MinimumPendingMessages = 8;
        private int outTotal;
        private IPeerNeighbor owner;
        private int pendingSends;
        private TimeSpan pruneInterval;
        private const int PruneIntervalMilliseconds = 0x2710;
        private PruneNeighborCallback pruneNeighbor;
        private IOThreadTimer pruneTimer;
        private object thisLock = new object();
        private object throttleLock = new object();
        private uint updateCount;

        public event EventHandler UtilityInfoReceived;

        public event EventHandler UtilityInfoSent;

        private UtilityExtension()
        {
            this.ackTimer = new IOThreadTimer(new Action<object>(this.AcknowledgeLoop), null, false);
            this.pendingSends = 0;
            this.pruneTimer = new IOThreadTimer(new Action<object>(this.VerifyCheckPoint), null, false);
            this.pruneInterval = TimeSpan.FromMilliseconds((double) (0x2710 + new Random(Process.GetCurrentProcess().Id).Next(0x2710)));
        }

        private void AcknowledgeLoop(object state)
        {
            IPeerNeighbor owner = this.owner;
            if ((owner != null) && owner.IsConnected)
            {
                this.FlushAcknowledge();
                if (this.owner != null)
                {
                    this.ackTimer.Set(0x7530);
                }
            }
        }

        public void Attach(IPeerNeighbor host)
        {
            this.owner = host;
            this.ackTimer.Set(0x7530);
        }

        public void BeginCheckPoint(PruneNeighborCallback pruneCallback)
        {
            if (!this.isMonitoring)
            {
                lock (this.throttleLock)
                {
                    if (!this.isMonitoring)
                    {
                        this.checkPointPendingSends = this.pendingSends;
                        this.pruneNeighbor = pruneCallback;
                        this.expectedClearance = this.pendingSends / 2;
                        this.isMonitoring = true;
                        if (this.owner != null)
                        {
                            this.pruneTimer.Set(this.pruneInterval);
                        }
                    }
                }
            }
        }

        private uint Calculate(uint current, bool increase)
        {
            uint num = 0;
            num = (current * 0x1f) / 0x20;
            if (increase)
            {
                num += 0x80;
            }
            if (num > 0x1000)
            {
                throw Fx.AssertAndThrow("Link utility should not exceed " + 0x1000);
            }
            if (!this.IsAccurate)
            {
                this.updateCount++;
            }
            return num;
        }

        public void Detach(IPeerNeighbor host)
        {
            this.ackTimer.Cancel();
            this.owner = null;
            lock (this.throttleLock)
            {
                this.pruneTimer.Cancel();
            }
        }

        public void FlushAcknowledge()
        {
            if (this.inTotal != 0)
            {
                uint useful = 0;
                uint total = 0;
                lock (this.ThisLock)
                {
                    useful = this.inUseful;
                    total = this.inTotal;
                    this.inUseful = 0;
                    this.inTotal = 0;
                }
                this.SendUtilityMessage(useful, total);
            }
        }

        private Exception HandleSendException(IPeerNeighbor host, Exception e, UtilityInfo umessage)
        {
            if ((!(e is ObjectDisposedException) && !(e is TimeoutException)) && !(e is CommunicationException))
            {
                return e;
            }
            if (e.InnerException is QuotaExceededException)
            {
                throw Fx.AssertAndThrow("insufficient quota for sending messages!");
            }
            lock (this.ThisLock)
            {
                this.inTotal += umessage.Total;
                this.inUseful += umessage.Useful;
            }
            return null;
        }

        public void OnEndSend(FloodAsyncResult fresult)
        {
            Interlocked.Decrement(ref this.pendingSends);
        }

        public static void OnEndSend(IPeerNeighbor neighbor, FloodAsyncResult fresult)
        {
            if (neighbor.State < PeerNeighborState.Disconnecting)
            {
                UtilityExtension utility = neighbor.Utility;
                if (utility != null)
                {
                    utility.OnEndSend(fresult);
                }
            }
        }

        private void OnMessageSent()
        {
            lock (this.ThisLock)
            {
                this.outTotal++;
            }
            Interlocked.Increment(ref this.pendingSends);
        }

        public static void OnMessageSent(IPeerNeighbor neighbor)
        {
            UtilityExtension extension = neighbor.Extensions.Find<UtilityExtension>();
            if (extension != null)
            {
                extension.OnMessageSent();
            }
        }

        public static void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            UtilityExtension item = neighbor.Extensions.Find<UtilityExtension>();
            if (item != null)
            {
                neighbor.Extensions.Remove(item);
            }
        }

        public static void OnNeighborConnected(IPeerNeighbor neighbor)
        {
            neighbor.Extensions.Add(new UtilityExtension());
        }

        public static void ProcessLinkUtility(IPeerNeighbor neighbor, UtilityInfo umessage)
        {
            UtilityExtension extension = neighbor.Extensions.Find<UtilityExtension>();
            if (extension != null)
            {
                extension.ProcessLinkUtility(umessage.Useful, umessage.Total);
            }
        }

        private void ProcessLinkUtility(uint useful, uint total)
        {
            uint num = 0;
            lock (this.ThisLock)
            {
                if (((total > 0x20) || (useful > total)) || (this.outTotal < total))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerLinkUtilityInvalidValues", new object[] { useful, total })));
                }
                num = 0;
                while (num < useful)
                {
                    this.linkUtility = this.Calculate(this.linkUtility, true);
                    num++;
                }
                while (num < total)
                {
                    this.linkUtility = this.Calculate(this.linkUtility, false);
                    num++;
                }
                this.outTotal -= (int) total;
            }
            if (this.UtilityInfoReceived != null)
            {
                this.UtilityInfoReceived(this, EventArgs.Empty);
            }
        }

        private void ReportCacheMiss(int missedBy)
        {
            lock (this.ThisLock)
            {
                for (int i = 0; i < missedBy; i++)
                {
                    this.linkUtility = this.Calculate(this.linkUtility, false);
                }
            }
        }

        internal static void ReportCacheMiss(IPeerNeighbor neighbor, int missedBy)
        {
            if (neighbor.IsConnected)
            {
                UtilityExtension extension = neighbor.Extensions.Find<UtilityExtension>();
                if (extension != null)
                {
                    extension.ReportCacheMiss(missedBy);
                }
            }
        }

        private void SendUtilityMessage(uint useful, uint total)
        {
            IPeerNeighbor owner = this.owner;
            if (((owner != null) && PeerNeighborStateHelper.IsConnected(owner.State)) && (total != 0))
            {
                UtilityInfo typedMessage = new UtilityInfo(useful, total);
                IAsyncResult result = null;
                Message message = this.MessageConverter.ToMessage(typedMessage, MessageVersion.Soap12WSAddressing10);
                bool flag = false;
                try
                {
                    result = owner.BeginSend(message, Fx.ThunkCallback(new AsyncCallback(this.UtilityMessageSent)), new AsyncUtilityState(message, typedMessage));
                    if (result.CompletedSynchronously)
                    {
                        owner.EndSend(result);
                        EventHandler utilityInfoSent = this.UtilityInfoSent;
                        if (utilityInfoSent != null)
                        {
                            utilityInfoSent(this, EventArgs.Empty);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        flag = true;
                        throw;
                    }
                    if (this.HandleSendException(owner, exception, typedMessage) != null)
                    {
                        throw;
                    }
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                finally
                {
                    if (!flag && ((result == null) || result.CompletedSynchronously))
                    {
                        message.Close();
                    }
                }
            }
        }

        public uint UpdateLinkUtility(bool useful)
        {
            lock (this.ThisLock)
            {
                this.inTotal++;
                if (useful)
                {
                    this.inUseful++;
                }
                this.linkUtility = this.Calculate(this.linkUtility, useful);
                if (this.inTotal == 0x20)
                {
                    this.FlushAcknowledge();
                }
            }
            return this.linkUtility;
        }

        public static uint UpdateLinkUtility(IPeerNeighbor neighbor, bool useful)
        {
            uint num = 0;
            UtilityExtension extension = neighbor.Extensions.Find<UtilityExtension>();
            if (extension != null)
            {
                num = extension.UpdateLinkUtility(useful);
            }
            return num;
        }

        private void UtilityMessageSent(IAsyncResult result)
        {
            if ((result != null) && (result.AsyncState != null))
            {
                IPeerNeighbor owner = this.owner;
                if (((owner != null) && PeerNeighborStateHelper.IsConnected(owner.State)) && !result.CompletedSynchronously)
                {
                    AsyncUtilityState asyncState = (AsyncUtilityState) result.AsyncState;
                    Message message = asyncState.message;
                    UtilityInfo umessage = asyncState.info;
                    bool flag = false;
                    if (umessage == null)
                    {
                        throw Fx.AssertAndThrow("expecting a UtilityInfo message in the AsyncState!");
                    }
                    try
                    {
                        owner.EndSend(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            flag = true;
                            throw;
                        }
                        if (this.HandleSendException(owner, exception, umessage) != null)
                        {
                            throw;
                        }
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    finally
                    {
                        if (!flag)
                        {
                            message.Close();
                        }
                    }
                    EventHandler utilityInfoSent = this.UtilityInfoSent;
                    if (utilityInfoSent != null)
                    {
                        utilityInfoSent(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void VerifyCheckPoint(object state)
        {
            IPeerNeighbor owner = this.owner;
            if ((owner != null) && owner.IsConnected)
            {
                int pendingSends;
                int checkPointPendingSends;
                lock (this.throttleLock)
                {
                    pendingSends = this.pendingSends;
                    checkPointPendingSends = this.checkPointPendingSends;
                }
                if (pendingSends <= 8)
                {
                    lock (this.throttleLock)
                    {
                        this.isMonitoring = false;
                        return;
                    }
                }
                if ((pendingSends + this.expectedClearance) >= checkPointPendingSends)
                {
                    this.pruneNeighbor(owner);
                }
                else
                {
                    lock (this.throttleLock)
                    {
                        if (this.owner != null)
                        {
                            this.checkPointPendingSends = this.pendingSends;
                            this.expectedClearance /= 2;
                            this.pruneTimer.Set(this.pruneInterval);
                        }
                    }
                }
            }
        }

        public bool IsAccurate
        {
            get
            {
                return (this.updateCount >= 0x20);
            }
        }

        public uint LinkUtility
        {
            get
            {
                return this.linkUtility;
            }
        }

        internal TypedMessageConverter MessageConverter
        {
            get
            {
                if (this.messageConverter == null)
                {
                    this.messageConverter = TypedMessageConverter.Create(typeof(UtilityInfo), "http://schemas.microsoft.com/net/2006/05/peer/LinkUtility");
                }
                return this.messageConverter;
            }
        }

        public int PendingMessages
        {
            get
            {
                return this.pendingSends;
            }
        }

        public object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private class AsyncUtilityState
        {
            public UtilityInfo info;
            public Message message;

            public AsyncUtilityState(Message message, UtilityInfo info)
            {
                this.message = message;
                this.info = info;
            }
        }

        public delegate void PruneNeighborCallback(IPeerNeighbor peer);
    }
}

