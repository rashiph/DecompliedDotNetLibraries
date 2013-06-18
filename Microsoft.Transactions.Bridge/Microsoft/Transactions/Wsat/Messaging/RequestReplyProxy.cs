namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class RequestReplyProxy : Proxy<IRequestReplyService>
    {
        protected RequestReplyProxy(CoordinationService coordination, EndpointAddress to) : base(coordination, to, null)
        {
        }

        protected IAsyncResult BeginSendRequest(Message message, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            base.AddRef();
            try
            {
                result = base.GetChannel(message).BeginRequest(message, callback, state);
            }
            catch (TimeoutException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnChannelFailure();
                result = new SendMessageFailureAsyncResult(exception, callback, state);
            }
            catch (QuotaExceededException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                base.OnChannelFailure();
                result = new SendMessageFailureAsyncResult(exception2, callback, state);
            }
            catch (FaultException exception3)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                base.OnChannelFailure();
                result = new SendMessageFailureAsyncResult(exception3, callback, state);
            }
            catch (CommunicationException exception4)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Warning);
                base.OnChannelFailure();
                result = new SendMessageFailureAsyncResult(exception4, callback, state);
            }
            catch (Exception exception5)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} in RequestReplyProxy.BeginSendRequest: {1}", exception5.GetType().Name, exception5);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception5);
            }
            return result;
        }

        protected Message EndSendRequest(IAsyncResult ar, string replyAction)
        {
            Message message;
            try
            {
                SendMessageFailureAsyncResult result = ar as SendMessageFailureAsyncResult;
                if (result != null)
                {
                    result.End();
                    message = null;
                }
                else
                {
                    message = base.GetChannel(null).EndRequest(ar);
                }
            }
            catch (TimeoutException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatSendFailureException(exception));
            }
            catch (QuotaExceededException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatSendFailureException(exception2));
            }
            catch (FaultException exception3)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatFaultException(exception3.CreateMessageFault(), exception3.Action));
            }
            catch (CommunicationException exception4)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Warning);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatSendFailureException(exception4));
            }
            catch (Exception exception5)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} in RequestReplyProxy.ReadReply: {1}", exception5.GetType().Name, exception5);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception5);
            }
            finally
            {
                base.Release();
            }
            try
            {
                this.ValidateReply(message, replyAction);
            }
            catch
            {
                message.Close();
                throw;
            }
            return message;
        }

        public Message SendRequest(Message message, string replyAction)
        {
            Message message2;
            try
            {
                message2 = base.GetChannel(message).SendRequest(message);
            }
            catch (TimeoutException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatSendFailureException(exception));
            }
            catch (QuotaExceededException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatSendFailureException(exception2));
            }
            catch (FaultException exception3)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatFaultException(exception3.CreateMessageFault(), exception3.Action));
            }
            catch (CommunicationException exception4)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Warning);
                base.OnChannelFailure();
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatSendFailureException(exception4));
            }
            catch (Exception exception5)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} in RequestReplyProxy.SendRequest: {1}", exception5.GetType().Name, exception5);
                base.OnChannelFailure();
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception5);
                return null;
            }
            try
            {
                this.ValidateReply(message2, replyAction);
            }
            catch
            {
                message2.Close();
                throw;
            }
            return message2;
        }

        private void ValidateReply(Message reply, string replyAction)
        {
            if (base.interoperating)
            {
                if (!base.coordinationService.Security.CheckIdentity(this, reply))
                {
                    throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatReceiveFailureException(Microsoft.Transactions.SR.GetString("ReplyServerCredentialMismatch")));
                }
                if (!base.coordinationService.GlobalAcl.AccessCheckReply(reply, BindingStrings.InteropBindingQName(base.protocolVersion)))
                {
                    throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatReceiveFailureException(Microsoft.Transactions.SR.GetString("ReplyServerIdentityAccessDenied", new object[] { base.to.Uri })));
                }
            }
            string action = reply.Headers.Action;
            if ((reply.IsFault || (action == base.atomicTransactionStrings.FaultAction)) || (action == base.coordinationStrings.FaultAction))
            {
                MessageFault fault = MessageFault.CreateFault(reply, 0x10000);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatFaultException(fault, action));
            }
            if (action != replyAction)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatReceiveFailureException(Microsoft.Transactions.SR.GetString("InvalidMessageAction", new object[] { action })));
            }
        }

        public override ChannelMruCache<IRequestReplyService> ChannelCache
        {
            get
            {
                return base.coordinationService.RequestReplyChannelCache;
            }
        }
    }
}

