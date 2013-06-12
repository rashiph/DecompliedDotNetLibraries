namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.Threading;

    [Serializable, SecurityCritical, ComVisible(true)]
    public sealed class CallContext
    {
        private CallContext()
        {
        }

        [SecurityCritical]
        public static void FreeNamedDataSlot(string name)
        {
            Thread.CurrentThread.GetLogicalCallContext().FreeNamedDataSlot(name);
            Thread.CurrentThread.GetIllogicalCallContext().FreeNamedDataSlot(name);
        }

        [SecurityCritical]
        public static object GetData(string name)
        {
            object obj2 = LogicalGetData(name);
            if (obj2 == null)
            {
                return IllogicalGetData(name);
            }
            return obj2;
        }

        [SecurityCritical]
        public static Header[] GetHeaders()
        {
            return Thread.CurrentThread.GetLogicalCallContext().InternalGetHeaders();
        }

        internal static LogicalCallContext GetLogicalCallContext()
        {
            return Thread.CurrentThread.GetLogicalCallContext();
        }

        private static object IllogicalGetData(string name)
        {
            return Thread.CurrentThread.GetIllogicalCallContext().GetData(name);
        }

        [SecurityCritical]
        public static object LogicalGetData(string name)
        {
            return Thread.CurrentThread.GetLogicalCallContext().GetData(name);
        }

        [SecurityCritical]
        public static void LogicalSetData(string name, object data)
        {
            Thread.CurrentThread.GetIllogicalCallContext().FreeNamedDataSlot(name);
            Thread.CurrentThread.GetLogicalCallContext().SetData(name, data);
        }

        [SecurityCritical]
        public static void SetData(string name, object data)
        {
            if (data is ILogicalThreadAffinative)
            {
                LogicalSetData(name, data);
            }
            else
            {
                Thread.CurrentThread.GetLogicalCallContext().FreeNamedDataSlot(name);
                Thread.CurrentThread.GetIllogicalCallContext().SetData(name, data);
            }
        }

        [SecurityCritical]
        public static void SetHeaders(Header[] headers)
        {
            Thread.CurrentThread.GetLogicalCallContext().InternalSetHeaders(headers);
        }

        internal static LogicalCallContext SetLogicalCallContext(LogicalCallContext callCtx)
        {
            return Thread.CurrentThread.SetLogicalCallContext(callCtx);
        }

        internal static LogicalCallContext SetLogicalCallContext(Thread currThread, LogicalCallContext callCtx)
        {
            return currThread.SetLogicalCallContext(callCtx);
        }

        public static object HostContext
        {
            [SecurityCritical]
            get
            {
                object hostContext = Thread.CurrentThread.GetIllogicalCallContext().HostContext;
                if (hostContext == null)
                {
                    hostContext = GetLogicalCallContext().HostContext;
                }
                return hostContext;
            }
            [SecurityCritical]
            set
            {
                if (value is ILogicalThreadAffinative)
                {
                    Thread.CurrentThread.GetIllogicalCallContext().HostContext = null;
                    GetLogicalCallContext().HostContext = value;
                }
                else
                {
                    GetLogicalCallContext().HostContext = null;
                    Thread.CurrentThread.GetIllogicalCallContext().HostContext = value;
                }
            }
        }

        internal static IPrincipal Principal
        {
            [SecurityCritical]
            get
            {
                return GetLogicalCallContext().Principal;
            }
            [SecurityCritical]
            set
            {
                GetLogicalCallContext().Principal = value;
            }
        }

        internal static CallContextRemotingData RemotingData
        {
            [SecurityCritical]
            get
            {
                return Thread.CurrentThread.GetLogicalCallContext().RemotingData;
            }
        }

        internal static CallContextSecurityData SecurityData
        {
            [SecurityCritical]
            get
            {
                return Thread.CurrentThread.GetLogicalCallContext().SecurityData;
            }
        }
    }
}

