namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Xml;

    public sealed class ClientRuntime
    {
        private bool addTransactionFlowProperties;
        private System.Type callbackProxyType;
        private ProxyBehaviorCollection<IChannelInitializer> channelInitializers;
        private string contractName;
        private string contractNamespace;
        private System.Type contractProxyType;
        private System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime;
        private System.ServiceModel.Security.IdentityVerifier identityVerifier;
        private ProxyBehaviorCollection<IInteractiveChannelInitializer> interactiveChannelInitializers;
        private int maxFaultSize;
        private ProxyBehaviorCollection<IClientMessageInspector> messageInspectors;
        private bool messageVersionNoneFaultsEnabled;
        private OperationCollection operations;
        private IClientOperationSelector operationSelector;
        private ImmutableClientRuntime runtime;
        private SharedRuntimeState shared;
        private ClientOperation unhandled;
        private bool useSynchronizationContext;
        private Uri via;

        internal ClientRuntime(System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime, SharedRuntimeState shared) : this(dispatchRuntime.EndpointDispatcher.ContractName, dispatchRuntime.EndpointDispatcher.ContractNamespace, shared)
        {
            if (dispatchRuntime == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatchRuntime");
            }
            this.dispatchRuntime = dispatchRuntime;
            this.shared = shared;
        }

        internal ClientRuntime(string contractName, string contractNamespace) : this(contractName, contractNamespace, new SharedRuntimeState(false))
        {
        }

        private ClientRuntime(string contractName, string contractNamespace, SharedRuntimeState shared)
        {
            this.addTransactionFlowProperties = true;
            this.useSynchronizationContext = true;
            this.contractName = contractName;
            this.contractNamespace = contractNamespace;
            this.shared = shared;
            this.operations = new OperationCollection(this);
            this.channelInitializers = new ProxyBehaviorCollection<IChannelInitializer>(this);
            this.messageInspectors = new ProxyBehaviorCollection<IClientMessageInspector>(this);
            this.interactiveChannelInitializers = new ProxyBehaviorCollection<IInteractiveChannelInitializer>(this);
            this.unhandled = new ClientOperation(this, "*", "*", "*");
            this.unhandled.InternalFormatter = new MessageOperationFormatter();
            this.maxFaultSize = 0x10000;
        }

        internal T[] GetArray<T>(SynchronizedCollection<T> collection)
        {
            lock (collection.SyncRoot)
            {
                if (collection.Count == 0)
                {
                    return EmptyArray<T>.Instance;
                }
                T[] array = new T[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }
        }

        internal ImmutableClientRuntime GetRuntime()
        {
            lock (this.ThisLock)
            {
                if (this.runtime == null)
                {
                    this.runtime = new ImmutableClientRuntime(this);
                }
                return this.runtime;
            }
        }

        internal void InvalidateRuntime()
        {
            lock (this.ThisLock)
            {
                this.shared.ThrowIfImmutable();
                this.runtime = null;
            }
        }

        internal bool IsFault(ref Message reply)
        {
            if (reply == null)
            {
                return false;
            }
            return (reply.IsFault || (this.MessageVersionNoneFaultsEnabled && IsMessageVersionNoneFault(ref reply, this.MaxFaultSize)));
        }

        internal static bool IsMessageVersionNoneFault(ref Message message, int maxFaultSize)
        {
            bool flag;
            if ((message.Version != MessageVersion.None) || message.IsEmpty)
            {
                return false;
            }
            HttpResponseMessageProperty property = message.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
            if ((property == null) || (property.StatusCode != HttpStatusCode.InternalServerError))
            {
                return false;
            }
            using (MessageBuffer buffer = message.CreateBufferedCopy(maxFaultSize))
            {
                message.Close();
                message = buffer.CreateMessage();
                using (Message message2 = buffer.CreateMessage())
                {
                    using (XmlDictionaryReader reader = message2.GetReaderAtBodyContents())
                    {
                        flag = reader.IsStartElement(XD.MessageDictionary.Fault, MessageVersion.None.Envelope.DictionaryNamespace);
                    }
                }
            }
            return flag;
        }

        internal void LockDownProperties()
        {
            this.shared.LockDownProperties();
        }

        internal SynchronizedCollection<T> NewBehaviorCollection<T>()
        {
            return new ProxyBehaviorCollection<T>(this);
        }

        internal bool AddTransactionFlowProperties
        {
            get
            {
                return this.addTransactionFlowProperties;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.addTransactionFlowProperties = value;
                }
            }
        }

        public System.Type CallbackClientType
        {
            get
            {
                return this.callbackProxyType;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.callbackProxyType = value;
                }
            }
        }

        public System.ServiceModel.Dispatcher.DispatchRuntime CallbackDispatchRuntime
        {
            get
            {
                if (this.dispatchRuntime == null)
                {
                    this.dispatchRuntime = new System.ServiceModel.Dispatcher.DispatchRuntime(this, this.shared);
                }
                return this.dispatchRuntime;
            }
        }

        public SynchronizedCollection<IChannelInitializer> ChannelInitializers
        {
            get
            {
                return this.channelInitializers;
            }
        }

        public System.Type ContractClientType
        {
            get
            {
                return this.contractProxyType;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.contractProxyType = value;
                }
            }
        }

        public string ContractName
        {
            get
            {
                return this.contractName;
            }
        }

        public string ContractNamespace
        {
            get
            {
                return this.contractNamespace;
            }
        }

        internal System.ServiceModel.Dispatcher.DispatchRuntime DispatchRuntime
        {
            get
            {
                return this.dispatchRuntime;
            }
        }

        internal bool EnableFaults
        {
            get
            {
                if (this.IsOnServer)
                {
                    return this.dispatchRuntime.EnableFaults;
                }
                return this.shared.EnableFaults;
            }
            set
            {
                lock (this.ThisLock)
                {
                    if (this.IsOnServer)
                    {
                        string message = System.ServiceModel.SR.GetString("SFxSetEnableFaultsOnChannelDispatcher0");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(message));
                    }
                    this.InvalidateRuntime();
                    this.shared.EnableFaults = value;
                }
            }
        }

        internal System.ServiceModel.Security.IdentityVerifier IdentityVerifier
        {
            get
            {
                if (this.identityVerifier == null)
                {
                    this.identityVerifier = System.ServiceModel.Security.IdentityVerifier.CreateDefault();
                }
                return this.identityVerifier;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.InvalidateRuntime();
                this.identityVerifier = value;
            }
        }

        public SynchronizedCollection<IInteractiveChannelInitializer> InteractiveChannelInitializers
        {
            get
            {
                return this.interactiveChannelInitializers;
            }
        }

        internal bool IsOnServer
        {
            get
            {
                return this.shared.IsOnServer;
            }
        }

        public bool ManualAddressing
        {
            get
            {
                if (this.IsOnServer)
                {
                    return this.dispatchRuntime.ManualAddressing;
                }
                return this.shared.ManualAddressing;
            }
            set
            {
                lock (this.ThisLock)
                {
                    if (this.IsOnServer)
                    {
                        string message = System.ServiceModel.SR.GetString("SFxSetManualAddresssingOnChannelDispatcher0");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(message));
                    }
                    this.InvalidateRuntime();
                    this.shared.ManualAddressing = value;
                }
            }
        }

        public int MaxFaultSize
        {
            get
            {
                return this.maxFaultSize;
            }
            set
            {
                this.InvalidateRuntime();
                this.maxFaultSize = value;
            }
        }

        internal int MaxParameterInspectors
        {
            get
            {
                lock (this.ThisLock)
                {
                    int num = 0;
                    for (int i = 0; i < this.operations.Count; i++)
                    {
                        num = Math.Max(num, this.operations[i].ParameterInspectors.Count);
                    }
                    return num;
                }
            }
        }

        public SynchronizedCollection<IClientMessageInspector> MessageInspectors
        {
            get
            {
                return this.messageInspectors;
            }
        }

        public bool MessageVersionNoneFaultsEnabled
        {
            get
            {
                return this.messageVersionNoneFaultsEnabled;
            }
            set
            {
                this.InvalidateRuntime();
                this.messageVersionNoneFaultsEnabled = value;
            }
        }

        public SynchronizedKeyedCollection<string, ClientOperation> Operations
        {
            get
            {
                return this.operations;
            }
        }

        public IClientOperationSelector OperationSelector
        {
            get
            {
                return this.operationSelector;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.operationSelector = value;
                }
            }
        }

        internal object ThisLock
        {
            get
            {
                return this.shared;
            }
        }

        public ClientOperation UnhandledClientOperation
        {
            get
            {
                return this.unhandled;
            }
        }

        internal bool UseSynchronizationContext
        {
            get
            {
                return this.useSynchronizationContext;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.useSynchronizationContext = value;
                }
            }
        }

        public bool ValidateMustUnderstand
        {
            get
            {
                return this.shared.ValidateMustUnderstand;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.shared.ValidateMustUnderstand = value;
                }
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.InvalidateRuntime();
                    this.via = value;
                }
            }
        }

        private class OperationCollection : SynchronizedKeyedCollection<string, ClientOperation>
        {
            private ClientRuntime outer;

            internal OperationCollection(ClientRuntime outer) : base(outer.ThisLock)
            {
                this.outer = outer;
            }

            protected override void ClearItems()
            {
                this.outer.InvalidateRuntime();
                base.ClearItems();
            }

            protected override string GetKeyForItem(ClientOperation item)
            {
                return item.Name;
            }

            protected override void InsertItem(int index, ClientOperation item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                if (item.Parent != this.outer)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxMismatchedOperationParent"));
                }
                this.outer.InvalidateRuntime();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                this.outer.InvalidateRuntime();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, ClientOperation item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                if (item.Parent != this.outer)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxMismatchedOperationParent"));
                }
                this.outer.InvalidateRuntime();
                base.SetItem(index, item);
            }
        }

        private class ProxyBehaviorCollection<T> : SynchronizedCollection<T>
        {
            private ClientRuntime outer;

            internal ProxyBehaviorCollection(ClientRuntime outer) : base(outer.ThisLock)
            {
                this.outer = outer;
            }

            protected override void ClearItems()
            {
                this.outer.InvalidateRuntime();
                base.ClearItems();
            }

            protected override void InsertItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                this.outer.InvalidateRuntime();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                this.outer.InvalidateRuntime();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, T item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                this.outer.InvalidateRuntime();
                base.SetItem(index, item);
            }
        }
    }
}

