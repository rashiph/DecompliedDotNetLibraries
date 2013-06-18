namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class ReplyChannelWrapper : ChannelWrapper<IReplyChannel, RequestContext>, IReplyChannel, IChannel, ICommunicationObject
    {
        public ReplyChannelWrapper(ChannelManagerBase channelManager, IReplyChannel innerChannel, RequestContext firstRequest) : base(channelManager, innerChannel, firstRequest)
        {
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            RequestContext firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return new ChannelWrapper<IReplyChannel, RequestContext>.ReceiveAsyncResult(firstItem, callback, state);
            }
            return base.InnerChannel.BeginReceiveRequest(callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            RequestContext firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return new ChannelWrapper<IReplyChannel, RequestContext>.ReceiveAsyncResult(firstItem, callback, state);
            }
            return base.InnerChannel.BeginReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            RequestContext firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return new ChannelWrapper<IReplyChannel, RequestContext>.ReceiveAsyncResult(firstItem, callback, state);
            }
            return base.InnerChannel.BeginTryReceiveRequest(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (base.HaveFirstItem())
            {
                return new ChannelWrapper<IReplyChannel, RequestContext>.WaitAsyncResult(callback, state);
            }
            return base.InnerChannel.BeginWaitForRequest(timeout, callback, state);
        }

        protected override void CloseFirstItem(TimeSpan timeout)
        {
            RequestContext firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                try
                {
                    firstItem.RequestMessage.Close();
                    firstItem.Close(timeout);
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                catch (TimeoutException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
            }
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            if (result is ChannelWrapper<IReplyChannel, RequestContext>.ReceiveAsyncResult)
            {
                return ChannelWrapper<IReplyChannel, RequestContext>.ReceiveAsyncResult.End(result);
            }
            return base.InnerChannel.EndReceiveRequest(result);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext request)
        {
            if (result is ChannelWrapper<IReplyChannel, RequestContext>.ReceiveAsyncResult)
            {
                request = ChannelWrapper<IReplyChannel, RequestContext>.ReceiveAsyncResult.End(result);
                return true;
            }
            return base.InnerChannel.EndTryReceiveRequest(result, out request);
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            if (result is ChannelWrapper<IReplyChannel, RequestContext>.WaitAsyncResult)
            {
                return ChannelWrapper<IReplyChannel, RequestContext>.WaitAsyncResult.End(result);
            }
            return base.InnerChannel.EndWaitForRequest(result);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        public RequestContext ReceiveRequest()
        {
            RequestContext firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return firstItem;
            }
            return base.InnerChannel.ReceiveRequest();
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            RequestContext firstItem = base.GetFirstItem();
            if (firstItem != null)
            {
                return firstItem;
            }
            return base.InnerChannel.ReceiveRequest(timeout);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext request)
        {
            request = base.GetFirstItem();
            return ((request != null) || base.InnerChannel.TryReceiveRequest(timeout, out request));
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            return (base.HaveFirstItem() || base.InnerChannel.WaitForRequest(timeout));
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return base.InnerChannel.LocalAddress;
            }
        }
    }
}

