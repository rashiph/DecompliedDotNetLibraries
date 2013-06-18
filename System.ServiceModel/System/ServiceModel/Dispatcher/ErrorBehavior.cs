namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    internal class ErrorBehavior
    {
        private bool debug;
        private IErrorHandler[] handlers;
        private bool isOnServer;
        private MessageVersion messageVersion;

        internal ErrorBehavior(ChannelDispatcher channelDispatcher)
        {
            this.handlers = EmptyArray<IErrorHandler>.ToArray(channelDispatcher.ErrorHandlers);
            this.debug = channelDispatcher.IncludeExceptionDetailInFaults;
            this.isOnServer = channelDispatcher.IsOnServer;
            this.messageVersion = channelDispatcher.MessageVersion;
        }

        internal bool HandleError(Exception error)
        {
            ErrorHandlerFaultInfo faultInfo = new ErrorHandlerFaultInfo(this.messageVersion.Addressing.DefaultFaultAction);
            return this.HandleError(error, ref faultInfo);
        }

        internal void HandleError(ref MessageRpc rpc)
        {
            if (rpc.Error != null)
            {
                this.HandleErrorCore(ref rpc);
            }
        }

        internal bool HandleError(Exception error, ref ErrorHandlerFaultInfo faultInfo)
        {
            return this.HandleErrorCommon(error, ref faultInfo);
        }

        private bool HandleErrorCommon(Exception error, ref ErrorHandlerFaultInfo faultInfo)
        {
            bool flag;
            if ((faultInfo.Fault != null) && !faultInfo.IsConsideredUnhandled)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }
            try
            {
                if (TD.ServiceExceptionIsEnabled())
                {
                    TD.ServiceException(error.ToString(), error.GetType().FullName);
                }
                for (int i = 0; i < this.handlers.Length; i++)
                {
                    bool handled = this.handlers[i].HandleError(error);
                    flag = handled || flag;
                    if (TD.ErrorHandlerInvokedIsEnabled())
                    {
                        TD.ErrorHandlerInvoked(this.handlers[i].GetType().FullName, handled, error.GetType().FullName);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
            return flag;
        }

        private void HandleErrorCore(ref MessageRpc rpc)
        {
            if (this.HandleErrorCommon(rpc.Error, ref rpc.FaultInfo))
            {
                rpc.Error = null;
            }
        }

        private void InitializeFault(ref MessageRpc rpc)
        {
            FaultException error = rpc.Error as FaultException;
            if (error != null)
            {
                string defaultFaultAction;
                MessageFault fault = rpc.Operation.FaultFormatter.Serialize(error, out defaultFaultAction);
                if (defaultFaultAction == null)
                {
                    defaultFaultAction = rpc.RequestVersion.Addressing.DefaultFaultAction;
                }
                if (fault != null)
                {
                    rpc.FaultInfo.Fault = Message.CreateMessage(rpc.RequestVersion, fault, defaultFaultAction);
                }
            }
        }

        internal void ProvideFault(Exception e, FaultConverter faultConverter, ref ErrorHandlerFaultInfo faultInfo)
        {
            this.ProvideWellKnownFault(e, faultConverter, ref faultInfo);
            for (int i = 0; i < this.handlers.Length; i++)
            {
                Message fault = faultInfo.Fault;
                this.handlers[i].ProvideFault(e, this.messageVersion, ref fault);
                faultInfo.Fault = fault;
                if (TD.FaultProviderInvokedIsEnabled())
                {
                    TD.FaultProviderInvoked(this.handlers[i].GetType().FullName, e.Message);
                }
            }
            this.ProvideFaultOfLastResort(e, ref faultInfo);
        }

        private void ProvideFaultOfLastResort(Exception error, ref ErrorHandlerFaultInfo faultInfo)
        {
            if (faultInfo.Fault == null)
            {
                MessageFault fault;
                FaultCode subCode = new FaultCode("InternalServiceFault", "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher");
                subCode = FaultCode.CreateReceiverFaultCode(subCode);
                string action = "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault";
                if (this.debug)
                {
                    faultInfo.DefaultFaultAction = action;
                    fault = MessageFault.CreateFault(subCode, new FaultReason(error.Message), new ExceptionDetail(error));
                }
                else
                {
                    string text = this.isOnServer ? System.ServiceModel.SR.GetString("SFxInternalServerError") : System.ServiceModel.SR.GetString("SFxInternalCallbackError");
                    fault = MessageFault.CreateFault(subCode, new FaultReason(text));
                }
                faultInfo.IsConsideredUnhandled = true;
                faultInfo.Fault = Message.CreateMessage(this.messageVersion, fault, action);
            }
            else if (error != null)
            {
                FaultException exception = error as FaultException;
                if ((((exception != null) && (exception.Fault != null)) && ((exception.Fault.Code != null) && (exception.Fault.Code.SubCode != null))) && ((string.Compare(exception.Fault.Code.SubCode.Namespace, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher", StringComparison.Ordinal) == 0) && (string.Compare(exception.Fault.Code.SubCode.Name, "InternalServiceFault", StringComparison.Ordinal) == 0)))
                {
                    faultInfo.IsConsideredUnhandled = true;
                }
            }
        }

        internal void ProvideMessageFault(ref MessageRpc rpc)
        {
            if (rpc.Error != null)
            {
                this.ProvideMessageFaultCore(ref rpc);
            }
        }

        private void ProvideMessageFaultCore(ref MessageRpc rpc)
        {
            MessageVersion requestVersion = rpc.RequestVersion;
            MessageVersion messageVersion = this.messageVersion;
            this.InitializeFault(ref rpc);
            this.ProvideFault(rpc.Error, rpc.Channel.GetProperty<FaultConverter>(), ref rpc.FaultInfo);
            this.ProvideMessageFaultCoreCoda(ref rpc);
        }

        private void ProvideMessageFaultCoreCoda(ref MessageRpc rpc)
        {
            if (rpc.FaultInfo.Fault.Headers.Action == null)
            {
                rpc.FaultInfo.Fault.Headers.Action = rpc.RequestVersion.Addressing.DefaultFaultAction;
            }
            rpc.Reply = rpc.FaultInfo.Fault;
        }

        internal void ProvideOnlyFaultOfLastResort(ref MessageRpc rpc)
        {
            this.ProvideFaultOfLastResort(rpc.Error, ref rpc.FaultInfo);
            this.ProvideMessageFaultCoreCoda(ref rpc);
        }

        private void ProvideWellKnownFault(Exception e, FaultConverter faultConverter, ref ErrorHandlerFaultInfo faultInfo)
        {
            Message message;
            if ((faultConverter != null) && faultConverter.TryCreateFaultMessage(e, out message))
            {
                faultInfo.Fault = message;
            }
            else if (e is NetDispatcherFaultException)
            {
                NetDispatcherFaultException exception = e as NetDispatcherFaultException;
                if (this.debug)
                {
                    ExceptionDetail detail = new ExceptionDetail(exception);
                    faultInfo.Fault = Message.CreateMessage(this.messageVersion, MessageFault.CreateFault(exception.Code, exception.Reason, detail), exception.Action);
                }
                else
                {
                    faultInfo.Fault = Message.CreateMessage(this.messageVersion, exception.CreateMessageFault(), exception.Action);
                }
            }
        }

        internal static bool ShouldRethrowClientSideExceptionAsIs(Exception e)
        {
            return true;
        }

        internal static bool ShouldRethrowExceptionAsIs(Exception e)
        {
            return true;
        }

        internal static void ThrowAndCatch(Exception e)
        {
            ThrowAndCatch(e, null);
        }

        internal static void ThrowAndCatch(Exception e, Message message)
        {
            try
            {
                if (Debugger.IsAttached)
                {
                    if (message == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
                    }
                    throw TraceUtility.ThrowHelperError(e, message);
                }
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
                }
                TraceUtility.ThrowHelperError(e, message);
            }
            catch (Exception exception)
            {
                if (!object.ReferenceEquals(e, exception))
                {
                    throw;
                }
            }
        }

        internal IErrorHandler[] Handlers
        {
            get
            {
                return this.handlers;
            }
        }
    }
}

