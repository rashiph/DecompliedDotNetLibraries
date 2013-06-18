namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics.Application;

    internal class ProxyOperationRuntime
    {
        private string action;
        private MethodInfo beginMethod;
        private bool deserializeReply;
        internal static readonly object[] EmptyArray = new object[0];
        private ParameterInfo[] endOutParams;
        private readonly IClientFaultFormatter faultFormatter;
        private readonly IClientMessageFormatter formatter;
        private ParameterInfo[] inParams;
        private readonly bool isInitiating;
        private readonly bool isOneWay;
        private readonly bool isTerminating;
        private readonly string name;
        internal static readonly ParameterInfo[] NoParams = new ParameterInfo[0];
        private ParameterInfo[] outParams;
        private readonly IParameterInspector[] parameterInspectors;
        private readonly ImmutableClientRuntime parent;
        private string replyAction;
        private ParameterInfo returnParam;
        private bool serializeRequest;
        private MethodInfo syncMethod;

        internal ProxyOperationRuntime(ClientOperation operation, ImmutableClientRuntime parent)
        {
            if (operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
            }
            if (parent == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
            }
            this.parent = parent;
            this.formatter = operation.Formatter;
            this.isInitiating = operation.IsInitiating;
            this.isOneWay = operation.IsOneWay;
            this.isTerminating = operation.IsTerminating;
            this.name = operation.Name;
            this.parameterInspectors = EmptyArray<IParameterInspector>.ToArray(operation.ParameterInspectors);
            this.faultFormatter = operation.FaultFormatter;
            this.serializeRequest = operation.SerializeRequest;
            this.deserializeReply = operation.DeserializeReply;
            this.action = operation.Action;
            this.replyAction = operation.ReplyAction;
            this.beginMethod = operation.BeginMethod;
            this.syncMethod = operation.SyncMethod;
            if (this.beginMethod != null)
            {
                this.inParams = ServiceReflector.GetInputParameters(this.beginMethod, true);
                if (this.syncMethod != null)
                {
                    this.outParams = ServiceReflector.GetOutputParameters(this.syncMethod, false);
                }
                else
                {
                    this.outParams = NoParams;
                }
                this.endOutParams = ServiceReflector.GetOutputParameters(operation.EndMethod, true);
                this.returnParam = operation.EndMethod.ReturnParameter;
            }
            else if (this.syncMethod != null)
            {
                this.inParams = ServiceReflector.GetInputParameters(this.syncMethod, false);
                this.outParams = ServiceReflector.GetOutputParameters(this.syncMethod, false);
                this.returnParam = this.syncMethod.ReturnParameter;
            }
            if ((this.formatter == null) && (this.serializeRequest || this.deserializeReply))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ClientRuntimeRequiresFormatter0", new object[] { this.name })));
            }
        }

        internal void AfterReply(ref ProxyRpc rpc)
        {
            if (!this.isOneWay)
            {
                System.ServiceModel.Channels.Message reply = rpc.Reply;
                if (this.deserializeReply)
                {
                    rpc.ReturnValue = this.formatter.DeserializeReply(reply, rpc.OutputParameters);
                }
                else
                {
                    rpc.ReturnValue = reply;
                }
                int parameterInspectorCorrelationOffset = this.parent.ParameterInspectorCorrelationOffset;
                try
                {
                    for (int i = this.parameterInspectors.Length - 1; i >= 0; i--)
                    {
                        this.parameterInspectors[i].AfterCall(this.name, rpc.OutputParameters, rpc.ReturnValue, rpc.Correlation[parameterInspectorCorrelationOffset + i]);
                        if (TD.ClientParameterInspectorAfterCallInvokedIsEnabled())
                        {
                            TD.ClientParameterInspectorAfterCallInvoked(this.parameterInspectors[i].GetType().FullName);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (System.ServiceModel.Dispatcher.ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
                if (this.parent.ValidateMustUnderstand)
                {
                    Collection<MessageHeaderInfo> headersNotUnderstood = reply.Headers.GetHeadersNotUnderstood();
                    if ((headersNotUnderstood != null) && (headersNotUnderstood.Count > 0))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("SFxHeaderNotUnderstood", new object[] { headersNotUnderstood[0].Name, headersNotUnderstood[0].Namespace })));
                    }
                }
            }
        }

        internal void BeforeRequest(ref ProxyRpc rpc)
        {
            int parameterInspectorCorrelationOffset = this.parent.ParameterInspectorCorrelationOffset;
            try
            {
                for (int i = 0; i < this.parameterInspectors.Length; i++)
                {
                    rpc.Correlation[parameterInspectorCorrelationOffset + i] = this.parameterInspectors[i].BeforeCall(this.name, rpc.InputParameters);
                    if (TD.ClientParameterInspectorBeforeCallInvokedIsEnabled())
                    {
                        TD.ClientParameterInspectorBeforeCallInvoked(this.parameterInspectors[i].GetType().FullName);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (System.ServiceModel.Dispatcher.ErrorBehavior.ShouldRethrowClientSideExceptionAsIs(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
            if (this.serializeRequest)
            {
                rpc.Request = this.formatter.SerializeRequest(rpc.MessageVersion, rpc.InputParameters);
            }
            else
            {
                if (rpc.InputParameters[0] == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxProxyRuntimeMessageCannotBeNull", new object[] { this.name })));
                }
                rpc.Request = (System.ServiceModel.Channels.Message) rpc.InputParameters[0];
                if (!IsValidAction(rpc.Request, this.Action))
                {
                    object[] args = new object[] { this.Name, rpc.Request.Headers.Action ?? "{NULL}", this.Action };
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidRequestAction", args)));
                }
            }
        }

        internal static object GetDefaultParameterValue(System.Type parameterType)
        {
            if (parameterType.IsValueType && (parameterType != typeof(void)))
            {
                return Activator.CreateInstance(parameterType);
            }
            return null;
        }

        [SecurityCritical]
        internal bool IsBeginCall(IMethodCallMessage methodCall)
        {
            if (this.beginMethod == null)
            {
                return false;
            }
            return (methodCall.MethodBase.MethodHandle == this.beginMethod.MethodHandle);
        }

        [SecurityCritical]
        internal bool IsSyncCall(IMethodCallMessage methodCall)
        {
            if (this.syncMethod == null)
            {
                return false;
            }
            return (methodCall.MethodBase.MethodHandle == this.syncMethod.MethodHandle);
        }

        internal static bool IsValidAction(System.ServiceModel.Channels.Message message, string action)
        {
            if (message == null)
            {
                return false;
            }
            return (message.IsFault || ((action == "*") || (string.CompareOrdinal(message.Headers.Action, action) == 0)));
        }

        [SecurityCritical]
        internal object[] MapAsyncBeginInputs(IMethodCallMessage methodCall, out AsyncCallback callback, out object asyncState)
        {
            object[] emptyArray;
            if (this.inParams.Length == 0)
            {
                emptyArray = EmptyArray;
            }
            else
            {
                emptyArray = new object[this.inParams.Length];
            }
            object[] args = methodCall.Args;
            for (int i = 0; i < emptyArray.Length; i++)
            {
                emptyArray[i] = args[this.inParams[i].Position];
            }
            callback = args[methodCall.ArgCount - 2] as AsyncCallback;
            asyncState = args[methodCall.ArgCount - 1];
            return emptyArray;
        }

        [SecurityCritical]
        internal void MapAsyncEndInputs(IMethodCallMessage methodCall, out IAsyncResult result, out object[] outs)
        {
            outs = new object[this.endOutParams.Length];
            result = methodCall.Args[methodCall.ArgCount - 1] as IAsyncResult;
        }

        [SecurityCritical]
        internal object[] MapAsyncOutputs(IMethodCallMessage methodCall, object[] outs, ref object ret)
        {
            return this.MapOutputs(this.endOutParams, methodCall, outs, ref ret);
        }

        [SecurityCritical]
        private object[] MapOutputs(ParameterInfo[] parameters, IMethodCallMessage methodCall, object[] outs, ref object ret)
        {
            if ((ret == null) && (this.returnParam != null))
            {
                ret = GetDefaultParameterValue(TypeLoader.GetParameterType(this.returnParam));
            }
            if (parameters.Length == 0)
            {
                return null;
            }
            object[] args = methodCall.Args;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (outs[i] == null)
                {
                    args[parameters[i].Position] = GetDefaultParameterValue(TypeLoader.GetParameterType(parameters[i]));
                }
                else
                {
                    args[parameters[i].Position] = outs[i];
                }
            }
            return args;
        }

        [SecurityCritical]
        internal object[] MapSyncInputs(IMethodCallMessage methodCall, out object[] outs)
        {
            if (this.outParams.Length == 0)
            {
                outs = EmptyArray;
            }
            else
            {
                outs = new object[this.outParams.Length];
            }
            if (this.inParams.Length == 0)
            {
                return EmptyArray;
            }
            return methodCall.InArgs;
        }

        [SecurityCritical]
        internal object[] MapSyncOutputs(IMethodCallMessage methodCall, object[] outs, ref object ret)
        {
            return this.MapOutputs(this.outParams, methodCall, outs, ref ret);
        }

        internal string Action
        {
            get
            {
                return this.action;
            }
        }

        internal bool DeserializeReply
        {
            get
            {
                return this.deserializeReply;
            }
        }

        internal IClientFaultFormatter FaultFormatter
        {
            get
            {
                return this.faultFormatter;
            }
        }

        internal bool IsInitiating
        {
            get
            {
                return this.isInitiating;
            }
        }

        internal bool IsOneWay
        {
            get
            {
                return this.isOneWay;
            }
        }

        internal bool IsTerminating
        {
            get
            {
                return this.isTerminating;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal ImmutableClientRuntime Parent
        {
            get
            {
                return this.parent;
            }
        }

        internal string ReplyAction
        {
            get
            {
                return this.replyAction;
            }
        }

        internal bool SerializeRequest
        {
            get
            {
                return this.serializeRequest;
            }
        }
    }
}

