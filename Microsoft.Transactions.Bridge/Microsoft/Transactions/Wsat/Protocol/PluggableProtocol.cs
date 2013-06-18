namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.Runtime;

    internal abstract class PluggableProtocol : IProtocolProvider
    {
        protected string name;
        protected Guid protocolId;
        private ProtocolProviderState protocolProviderState = ProtocolProviderState.Uninitialized;
        protected ProtocolVersion protocolVersion;
        protected ProtocolState state;

        public PluggableProtocol(Guid protocolId, string name, ProtocolVersion protocolVersion)
        {
            this.protocolId = protocolId;
            this.name = name;
            this.protocolVersion = protocolVersion;
        }

        public abstract byte[] GetProtocolInformation();
        public static Guid Id(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(PluggableProtocol), "ProtocolGuid");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return PluggableProtocol10.ProtocolGuid;

                case ProtocolVersion.Version11:
                    return PluggableProtocol11.ProtocolGuid;
            }
            return Guid.NewGuid();
        }

        public void Initialize(TransactionManager transactionManager)
        {
            DebugTrace.TraceEnter(this, "Initialize");
            try
            {
                this.state = new ProtocolState(transactionManager, this.protocolVersion);
                this.protocolProviderState = ProtocolProviderState.Initialized;
                if (ProtocolInitializedRecord.ShouldTrace)
                {
                    ProtocolInitializedRecord.Trace(this.protocolId, this.name);
                }
            }
            catch (Exception exception)
            {
                DebugTrace.Trace(TraceLevel.Error, "Could not initialize protocol: {0}", exception);
                ProtocolInitializationFailureRecord.TraceAndLog(this.protocolId, this.name, exception);
                throw;
            }
            finally
            {
                DebugTrace.TraceLeave(this, "Initialize");
            }
        }

        public static string Name(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(PluggableProtocol), "Name");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return "WS-AtomicTransaction 1.0";

                case ProtocolVersion.Version11:
                    return "WS-AtomicTransaction 1.1";
            }
            return null;
        }

        public void Start()
        {
            DebugTrace.TraceEnter(this, "Start");
            try
            {
                this.protocolProviderState = ProtocolProviderState.Starting;
                this.state.Start();
                this.protocolProviderState = ProtocolProviderState.Started;
                if (ProtocolStartedRecord.ShouldTrace)
                {
                    ProtocolStartedRecord.Trace(this.protocolId, this.name);
                }
            }
            catch (Exception exception)
            {
                DebugTrace.Trace(TraceLevel.Error, "Could not start protocol: {0}", exception);
                ProtocolStartFailureRecord.TraceAndLog(this.protocolId, this.name, exception);
                throw;
            }
            finally
            {
                DebugTrace.TraceLeave(this, "Start");
            }
        }

        public void Stop()
        {
            DebugTrace.TraceEnter(this, "Stop");
            try
            {
                this.protocolProviderState = ProtocolProviderState.Stopping;
                this.state.Stop();
                this.protocolProviderState = ProtocolProviderState.Stopped;
                ProtocolStoppedRecord.TraceAndLog(this.protocolId, this.name);
            }
            catch (Exception exception)
            {
                DebugTrace.Trace(TraceLevel.Error, "Could not stop protocol: {0}", exception);
                ProtocolStopFailureRecord.TraceAndLog(this.protocolId, this.name, exception);
                throw;
            }
            finally
            {
                DebugTrace.TraceLeave(this, "Stop");
            }
        }

        public IProtocolProviderCoordinatorService CoordinatorService
        {
            get
            {
                return this.state.TransactionManagerReceive;
            }
        }

        public uint MarshalCapabilities
        {
            get
            {
                return 4;
            }
        }

        public IProtocolProviderPropagationService PropagationService
        {
            get
            {
                return this.state.TransactionManagerReceive;
            }
        }

        public abstract Guid ProtocolId { get; }

        public ProtocolProviderState State
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolProviderState;
            }
        }
    }
}

