namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal abstract class DatagramProxy : Proxy<IDatagramService>
    {
        private static EndpointAddress noneAddress = new EndpointAddress(EndpointAddress.NoneUri, new AddressHeader[0]);

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected DatagramProxy(CoordinationService coordination, EndpointAddress to, EndpointAddress from) : base(coordination, to, from)
        {
        }

        public IAsyncResult BeginSendFault(UniqueId messageID, Fault fault, AsyncCallback callback, object state)
        {
            Message message = Library.CreateFaultMessage(messageID, base.messageVersion, fault);
            return this.BeginSendMessage(message, callback, state);
        }

        public IAsyncResult BeginSendMessage(Message message, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            if (base.from != null)
            {
                MessagingVersionHelper.SetReplyAddress(message, base.from, base.protocolVersion);
            }
            if (base.protocolVersion == ProtocolVersion.Version11)
            {
                message.Headers.ReplyTo = noneAddress;
            }
            base.AddRef();
            try
            {
                result = base.GetChannel(message).BeginSend(message, callback, state);
            }
            catch (TimeoutException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                result = new SendMessageFailureAsyncResult(exception, callback, state);
            }
            catch (QuotaExceededException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                result = new SendMessageFailureAsyncResult(exception2, callback, state);
            }
            catch (CommunicationException exception3)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                result = new SendMessageFailureAsyncResult(exception3, callback, state);
            }
            catch (Exception exception4)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} in DatagramProxy.BeginSendMessage: {1}", exception4.GetType().Name, exception4);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception4);
            }
            return result;
        }

        public void EndSendMessage(IAsyncResult ar)
        {
            try
            {
                SendMessageFailureAsyncResult result = ar as SendMessageFailureAsyncResult;
                if (result != null)
                {
                    result.End();
                }
                else
                {
                    base.GetChannel(null).EndSend(ar);
                }
            }
            catch (TimeoutException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatSendFailureException(exception));
            }
            catch (QuotaExceededException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatSendFailureException(exception2));
            }
            catch (CommunicationException exception3)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatSendFailureException(exception3));
            }
            catch (Exception exception4)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} in DatagramProxy.EndSendMessage: {1}", exception4.GetType().Name, exception4);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception4);
            }
            finally
            {
                base.Release();
            }
        }

        protected override IChannelFactory<IDatagramService> SelectChannelFactory(out MessageVersion messageVersion)
        {
            messageVersion = base.coordinationService.InteropDatagramBinding.MessageVersion;
            return base.coordinationService.InteropDatagramChannelFactory;
        }

        public override ChannelMruCache<IDatagramService> ChannelCache
        {
            get
            {
                return base.coordinationService.DatagramChannelCache;
            }
        }
    }
}

