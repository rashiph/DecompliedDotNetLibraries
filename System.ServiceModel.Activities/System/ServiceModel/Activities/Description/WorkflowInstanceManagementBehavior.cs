namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net.Security;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Xml;

    public sealed class WorkflowInstanceManagementBehavior : IServiceBehavior
    {
        public const string ControlEndpointAddress = "System.ServiceModel.Activities_IWorkflowInstanceManagement";
        private static Binding httpBinding;
        private static Binding namedPipeBinding;
        private string windowsGroup = GetDefaultBuiltinAdministratorsGroup();

        public void AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceHostBase");
            }
            WorkflowServiceHost workflowServiceHost = serviceHostBase as WorkflowServiceHost;
            if (workflowServiceHost != null)
            {
                this.CreateWorkflowManagementEndpoint(workflowServiceHost);
            }
        }

        private void CreateWorkflowManagementEndpoint(WorkflowServiceHost workflowServiceHost)
        {
            Binding namedPipeControlEndpointBinding;
            IChannelListener listener;
            if (workflowServiceHost.InternalBaseAddresses.Contains(Uri.UriSchemeNetPipe))
            {
                namedPipeControlEndpointBinding = NamedPipeControlEndpointBinding;
            }
            else if (workflowServiceHost.InternalBaseAddresses.Contains(Uri.UriSchemeHttp))
            {
                namedPipeControlEndpointBinding = HttpControlEndpointBinding;
            }
            else
            {
                return;
            }
            Uri listenUriBaseAddress = ServiceHostBase.GetVia(namedPipeControlEndpointBinding.Scheme, new Uri("System.ServiceModel.Activities_IWorkflowInstanceManagement", UriKind.Relative), workflowServiceHost.InternalBaseAddresses);
            XmlQualifiedName contractName = new XmlQualifiedName("IWorkflowInstanceManagement", "http://schemas.datacontract.org/2008/10/WorkflowServices");
            EndpointAddress address = new EndpointAddress(listenUriBaseAddress.AbsoluteUri);
            EndpointDispatcher item = new EndpointDispatcher(address, "IWorkflowInstanceManagement", "http://schemas.datacontract.org/2008/10/WorkflowServices", true) {
                ContractFilter = new ActionMessageFilter(new string[] { NamingHelper.GetMessageAction(contractName, "Abandon", null, false), NamingHelper.GetMessageAction(contractName, "Cancel", null, false), NamingHelper.GetMessageAction(contractName, "Run", null, false), NamingHelper.GetMessageAction(contractName, "Suspend", null, false), NamingHelper.GetMessageAction(contractName, "Terminate", null, false), NamingHelper.GetMessageAction(contractName, "TransactedCancel", null, false), NamingHelper.GetMessageAction(contractName, "TransactedRun", null, false), NamingHelper.GetMessageAction(contractName, "TransactedSuspend", null, false), NamingHelper.GetMessageAction(contractName, "TransactedTerminate", null, false), NamingHelper.GetMessageAction(contractName, "TransactedUnsuspend", null, false), NamingHelper.GetMessageAction(contractName, "Unsuspend", null, false) })
            };
            BindingParameterCollection parameters = new BindingParameterCollection();
            VirtualPathExtension extension = workflowServiceHost.Extensions.Find<VirtualPathExtension>();
            if (extension != null)
            {
                parameters.Add(extension);
            }
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            requirements.Add(ChannelProtectionRequirements.CreateFromContract(WorkflowControlEndpoint.WorkflowControlServiceContract, ProtectionLevel.EncryptAndSign, ProtectionLevel.EncryptAndSign, false));
            parameters.Add(requirements);
            if (namedPipeControlEndpointBinding.CanBuildChannelListener<IDuplexSessionChannel>(new object[] { listenUriBaseAddress, parameters }))
            {
                listener = namedPipeControlEndpointBinding.BuildChannelListener<IDuplexSessionChannel>(listenUriBaseAddress, parameters);
            }
            else if (namedPipeControlEndpointBinding.CanBuildChannelListener<IReplySessionChannel>(new object[] { listenUriBaseAddress, parameters }))
            {
                listener = namedPipeControlEndpointBinding.BuildChannelListener<IReplySessionChannel>(listenUriBaseAddress, parameters);
            }
            else
            {
                listener = namedPipeControlEndpointBinding.BuildChannelListener<IReplyChannel>(listenUriBaseAddress, parameters);
            }
            foreach (OperationDescription description in WorkflowControlEndpoint.WorkflowControlServiceContract.Operations)
            {
                bool flag;
                bool flag2;
                DataContractSerializerOperationBehavior behavior = new DataContractSerializerOperationBehavior(description);
                DispatchOperation operation = new DispatchOperation(item.DispatchRuntime, description.Name, NamingHelper.GetMessageAction(description, false), NamingHelper.GetMessageAction(description, true)) {
                    Formatter = (IDispatchMessageFormatter) behavior.GetFormatter(description, out flag, out flag2, false),
                    Invoker = new ControlOperationInvoker(description, new WorkflowControlEndpoint(namedPipeControlEndpointBinding, address), null, workflowServiceHost)
                };
                item.DispatchRuntime.Operations.Add(operation);
                OperationBehaviorAttribute attribute = description.Behaviors.Find<OperationBehaviorAttribute>();
                ((IOperationBehavior) attribute).ApplyDispatchBehavior(description, operation);
                if (attribute.TransactionScopeRequired)
                {
                    ((ITransactionChannelManager) listener).Dictionary.Add(new DirectionalAction(MessageDirection.Input, NamingHelper.GetMessageAction(description, false)), TransactionFlowOption.Allowed);
                }
            }
            DispatchRuntime dispatchRuntime = item.DispatchRuntime;
            dispatchRuntime.ConcurrencyMode = ConcurrencyMode.Multiple;
            dispatchRuntime.InstanceContextProvider = new DurableInstanceContextProvider(workflowServiceHost);
            dispatchRuntime.InstanceProvider = new DurableInstanceProvider(workflowServiceHost);
            dispatchRuntime.ServiceAuthorizationManager = new WindowsAuthorizationManager(this.WindowsGroup);
            ServiceDebugBehavior behavior2 = workflowServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            ServiceBehaviorAttribute attribute2 = workflowServiceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            bool flag3 = false;
            if (behavior2 != null)
            {
                flag3 |= behavior2.IncludeExceptionDetailInFaults;
            }
            if (attribute2 != null)
            {
                flag3 |= attribute2.IncludeExceptionDetailInFaults;
            }
            ChannelDispatcher dispatcher4 = new ChannelDispatcher(listener, namedPipeControlEndpointBinding.Name, namedPipeControlEndpointBinding) {
                MessageVersion = namedPipeControlEndpointBinding.MessageVersion
            };
            dispatcher4.Endpoints.Add(item);
            dispatcher4.ServiceThrottle = workflowServiceHost.ServiceThrottle;
            ChannelDispatcher dispatcher2 = dispatcher4;
            workflowServiceHost.ChannelDispatchers.Add(dispatcher2);
        }

        internal static string GetDefaultBuiltinAdministratorsGroup()
        {
            SecurityIdentifier identifier = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            NTAccount account = (NTAccount) identifier.Translate(typeof(NTAccount));
            return account.Value;
        }

        public void Validate(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public static Binding HttpControlEndpointBinding
        {
            get
            {
                if (httpBinding == null)
                {
                    WSHttpBinding binding = new WSHttpBinding {
                        TransactionFlow = true
                    };
                    WSHttpSecurity security = new WSHttpSecurity {
                        Mode = SecurityMode.Message
                    };
                    NonDualMessageSecurityOverHttp http = new NonDualMessageSecurityOverHttp {
                        ClientCredentialType = MessageCredentialType.Windows
                    };
                    security.Message = http;
                    binding.Security = security;
                    httpBinding = binding;
                }
                return httpBinding;
            }
        }

        public static Binding NamedPipeControlEndpointBinding
        {
            get
            {
                if (namedPipeBinding == null)
                {
                    NetNamedPipeBinding binding = new NetNamedPipeBinding {
                        TransactionFlow = true
                    };
                    NetNamedPipeSecurity security = new NetNamedPipeSecurity {
                        Mode = NetNamedPipeSecurityMode.Transport
                    };
                    NamedPipeTransportSecurity security2 = new NamedPipeTransportSecurity {
                        ProtectionLevel = ProtectionLevel.Sign
                    };
                    security.Transport = security2;
                    binding.Security = security;
                    namedPipeBinding = binding;
                }
                return namedPipeBinding;
            }
        }

        public string WindowsGroup
        {
            get
            {
                return this.windowsGroup;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("WindowsGroup");
                }
                this.windowsGroup = value;
            }
        }

        private sealed class WindowsAuthorizationManager : ServiceAuthorizationManager
        {
            private SecurityIdentifier sid;

            public WindowsAuthorizationManager(string windowsGroup)
            {
                NTAccount account = new NTAccount(windowsGroup);
                try
                {
                    this.sid = account.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                }
                catch (IdentityNotMappedException)
                {
                    throw FxTrace.Exception.Argument(windowsGroup, System.ServiceModel.Activities.SR.WindowsGroupNotFound(windowsGroup));
                }
            }

            protected override bool CheckAccessCore(OperationContext operationContext)
            {
                WindowsPrincipal principal = new WindowsPrincipal(operationContext.ServiceSecurityContext.WindowsIdentity);
                bool flag = false;
                if (!operationContext.ServiceSecurityContext.IsAnonymous)
                {
                    flag = principal.IsInRole(this.sid);
                }
                return flag;
            }
        }
    }
}

