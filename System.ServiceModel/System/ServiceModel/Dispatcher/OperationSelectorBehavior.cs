namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    internal class OperationSelectorBehavior : IContractBehavior
    {
        void IContractBehavior.AddBindingParameters(ContractDescription description, ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription description, ServiceEndpoint endpoint, ClientRuntime proxy)
        {
            proxy.OperationSelector = new MethodInfoOperationSelector(description, MessageDirection.Input);
        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription description, ServiceEndpoint endpoint, DispatchRuntime dispatch)
        {
            if (dispatch.ClientRuntime != null)
            {
                dispatch.ClientRuntime.OperationSelector = new MethodInfoOperationSelector(description, MessageDirection.Output);
            }
        }

        void IContractBehavior.Validate(ContractDescription description, ServiceEndpoint endpoint)
        {
        }

        internal class MethodInfoOperationSelector : IClientOperationSelector
        {
            private Dictionary<object, string> operationMap = new Dictionary<object, string>();

            internal MethodInfoOperationSelector(ContractDescription description, MessageDirection directionThatRequiresClientOpSelection)
            {
                for (int i = 0; i < description.Operations.Count; i++)
                {
                    OperationDescription description2 = description.Operations[i];
                    if (description2.Messages[0].Direction == directionThatRequiresClientOpSelection)
                    {
                        if ((description2.SyncMethod != null) && !this.operationMap.ContainsKey(description2.SyncMethod.MethodHandle))
                        {
                            this.operationMap.Add(description2.SyncMethod.MethodHandle, description2.Name);
                        }
                        if ((description2.BeginMethod != null) && !this.operationMap.ContainsKey(description2.BeginMethod.MethodHandle))
                        {
                            this.operationMap.Add(description2.BeginMethod.MethodHandle, description2.Name);
                            this.operationMap.Add(description2.EndMethod.MethodHandle, description2.Name);
                        }
                    }
                }
            }

            public string SelectOperation(MethodBase method, object[] parameters)
            {
                if (this.operationMap.ContainsKey(method.MethodHandle))
                {
                    return this.operationMap[method.MethodHandle];
                }
                return null;
            }

            public bool AreParametersRequiredForSelection
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

