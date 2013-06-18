namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class Proxy<T> : Proxy where T: class
    {
        private IChannelFactory<T> cf;
        protected bool interoperating;
        private ReferenceCountedChannel<T> referenceCountedChannel;

        protected Proxy(CoordinationService coordination, EndpointAddress to, EndpointAddress from) : base(coordination, to, from)
        {
            this.interoperating = true;
            if (DebugTrace.Verbose)
            {
                EndpointIdentity identity = base.to.Identity;
                DebugTrace.Trace(TraceLevel.Verbose, "Creating {0} for {1} at {2}", base.GetType().Name, (identity == null) ? "host" : identity.ToString(), base.to.Uri);
            }
            this.cf = this.SelectChannelFactory(out this.messageVersion);
            if (this.interoperating && (base.to.Identity != null))
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CreateChannelFailureException(Microsoft.Transactions.SR.GetString("InvalidTrustIdentity")));
            }
            try
            {
                this.referenceCountedChannel = ReferenceCountedChannel<T>.GetChannel((Proxy<T>) this);
            }
            catch (ArgumentException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CreateChannelFailureException(Microsoft.Transactions.SR.GetString("ProxyCreationFailed"), exception));
            }
            catch (CommunicationException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CreateChannelFailureException(Microsoft.Transactions.SR.GetString("ProxyCreationFailed"), exception2));
            }
            catch (Exception exception3)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} creating a proxy: {1}", exception3.GetType().Name, exception3);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception3);
            }
            if (base.coordinationService.Config.OperationTimeout != TimeSpan.Zero)
            {
                IContextChannel channel = (IContextChannel) this.referenceCountedChannel.Channel;
                channel.OperationTimeout = base.coordinationService.Config.OperationTimeout;
            }
        }

        protected override void Close()
        {
            this.referenceCountedChannel.Release();
        }

        public T CreateChannel(EndpointAddress ea)
        {
            return this.cf.CreateChannel(ea);
        }

        protected T GetChannel(Message message)
        {
            if (message != null)
            {
                base.to.ApplyTo(message);
            }
            return this.referenceCountedChannel.Channel;
        }

        protected void OnChannelFailure()
        {
            this.referenceCountedChannel.OnChannelFailure();
        }

        protected abstract IChannelFactory<T> SelectChannelFactory(out MessageVersion MessageVersion);

        public abstract ChannelMruCache<T> ChannelCache { get; }
    }
}

