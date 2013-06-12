namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security;

    internal class ErrorMessage : IMethodCallMessage, IMethodMessage, IMessage
    {
        private int m_ArgCount;
        private string m_ArgName = "Unknown";
        private string m_MethodName = "Unknown";
        private object m_MethodSignature;
        private string m_TypeName = "Unknown";
        private string m_URI = "Exception";

        [SecurityCritical]
        public object GetArg(int argNum)
        {
            return null;
        }

        [SecurityCritical]
        public string GetArgName(int index)
        {
            return this.m_ArgName;
        }

        [SecurityCritical]
        public object GetInArg(int argNum)
        {
            return null;
        }

        [SecurityCritical]
        public string GetInArgName(int index)
        {
            return null;
        }

        public int ArgCount
        {
            [SecurityCritical]
            get
            {
                return this.m_ArgCount;
            }
        }

        public object[] Args
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }

        public bool HasVarArgs
        {
            [SecurityCritical]
            get
            {
                return false;
            }
        }

        public int InArgCount
        {
            [SecurityCritical]
            get
            {
                return this.m_ArgCount;
            }
        }

        public object[] InArgs
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }

        public System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }

        public System.Reflection.MethodBase MethodBase
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }

        public string MethodName
        {
            [SecurityCritical]
            get
            {
                return this.m_MethodName;
            }
        }

        public object MethodSignature
        {
            [SecurityCritical]
            get
            {
                return this.m_MethodSignature;
            }
        }

        public IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                return null;
            }
        }

        public string TypeName
        {
            [SecurityCritical]
            get
            {
                return this.m_TypeName;
            }
        }

        public string Uri
        {
            [SecurityCritical]
            get
            {
                return this.m_URI;
            }
        }
    }
}

