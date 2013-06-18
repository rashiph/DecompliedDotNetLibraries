namespace Microsoft.Transactions.Bridge
{
    using Microsoft.Transactions;
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Internal;

    internal abstract class TransactionManager
    {
        private object bridgeConfig;
        private Guid id;
        private Microsoft.Transactions.Bridge.IProtocolProvider protocolProvider;
        private Microsoft.Transactions.Bridge.IProtocolProviderCoordinatorService protocolProviderCoordinatorService;
        private Microsoft.Transactions.Bridge.IProtocolProviderPropagationService protocolProviderPropagationService;
        private Microsoft.Transactions.Bridge.TransactionManagerCoordinatorService transactionManagerCoordinatorService;
        private Microsoft.Transactions.Bridge.TransactionManagerPropagationService transactionManagerPropagationService;
        private Microsoft.Transactions.Bridge.TransactionManagerSettings transactionManagerSettings;

        protected TransactionManager()
        {
            PropagationProtocolsTracing.TraceVerbose("TransactionManager::TransactionManager");
            this.id = Guid.NewGuid();
            PropagationProtocolsTracing.TraceVerbose(this.id.ToString("B", null));
        }

        public abstract void Initialize();
        public void Initialize(string fullyQualifiedTypeName, object bridgeConfig)
        {
            PropagationProtocolsTracing.TraceVerbose("TransactionManager::Initialize");
            PropagationProtocolsTracing.TraceVerbose(fullyQualifiedTypeName);
            if (!TransactionBridge.IsAssemblyMicrosoftSigned(fullyQualifiedTypeName))
            {
                PropagationProtocolsTracing.TraceVerbose("Protocol type has wrong signature: " + fullyQualifiedTypeName);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(Microsoft.Transactions.SR.GetString("ProtocolTypeWrongSignature")));
            }
            this.bridgeConfig = bridgeConfig;
            Type type = Type.GetType(fullyQualifiedTypeName, true);
            PropagationProtocolsTracing.TraceVerbose(type.ToString());
            this.protocolProvider = (Microsoft.Transactions.Bridge.IProtocolProvider) Activator.CreateInstance(type);
            this.Initialize();
            this.protocolProviderCoordinatorService = this.protocolProvider.CoordinatorService;
            this.protocolProviderPropagationService = this.protocolProvider.PropagationService;
        }

        public abstract void Recover();
        public abstract void Start();
        public abstract void Stop();
        public override string ToString()
        {
            return (base.GetType().ToString() + " " + this.id.ToString("B", null));
        }

        protected static void UnhandledExceptionHandler(Exception exception)
        {
            DiagnosticUtility.InvokeFinalHandler(exception);
        }

        protected object BridgeConfiguration
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.bridgeConfig;
            }
        }

        public Microsoft.Transactions.Bridge.TransactionManagerCoordinatorService CoordinatorService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionManagerCoordinatorService;
            }
        }

        public Microsoft.Transactions.Bridge.IProtocolProvider IProtocolProvider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolProvider;
            }
        }

        public Microsoft.Transactions.Bridge.IProtocolProviderCoordinatorService IProtocolProviderCoordinatorService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolProviderCoordinatorService;
            }
        }

        public Microsoft.Transactions.Bridge.IProtocolProviderPropagationService IProtocolProviderPropagationService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolProviderPropagationService;
            }
        }

        public abstract int MaxLogEntrySize { get; }

        public Microsoft.Transactions.Bridge.TransactionManagerPropagationService PropagationService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionManagerPropagationService;
            }
        }

        public Microsoft.Transactions.Bridge.TransactionManagerSettings Settings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.TransactionManagerSettings;
            }
        }

        protected Microsoft.Transactions.Bridge.TransactionManagerCoordinatorService TransactionManagerCoordinatorService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionManagerCoordinatorService;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.transactionManagerCoordinatorService = value;
            }
        }

        protected Microsoft.Transactions.Bridge.TransactionManagerPropagationService TransactionManagerPropagationService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionManagerPropagationService;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.transactionManagerPropagationService = value;
            }
        }

        protected Microsoft.Transactions.Bridge.TransactionManagerSettings TransactionManagerSettings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactionManagerSettings;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.transactionManagerSettings = value;
            }
        }
    }
}

