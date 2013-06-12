namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Principal;
    using System.Threading;

    internal class CrossAppDomainSink : InternalSink, IMessageSink
    {
        internal static int[] _sinkKeys;
        internal static CrossAppDomainSink[] _sinks;
        internal CrossAppDomainData _xadData;
        internal const int GROW_BY = 8;
        internal const string LCC_DATA_KEY = "__xADCall";
        private static InternalCrossContextDelegate s_xctxDel = new InternalCrossContextDelegate(CrossAppDomainSink.DoTransitionDispatchCallback);
        private static object staticSyncObject = new object();

        internal CrossAppDomainSink(CrossAppDomainData xadData)
        {
            this._xadData = xadData;
        }

        [SecurityCritical]
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
        {
            ADAsyncWorkItem item = new ADAsyncWorkItem(reqMsg, this, replySink);
            WaitCallback callBack = new WaitCallback(item.FinishAsyncWork);
            ThreadPool.QueueUserWorkItem(callBack);
            return null;
        }

        [SecurityCritical]
        internal static byte[] DoDispatch(byte[] reqStmBuff, SmuggledMethodCallMessage smuggledMcm, out SmuggledMethodReturnMessage smuggledMrm)
        {
            IMessage msg = null;
            if (smuggledMcm != null)
            {
                ArrayList deserializedArgs = smuggledMcm.FixupForNewAppDomain();
                msg = new MethodCall(smuggledMcm, deserializedArgs);
            }
            else
            {
                MemoryStream stm = new MemoryStream(reqStmBuff);
                msg = CrossAppDomainSerializer.DeserializeMessage(stm);
            }
            LogicalCallContext logicalCallContext = CallContext.GetLogicalCallContext();
            logicalCallContext.SetData("__xADCall", true);
            IMessage message2 = ChannelServices.SyncDispatchMessage(msg);
            logicalCallContext.FreeNamedDataSlot("__xADCall");
            smuggledMrm = SmuggledMethodReturnMessage.SmuggleIfPossible(message2);
            if (smuggledMrm != null)
            {
                return null;
            }
            if (message2 == null)
            {
                return null;
            }
            LogicalCallContext context2 = (LogicalCallContext) message2.Properties[Message.CallContextKey];
            if ((context2 != null) && (context2.Principal != null))
            {
                context2.Principal = null;
            }
            return CrossAppDomainSerializer.SerializeMessage(message2).GetBuffer();
        }

        internal static void DomainUnloaded(int domainID)
        {
            int num = domainID;
            lock (staticSyncObject)
            {
                if (_sinks != null)
                {
                    int index = 0;
                    int num3 = -1;
                    while (_sinks[index] != null)
                    {
                        if (_sinkKeys[index] == num)
                        {
                            num3 = index;
                        }
                        index++;
                        if (index == _sinks.Length)
                        {
                            break;
                        }
                    }
                    if (num3 != -1)
                    {
                        _sinkKeys[num3] = _sinkKeys[index - 1];
                        _sinks[num3] = _sinks[index - 1];
                        _sinkKeys[index - 1] = 0;
                        _sinks[index - 1] = null;
                    }
                }
            }
        }

        [SecurityCritical]
        internal byte[] DoTransitionDispatch(byte[] reqStmBuff, SmuggledMethodCallMessage smuggledMcm, out SmuggledMethodReturnMessage smuggledMrm)
        {
            byte[] buffer = null;
            object[] objArray2 = new object[3];
            objArray2[0] = reqStmBuff;
            objArray2[1] = smuggledMcm;
            object[] args = objArray2;
            buffer = (byte[]) Thread.CurrentThread.InternalCrossContextCallback(null, this._xadData.ContextID, this._xadData.DomainID, s_xctxDel, args);
            smuggledMrm = (SmuggledMethodReturnMessage) args[2];
            return buffer;
        }

        [SecurityCritical]
        internal static object DoTransitionDispatchCallback(object[] args)
        {
            byte[] reqStmBuff = (byte[]) args[0];
            SmuggledMethodCallMessage smuggledMcm = (SmuggledMethodCallMessage) args[1];
            SmuggledMethodReturnMessage smuggledMrm = null;
            byte[] buffer = null;
            try
            {
                buffer = DoDispatch(reqStmBuff, smuggledMcm, out smuggledMrm);
            }
            catch (Exception exception)
            {
                IMessage msg = new ReturnMessage(exception, new ErrorMessage());
                buffer = CrossAppDomainSerializer.SerializeMessage(msg).GetBuffer();
                msg = null;
            }
            args[2] = smuggledMrm;
            return buffer;
        }

        internal static CrossAppDomainSink FindOrCreateSink(CrossAppDomainData xadData)
        {
            lock (staticSyncObject)
            {
                int domainID = xadData.DomainID;
                if (_sinks == null)
                {
                    GrowArrays(0);
                }
                int index = 0;
                while (_sinks[index] != null)
                {
                    if (_sinkKeys[index] == domainID)
                    {
                        return _sinks[index];
                    }
                    index++;
                    if (index == _sinks.Length)
                    {
                        GrowArrays(index);
                        break;
                    }
                }
                _sinks[index] = new CrossAppDomainSink(xadData);
                _sinkKeys[index] = domainID;
                return _sinks[index];
            }
        }

        internal static void GrowArrays(int oldSize)
        {
            if (_sinks == null)
            {
                _sinks = new CrossAppDomainSink[8];
                _sinkKeys = new int[8];
            }
            else
            {
                CrossAppDomainSink[] destinationArray = new CrossAppDomainSink[_sinks.Length + 8];
                int[] numArray = new int[_sinkKeys.Length + 8];
                Array.Copy(_sinks, destinationArray, _sinks.Length);
                Array.Copy(_sinkKeys, numArray, _sinkKeys.Length);
                _sinks = destinationArray;
                _sinkKeys = numArray;
            }
        }

        [SecurityCritical]
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {
            IMessage message = InternalSink.ValidateMessage(reqMsg);
            if (message != null)
            {
                return message;
            }
            IPrincipal principal = null;
            IMessage message2 = null;
            try
            {
                SmuggledMethodReturnMessage message5;
                IMethodCallMessage message3 = reqMsg as IMethodCallMessage;
                if (message3 != null)
                {
                    LogicalCallContext logicalCallContext = message3.LogicalCallContext;
                    if (logicalCallContext != null)
                    {
                        principal = logicalCallContext.RemovePrincipalIfNotSerializable();
                    }
                }
                MemoryStream stream = null;
                SmuggledMethodCallMessage smuggledMcm = SmuggledMethodCallMessage.SmuggleIfPossible(reqMsg);
                if (smuggledMcm == null)
                {
                    stream = CrossAppDomainSerializer.SerializeMessage(reqMsg);
                }
                LogicalCallContext callCtx = CallContext.SetLogicalCallContext(null);
                MemoryStream stm = null;
                byte[] buffer = null;
                try
                {
                    if (smuggledMcm != null)
                    {
                        buffer = this.DoTransitionDispatch(null, smuggledMcm, out message5);
                    }
                    else
                    {
                        buffer = this.DoTransitionDispatch(stream.GetBuffer(), null, out message5);
                    }
                }
                finally
                {
                    CallContext.SetLogicalCallContext(callCtx);
                }
                if (message5 != null)
                {
                    ArrayList deserializedArgs = message5.FixupForNewAppDomain();
                    message2 = new MethodResponse((IMethodCallMessage) reqMsg, message5, deserializedArgs);
                }
                else if (buffer != null)
                {
                    stm = new MemoryStream(buffer);
                    message2 = CrossAppDomainSerializer.DeserializeMessage(stm, reqMsg as IMethodCallMessage);
                }
            }
            catch (Exception exception)
            {
                try
                {
                    message2 = new ReturnMessage(exception, reqMsg as IMethodCallMessage);
                }
                catch (Exception)
                {
                }
            }
            if (principal != null)
            {
                IMethodReturnMessage message6 = message2 as IMethodReturnMessage;
                if (message6 != null)
                {
                    message6.LogicalCallContext.Principal = principal;
                }
            }
            return message2;
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

