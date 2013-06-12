namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    internal class ComRedirectionProxy : MarshalByRefObject, IMessageSink
    {
        private MarshalByRefObject _comObject;
        private Type _serverType;

        internal ComRedirectionProxy(MarshalByRefObject comObject, Type serverType)
        {
            this._comObject = comObject;
            this._serverType = serverType;
        }

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            IMessage message = null;
            message = this.SyncProcessMessage(msg);
            if (replySink != null)
            {
                replySink.SyncProcessMessage(message);
            }
            return null;
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage msg)
        {
            IMethodCallMessage reqMsg = (IMethodCallMessage) msg;
            IMethodReturnMessage message2 = null;
            message2 = RemotingServices.ExecuteMessage(this._comObject, reqMsg);
            if (message2 == null)
            {
                return message2;
            }
            COMException exception = message2.Exception as COMException;
            if ((exception == null) || ((exception._HResult != -2147023174) && (exception._HResult != -2147023169)))
            {
                return message2;
            }
            this._comObject = (MarshalByRefObject) Activator.CreateInstance(this._serverType, true);
            return RemotingServices.ExecuteMessage(this._comObject, reqMsg);
        }

        public IMessageSink NextSink
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }
    }
}

