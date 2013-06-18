namespace System.ServiceModel
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CallbackBehaviorAttribute : Attribute, IEndpointBehavior
    {
        private bool automaticSessionShutdown = true;
        private System.ServiceModel.ConcurrencyMode concurrencyMode;
        internal static IsolationLevel DefaultIsolationLevel = IsolationLevel.Unspecified;
        private bool ignoreExtensionDataObject;
        private bool includeExceptionDetailInFaults;
        private bool isolationLevelSet;
        private int maxItemsInObjectGraph = 0x10000;
        private IsolationLevel transactionIsolationLevel = DefaultIsolationLevel;
        private TimeSpan transactionTimeout = TimeSpan.Zero;
        private bool transactionTimeoutSet;
        private string transactionTimeoutString;
        private bool useSynchronizationContext = true;
        private bool validateMustUnderstand = true;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SetIsolationLevel(ChannelDispatcher channelDispatcher)
        {
            if (channelDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelDispatcher");
            }
            channelDispatcher.TransactionIsolationLevel = this.transactionIsolationLevel;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection parameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
        {
            if (!serviceEndpoint.Contract.IsDuplex())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCallbackBehaviorAttributeOnlyOnDuplex", new object[] { serviceEndpoint.Contract.Name })));
            }
            DispatchRuntime dispatchRuntime = clientRuntime.DispatchRuntime;
            dispatchRuntime.ValidateMustUnderstand = this.validateMustUnderstand;
            dispatchRuntime.ConcurrencyMode = this.concurrencyMode;
            dispatchRuntime.ChannelDispatcher.IncludeExceptionDetailInFaults = this.includeExceptionDetailInFaults;
            dispatchRuntime.AutomaticInputSessionShutdown = this.automaticSessionShutdown;
            if (!this.useSynchronizationContext)
            {
                dispatchRuntime.SynchronizationContext = null;
            }
            dispatchRuntime.ChannelDispatcher.TransactionTimeout = this.transactionTimeout;
            if (this.isolationLevelSet)
            {
                this.SetIsolationLevel(dispatchRuntime.ChannelDispatcher);
            }
            DataContractSerializerServiceBehavior.ApplySerializationSettings(serviceEndpoint, this.ignoreExtensionDataObject, this.maxItemsInObjectGraph);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFXEndpointBehaviorUsedOnWrongSide", new object[] { typeof(CallbackBehaviorAttribute).Name })));
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        public bool AutomaticSessionShutdown
        {
            get
            {
                return this.automaticSessionShutdown;
            }
            set
            {
                this.automaticSessionShutdown = value;
            }
        }

        public System.ServiceModel.ConcurrencyMode ConcurrencyMode
        {
            get
            {
                return this.concurrencyMode;
            }
            set
            {
                if (!ConcurrencyModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.concurrencyMode = value;
            }
        }

        public bool IgnoreExtensionDataObject
        {
            get
            {
                return this.ignoreExtensionDataObject;
            }
            set
            {
                this.ignoreExtensionDataObject = value;
            }
        }

        public bool IncludeExceptionDetailInFaults
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

        internal bool IsolationLevelSet
        {
            get
            {
                return this.isolationLevelSet;
            }
        }

        public int MaxItemsInObjectGraph
        {
            get
            {
                return this.maxItemsInObjectGraph;
            }
            set
            {
                this.maxItemsInObjectGraph = value;
            }
        }

        public IsolationLevel TransactionIsolationLevel
        {
            get
            {
                return this.transactionIsolationLevel;
            }
            set
            {
                switch (value)
                {
                    case IsolationLevel.Serializable:
                    case IsolationLevel.RepeatableRead:
                    case IsolationLevel.ReadCommitted:
                    case IsolationLevel.ReadUncommitted:
                    case IsolationLevel.Snapshot:
                    case IsolationLevel.Chaos:
                    case IsolationLevel.Unspecified:
                        this.transactionIsolationLevel = value;
                        this.isolationLevelSet = true;
                        return;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
            }
        }

        public string TransactionTimeout
        {
            get
            {
                return this.transactionTimeoutString;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                try
                {
                    TimeSpan span = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                    if (span < TimeSpan.Zero)
                    {
                        string message = System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, message));
                    }
                    this.transactionTimeout = span;
                    this.transactionTimeoutString = value;
                    this.transactionTimeoutSet = true;
                }
                catch (FormatException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxTimeoutInvalidStringFormat"), "value", exception));
                }
                catch (OverflowException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
            }
        }

        internal bool TransactionTimeoutSet
        {
            get
            {
                return this.transactionTimeoutSet;
            }
        }

        public bool UseSynchronizationContext
        {
            get
            {
                return this.useSynchronizationContext;
            }
            set
            {
                this.useSynchronizationContext = value;
            }
        }

        public bool ValidateMustUnderstand
        {
            get
            {
                return this.validateMustUnderstand;
            }
            set
            {
                this.validateMustUnderstand = value;
            }
        }
    }
}

