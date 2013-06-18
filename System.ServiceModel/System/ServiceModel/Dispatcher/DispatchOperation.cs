namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    public sealed class DispatchOperation
    {
        private string action;
        private bool autoDisposeParameters;
        private SynchronizedCollection<ICallContextInitializer> callContextInitializers;
        private bool deserializeRequest;
        private SynchronizedCollection<FaultContractInfo> faultContractInfos;
        private IDispatchFaultFormatter faultFormatter;
        private IDispatchMessageFormatter formatter;
        private bool hasNoDisposableParameters;
        private ImpersonationOption impersonation;
        private bool includeExceptionDetailInFaults;
        private IOperationInvoker invoker;
        private bool isFaultFormatterSetExplicit;
        private bool isInsideTransactedReceiveScope;
        private bool isOneWay;
        private bool isTerminating;
        private string name;
        private SynchronizedCollection<IParameterInspector> parameterInspectors;
        private DispatchRuntime parent;
        private bool releaseInstanceAfterCall;
        private bool releaseInstanceBeforeCall;
        private string replyAction;
        private bool serializeReply;
        private bool transactionAutoComplete;
        private bool transactionRequired;

        public DispatchOperation(DispatchRuntime parent, string name, string action)
        {
            this.deserializeRequest = true;
            this.serializeReply = true;
            this.autoDisposeParameters = true;
            if (parent == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
            }
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            this.parent = parent;
            this.name = name;
            this.action = action;
            this.impersonation = ImpersonationOption.NotAllowed;
            this.callContextInitializers = parent.NewBehaviorCollection<ICallContextInitializer>();
            this.faultContractInfos = parent.NewBehaviorCollection<FaultContractInfo>();
            this.parameterInspectors = parent.NewBehaviorCollection<IParameterInspector>();
            this.isOneWay = true;
        }

        public DispatchOperation(DispatchRuntime parent, string name, string action, string replyAction) : this(parent, name, action)
        {
            this.replyAction = replyAction;
            this.isOneWay = false;
        }

        public string Action
        {
            get
            {
                return this.action;
            }
        }

        public bool AutoDisposeParameters
        {
            get
            {
                return this.autoDisposeParameters;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.autoDisposeParameters = value;
                }
            }
        }

        internal bool BufferedReceiveEnabled
        {
            get
            {
                return this.parent.ChannelDispatcher.BufferedReceiveEnabled;
            }
            set
            {
                this.parent.ChannelDispatcher.BufferedReceiveEnabled = value;
            }
        }

        public SynchronizedCollection<ICallContextInitializer> CallContextInitializers
        {
            get
            {
                return this.callContextInitializers;
            }
        }

        public bool DeserializeRequest
        {
            get
            {
                return this.deserializeRequest;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.deserializeRequest = value;
                }
            }
        }

        public SynchronizedCollection<FaultContractInfo> FaultContractInfos
        {
            get
            {
                return this.faultContractInfos;
            }
        }

        internal IDispatchFaultFormatter FaultFormatter
        {
            get
            {
                if (this.faultFormatter == null)
                {
                    this.faultFormatter = new DataContractSerializerFaultFormatter(this.faultContractInfos);
                }
                return this.faultFormatter;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.faultFormatter = value;
                    this.isFaultFormatterSetExplicit = true;
                }
            }
        }

        public IDispatchMessageFormatter Formatter
        {
            get
            {
                return this.formatter;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.formatter = value;
                }
            }
        }

        internal bool HasNoDisposableParameters
        {
            get
            {
                return this.hasNoDisposableParameters;
            }
            set
            {
                this.hasNoDisposableParameters = value;
            }
        }

        public ImpersonationOption Impersonation
        {
            get
            {
                return this.impersonation;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.impersonation = value;
                }
            }
        }

        internal bool IncludeExceptionDetailInFaults
        {
            get
            {
                return this.includeExceptionDetailInFaults;
            }
            set
            {
                this.includeExceptionDetailInFaults = value;
            }
        }

        internal IDispatchMessageFormatter InternalFormatter
        {
            get
            {
                return this.formatter;
            }
            set
            {
                this.formatter = value;
            }
        }

        internal IOperationInvoker InternalInvoker
        {
            get
            {
                return this.invoker;
            }
            set
            {
                this.invoker = value;
            }
        }

        public IOperationInvoker Invoker
        {
            get
            {
                return this.invoker;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.invoker = value;
                }
            }
        }

        internal bool IsFaultFormatterSetExplicit
        {
            get
            {
                return this.isFaultFormatterSetExplicit;
            }
        }

        public bool IsInsideTransactedReceiveScope
        {
            get
            {
                return this.isInsideTransactedReceiveScope;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.isInsideTransactedReceiveScope = value;
                }
            }
        }

        public bool IsOneWay
        {
            get
            {
                return this.isOneWay;
            }
        }

        public bool IsTerminating
        {
            get
            {
                return this.isTerminating;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.isTerminating = value;
                }
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public SynchronizedCollection<IParameterInspector> ParameterInspectors
        {
            get
            {
                return this.parameterInspectors;
            }
        }

        public DispatchRuntime Parent
        {
            get
            {
                return this.parent;
            }
        }

        internal System.ServiceModel.Dispatcher.ReceiveContextAcknowledgementMode ReceiveContextAcknowledgementMode { get; set; }

        public bool ReleaseInstanceAfterCall
        {
            get
            {
                return this.releaseInstanceAfterCall;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.releaseInstanceAfterCall = value;
                }
            }
        }

        public bool ReleaseInstanceBeforeCall
        {
            get
            {
                return this.releaseInstanceBeforeCall;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.releaseInstanceBeforeCall = value;
                }
            }
        }

        public string ReplyAction
        {
            get
            {
                return this.replyAction;
            }
        }

        public bool SerializeReply
        {
            get
            {
                return this.serializeReply;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.serializeReply = value;
                }
            }
        }

        public bool TransactionAutoComplete
        {
            get
            {
                return this.transactionAutoComplete;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.transactionAutoComplete = value;
                }
            }
        }

        public bool TransactionRequired
        {
            get
            {
                return this.transactionRequired;
            }
            set
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.InvalidateRuntime();
                    this.transactionRequired = value;
                }
            }
        }
    }
}

