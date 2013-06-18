namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class ReferenceCountedChannel<TChannel> : ReferenceCountedObject where TChannel: class
    {
        private TChannel channel;
        private ChannelMruCacheKey key;
        private Proxy<TChannel> proxy;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private ReferenceCountedChannel(Proxy<TChannel> proxy, TChannel channel, ChannelMruCacheKey key)
        {
            this.proxy = proxy;
            this.channel = channel;
            this.key = key;
        }

        protected override void Close()
        {
            IChannel state = (IChannel) this.channel;
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Closing {0}", state.GetType().Name);
            }
            try
            {
                IAsyncResult result = state.BeginClose(Fx.ThunkCallback(new AsyncCallback(this.OnCloseComplete)), state);
                if (result.CompletedSynchronously)
                {
                    this.CloseComplete(result);
                }
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                DebugTrace.Trace(TraceLevel.Warning, "Exception {0} closing a proxy: {1}", exception.GetType().Name, exception.Message);
            }
            catch (TimeoutException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                DebugTrace.Trace(TraceLevel.Warning, "Exception {0} closing a proxy: {1}", exception2.GetType().Name, exception2.Message);
            }
            catch (Exception exception3)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} closing a proxy: {1}", exception3.GetType().Name, exception3);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception3);
            }
        }

        private void CloseComplete(IAsyncResult result)
        {
            try
            {
                ((IChannel) result.AsyncState).EndClose(result);
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Closed {0}", base.GetType().Name);
                }
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                DebugTrace.Trace(TraceLevel.Warning, "Exception {0} closing a proxy: {1}", exception.GetType().Name, exception.Message);
            }
            catch (TimeoutException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                DebugTrace.Trace(TraceLevel.Warning, "Exception {0} closing a proxy: {1}", exception2.GetType().Name, exception2.Message);
            }
            catch (Exception exception3)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} closing a proxy: {1}", exception3.GetType().Name, exception3);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception3);
            }
        }

        public static ReferenceCountedChannel<TChannel> GetChannel(Proxy<TChannel> proxy)
        {
            ReferenceCountedChannel<TChannel> channel;
            EndpointAddress to = proxy.To;
            ChannelMruCacheKey key = new ChannelMruCacheKey(to.Uri.AbsoluteUri, to.Identity);
            ChannelMruCache<TChannel> channelCache = proxy.ChannelCache;
            lock (channelCache)
            {
                if (channelCache.TryGetValue(key, out channel))
                {
                    channel.AddRef();
                    return channel;
                }
            }
            TChannel local = proxy.CreateChannel(new EndpointAddress(to.Uri, to.Identity, new AddressHeader[0]));
            lock (channelCache)
            {
                if (!channelCache.TryGetValue(key, out channel))
                {
                    channel = new ReferenceCountedChannel<TChannel>(proxy, local, key);
                    channelCache.Add(key, channel);
                }
                else
                {
                    ((IChannel) local).Close();
                }
                channel.AddRef();
            }
            return channel;
        }

        public void OnChannelFailure()
        {
            ChannelMruCache<TChannel> channelCache = this.proxy.ChannelCache;
            lock (channelCache)
            {
                channelCache.Remove(this.key);
            }
        }

        private void OnCloseComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                this.CloseComplete(result);
            }
        }

        public TChannel Channel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.channel;
            }
        }
    }
}

