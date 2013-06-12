namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Serialization;
    using System.Security;

    internal class MessageSurrogate : ISerializationSurrogate
    {
        private static Type _constructionCallType = typeof(ConstructionCall);
        private static Type _constructionResponseType = typeof(ConstructionResponse);
        private static Type _exceptionType = typeof(Exception);
        private static Type _methodCallType = typeof(MethodCall);
        private static Type _methodResponseType = typeof(MethodResponse);
        private static Type _objectType = typeof(object);
        [SecurityCritical]
        private RemotingSurrogateSelector _ss;

        [SecurityCritical]
        internal MessageSurrogate(RemotingSurrogateSelector ss)
        {
            this._ss = ss;
        }

        [SecurityCritical]
        public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            bool flag = false;
            bool flag2 = false;
            IMethodMessage msg = obj as IMethodMessage;
            if (msg == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_InvalidMsg"));
            }
            IDictionaryEnumerator enumerator = msg.Properties.GetEnumerator();
            if (msg is IMethodCallMessage)
            {
                if (obj is IConstructionCallMessage)
                {
                    flag2 = true;
                }
                info.SetType(flag2 ? _constructionCallType : _methodCallType);
            }
            else
            {
                if (!(msg is IMethodReturnMessage))
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_InvalidMsg"));
                }
                flag = true;
                info.SetType((obj is IConstructionReturnMessage) ? _constructionResponseType : _methodResponseType);
                if (((IMethodReturnMessage) msg).Exception != null)
                {
                    info.AddValue("__fault", ((IMethodReturnMessage) msg).Exception, _exceptionType);
                }
            }
            while (enumerator.MoveNext())
            {
                if (((obj != this._ss.GetRootObject()) || (this._ss.Filter == null)) || !this._ss.Filter((string) enumerator.Key, enumerator.Value))
                {
                    if (enumerator.Value != null)
                    {
                        string name = enumerator.Key.ToString();
                        if (name.Equals("__CallContext"))
                        {
                            LogicalCallContext context2 = (LogicalCallContext) enumerator.Value;
                            if (context2.HasInfo)
                            {
                                info.AddValue(name, context2);
                            }
                            else
                            {
                                info.AddValue(name, context2.RemotingData.LogicalCallID);
                            }
                        }
                        else if (name.Equals("__MethodSignature"))
                        {
                            if (flag2 || RemotingServices.IsMethodOverloaded(msg))
                            {
                                info.AddValue(name, enumerator.Value);
                            }
                        }
                        else
                        {
                            info.AddValue(name, enumerator.Value);
                        }
                    }
                    else
                    {
                        info.AddValue(enumerator.Key.ToString(), enumerator.Value, _objectType);
                    }
                }
            }
        }

        [SecurityCritical]
        public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_PopulateData"));
        }
    }
}

