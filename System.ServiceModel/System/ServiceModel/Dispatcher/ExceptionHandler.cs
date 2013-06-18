namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;

    public abstract class ExceptionHandler
    {
        private static readonly System.ServiceModel.Dispatcher.ExceptionHandler alwaysHandle = new AlwaysHandleExceptionHandler();
        private static System.ServiceModel.Dispatcher.ExceptionHandler transportExceptionHandler = alwaysHandle;

        protected ExceptionHandler()
        {
        }

        public abstract bool HandleException(Exception exception);
        internal static bool HandleTransportExceptionHelper(Exception exception)
        {
            if (exception == null)
            {
                throw Fx.AssertAndThrow("Null exception passed to HandleTransportExceptionHelper.");
            }
            System.ServiceModel.Dispatcher.ExceptionHandler transportExceptionHandler = TransportExceptionHandler;
            if (transportExceptionHandler == null)
            {
                return false;
            }
            try
            {
                if (!transportExceptionHandler.HandleException(exception))
                {
                    return false;
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                if (DiagnosticUtility.ShouldTraceError)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                }
                return false;
            }
            if (DiagnosticUtility.ShouldTraceError)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
            }
            return true;
        }

        public static System.ServiceModel.Dispatcher.ExceptionHandler AlwaysHandle
        {
            get
            {
                return alwaysHandle;
            }
        }

        public static System.ServiceModel.Dispatcher.ExceptionHandler AsynchronousThreadExceptionHandler
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                HandlerWrapper asynchronousThreadExceptionHandler = (HandlerWrapper) Fx.AsynchronousThreadExceptionHandler;
                if (asynchronousThreadExceptionHandler != null)
                {
                    return asynchronousThreadExceptionHandler.Handler;
                }
                return null;
            }
            [SecuritySafeCritical, SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
            set
            {
                Fx.AsynchronousThreadExceptionHandler = (value == null) ? null : new HandlerWrapper(value);
            }
        }

        public static System.ServiceModel.Dispatcher.ExceptionHandler TransportExceptionHandler
        {
            get
            {
                return transportExceptionHandler;
            }
            set
            {
                transportExceptionHandler = value;
            }
        }

        private class AlwaysHandleExceptionHandler : System.ServiceModel.Dispatcher.ExceptionHandler
        {
            public override bool HandleException(Exception exception)
            {
                return true;
            }
        }

        private class HandlerWrapper : Fx.ExceptionHandler
        {
            [SecurityCritical]
            private readonly System.ServiceModel.Dispatcher.ExceptionHandler handler;

            [SecurityCritical]
            public HandlerWrapper(System.ServiceModel.Dispatcher.ExceptionHandler handler)
            {
                this.handler = handler;
            }

            [SecuritySafeCritical]
            public override bool HandleException(Exception exception)
            {
                return this.handler.HandleException(exception);
            }

            public System.ServiceModel.Dispatcher.ExceptionHandler Handler
            {
                [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                get
                {
                    return this.handler;
                }
            }
        }
    }
}

