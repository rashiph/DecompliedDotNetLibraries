namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;

    internal class ErrorMessage : IMethodCallMessage, IMethodMessage, IMessage
    {
        private int m_ArgCount;
        private string m_ArgName = "Unknown";
        private string m_MethodName = "Unknown";
        private object m_MethodSignature;
        private string m_TypeName = "Unknown";
        private string m_URI = "Exception";

        public object GetArg(int argNum)
        {
            return null;
        }

        public string GetArgName(int index)
        {
            return this.m_ArgName;
        }

        public object GetInArg(int argNum)
        {
            return null;
        }

        public string GetInArgName(int index)
        {
            return null;
        }

        public int ArgCount
        {
            get
            {
                return this.m_ArgCount;
            }
        }

        public object[] Args
        {
            get
            {
                return null;
            }
        }

        public bool HasVarArgs
        {
            get
            {
                return false;
            }
        }

        public int InArgCount
        {
            get
            {
                return this.m_ArgCount;
            }
        }

        public object[] InArgs
        {
            get
            {
                return null;
            }
        }

        public System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext
        {
            get
            {
                return null;
            }
        }

        public System.Reflection.MethodBase MethodBase
        {
            get
            {
                return null;
            }
        }

        public string MethodName
        {
            get
            {
                return this.m_MethodName;
            }
        }

        public object MethodSignature
        {
            get
            {
                return this.m_MethodSignature;
            }
        }

        public IDictionary Properties
        {
            get
            {
                return null;
            }
        }

        public string TypeName
        {
            get
            {
                return this.m_TypeName;
            }
        }

        public string Uri
        {
            get
            {
                return this.m_URI;
            }
        }
    }
}

