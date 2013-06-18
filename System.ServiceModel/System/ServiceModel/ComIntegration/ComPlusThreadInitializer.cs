namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Transactions;

    internal class ComPlusThreadInitializer : ICallContextInitializer
    {
        private ComPlusAuthorization comAuth;
        private Guid iid;
        private ServiceInfo info;

        public ComPlusThreadInitializer(ContractDescription contract, DispatchOperation operation, ServiceInfo info)
        {
            this.info = info;
            this.iid = contract.ContractType.GUID;
            if (info.CheckRoles)
            {
                string[] serviceRoleMembers = null;
                string[] contractRoleMembers = null;
                string[] operationRoleMembers = null;
                serviceRoleMembers = info.ComponentRoleMembers;
                foreach (ContractInfo info2 in this.info.Contracts)
                {
                    if (!(info2.IID == this.iid))
                    {
                        continue;
                    }
                    contractRoleMembers = info2.InterfaceRoleMembers;
                    foreach (System.ServiceModel.ComIntegration.OperationInfo info3 in info2.Operations)
                    {
                        if (info3.Name == operation.Name)
                        {
                            operationRoleMembers = info3.MethodRoleMembers;
                            break;
                        }
                    }
                    if (operationRoleMembers == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("ComOperationNotFound", new object[] { contract.Name, operation.Name })));
                    }
                    break;
                }
                this.comAuth = new ComPlusAuthorization(serviceRoleMembers, contractRoleMembers, operationRoleMembers);
            }
        }

        public void AfterInvoke(object correlationState)
        {
            CorrelationState state = (CorrelationState) correlationState;
            if (state != null)
            {
                ComPlusMethodCallTrace.Trace(TraceEventType.Verbose, 0x50019, "TraceCodeComIntegrationInvokedMethod", this.info, state.From, state.Action, state.CallerIdentity, this.iid, state.InstanceID, false);
                state.Cleanup();
            }
        }

        public object BeforeInvoke(InstanceContext instanceContext, IClientChannel channel, Message message)
        {
            ComPlusServerSecurity serverSecurity = null;
            WindowsImpersonationContext context = null;
            object obj3;
            bool flag = false;
            WindowsIdentity clientIdentity = null;
            Uri from = null;
            int instanceID = 0;
            string action = null;
            TransactionProxy proxy = null;
            Transaction messageTransaction = null;
            Guid empty = Guid.Empty;
            try
            {
                try
                {
                    clientIdentity = MessageUtil.GetMessageIdentity(message);
                    if (message.Headers.From != null)
                    {
                        from = message.Headers.From.Uri;
                    }
                    instanceID = instanceContext.GetServiceInstance(message).GetHashCode();
                    action = message.Headers.Action;
                    ComPlusMethodCallTrace.Trace(TraceEventType.Verbose, 0x50018, "TraceCodeComIntegrationInvokingMethod", this.info, from, action, clientIdentity.Name, this.iid, instanceID, false);
                    if (this.info.CheckRoles && !this.comAuth.IsAuthorizedForOperation(clientIdentity))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.CallAccessDenied());
                    }
                    if (this.info.HostingMode != HostingMode.WebHostOutOfProcess)
                    {
                        serverSecurity = new ComPlusServerSecurity(clientIdentity, this.info.CheckRoles);
                    }
                    proxy = instanceContext.Extensions.Find<TransactionProxy>();
                    if (proxy != null)
                    {
                        messageTransaction = MessageUtil.GetMessageTransaction(message);
                        if (messageTransaction != null)
                        {
                            empty = messageTransaction.TransactionInformation.DistributedIdentifier;
                        }
                        try
                        {
                            if (messageTransaction == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.TransactionMismatch());
                            }
                            proxy.SetTransaction(messageTransaction);
                            ComPlusMethodCallTrace.Trace(TraceEventType.Verbose, 0x5001a, "TraceCodeComIntegrationInvokingMethodNewTransaction", this.info, from, action, clientIdentity.Name, this.iid, instanceID, empty);
                            goto Label_02DC;
                        }
                        catch (FaultException exception)
                        {
                            Transaction currentTransaction = proxy.CurrentTransaction;
                            Guid distributedIdentifier = Guid.Empty;
                            if (currentTransaction != null)
                            {
                                distributedIdentifier = currentTransaction.TransactionInformation.DistributedIdentifier;
                            }
                            string name = string.Empty;
                            if (clientIdentity != null)
                            {
                                name = clientIdentity.Name;
                            }
                            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.ComPlus, (EventLogEventId) (-1073610725), new string[] { empty.ToString("B").ToUpperInvariant(), distributedIdentifier.ToString("B").ToUpperInvariant(), from.ToString(), this.info.AppID.ToString("B").ToUpperInvariant(), this.info.Clsid.ToString("B").ToUpperInvariant(), this.iid.ToString(), action, instanceID.ToString(CultureInfo.InvariantCulture), Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture), System.ServiceModel.ComIntegration.SafeNativeMethods.GetCurrentThreadId().ToString(CultureInfo.InvariantCulture), name, exception.ToString() });
                            flag = true;
                            throw;
                        }
                    }
                    ComPlusMethodCallTrace.Trace(TraceEventType.Verbose, 0x5001b, "TraceCodeComIntegrationInvokingMethodContextTransaction", this.info, from, action, clientIdentity.Name, this.iid, instanceID, true);
                Label_02DC:
                    if (this.info.HostingMode == HostingMode.WebHostOutOfProcess)
                    {
                        context = clientIdentity.Impersonate();
                    }
                    CorrelationState state = new CorrelationState(context, serverSecurity, from, action, clientIdentity.Name, instanceID);
                    context = null;
                    serverSecurity = null;
                    obj3 = state;
                }
                finally
                {
                    if (context != null)
                    {
                        context.Undo();
                    }
                    if (serverSecurity != null)
                    {
                        ((IDisposable) serverSecurity).Dispose();
                    }
                }
            }
            catch (Exception exception2)
            {
                if (!flag && DiagnosticUtility.ShouldTraceError)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.ComPlus, (EventLogEventId) (-1073610727), new string[] { (from == null) ? string.Empty : from.ToString(), this.info.AppID.ToString("B").ToUpperInvariant(), this.info.Clsid.ToString("B").ToUpperInvariant(), this.iid.ToString("B").ToUpperInvariant(), action, instanceID.ToString(CultureInfo.InvariantCulture), Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture), System.ServiceModel.ComIntegration.SafeNativeMethods.GetCurrentThreadId().ToString(CultureInfo.InvariantCulture), clientIdentity.Name, exception2.ToString() });
                }
                throw;
            }
            return obj3;
        }

        private class CorrelationState
        {
            private string action;
            private string callerIdentity;
            private Uri from;
            private WindowsImpersonationContext impersonationContext;
            private int instanceID;
            private ComPlusServerSecurity serverSecurity;

            public CorrelationState(WindowsImpersonationContext context, ComPlusServerSecurity serverSecurity, Uri from, string action, string callerIdentity, int instanceID)
            {
                this.impersonationContext = context;
                this.serverSecurity = serverSecurity;
                this.from = from;
                this.action = action;
                this.callerIdentity = callerIdentity;
                this.instanceID = instanceID;
            }

            public void Cleanup()
            {
                if (this.impersonationContext != null)
                {
                    this.impersonationContext.Undo();
                }
                if (this.serverSecurity != null)
                {
                    ((IDisposable) this.serverSecurity).Dispose();
                }
            }

            public string Action
            {
                get
                {
                    return this.action;
                }
            }

            public string CallerIdentity
            {
                get
                {
                    return this.callerIdentity;
                }
            }

            public Uri From
            {
                get
                {
                    return this.from;
                }
            }

            public int InstanceID
            {
                get
                {
                    return this.instanceID;
                }
            }
        }
    }
}

