namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [ComVisible(true)]
    public sealed class ChannelServices
    {
        private unsafe static Perf_Contexts* perf_Contexts = GetPrivateContextsPerfCounters();
        private static object s_channelLock = new object();
        private static object[] s_currentChannelData = null;
        private static RegisteredChannelList s_registeredChannels = new RegisteredChannelList();
        private static bool unloadHandlerRegistered = false;
        private static IMessageSink xCtxChannel;

        private ChannelServices()
        {
        }

        [SecurityCritical]
        public static IMessageCtrl AsyncDispatchMessage(IMessage msg, IMessageSink replySink)
        {
            IMessageCtrl ctrl = null;
            try
            {
                if (msg == null)
                {
                    throw new ArgumentNullException("msg");
                }
                IncrementRemoteCalls();
                if (!(msg is TransitionCall))
                {
                    CheckDisconnectedOrCreateWellKnownObject(msg);
                }
                ctrl = GetCrossContextChannelSink().AsyncProcessMessage(msg, replySink);
            }
            catch (Exception exception)
            {
                if (replySink == null)
                {
                    return ctrl;
                }
                try
                {
                    IMethodCallMessage message = (IMethodCallMessage) msg;
                    ReturnMessage message2 = new ReturnMessage(exception, (IMethodCallMessage) msg);
                    if (msg != null)
                    {
                        message2.SetLogicalCallContext(message.LogicalCallContext);
                    }
                    replySink.SyncProcessMessage(message2);
                }
                catch (Exception)
                {
                }
            }
            return ctrl;
        }

        [SecurityCritical]
        internal static ServerIdentity CheckDisconnectedOrCreateWellKnownObject(IMessage msg)
        {
            ServerIdentity serverIdentity = InternalSink.GetServerIdentity(msg);
            if ((serverIdentity == null) || serverIdentity.IsRemoteDisconnected())
            {
                string uri = InternalSink.GetURI(msg);
                if (uri != null)
                {
                    ServerIdentity identity2 = RemotingConfigHandler.CreateWellKnownObject(uri);
                    if (identity2 != null)
                    {
                        serverIdentity = identity2;
                    }
                }
            }
            if ((serverIdentity != null) && !serverIdentity.IsRemoteDisconnected())
            {
                return serverIdentity;
            }
            string uRI = InternalSink.GetURI(msg);
            throw new RemotingException(Environment.GetResourceString("Remoting_Disconnected", new object[] { uRI }));
        }

        [SecurityCritical]
        private static object[] CollectChannelDataFromChannels()
        {
            RemotingServices.RegisterWellKnownChannels();
            RegisteredChannelList list = s_registeredChannels;
            int count = list.Count;
            int receiverCount = list.ReceiverCount;
            object[] objArray = new object[receiverCount];
            int num3 = 0;
            int index = 0;
            int num5 = 0;
            while (index < count)
            {
                IChannel channel = list.GetChannel(index);
                if (channel == null)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_ChannelNotRegistered", new object[] { "" }));
                }
                if (list.IsReceiver(index))
                {
                    object channelData = ((IChannelReceiver) channel).ChannelData;
                    objArray[num5] = channelData;
                    if (channelData != null)
                    {
                        num3++;
                    }
                    num5++;
                }
                index++;
            }
            if (num3 == receiverCount)
            {
                return objArray;
            }
            object[] objArray2 = new object[num3];
            int num6 = 0;
            for (int i = 0; i < receiverCount; i++)
            {
                object obj3 = objArray[i];
                if (obj3 != null)
                {
                    objArray2[num6++] = obj3;
                }
            }
            return objArray2;
        }

        [SecurityCritical]
        internal static IMessageSink CreateMessageSink(object data)
        {
            string str;
            return CreateMessageSink(null, data, out str);
        }

        [SecurityCritical]
        internal static IMessageSink CreateMessageSink(string url, object data, out string objectURI)
        {
            IMessageSink sink = null;
            objectURI = null;
            RegisteredChannelList list = s_registeredChannels;
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (list.IsSender(i))
                {
                    sink = ((IChannelSender) list.GetChannel(i)).CreateMessageSink(url, data, out objectURI);
                    if (sink != null)
                    {
                        break;
                    }
                }
            }
            if (objectURI == null)
            {
                objectURI = url;
            }
            return sink;
        }

        [SecurityCritical]
        public static IServerChannelSink CreateServerChannelSinkChain(IServerChannelSinkProvider provider, IChannelReceiver channel)
        {
            if (provider == null)
            {
                return new DispatchChannelSink();
            }
            IServerChannelSinkProvider next = provider;
            while (next.Next != null)
            {
                next = next.Next;
            }
            next.Next = new DispatchChannelSinkProvider();
            IServerChannelSink sink = provider.CreateSink(channel);
            next.Next = null;
            return sink;
        }

        [SecurityCritical]
        public static ServerProcessing DispatchMessage(IServerChannelSinkStack sinkStack, IMessage msg, out IMessage replyMsg)
        {
            ServerProcessing complete = ServerProcessing.Complete;
            replyMsg = null;
            try
            {
                if (msg == null)
                {
                    throw new ArgumentNullException("msg");
                }
                IncrementRemoteCalls();
                ServerIdentity identity = CheckDisconnectedOrCreateWellKnownObject(msg);
                if (identity.ServerType == typeof(AppDomain))
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_AppDomainsCantBeCalledRemotely"));
                }
                IMethodCallMessage message = msg as IMethodCallMessage;
                if (message == null)
                {
                    if (!typeof(IMessageSink).IsAssignableFrom(identity.ServerType))
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_AppDomainsCantBeCalledRemotely"));
                    }
                    complete = ServerProcessing.Complete;
                    replyMsg = GetCrossContextChannelSink().SyncProcessMessage(msg);
                    return complete;
                }
                MethodInfo methodBase = (MethodInfo) message.MethodBase;
                if (!IsMethodReallyPublic(methodBase) && !RemotingServices.IsMethodAllowedRemotely(methodBase))
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_NonPublicOrStaticCantBeCalledRemotely"));
                }
                InternalRemotingServices.GetReflectionCachedData((MethodBase) methodBase);
                if (RemotingServices.IsOneWay(methodBase))
                {
                    complete = ServerProcessing.OneWay;
                    GetCrossContextChannelSink().AsyncProcessMessage(msg, null);
                    return complete;
                }
                complete = ServerProcessing.Complete;
                if (!identity.ServerType.IsContextful)
                {
                    object[] args = new object[] { msg, identity.ServerContext };
                    replyMsg = (IMessage) CrossContextChannel.SyncProcessMessageCallback(args);
                    return complete;
                }
                replyMsg = GetCrossContextChannelSink().SyncProcessMessage(msg);
            }
            catch (Exception exception)
            {
                if (complete == ServerProcessing.OneWay)
                {
                    return complete;
                }
                try
                {
                    IMethodCallMessage mcm = (msg != null) ? ((IMethodCallMessage) msg) : ((IMethodCallMessage) new ErrorMessage());
                    replyMsg = new ReturnMessage(exception, mcm);
                    if (msg != null)
                    {
                        ((ReturnMessage) replyMsg).SetLogicalCallContext((LogicalCallContext) msg.Properties[Message.CallContextKey]);
                    }
                }
                catch (Exception)
                {
                }
            }
            return complete;
        }

        [SecurityCritical]
        internal static string FindFirstHttpUrlForObject(string objectUri)
        {
            if (objectUri != null)
            {
                RegisteredChannelList list = s_registeredChannels;
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    if (list.IsReceiver(i))
                    {
                        IChannelReceiver channel = (IChannelReceiver) list.GetChannel(i);
                        string fullName = channel.GetType().FullName;
                        if ((string.CompareOrdinal(fullName, "System.Runtime.Remoting.Channels.Http.HttpChannel") == 0) || (string.CompareOrdinal(fullName, "System.Runtime.Remoting.Channels.Http.HttpServerChannel") == 0))
                        {
                            string[] urlsForUri = channel.GetUrlsForUri(objectUri);
                            if ((urlsForUri != null) && (urlsForUri.Length > 0))
                            {
                                return urlsForUri[0];
                            }
                        }
                    }
                }
            }
            return null;
        }

        [SecurityCritical]
        public static IChannel GetChannel(string name)
        {
            RegisteredChannelList list = s_registeredChannels;
            int index = list.FindChannelIndex(name);
            if (0 > index)
            {
                return null;
            }
            IChannel channel = list.GetChannel(index);
            if ((channel is CrossAppDomainChannel) || (channel is CrossContextChannel))
            {
                return null;
            }
            return channel;
        }

        [SecurityCritical]
        internal static IMessageSink GetChannelSinkForProxy(object obj)
        {
            IMessageSink channelSink = null;
            if (RemotingServices.IsTransparentProxy(obj))
            {
                RemotingProxy realProxy = RemotingServices.GetRealProxy(obj) as RemotingProxy;
                if (realProxy != null)
                {
                    channelSink = realProxy.IdentityObject.ChannelSink;
                }
            }
            return channelSink;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static IDictionary GetChannelSinkProperties(object obj)
        {
            IMessageSink channelSinkForProxy = GetChannelSinkForProxy(obj);
            IClientChannelSink nextChannelSink = channelSinkForProxy as IClientChannelSink;
            if (nextChannelSink != null)
            {
                ArrayList dictionaries = new ArrayList();
                do
                {
                    IDictionary properties = nextChannelSink.Properties;
                    if (properties != null)
                    {
                        dictionaries.Add(properties);
                    }
                    nextChannelSink = nextChannelSink.NextChannelSink;
                }
                while (nextChannelSink != null);
                return new AggregateDictionary(dictionaries);
            }
            IDictionary dictionary2 = channelSinkForProxy as IDictionary;
            if (dictionary2 != null)
            {
                return dictionary2;
            }
            return null;
        }

        internal static IMessageSink GetCrossContextChannelSink()
        {
            if (xCtxChannel == null)
            {
                xCtxChannel = CrossContextChannel.MessageSink;
            }
            return xCtxChannel;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe Perf_Contexts* GetPrivateContextsPerfCounters();
        [SecurityCritical]
        public static string[] GetUrlsForObject(MarshalByRefObject obj)
        {
            bool flag;
            if (obj == null)
            {
                return null;
            }
            RegisteredChannelList list = s_registeredChannels;
            int count = list.Count;
            Hashtable hashtable = new Hashtable();
            Identity identity = MarshalByRefObject.GetIdentity(obj, out flag);
            if (identity != null)
            {
                string objURI = identity.ObjURI;
                if (objURI != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (list.IsReceiver(i))
                        {
                            try
                            {
                                string[] urlsForUri = ((IChannelReceiver) list.GetChannel(i)).GetUrlsForUri(objURI);
                                for (int j = 0; j < urlsForUri.Length; j++)
                                {
                                    hashtable.Add(urlsForUri[j], urlsForUri[j]);
                                }
                            }
                            catch (NotSupportedException)
                            {
                            }
                        }
                    }
                }
            }
            ICollection keys = hashtable.Keys;
            string[] strArray2 = new string[keys.Count];
            int num4 = 0;
            foreach (string str2 in keys)
            {
                strArray2[num4++] = str2;
            }
            return strArray2;
        }

        [SecurityCritical]
        internal static void IncrementRemoteCalls()
        {
            IncrementRemoteCalls(1L);
        }

        [SecurityCritical]
        internal static unsafe void IncrementRemoteCalls(long cCalls)
        {
            remoteCalls += cCalls;
            if (perf_Contexts != null)
            {
                perf_Contexts.cRemoteCalls += (int) cCalls;
            }
        }

        private static bool IsMethodReallyPublic(MethodInfo mi)
        {
            if (!mi.IsPublic || mi.IsStatic)
            {
                return false;
            }
            if (mi.IsGenericMethod)
            {
                foreach (Type type in mi.GetGenericArguments())
                {
                    if (!type.IsVisible)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [SecurityCritical]
        internal static void NotifyProfiler(IMessage msg, RemotingProfilerEvent profilerEvent)
        {
            switch (profilerEvent)
            {
                case RemotingProfilerEvent.ClientSend:
                    Guid guid;
                    if (!RemotingServices.CORProfilerTrackRemoting())
                    {
                        break;
                    }
                    RemotingServices.CORProfilerRemotingClientSendingMessage(out guid, false);
                    if (!RemotingServices.CORProfilerTrackRemotingCookie())
                    {
                        break;
                    }
                    msg.Properties["CORProfilerCookie"] = guid;
                    return;

                case RemotingProfilerEvent.ClientReceive:
                    if (RemotingServices.CORProfilerTrackRemoting())
                    {
                        Guid empty = Guid.Empty;
                        if (RemotingServices.CORProfilerTrackRemotingCookie())
                        {
                            object obj2 = msg.Properties["CORProfilerCookie"];
                            if (obj2 != null)
                            {
                                empty = (Guid) obj2;
                            }
                        }
                        RemotingServices.CORProfilerRemotingClientReceivingReply(empty, false);
                    }
                    break;

                default:
                    return;
            }
        }

        [SecurityCritical]
        internal static void RefreshChannelData()
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_channelLock, ref lockTaken);
                s_currentChannelData = CollectChannelDataFromChannels();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(s_channelLock);
                }
            }
        }

        [SecuritySafeCritical, Obsolete("Use System.Runtime.Remoting.ChannelServices.RegisterChannel(IChannel chnl, bool ensureSecurity) instead.", false), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterChannel(IChannel chnl)
        {
            RegisterChannelInternal(chnl, false);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterChannel(IChannel chnl, bool ensureSecurity)
        {
            RegisterChannelInternal(chnl, ensureSecurity);
        }

        [SecurityCritical]
        internal static unsafe void RegisterChannelInternal(IChannel chnl, bool ensureSecurity)
        {
            if (chnl == null)
            {
                throw new ArgumentNullException("chnl");
            }
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_channelLock, ref lockTaken);
                string channelName = chnl.ChannelName;
                RegisteredChannelList list = s_registeredChannels;
                if (((channelName != null) && (channelName.Length != 0)) && (-1 != list.FindChannelIndex(chnl.ChannelName)))
                {
                    goto Label_0180;
                }
                if (ensureSecurity)
                {
                    ISecurableChannel channel = chnl as ISecurableChannel;
                    if (channel == null)
                    {
                        object[] values = new object[] { chnl.ChannelName ?? chnl.ToString() };
                        throw new RemotingException(Environment.GetResourceString("Remoting_Channel_CannotBeSecured", values));
                    }
                    channel.IsSecured = ensureSecurity;
                }
                RegisteredChannel[] registeredChannels = list.RegisteredChannels;
                RegisteredChannel[] channels = null;
                if (registeredChannels == null)
                {
                    channels = new RegisteredChannel[1];
                }
                else
                {
                    channels = new RegisteredChannel[registeredChannels.Length + 1];
                }
                if (!unloadHandlerRegistered && !(chnl is CrossAppDomainChannel))
                {
                    AppDomain.CurrentDomain.DomainUnload += new EventHandler(ChannelServices.UnloadHandler);
                    unloadHandlerRegistered = true;
                }
                int channelPriority = chnl.ChannelPriority;
                int index = 0;
                while (index < registeredChannels.Length)
                {
                    RegisteredChannel channel2 = registeredChannels[index];
                    if (channelPriority > channel2.Channel.ChannelPriority)
                    {
                        channels[index] = new RegisteredChannel(chnl);
                        break;
                    }
                    channels[index] = channel2;
                    index++;
                }
                if (index != registeredChannels.Length)
                {
                    goto Label_014F;
                }
                channels[registeredChannels.Length] = new RegisteredChannel(chnl);
                goto Label_0157;
            Label_013D:
                channels[index + 1] = registeredChannels[index];
                index++;
            Label_014F:
                if (index < registeredChannels.Length)
                {
                    goto Label_013D;
                }
            Label_0157:
                if (perf_Contexts != null)
                {
                    perf_Contexts.cChannels++;
                }
                s_registeredChannels = new RegisteredChannelList(channels);
                goto Label_01A4;
            Label_0180:;
                throw new RemotingException(Environment.GetResourceString("Remoting_ChannelNameAlreadyRegistered", new object[] { chnl.ChannelName }));
            Label_01A4:
                RefreshChannelData();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(s_channelLock);
                }
            }
        }

        [SecurityCritical]
        private static void StopListeningOnAllChannels()
        {
            try
            {
                RegisteredChannelList list = s_registeredChannels;
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    if (list.IsReceiver(i))
                    {
                        ((IChannelReceiver) list.GetChannel(i)).StopListening(null);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        [SecurityCritical]
        public static IMessage SyncDispatchMessage(IMessage msg)
        {
            IMessage message = null;
            bool flag = false;
            try
            {
                if (msg == null)
                {
                    throw new ArgumentNullException("msg");
                }
                IncrementRemoteCalls();
                if (!(msg is TransitionCall))
                {
                    CheckDisconnectedOrCreateWellKnownObject(msg);
                    flag = RemotingServices.IsOneWay(((IMethodMessage) msg).MethodBase);
                }
                IMessageSink crossContextChannelSink = GetCrossContextChannelSink();
                if (!flag)
                {
                    return crossContextChannelSink.SyncProcessMessage(msg);
                }
                crossContextChannelSink.AsyncProcessMessage(msg, null);
            }
            catch (Exception exception)
            {
                if (flag)
                {
                    return message;
                }
                try
                {
                    IMethodCallMessage mcm = (msg != null) ? ((IMethodCallMessage) msg) : ((IMethodCallMessage) new ErrorMessage());
                    message = new ReturnMessage(exception, mcm);
                    if (msg != null)
                    {
                        ((ReturnMessage) message).SetLogicalCallContext(mcm.LogicalCallContext);
                    }
                }
                catch (Exception)
                {
                }
            }
            return message;
        }

        [SecurityCritical]
        internal static void UnloadHandler(object sender, EventArgs e)
        {
            StopListeningOnAllChannels();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static unsafe void UnregisterChannel(IChannel chnl)
        {
            bool lockTaken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_channelLock, ref lockTaken);
                if (chnl != null)
                {
                    RegisteredChannelList list = s_registeredChannels;
                    int num = list.FindChannelIndex(chnl);
                    if (-1 == num)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_ChannelNotRegistered", new object[] { chnl.ChannelName }));
                    }
                    RegisteredChannel[] registeredChannels = list.RegisteredChannels;
                    RegisteredChannel[] channels = null;
                    channels = new RegisteredChannel[registeredChannels.Length - 1];
                    IChannelReceiver receiver = chnl as IChannelReceiver;
                    if (receiver != null)
                    {
                        receiver.StopListening(null);
                    }
                    int index = 0;
                    int num3 = 0;
                    while (num3 < registeredChannels.Length)
                    {
                        if (num3 == num)
                        {
                            num3++;
                        }
                        else
                        {
                            channels[index] = registeredChannels[num3];
                            index++;
                            num3++;
                        }
                    }
                    if (perf_Contexts != null)
                    {
                        perf_Contexts.cChannels--;
                    }
                    s_registeredChannels = new RegisteredChannelList(channels);
                }
                RefreshChannelData();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(s_channelLock);
                }
            }
        }

        internal static object[] CurrentChannelData
        {
            [SecurityCritical]
            get
            {
                if (s_currentChannelData == null)
                {
                    RefreshChannelData();
                }
                return s_currentChannelData;
            }
        }

        public static IChannel[] RegisteredChannels
        {
            [SecurityCritical]
            get
            {
                RegisteredChannelList list = s_registeredChannels;
                int count = list.Count;
                if (count == 0)
                {
                    return new IChannel[0];
                }
                int num2 = count - 1;
                int num3 = 0;
                IChannel[] channelArray = new IChannel[num2];
                for (int i = 0; i < count; i++)
                {
                    IChannel channel = list.GetChannel(i);
                    if (!(channel is CrossAppDomainChannel))
                    {
                        channelArray[num3++] = channel;
                    }
                }
                return channelArray;
            }
        }

        private static long remoteCalls
        {
            get
            {
                return Thread.GetDomain().RemotingData.ChannelServicesData.remoteCalls;
            }
            set
            {
                Thread.GetDomain().RemotingData.ChannelServicesData.remoteCalls = value;
            }
        }
    }
}

