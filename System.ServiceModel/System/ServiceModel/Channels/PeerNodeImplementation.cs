namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal class PeerNodeImplementation : IPeerNodeMessageHandling
    {
        private BufferManager bufferManager;
        private PeerNodeTraceRecord completeTraceRecord;
        private PeerNodeConfig config;
        private ManualResetEvent connectCompletedEvent = new ManualResetEvent(false);
        internal PeerConnector connector;
        private int connectTimeout = 0xea60;
        internal static byte[] DefaultId = new byte[0];
        private MessageEncoder encoder = new BinaryMessageEncodingBindingElement().CreateMessageEncoderFactory().Encoder;
        internal MessageEncodingBindingElement EncodingElement;
        internal PeerFlooder flooder;
        private int idealNeighbors = 3;
        private PeerIPHelper ipHelper;
        private bool isOpen;
        private IPAddress listenIPAddress;
        private Uri listenUri;
        private PeerMaintainer maintainer;
        private int maintainerInterval = 0x493e0;
        private long maxBufferPoolSize;
        private int maxNeighbors = 7;
        private long maxReceivedMessageSize = 0x10000L;
        internal int MaxReceiveQueue = 0x80;
        private int maxReferrals = 10;
        internal int MaxSendQueue = 0x80;
        private const int maxViaSize = 0x1000;
        private string meshId;
        private Dictionary<object, MessageFilterRegistration> messageFilters = new Dictionary<object, MessageFilterRegistration>();
        private PeerMessagePropagationFilter messagePropagationFilter;
        private SynchronizationContext messagePropagationFilterContext;
        private int minNeighbors = 2;
        private PeerNeighborManager neighborManager;
        private Exception openException;
        internal static Dictionary<Uri, PeerNodeImplementation> peerNodes = new Dictionary<Uri, PeerNodeImplementation>();
        private int port = 0;
        private XmlDictionaryReaderQuotas readerQuotas;
        private int refCount;
        private bool registered;
        private PeerResolver resolver;
        private object resolverRegistrationId;
        private PeerSecurityManager securityManager;
        private PeerService service;
        private Dictionary<System.Type, object> serviceHandlers;
        private SimpleStateManager stateManager;
        private object thisLock = new object();
        private PeerNodeTraceRecord traceRecord;
        private Dictionary<Uri, RefCountedSecurityProtocol> uri2SecurityProtocol;

        public event EventHandler Aborted;

        public event EventHandler<PeerNeighborCloseEventArgs> NeighborClosed;

        public event EventHandler<PeerNeighborCloseEventArgs> NeighborClosing;

        public event EventHandler NeighborConnected;

        public event EventHandler NeighborOpened;

        public event EventHandler Offline;

        public event EventHandler Online;

        public PeerNodeImplementation()
        {
            this.stateManager = new SimpleStateManager(this);
            this.uri2SecurityProtocol = new Dictionary<Uri, RefCountedSecurityProtocol>();
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            this.maxBufferPoolSize = 0x80000L;
        }

        public void Abort()
        {
            this.stateManager.Abort();
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.stateManager.BeginClose(timeout, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state, bool waitForOnline)
        {
            return this.stateManager.BeginOpen(timeout, callback, state, waitForOnline);
        }

        public IAsyncResult BeginSend(object registrant, Message message, Uri via, ITransportFactorySettings settings, TimeSpan timeout, AsyncCallback callback, object state, SecurityProtocol securityProtocol)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            MessageBuffer encodedMessage = null;
            Message message2 = null;
            ulong maxValue = ulong.MaxValue;
            PeerMessagePropagation localAndRemote = PeerMessagePropagation.LocalAndRemote;
            int messageSize = -1;
            SendAsyncResult result = new SendAsyncResult(callback, state);
            AsyncCallback callback2 = Fx.ThunkCallback(new AsyncCallback(result.OnFloodComplete));
            try
            {
                PeerFlooder flooder;
                byte[] primarySignatureValue;
                lock (this.ThisLock)
                {
                    this.ThrowIfNotOpen();
                    flooder = this.flooder;
                }
                int maxBufferSize = (int) Math.Min(this.maxReceivedMessageSize, settings.MaxReceivedMessageSize);
                Guid guid = this.ProcessOutgoingMessage(message, via);
                this.SecureOutgoingMessage(ref message, via, timeout, securityProtocol);
                if (message is SecurityAppliedMessage)
                {
                    ArraySegment<byte> buffer = this.encoder.WriteMessage(message, 0x7fffffff, this.bufferManager);
                    message2 = this.encoder.ReadMessage(buffer, this.bufferManager);
                    primarySignatureValue = (message as SecurityAppliedMessage).PrimarySignatureValue;
                    messageSize = buffer.Count;
                }
                else
                {
                    message2 = message;
                    primarySignatureValue = guid.ToByteArray();
                }
                encodedMessage = message2.CreateBufferedCopy(maxBufferSize);
                string contentType = settings.MessageEncoderFactory.Encoder.ContentType;
                if (this.messagePropagationFilter != null)
                {
                    using (Message message3 = encodedMessage.CreateMessage())
                    {
                        localAndRemote = ((IPeerNodeMessageHandling) this).DetermineMessagePropagation(message3, PeerMessageOrigination.Local);
                    }
                }
                if (((localAndRemote & PeerMessagePropagation.Remote) != PeerMessagePropagation.None) && (maxValue == 0L))
                {
                    localAndRemote &= ~PeerMessagePropagation.Remote;
                }
                IAsyncResult result2 = null;
                if ((localAndRemote & PeerMessagePropagation.Remote) != PeerMessagePropagation.None)
                {
                    result2 = flooder.BeginFloodEncodedMessage(primarySignatureValue, encodedMessage, helper.RemainingTime(), callback2, null);
                    if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Verbose, 0x4003e, System.ServiceModel.SR.GetString("TraceCodePeerChannelMessageSent"), this, message);
                    }
                }
                else
                {
                    result2 = new CompletedAsyncResult(callback2, null);
                }
                if ((localAndRemote & PeerMessagePropagation.Local) != PeerMessagePropagation.None)
                {
                    using (Message message4 = encodedMessage.CreateMessage())
                    {
                        int i = message4.Headers.FindHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                        if (i >= 0)
                        {
                            message4.Headers.AddUnderstood(i);
                        }
                        using (MessageBuffer buffer3 = message4.CreateBufferedCopy(maxBufferSize))
                        {
                            this.DeliverMessageToClientChannels(registrant, buffer3, via, message.Headers.To, contentType, messageSize, -1, null);
                        }
                    }
                }
                result.OnLocalDispatchComplete(result);
            }
            finally
            {
                message.Close();
                if (message2 != null)
                {
                    message2.Close();
                }
                if (encodedMessage != null)
                {
                    encodedMessage.Close();
                }
            }
            return result;
        }

        public void Close(TimeSpan timeout)
        {
            this.stateManager.Close(timeout);
        }

        private void CloseCore(TimeSpan timeout, bool graceful)
        {
            PeerService service;
            PeerMaintainer maintainer;
            PeerNeighborManager neighborManager;
            PeerConnector connector;
            PeerIPHelper ipHelper;
            PeerNodeConfig config;
            PeerFlooder flooder;
            Exception exception = null;
            TimeoutHelper helper2 = new TimeoutHelper(timeout);
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40043, System.ServiceModel.SR.GetString("TraceCodePeerNodeClosing"), this.traceRecord, this, null);
            }
            lock (this.ThisLock)
            {
                this.isOpen = false;
                maintainer = this.maintainer;
                neighborManager = this.neighborManager;
                connector = this.connector;
                ipHelper = this.ipHelper;
                service = this.service;
                config = this.config;
                flooder = this.flooder;
            }
            try
            {
                if (graceful)
                {
                    this.UnregisterAddress(timeout);
                }
                else if (config != null)
                {
                    ActionItem.Schedule(new Action<object>(this.UnregisterAddress), config.UnregisterTimeout);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                if (exception == null)
                {
                    exception = exception2;
                }
            }
            try
            {
                if (connector != null)
                {
                    connector.Closing();
                }
                if (service != null)
                {
                    try
                    {
                        service.Abort();
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                        if (exception == null)
                        {
                            exception = exception3;
                        }
                    }
                }
                if (maintainer != null)
                {
                    try
                    {
                        maintainer.Close();
                    }
                    catch (Exception exception4)
                    {
                        if (Fx.IsFatal(exception4))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                        if (exception == null)
                        {
                            exception = exception4;
                        }
                    }
                }
                if (ipHelper != null)
                {
                    try
                    {
                        ipHelper.Close();
                        ipHelper.AddressChanged -= new EventHandler(this.stateManager.OnIPAddressesChanged);
                    }
                    catch (Exception exception5)
                    {
                        if (Fx.IsFatal(exception5))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                        if (exception == null)
                        {
                            exception = exception5;
                        }
                    }
                }
                if (neighborManager != null)
                {
                    neighborManager.NeighborConnected -= new EventHandler(this.OnNeighborConnected);
                    neighborManager.NeighborOpened -= new EventHandler(this.securityManager.OnNeighborOpened);
                    this.securityManager.OnNeighborAuthenticated = (EventHandler) Delegate.Remove(this.securityManager.OnNeighborAuthenticated, new EventHandler(this.OnNeighborAuthenticated));
                    neighborManager.Online -= new EventHandler(this.FireOnline);
                    neighborManager.Offline -= new EventHandler(this.FireOffline);
                    try
                    {
                        neighborManager.Shutdown(graceful, helper2.RemainingTime());
                    }
                    catch (Exception exception6)
                    {
                        if (Fx.IsFatal(exception6))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception6, TraceEventType.Information);
                        if (exception == null)
                        {
                            exception = exception6;
                        }
                    }
                    neighborManager.NeighborClosed -= new EventHandler<PeerNeighborCloseEventArgs>(this.OnNeighborClosed);
                    neighborManager.NeighborClosing -= new EventHandler<PeerNeighborCloseEventArgs>(this.OnNeighborClosing);
                    neighborManager.Close();
                }
                if (connector != null)
                {
                    try
                    {
                        connector.Close();
                    }
                    catch (Exception exception7)
                    {
                        if (Fx.IsFatal(exception7))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception7, TraceEventType.Information);
                        if (exception == null)
                        {
                            exception = exception7;
                        }
                    }
                }
                if (flooder != null)
                {
                    try
                    {
                        flooder.Close();
                    }
                    catch (Exception exception8)
                    {
                        if (Fx.IsFatal(exception8))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception8, TraceEventType.Information);
                        if (exception == null)
                        {
                            exception = exception8;
                        }
                    }
                }
            }
            catch (Exception exception9)
            {
                if (Fx.IsFatal(exception9))
                {
                    throw;
                }
                if (exception == null)
                {
                    exception = exception9;
                }
            }
            EventHandler aborted = null;
            lock (this.ThisLock)
            {
                this.neighborManager = null;
                this.connector = null;
                this.maintainer = null;
                this.flooder = null;
                this.ipHelper = null;
                this.service = null;
                this.config = null;
                this.meshId = null;
                aborted = this.Aborted;
            }
            if (!graceful && (aborted != null))
            {
                try
                {
                    aborted(this, EventArgs.Empty);
                }
                catch (Exception exception10)
                {
                    if (Fx.IsFatal(exception10))
                    {
                        throw;
                    }
                    System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception10, TraceEventType.Information);
                    if (exception == null)
                    {
                        exception = exception10;
                    }
                }
            }
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40044, System.ServiceModel.SR.GetString("TraceCodePeerNodeClosed"), this.traceRecord, this, null);
            }
            if ((exception != null) && graceful)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
        }

        private bool CompareVia(Uri via1, Uri via2)
        {
            return (Uri.Compare(via1, via2, UriComponents.Path | UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private void DeliverMessageToClientChannels(object registrant, MessageBuffer messageBuffer, Uri via, Uri peerTo, string contentType, int messageSize, int index, MessageHeader hopHeader)
        {
            Message message = null;
            try
            {
                ArrayList list = new ArrayList();
                Uri toCond = peerTo;
                if (this.isOpen)
                {
                    lock (this.ThisLock)
                    {
                        if (this.isOpen)
                        {
                            foreach (MessageFilterRegistration registration in this.messageFilters.Values)
                            {
                                bool flag = this.CompareVia(via, registration.via);
                                if (messageSize < 0)
                                {
                                    if (message == null)
                                    {
                                        message = messageBuffer.CreateMessage();
                                    }
                                    if (registrant != null)
                                    {
                                        messageSize = this.encoder.WriteMessage(message, 0x7fffffff, this.bufferManager).Count;
                                    }
                                }
                                flag = flag && (messageSize <= registration.settings.MaxReceivedMessageSize);
                                if (flag && (registration.filters != null))
                                {
                                    for (int i = 0; flag && (i < registration.filters.Length); i++)
                                    {
                                        flag = registration.filters[i].Match(via, toCond);
                                    }
                                }
                                if (flag)
                                {
                                    list.Add(registration.callback);
                                }
                            }
                        }
                    }
                }
                foreach (MessageAvailableCallback callback in list)
                {
                    try
                    {
                        Message message2 = messageBuffer.CreateMessage();
                        message2.Properties.Via = via;
                        message2.Headers.To = toCond;
                        try
                        {
                            int num2 = message2.Headers.FindHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                            if (num2 >= 0)
                            {
                                message2.Headers.AddUnderstood(num2);
                            }
                        }
                        catch (MessageHeaderException exception)
                        {
                            System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                        }
                        catch (SerializationException exception2)
                        {
                            System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                        }
                        catch (XmlException exception3)
                        {
                            System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                        }
                        if (index != -1)
                        {
                            message2.Headers.ReplaceAt(index, hopHeader);
                        }
                        callback(message2);
                    }
                    catch (ObjectDisposedException exception4)
                    {
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                    }
                    catch (CommunicationObjectAbortedException exception5)
                    {
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                    }
                    catch (CommunicationObjectFaultedException exception6)
                    {
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception6, TraceEventType.Information);
                    }
                }
            }
            finally
            {
                if (message != null)
                {
                    message.Close();
                }
            }
        }

        public static void EndClose(IAsyncResult result)
        {
            if (result == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            SimpleStateManager.EndClose(result);
        }

        public static void EndOpen(IAsyncResult result)
        {
            if (result == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            SimpleStateManager.EndOpen(result);
        }

        public static void EndSend(IAsyncResult result)
        {
            if (result == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            SendAsyncResult.End(result);
        }

        private void FireOffline(object sender, EventArgs e)
        {
            if (this.isOpen)
            {
                EventHandler offline = this.Offline;
                if (offline != null)
                {
                    offline(this, EventArgs.Empty);
                }
            }
        }

        private void FireOnline(object sender, EventArgs e)
        {
            if (this.isOpen)
            {
                EventHandler online = this.Online;
                if (online != null)
                {
                    online(this, EventArgs.Empty);
                }
            }
        }

        internal static PeerNodeImplementation Get(Uri listenUri)
        {
            PeerNodeImplementation result = null;
            if (!TryGet(listenUri, out result))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoTransportManagerForUri", new object[] { listenUri })));
            }
            return result;
        }

        public static PeerNodeImplementation Get(Uri listenUri, Registration registration)
        {
            if (listenUri == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listenUri");
            }
            if (listenUri.Scheme != "net.p2p")
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("listenUri", System.ServiceModel.SR.GetString("InvalidUriScheme", new object[] { listenUri.Scheme, "net.p2p" }));
            }
            Uri key = new UriBuilder("net.p2p", listenUri.Host).Uri;
            lock (peerNodes)
            {
                PeerNodeImplementation peerNode = null;
                PeerNodeImplementation implementation2 = null;
                if (peerNodes.TryGetValue(key, out implementation2))
                {
                    peerNode = implementation2;
                    registration.CheckIfCompatible(peerNode, listenUri);
                    peerNode.refCount++;
                    return peerNode;
                }
                peerNode = registration.CreatePeerNode();
                peerNodes[key] = peerNode;
                peerNode.refCount = 1;
                return peerNode;
            }
        }

        private void InternalClose(TimeSpan timeout, bool graceful)
        {
            this.CloseCore(timeout, graceful);
            lock (this.ThisLock)
            {
                this.messageFilters.Clear();
            }
        }

        protected void OnAbort()
        {
            this.InternalClose(TimeSpan.FromTicks(0L), false);
        }

        protected void OnClose(TimeSpan timeout)
        {
            this.InternalClose(timeout, true);
        }

        private void OnConnectionAttemptCompleted(Exception e)
        {
            this.openException = e;
            if ((this.openException == null) && System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40041, System.ServiceModel.SR.GetString("TraceCodePeerNodeOpened"), this.completeTraceRecord, this, null);
            }
            else if ((this.openException != null) && System.ServiceModel.DiagnosticUtility.ShouldTraceError)
            {
                TraceUtility.TraceEvent(TraceEventType.Error, 0x40042, System.ServiceModel.SR.GetString("TraceCodePeerNodeOpenFailed"), this.completeTraceRecord, this, e);
            }
            this.connectCompletedEvent.Set();
        }

        private void OnIPAddressChange()
        {
            string lclMeshId = null;
            PeerNodeAddress listenAddress = null;
            object registrationId = null;
            bool registered = false;
            PeerIPHelper ipHelper = this.ipHelper;
            PeerNodeConfig config = this.config;
            bool flag2 = false;
            TimeoutHelper helper2 = new TimeoutHelper(ServiceDefaults.SendTimeout);
            if ((ipHelper != null) && (this.config != null))
            {
                listenAddress = config.GetListenAddress(false);
                flag2 = ipHelper.AddressesChanged(listenAddress.IPAddresses);
                if (flag2)
                {
                    listenAddress = new PeerNodeAddress(listenAddress.EndpointAddress, ipHelper.GetLocalAddresses());
                }
            }
            lock (this.ThisLock)
            {
                if (flag2 && this.isOpen)
                {
                    lclMeshId = this.meshId;
                    registrationId = this.resolverRegistrationId;
                    registered = this.registered;
                    this.config.SetListenAddress(listenAddress);
                    this.completeTraceRecord = new PeerNodeTraceRecord(this.config.NodeId, this.meshId, listenAddress);
                }
                else
                {
                    return;
                }
            }
            try
            {
                if (listenAddress.IPAddresses.Count > 0)
                {
                    if (registered)
                    {
                        this.resolver.Update(registrationId, listenAddress, helper2.RemainingTime());
                    }
                    else
                    {
                        this.RegisterAddress(lclMeshId, listenAddress, helper2.RemainingTime());
                    }
                }
                else
                {
                    this.UnregisterAddress(helper2.RemainingTime());
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
            }
            this.PingConnections();
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4003f, System.ServiceModel.SR.GetString("TraceCodePeerNodeAddressChanged"), this.completeTraceRecord, this, null);
            }
        }

        private void OnNeighborAuthenticated(object sender, EventArgs e)
        {
            IPeerNeighbor neighbor = (IPeerNeighbor) sender;
            if (this.connector != null)
            {
                this.connector.OnNeighborAuthenticated(neighbor);
            }
            EventHandler neighborOpened = this.NeighborOpened;
            if (neighborOpened != null)
            {
                neighborOpened(this, EventArgs.Empty);
            }
        }

        private void OnNeighborClosed(object sender, PeerNeighborCloseEventArgs e)
        {
            IPeerNeighbor neighbor = (IPeerNeighbor) sender;
            PeerConnector connector = this.connector;
            PeerMaintainer maintainer = this.maintainer;
            PeerFlooder flooder = this.flooder;
            UtilityExtension.OnNeighborClosed(neighbor);
            PeerChannelAuthenticatorExtension.OnNeighborClosed(neighbor);
            if (connector != null)
            {
                connector.OnNeighborClosed(neighbor);
            }
            if (maintainer != null)
            {
                maintainer.OnNeighborClosed(neighbor);
            }
            if (flooder != null)
            {
                flooder.OnNeighborClosed(neighbor);
            }
            EventHandler<PeerNeighborCloseEventArgs> neighborClosed = this.NeighborClosed;
            if (neighborClosed != null)
            {
                neighborClosed(this, e);
            }
        }

        private void OnNeighborClosing(object sender, PeerNeighborCloseEventArgs e)
        {
            IPeerNeighbor neighbor = (IPeerNeighbor) sender;
            PeerConnector connector = this.connector;
            if (connector != null)
            {
                connector.OnNeighborClosing(neighbor, e.Reason);
            }
            EventHandler<PeerNeighborCloseEventArgs> neighborClosing = this.NeighborClosing;
            if (neighborClosing != null)
            {
                neighborClosing(this, e);
            }
        }

        private void OnNeighborConnected(object sender, EventArgs e)
        {
            IPeerNeighbor neighbor = (IPeerNeighbor) sender;
            PeerMaintainer maintainer = this.maintainer;
            PeerFlooder flooder = this.flooder;
            if (flooder != null)
            {
                flooder.OnNeighborConnected(neighbor);
            }
            if (maintainer != null)
            {
                maintainer.OnNeighborConnected(neighbor);
            }
            UtilityExtension.OnNeighborConnected(neighbor);
            EventHandler neighborConnected = this.NeighborConnected;
            if (neighborConnected != null)
            {
                neighborConnected(this, EventArgs.Empty);
            }
        }

        private void OnOpen(TimeSpan timeout, bool waitForOnline)
        {
            bool aborted = false;
            EventHandler handler = (source, args) => this.connectCompletedEvent.Set();
            EventHandler handler2 = delegate (object source, EventArgs args) {
                aborted = true;
                this.connectCompletedEvent.Set();
            };
            this.openException = null;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            try
            {
                this.NeighborConnected += handler;
                this.Aborted += handler2;
                this.OpenCore(timeout);
                if (waitForOnline && !TimeoutHelper.WaitOne(this.connectCompletedEvent, helper.RemainingTime()))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
                if (aborted)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("PeerNodeAborted")));
                }
                if (this.isOpen)
                {
                    if (this.openException != null)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.openException);
                    }
                    string lclMeshId = null;
                    PeerNodeConfig config = null;
                    lock (this.ThisLock)
                    {
                        lclMeshId = this.meshId;
                        config = this.config;
                    }
                    this.RegisterAddress(lclMeshId, config.GetListenAddress(false), helper.RemainingTime());
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.CloseCore(TimeSpan.FromTicks(0L), false);
                throw;
            }
            finally
            {
                this.NeighborConnected -= handler;
                this.Aborted -= handler2;
            }
        }

        internal void Open(TimeSpan timeout, bool waitForOnline)
        {
            this.stateManager.Open(timeout, waitForOnline);
        }

        private void OpenCore(TimeSpan timeout)
        {
            PeerMaintainer maintainer;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            lock (this.ThisLock)
            {
                if (this.ListenUri == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ListenUriNotSet", new object[] { base.GetType() })));
                }
                this.meshId = this.ListenUri.Host;
                byte[] buffer = new byte[8];
                ulong id = 0L;
                do
                {
                    CryptoHelper.FillRandomBytes(buffer);
                    for (int i = 0; i < 8; i++)
                    {
                        id |= buffer[i] << (i * 8);
                    }
                }
                while (id == 0L);
                this.traceRecord = new PeerNodeTraceRecord(id, this.meshId);
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0x40040, System.ServiceModel.SR.GetString("TraceCodePeerNodeOpening"), this.traceRecord, this, null);
                }
                this.config = new PeerNodeConfig(this.meshId, id, this.resolver, this.messagePropagationFilter, this.encoder, this.ListenUri, this.listenIPAddress, this.port, this.maxReceivedMessageSize, this.minNeighbors, this.idealNeighbors, this.maxNeighbors, this.maxReferrals, this.connectTimeout, this.maintainerInterval, this.securityManager, this.readerQuotas, this.maxBufferPoolSize, this.MaxSendQueue, this.MaxReceiveQueue);
                if (this.listenIPAddress != null)
                {
                    this.ipHelper = new PeerIPHelper(this.listenIPAddress);
                }
                else
                {
                    this.ipHelper = new PeerIPHelper();
                }
                this.bufferManager = BufferManager.CreateBufferManager(0x40L * this.config.MaxReceivedMessageSize, (int) this.config.MaxReceivedMessageSize);
                this.neighborManager = new PeerNeighborManager(this.ipHelper, this.config, this);
                this.flooder = PeerFlooder.CreateFlooder(this.config, this.neighborManager, this);
                this.maintainer = new PeerMaintainer(this.config, this.neighborManager, this.flooder);
                this.connector = new PeerConnector(this.config, this.neighborManager, this.maintainer);
                Dictionary<System.Type, object> serviceHandlers = this.serviceHandlers;
                if (serviceHandlers == null)
                {
                    serviceHandlers = new Dictionary<System.Type, object>();
                    serviceHandlers.Add(typeof(IPeerConnectorContract), this.connector);
                    serviceHandlers.Add(typeof(IPeerFlooderContract<Message, UtilityInfo>), this.flooder);
                }
                this.service = new PeerService(this.config, new PeerService.ChannelCallback(this.neighborManager.ProcessIncomingChannel), new PeerService.GetNeighborCallback(this.neighborManager.GetNeighborFromProxy), serviceHandlers, this);
                this.securityManager.MeshId = this.meshId;
                this.service.Open(helper.RemainingTime());
                this.neighborManager.NeighborClosed += new EventHandler<PeerNeighborCloseEventArgs>(this.OnNeighborClosed);
                this.neighborManager.NeighborClosing += new EventHandler<PeerNeighborCloseEventArgs>(this.OnNeighborClosing);
                this.neighborManager.NeighborConnected += new EventHandler(this.OnNeighborConnected);
                this.neighborManager.NeighborOpened += new EventHandler(this.SecurityManager.OnNeighborOpened);
                this.securityManager.OnNeighborAuthenticated = (EventHandler) Delegate.Combine(this.securityManager.OnNeighborAuthenticated, new EventHandler(this.OnNeighborAuthenticated));
                this.neighborManager.Online += new EventHandler(this.FireOnline);
                this.neighborManager.Offline += new EventHandler(this.FireOffline);
                this.ipHelper.AddressChanged += new EventHandler(this.stateManager.OnIPAddressesChanged);
                this.ipHelper.Open();
                PeerNodeAddress address = new PeerNodeAddress(this.service.GetListenAddress(), this.ipHelper.GetLocalAddresses());
                this.config.SetListenAddress(address);
                this.neighborManager.Open(this.service.Binding, this.service);
                this.connector.Open();
                this.maintainer.Open();
                this.flooder.Open();
                this.isOpen = true;
                this.completeTraceRecord = new PeerNodeTraceRecord(id, this.meshId, address);
                maintainer = this.maintainer;
                this.openException = null;
            }
            if (this.isOpen)
            {
                maintainer.ScheduleConnect(new PeerMaintainerBase<ConnectAlgorithms>.ConnectCallback(this.OnConnectionAttemptCompleted));
            }
        }

        public void PingConnections()
        {
            PeerMaintainer maintainer = null;
            lock (this.ThisLock)
            {
                maintainer = this.maintainer;
            }
            if (maintainer != null)
            {
                maintainer.PingConnections();
            }
        }

        public Guid ProcessOutgoingMessage(Message message, Uri via)
        {
            Guid guid = Guid.NewGuid();
            UniqueId messageId = new UniqueId(guid);
            if (-1 != message.Headers.FindHeader("MessageID", "http://schemas.microsoft.com/net/2006/05/peer"))
            {
                PeerExceptionHelper.ThrowInvalidOperation_ConflictingHeader("MessageID");
            }
            if (-1 != message.Headers.FindHeader("PeerTo", "http://schemas.microsoft.com/net/2006/05/peer"))
            {
                PeerExceptionHelper.ThrowInvalidOperation_ConflictingHeader("PeerTo");
            }
            if (-1 != message.Headers.FindHeader("PeerVia", "http://schemas.microsoft.com/net/2006/05/peer"))
            {
                PeerExceptionHelper.ThrowInvalidOperation_ConflictingHeader("PeerVia");
            }
            if (-1 != message.Headers.FindHeader("FloodMessage", "http://schemas.microsoft.com/net/2006/05/peer", new string[] { "PeerFlooder" }))
            {
                PeerExceptionHelper.ThrowInvalidOperation_ConflictingHeader("FloodMessage");
            }
            message.Headers.Add(PeerDictionaryHeader.CreateMessageIdHeader(messageId));
            message.Properties.Via = via;
            message.Headers.Add(MessageHeader.CreateHeader("PeerTo", "http://schemas.microsoft.com/net/2006/05/peer", message.Headers.To));
            message.Headers.Add(PeerDictionaryHeader.CreateViaHeader(via));
            message.Headers.Add(PeerDictionaryHeader.CreateFloodRole());
            return guid;
        }

        public void RefreshConnection()
        {
            PeerMaintainer maintainer = null;
            lock (this.ThisLock)
            {
                this.ThrowIfNotOpen();
                maintainer = this.maintainer;
            }
            if (maintainer != null)
            {
                maintainer.RefreshConnection();
            }
        }

        private void RegisterAddress(string lclMeshId, PeerNodeAddress nodeAddress, TimeSpan timeout)
        {
            if (nodeAddress.IPAddresses.Count > 0)
            {
                object obj2 = null;
                try
                {
                    obj2 = this.resolver.Register(lclMeshId, nodeAddress, timeout);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("ResolverException"), exception));
                }
                lock (this.ThisLock)
                {
                    if (this.registered)
                    {
                        throw Fx.AssertAndThrow("registered expected to be false");
                    }
                    this.registered = true;
                    this.resolverRegistrationId = obj2;
                }
            }
        }

        internal void RegisterMessageFilter(object registrant, Uri via, PeerMessageFilter[] filters, ITransportFactorySettings settings, MessageAvailableCallback callback, SecurityProtocol securityProtocol)
        {
            MessageFilterRegistration registration = new MessageFilterRegistration {
                registrant = registrant,
                via = via,
                filters = filters,
                settings = settings,
                callback = callback,
                securityProtocol = securityProtocol
            };
            lock (this.ThisLock)
            {
                this.messageFilters.Add(registrant, registration);
                RefCountedSecurityProtocol protocol = null;
                if (!this.uri2SecurityProtocol.TryGetValue(via, out protocol))
                {
                    protocol = new RefCountedSecurityProtocol(securityProtocol);
                    this.uri2SecurityProtocol.Add(via, protocol);
                }
                else
                {
                    protocol.AddRef();
                }
            }
        }

        internal void Release()
        {
            lock (peerNodes)
            {
                if (peerNodes.ContainsValue(this) && (--this.refCount == 0))
                {
                    peerNodes.Remove(this.listenUri);
                }
            }
        }

        public void SecureOutgoingMessage(ref Message message, Uri via, TimeSpan timeout, SecurityProtocol securityProtocol)
        {
            if (securityProtocol != null)
            {
                securityProtocol.SecureOutgoingMessage(ref message, timeout);
            }
        }

        public void SetServiceHandlers(Dictionary<System.Type, object> services)
        {
            lock (this.ThisLock)
            {
                this.serviceHandlers = services;
            }
        }

        PeerMessagePropagation IPeerNodeMessageHandling.DetermineMessagePropagation(Message message, PeerMessageOrigination origination)
        {
            SendOrPostCallback d = null;
            PeerMessagePropagation propagateFlags = PeerMessagePropagation.LocalAndRemote;
            PeerMessagePropagationFilter filter = this.MessagePropagationFilter;
            if (filter != null)
            {
                try
                {
                    SynchronizationContext messagePropagationFilterContext = this.messagePropagationFilterContext;
                    if (messagePropagationFilterContext != null)
                    {
                        if (d == null)
                        {
                            d = delegate (object state) {
                                propagateFlags = filter.ShouldMessagePropagate(message, origination);
                            };
                        }
                        messagePropagationFilterContext.Send(d, null);
                    }
                    else
                    {
                        propagateFlags = filter.ShouldMessagePropagate(message, origination);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(System.ServiceModel.SR.GetString("MessagePropagationException"), exception);
                }
            }
            if (!this.isOpen)
            {
                propagateFlags = PeerMessagePropagation.None;
            }
            return propagateFlags;
        }

        void IPeerNodeMessageHandling.HandleIncomingMessage(MessageBuffer messageBuffer, PeerMessagePropagation propagateFlags, int index, MessageHeader hopHeader, Uri via, Uri to)
        {
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40045, System.ServiceModel.SR.GetString("TraceCodePeerFloodedMessageReceived"), this.traceRecord, this, null);
            }
            if (via == null)
            {
                using (Message message = messageBuffer.CreateMessage())
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerMessageMustHaveVia", new object[] { message.Headers.Action })));
                }
            }
            if ((propagateFlags & PeerMessagePropagation.Local) != PeerMessagePropagation.None)
            {
                this.DeliverMessageToClientChannels(null, messageBuffer, via, to, messageBuffer.MessageContentType, (int) this.maxReceivedMessageSize, index, hopHeader);
                messageBuffer = null;
            }
            else if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
            {
                using (Message message2 = messageBuffer.CreateMessage())
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40046, System.ServiceModel.SR.GetString("TraceCodePeerFloodedMessageNotPropagated"), this.traceRecord, this, null, message2);
                }
            }
        }

        bool IPeerNodeMessageHandling.IsKnownVia(Uri via)
        {
            lock (this.ThisLock)
            {
                return this.uri2SecurityProtocol.ContainsKey(via);
            }
        }

        bool IPeerNodeMessageHandling.IsNotSeenBefore(Message message, out byte[] id, out int cacheMiss)
        {
            PeerFlooder flooder = this.flooder;
            id = DefaultId;
            cacheMiss = -1;
            return ((flooder != null) && flooder.IsNotSeenBefore(message, out id, out cacheMiss));
        }

        bool IPeerNodeMessageHandling.ValidateIncomingMessage(ref Message message, Uri via)
        {
            SecurityProtocol protocol = null;
            if (via == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerMessageMustHaveVia", new object[] { message.Headers.Action })));
            }
            if (this.TryGetSecurityProtocol(via, out protocol))
            {
                protocol.VerifyIncomingMessage(ref message, ServiceDefaults.SendTimeout, null);
                return true;
            }
            return false;
        }

        private void ThrowIfNotOpen()
        {
            if (!this.isOpen)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransportManagerNotOpen")));
            }
        }

        private void ThrowIfOpen()
        {
            if (this.isOpen)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransportManagerOpen")));
            }
        }

        public override string ToString()
        {
            lock (this.ThisLock)
            {
                if (this.isOpen)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", new object[] { this.MeshId, this.NodeId });
                }
                return base.GetType().ToString();
            }
        }

        public static bool TryGet(string meshId, out PeerNodeImplementation result)
        {
            UriBuilder builder = new UriBuilder {
                Host = meshId,
                Scheme = "net.p2p"
            };
            return TryGet(builder.Uri, out result);
        }

        protected internal static bool TryGet(Uri listenUri, out PeerNodeImplementation result)
        {
            if (listenUri == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listenUri");
            }
            if (listenUri.Scheme != "net.p2p")
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("listenUri", System.ServiceModel.SR.GetString("InvalidUriScheme", new object[] { listenUri.Scheme, "net.p2p" }));
            }
            result = null;
            bool flag = false;
            Uri key = new UriBuilder("net.p2p", listenUri.Host).Uri;
            lock (peerNodes)
            {
                if (peerNodes.ContainsKey(key))
                {
                    result = peerNodes[key];
                    flag = true;
                }
            }
            return flag;
        }

        internal bool TryGetSecurityProtocol(Uri via, out SecurityProtocol protocol)
        {
            lock (this.ThisLock)
            {
                RefCountedSecurityProtocol protocol2 = null;
                bool flag = false;
                protocol = null;
                if (this.uri2SecurityProtocol.TryGetValue(via, out protocol2))
                {
                    protocol = protocol2.Protocol;
                    flag = true;
                }
                return flag;
            }
        }

        private void UnregisterAddress(object timeout)
        {
            try
            {
                this.UnregisterAddress((TimeSpan) timeout);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
            }
        }

        private void UnregisterAddress(TimeSpan timeout)
        {
            bool flag = false;
            object registrationId = null;
            lock (this.ThisLock)
            {
                if (this.registered)
                {
                    flag = true;
                    registrationId = this.resolverRegistrationId;
                    this.registered = false;
                }
                this.resolverRegistrationId = null;
            }
            if (flag)
            {
                try
                {
                    this.resolver.Unregister(registrationId, timeout);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("ResolverException"), exception));
                }
            }
        }

        internal void UnregisterMessageFilter(object registrant, Uri via)
        {
            lock (this.ThisLock)
            {
                this.messageFilters.Remove(registrant);
                RefCountedSecurityProtocol protocol = null;
                if (this.uri2SecurityProtocol.TryGetValue(via, out protocol) && (protocol.Release() == 0))
                {
                    this.uri2SecurityProtocol.Remove(via);
                }
            }
        }

        internal static void ValidateVia(Uri uri)
        {
            int byteCount = Encoding.UTF8.GetByteCount(uri.OriginalString);
            if (byteCount > 0x1000)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataException(System.ServiceModel.SR.GetString("PeerChannelViaTooLong", new object[] { uri, byteCount, 0x1000 })));
            }
        }

        public PeerNodeConfig Config
        {
            get
            {
                return this.config;
            }
            private set
            {
                this.config = value;
            }
        }

        public MessageEncodingBindingElement EncodingBindingElement
        {
            get
            {
                return this.EncodingElement;
            }
        }

        public bool IsOnline
        {
            get
            {
                lock (this.ThisLock)
                {
                    return (this.isOpen && this.neighborManager.IsOnline);
                }
            }
        }

        internal bool IsOpen
        {
            get
            {
                return this.isOpen;
            }
        }

        public int ListenerPort
        {
            get
            {
                this.ThrowIfNotOpen();
                return this.config.ListenerPort;
            }
        }

        public IPAddress ListenIPAddress
        {
            get
            {
                return this.listenIPAddress;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfOpen();
                    this.listenIPAddress = value;
                }
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
            set
            {
                if (value == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value.Scheme != "net.p2p")
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", System.ServiceModel.SR.GetString("InvalidUriScheme", new object[] { value.Scheme, "net.p2p" }));
                }
                lock (this.ThisLock)
                {
                    this.ThrowIfOpen();
                    this.listenUri = value;
                }
            }
        }

        public long MaxBufferPoolSize
        {
            get
            {
                return this.maxBufferPoolSize;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfOpen();
                    this.maxBufferPoolSize = value;
                }
            }
        }

        public long MaxReceivedMessageSize
        {
            get
            {
                return this.maxReceivedMessageSize;
            }
            set
            {
                if (value < 0x4000L)
                {
                    throw Fx.AssertAndThrow("invalid MaxReceivedMessageSize");
                }
                lock (this.ThisLock)
                {
                    this.ThrowIfOpen();
                    this.maxReceivedMessageSize = value;
                }
            }
        }

        public string MeshId
        {
            get
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfNotOpen();
                    return this.meshId;
                }
            }
        }

        public PeerMessagePropagationFilter MessagePropagationFilter
        {
            get
            {
                return this.messagePropagationFilter;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.messagePropagationFilter = value;
                    this.messagePropagationFilterContext = ThreadBehavior.GetCurrentSynchronizationContext();
                }
            }
        }

        public PeerNeighborManager NeighborManager
        {
            get
            {
                return this.neighborManager;
            }
        }

        public ulong NodeId
        {
            get
            {
                this.ThrowIfNotOpen();
                return this.config.NodeId;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfOpen();
                    this.port = value;
                }
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
        }

        public PeerResolver Resolver
        {
            get
            {
                return this.resolver;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfOpen();
                    this.resolver = value;
                }
            }
        }

        public PeerSecurityManager SecurityManager
        {
            get
            {
                return this.securityManager;
            }
            set
            {
                this.securityManager = value;
            }
        }

        internal PeerService Service
        {
            get
            {
                return this.service;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfNotOpen();
                    this.service = value;
                }
            }
        }

        bool IPeerNodeMessageHandling.HasMessagePropagation
        {
            get
            {
                return (this.messagePropagationFilter != null);
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        internal class ChannelRegistration
        {
            public System.Type channelType;
            public object registrant;
            public SecurityProtocol securityProtocol;
            public ITransportFactorySettings settings;
            public Uri via;
        }

        public delegate void MessageAvailableCallback(Message message);

        private class MessageFilterRegistration : PeerNodeImplementation.ChannelRegistration
        {
            public PeerNodeImplementation.MessageAvailableCallback callback;
            public PeerMessageFilter[] filters;
        }

        private class RefCountedSecurityProtocol
        {
            public SecurityProtocol Protocol;
            private int refCount;

            public RefCountedSecurityProtocol(SecurityProtocol securityProtocol)
            {
                this.Protocol = securityProtocol;
                this.refCount = 1;
            }

            public int AddRef()
            {
                return ++this.refCount;
            }

            public int Release()
            {
                return --this.refCount;
            }
        }

        internal class Registration
        {
            private IPAddress listenIPAddress;
            private Uri listenUri;
            private long maxBufferPoolSize;
            private long maxReceivedMessageSize;
            private int port;
            private XmlDictionaryReaderQuotas readerQuotas;
            private PeerResolver resolver;
            private PeerSecurityManager securityManager;

            public Registration(Uri listenUri, IPeerFactory factory)
            {
                if (factory.Resolver == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerResolverRequired")));
                }
                if (factory.ListenIPAddress != null)
                {
                    this.listenIPAddress = factory.ListenIPAddress;
                }
                this.listenUri = new UriBuilder("net.p2p", listenUri.Host).Uri;
                this.port = factory.Port;
                this.maxReceivedMessageSize = factory.MaxReceivedMessageSize;
                this.resolver = factory.Resolver;
                this.securityManager = factory.SecurityManager;
                this.readerQuotas = new XmlDictionaryReaderQuotas();
                factory.ReaderQuotas.CopyTo(this.readerQuotas);
                this.maxBufferPoolSize = factory.MaxBufferPoolSize;
            }

            public void CheckIfCompatible(PeerNodeImplementation peerNode, Uri via)
            {
                string result = null;
                if (this.listenUri != peerNode.ListenUri)
                {
                    result = PeerBindingPropertyNames.ListenUri;
                }
                else if (this.port != peerNode.Port)
                {
                    result = PeerBindingPropertyNames.Port;
                }
                else if (this.maxReceivedMessageSize != peerNode.MaxReceivedMessageSize)
                {
                    result = PeerBindingPropertyNames.MaxReceivedMessageSize;
                }
                else if (this.maxBufferPoolSize != peerNode.MaxBufferPoolSize)
                {
                    result = PeerBindingPropertyNames.MaxBufferPoolSize;
                }
                else if (!this.HasMismatchedReaderQuotas(peerNode.ReaderQuotas, this.readerQuotas, out result))
                {
                    if (this.resolver.GetType() != peerNode.Resolver.GetType())
                    {
                        result = PeerBindingPropertyNames.Resolver;
                    }
                    else if (!this.resolver.Equals(peerNode.Resolver))
                    {
                        result = PeerBindingPropertyNames.ResolverSettings;
                    }
                    else if (this.listenIPAddress != peerNode.ListenIPAddress)
                    {
                        if (((this.listenIPAddress == null) || (peerNode.ListenIPAddress == null)) || !this.listenIPAddress.Equals(peerNode.ListenIPAddress))
                        {
                            result = PeerBindingPropertyNames.ListenIPAddress;
                        }
                    }
                    else if ((this.securityManager == null) && (peerNode.SecurityManager != null))
                    {
                        result = PeerBindingPropertyNames.Security;
                    }
                }
                if (result != null)
                {
                    PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(result);
                }
                this.securityManager.CheckIfCompatibleNodeSettings(peerNode.SecurityManager);
            }

            public PeerNodeImplementation CreatePeerNode()
            {
                PeerNodeImplementation implementation = new PeerNodeImplementation {
                    ListenIPAddress = this.listenIPAddress,
                    ListenUri = this.listenUri,
                    MaxReceivedMessageSize = this.maxReceivedMessageSize,
                    Port = this.port,
                    Resolver = this.resolver,
                    SecurityManager = this.securityManager
                };
                this.readerQuotas.CopyTo(implementation.readerQuotas);
                implementation.MaxBufferPoolSize = this.maxBufferPoolSize;
                return implementation;
            }

            private bool HasMismatchedReaderQuotas(XmlDictionaryReaderQuotas existingOne, XmlDictionaryReaderQuotas newOne, out string result)
            {
                result = null;
                if (existingOne.MaxArrayLength != newOne.MaxArrayLength)
                {
                    result = PeerBindingPropertyNames.ReaderQuotasDotArrayLength;
                }
                else if (existingOne.MaxStringContentLength != newOne.MaxStringContentLength)
                {
                    result = PeerBindingPropertyNames.ReaderQuotasDotStringLength;
                }
                else if (existingOne.MaxDepth != newOne.MaxDepth)
                {
                    result = PeerBindingPropertyNames.ReaderQuotasDotMaxDepth;
                }
                else if (existingOne.MaxNameTableCharCount != newOne.MaxNameTableCharCount)
                {
                    result = PeerBindingPropertyNames.ReaderQuotasDotMaxCharCount;
                }
                else if (existingOne.MaxBytesPerRead != newOne.MaxBytesPerRead)
                {
                    result = PeerBindingPropertyNames.ReaderQuotasDotMaxBytesPerRead;
                }
                return (result != null);
            }
        }

        private class SendAsyncResult : AsyncResult
        {
            private bool floodComplete;
            private Exception floodException;
            private bool localDispatchComplete;
            private object thisLock;

            public SendAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
                this.thisLock = new object();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PeerNodeImplementation.SendAsyncResult>(result);
            }

            public void OnFloodComplete(IAsyncResult result)
            {
                if (!this.floodComplete && !base.IsCompleted)
                {
                    bool flag = false;
                    lock (this.ThisLock)
                    {
                        if (this.localDispatchComplete)
                        {
                            flag = true;
                        }
                        this.floodComplete = true;
                    }
                    try
                    {
                        PeerFlooderBase<Message, UtilityInfo>.EndFloodEncodedMessage(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        this.floodException = exception;
                    }
                    if (flag)
                    {
                        base.Complete(result.CompletedSynchronously, this.floodException);
                    }
                }
            }

            public void OnLocalDispatchComplete(IAsyncResult result)
            {
                PeerNodeImplementation.SendAsyncResult result1 = (PeerNodeImplementation.SendAsyncResult) result;
                if (!this.localDispatchComplete && !base.IsCompleted)
                {
                    bool flag = false;
                    lock (this.ThisLock)
                    {
                        if (this.floodComplete)
                        {
                            flag = true;
                        }
                        this.localDispatchComplete = true;
                    }
                    if (flag)
                    {
                        base.Complete(true, this.floodException);
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

        private class SimpleStateManager
        {
            private State currentState;
            private int openCount;
            private PeerNodeImplementation peerNode;
            private Queue<IOperation> queue = new Queue<IOperation>();
            private bool queueRunning;
            private object thisLock = new object();

            public SimpleStateManager(PeerNodeImplementation peerNode)
            {
                this.peerNode = peerNode;
            }

            public void Abort()
            {
                lock (this.ThisLock)
                {
                    bool flag = false;
                    if ((this.openCount <= 1) && (this.currentState != State.NotOpened))
                    {
                        flag = true;
                    }
                    if (this.openCount > 0)
                    {
                        this.openCount--;
                    }
                    if (flag)
                    {
                        try
                        {
                            this.peerNode.OnAbort();
                        }
                        finally
                        {
                            this.currentState = State.NotOpened;
                        }
                    }
                }
            }

            public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                CloseOperation item = null;
                lock (this.ThisLock)
                {
                    if (this.openCount > 0)
                    {
                        this.openCount--;
                    }
                    if (this.openCount > 0)
                    {
                        return new CompletedAsyncResult(callback, state);
                    }
                    item = new CloseOperation(this, this.peerNode, timeout, callback, state);
                    this.queue.Enqueue(item);
                    this.RunQueue();
                }
                return item;
            }

            public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state, bool waitForOnline)
            {
                bool flag = false;
                OpenOperation item = null;
                lock (this.ThisLock)
                {
                    this.openCount++;
                    if ((this.openCount > 1) && (this.currentState == State.Opened))
                    {
                        flag = true;
                    }
                    else
                    {
                        item = new OpenOperation(this, this.peerNode, timeout, callback, state, waitForOnline);
                        this.queue.Enqueue(item);
                        this.RunQueue();
                    }
                }
                if (flag)
                {
                    return new CompletedAsyncResult(callback, state);
                }
                return item;
            }

            public void Close(TimeSpan timeout)
            {
                EndClose(this.BeginClose(timeout, null, null));
            }

            public static void EndClose(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    OperationBase.End(result);
                }
            }

            public static void EndOpen(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    OperationBase.End(result);
                }
            }

            public void OnIPAddressesChanged(object sender, EventArgs e)
            {
                IPAddressChangeOperation item = null;
                lock (this.ThisLock)
                {
                    item = new IPAddressChangeOperation(this.peerNode);
                    this.queue.Enqueue(item);
                    this.RunQueue();
                }
            }

            public void Open(TimeSpan timeout, bool waitForOnline)
            {
                EndOpen(this.BeginOpen(timeout, null, null, waitForOnline));
            }

            private void RunQueue()
            {
                if (!this.queueRunning)
                {
                    this.queueRunning = true;
                    ActionItem.Schedule(new Action<object>(this.RunQueueCallback), null);
                }
            }

            private void RunQueueCallback(object state)
            {
                IOperation operation;
                lock (this.ThisLock)
                {
                    operation = this.queue.Dequeue();
                }
                try
                {
                    operation.Run();
                }
                finally
                {
                    lock (this.ThisLock)
                    {
                        if (this.queue.Count > 0)
                        {
                            try
                            {
                                ActionItem.Schedule(new Action<object>(this.RunQueueCallback), null);
                            }
                            catch (Exception exception)
                            {
                                if (Fx.IsFatal(exception))
                                {
                                    throw;
                                }
                                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                            }
                        }
                        else
                        {
                            this.queueRunning = false;
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

            private class CloseOperation : PeerNodeImplementation.SimpleStateManager.OperationBase
            {
                private PeerNodeImplementation peerNode;

                public CloseOperation(PeerNodeImplementation.SimpleStateManager stateManager, PeerNodeImplementation peerNode, TimeSpan timeout, AsyncCallback callback, object state) : base(stateManager, timeout, callback, state)
                {
                    this.peerNode = peerNode;
                }

                protected override void Run()
                {
                    Exception exception = null;
                    try
                    {
                        lock (base.ThisLock)
                        {
                            if (base.stateManager.openCount > 0)
                            {
                                base.invokeOperation = false;
                            }
                            else if (base.stateManager.currentState == PeerNodeImplementation.SimpleStateManager.State.NotOpened)
                            {
                                base.invokeOperation = false;
                            }
                            else
                            {
                                if (this.timeoutHelper.RemainingTime() <= TimeSpan.Zero)
                                {
                                    base.invokeOperation = false;
                                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                                }
                                if ((base.stateManager.currentState == PeerNodeImplementation.SimpleStateManager.State.Opening) || (base.stateManager.currentState == PeerNodeImplementation.SimpleStateManager.State.Closing))
                                {
                                    throw Fx.AssertAndThrow("Open and close are serialized by queue We should not be either in Closing or Opening state at this point");
                                }
                                if (base.stateManager.currentState != PeerNodeImplementation.SimpleStateManager.State.NotOpened)
                                {
                                    base.stateManager.currentState = PeerNodeImplementation.SimpleStateManager.State.Closing;
                                    base.invokeOperation = true;
                                }
                            }
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        exception = exception2;
                    }
                    if (base.invokeOperation)
                    {
                        try
                        {
                            this.peerNode.OnClose(this.timeoutHelper.RemainingTime());
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                            System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                            exception = exception3;
                        }
                        lock (base.ThisLock)
                        {
                            base.stateManager.currentState = PeerNodeImplementation.SimpleStateManager.State.NotOpened;
                        }
                    }
                    base.Complete(exception);
                }
            }

            private interface IOperation
            {
                void Run();
            }

            private class IPAddressChangeOperation : PeerNodeImplementation.SimpleStateManager.IOperation
            {
                private PeerNodeImplementation peerNode;

                public IPAddressChangeOperation(PeerNodeImplementation peerNode)
                {
                    this.peerNode = peerNode;
                }

                void PeerNodeImplementation.SimpleStateManager.IOperation.Run()
                {
                    this.peerNode.OnIPAddressChange();
                }
            }

            private class OpenOperation : PeerNodeImplementation.SimpleStateManager.OperationBase
            {
                private PeerNodeImplementation peerNode;
                private bool waitForOnline;

                public OpenOperation(PeerNodeImplementation.SimpleStateManager stateManager, PeerNodeImplementation peerNode, TimeSpan timeout, AsyncCallback callback, object state, bool waitForOnline) : base(stateManager, timeout, callback, state)
                {
                    this.peerNode = peerNode;
                    this.waitForOnline = waitForOnline;
                }

                protected override void Run()
                {
                    Exception exception = null;
                    try
                    {
                        lock (base.ThisLock)
                        {
                            if (base.stateManager.openCount < 1)
                            {
                                base.invokeOperation = false;
                            }
                            else if (base.stateManager.currentState == PeerNodeImplementation.SimpleStateManager.State.Opened)
                            {
                                base.invokeOperation = false;
                            }
                            else
                            {
                                if (this.timeoutHelper.RemainingTime() <= TimeSpan.Zero)
                                {
                                    base.invokeOperation = false;
                                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                                }
                                if ((base.stateManager.currentState == PeerNodeImplementation.SimpleStateManager.State.Opening) || (base.stateManager.currentState == PeerNodeImplementation.SimpleStateManager.State.Closing))
                                {
                                    throw Fx.AssertAndThrow("Open and close are serialized by queue We should not be either in Closing or Opening state at this point");
                                }
                                if (base.stateManager.currentState != PeerNodeImplementation.SimpleStateManager.State.Opened)
                                {
                                    base.stateManager.currentState = PeerNodeImplementation.SimpleStateManager.State.Opening;
                                    base.invokeOperation = true;
                                }
                            }
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        exception = exception2;
                    }
                    if (base.invokeOperation)
                    {
                        try
                        {
                            this.peerNode.OnOpen(this.timeoutHelper.RemainingTime(), this.waitForOnline);
                            lock (base.ThisLock)
                            {
                                base.stateManager.currentState = PeerNodeImplementation.SimpleStateManager.State.Opened;
                            }
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                            lock (base.ThisLock)
                            {
                                base.stateManager.currentState = PeerNodeImplementation.SimpleStateManager.State.NotOpened;
                                base.stateManager.openCount--;
                            }
                            exception = exception3;
                            System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                        }
                    }
                    base.Complete(exception);
                }
            }

            private abstract class OperationBase : AsyncResult, PeerNodeImplementation.SimpleStateManager.IOperation
            {
                private AsyncCallback callback;
                private bool completed;
                protected bool invokeOperation;
                protected PeerNodeImplementation.SimpleStateManager stateManager;
                protected TimeoutHelper timeoutHelper;

                public OperationBase(PeerNodeImplementation.SimpleStateManager stateManager, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.stateManager = stateManager;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.callback = callback;
                    this.invokeOperation = false;
                    this.completed = false;
                }

                private void AsyncComplete(object o)
                {
                    try
                    {
                        base.Complete(false, (Exception) o);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(System.ServiceModel.SR.GetString("AsyncCallbackException"), exception);
                    }
                }

                protected void Complete(Exception exception)
                {
                    if (!this.completed)
                    {
                        lock (this.ThisLock)
                        {
                            if (this.completed)
                            {
                                return;
                            }
                            this.completed = true;
                        }
                        try
                        {
                            if (this.callback != null)
                            {
                                ActionItem.Schedule(new Action<object>(this.AsyncComplete), exception);
                            }
                            else
                            {
                                this.AsyncComplete(exception);
                            }
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(System.ServiceModel.SR.GetString("MessagePropagationException"), exception2);
                        }
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<PeerNodeImplementation.SimpleStateManager.OperationBase>(result);
                }

                protected abstract void Run();
                void PeerNodeImplementation.SimpleStateManager.IOperation.Run()
                {
                    this.Run();
                }

                protected object ThisLock
                {
                    get
                    {
                        return this.stateManager.thisLock;
                    }
                }
            }

            internal enum State
            {
                NotOpened,
                Opening,
                Opened,
                Closing
            }
        }
    }
}

