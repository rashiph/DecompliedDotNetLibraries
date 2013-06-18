namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal class ContractInstanceProvider : ProviderBase, IWmiProvider
    {
        private static Dictionary<string, ContractDescription> knownContracts = new Dictionary<string, ContractDescription>();

        internal static string ContractReference(string contractName)
        {
            return string.Format(CultureInfo.InvariantCulture, "Contract.Name='{0}',ProcessId={1},AppDomainId={2}", new object[] { contractName, AppDomainInfo.Current.ProcessId, AppDomainInfo.Current.Id });
        }

        private static void FillBehaviorInfo(IContractBehavior behavior, IWmiInstance existingInstance, out IWmiInstance instance)
        {
            instance = null;
            if (behavior is DeliveryRequirementsAttribute)
            {
                instance = existingInstance.NewInstance("DeliveryRequirementsAttribute");
                DeliveryRequirementsAttribute attribute = (DeliveryRequirementsAttribute) behavior;
                instance.SetProperty("QueuedDeliveryRequirements", attribute.QueuedDeliveryRequirements.ToString());
                instance.SetProperty("RequireOrderedDelivery", attribute.RequireOrderedDelivery);
                if (null != attribute.TargetContract)
                {
                    instance.SetProperty("TargetContract", attribute.TargetContract.ToString());
                }
            }
            else if (behavior is IWmiInstanceProvider)
            {
                IWmiInstanceProvider provider = (IWmiInstanceProvider) behavior;
                instance = existingInstance.NewInstance(provider.GetInstanceType());
                provider.FillInstance(instance);
            }
            else
            {
                instance = existingInstance.NewInstance("Behavior");
            }
            if (instance != null)
            {
                instance.SetProperty("Type", behavior.GetType().FullName);
            }
        }

        private static void FillBehaviorInfo(IOperationBehavior behavior, IWmiInstance existingInstance, out IWmiInstance instance)
        {
            instance = null;
            if (behavior is DataContractSerializerOperationBehavior)
            {
                instance = existingInstance.NewInstance("DataContractSerializerOperationBehavior");
                DataContractSerializerOperationBehavior behavior2 = (DataContractSerializerOperationBehavior) behavior;
                instance.SetProperty("IgnoreExtensionDataObject", behavior2.IgnoreExtensionDataObject);
                instance.SetProperty("MaxItemsInObjectGraph", behavior2.MaxItemsInObjectGraph);
                if (behavior2.DataContractFormatAttribute != null)
                {
                    instance.SetProperty("Style", behavior2.DataContractFormatAttribute.Style.ToString());
                }
            }
            else if (behavior is OperationBehaviorAttribute)
            {
                instance = existingInstance.NewInstance("OperationBehaviorAttribute");
                OperationBehaviorAttribute attribute = (OperationBehaviorAttribute) behavior;
                instance.SetProperty("AutoDisposeParameters", attribute.AutoDisposeParameters);
                instance.SetProperty("Impersonation", attribute.Impersonation.ToString());
                instance.SetProperty("ReleaseInstanceMode", attribute.ReleaseInstanceMode.ToString());
                instance.SetProperty("TransactionAutoComplete", attribute.TransactionAutoComplete);
                instance.SetProperty("TransactionScopeRequired", attribute.TransactionScopeRequired);
            }
            else if (behavior is TransactionFlowAttribute)
            {
                instance = existingInstance.NewInstance("TransactionFlowAttribute");
                TransactionFlowAttribute attribute2 = (TransactionFlowAttribute) behavior;
                instance.SetProperty("TransactionFlowOption", attribute2.Transactions.ToString());
            }
            else if (behavior is XmlSerializerOperationBehavior)
            {
                instance = existingInstance.NewInstance("XmlSerializerOperationBehavior");
                XmlSerializerOperationBehavior behavior3 = (XmlSerializerOperationBehavior) behavior;
                if (behavior3.XmlSerializerFormatAttribute != null)
                {
                    instance.SetProperty("Style", behavior3.XmlSerializerFormatAttribute.Style.ToString());
                    instance.SetProperty("Use", behavior3.XmlSerializerFormatAttribute.Use.ToString());
                    instance.SetProperty("SupportFaults", behavior3.XmlSerializerFormatAttribute.SupportFaults.ToString());
                }
            }
            else if (behavior is IWmiInstanceProvider)
            {
                IWmiInstanceProvider provider = (IWmiInstanceProvider) behavior;
                instance = existingInstance.NewInstance(provider.GetInstanceType());
                provider.FillInstance(instance);
            }
            else
            {
                instance = existingInstance.NewInstance("Behavior");
            }
            if (instance != null)
            {
                instance.SetProperty("Type", behavior.GetType().FullName);
            }
        }

        private static void FillBehaviorsInfo(IWmiInstance operation, KeyedByTypeCollection<IContractBehavior> behaviors)
        {
            List<IWmiInstance> list = new List<IWmiInstance>(behaviors.Count);
            foreach (IContractBehavior behavior in behaviors)
            {
                IWmiInstance instance;
                FillBehaviorInfo(behavior, operation, out instance);
                if (instance != null)
                {
                    list.Add(instance);
                }
            }
            operation.SetProperty("Behaviors", list.ToArray());
        }

        private static void FillBehaviorsInfo(IWmiInstance operation, KeyedByTypeCollection<IOperationBehavior> behaviors)
        {
            List<IWmiInstance> list = new List<IWmiInstance>(behaviors.Count);
            foreach (IOperationBehavior behavior in behaviors)
            {
                IWmiInstance instance;
                FillBehaviorInfo(behavior, operation, out instance);
                if (instance != null)
                {
                    list.Add(instance);
                }
            }
            operation.SetProperty("Behaviors", list.ToArray());
        }

        private static void FillContract(IWmiInstance contract, ContractDescription contractDescription)
        {
            contract.SetProperty("Type", contractDescription.ContractType.Name);
            if (null != contractDescription.CallbackContractType)
            {
                contract.SetProperty("CallbackContract", ContractReference(contractDescription.CallbackContractType.Name));
            }
            contract.SetProperty("Name", contractDescription.Name);
            contract.SetProperty("Namespace", contractDescription.Namespace);
            contract.SetProperty("SessionMode", contractDescription.SessionMode.ToString());
            IWmiInstance[] instanceArray = new IWmiInstance[contractDescription.Operations.Count];
            for (int i = 0; i < instanceArray.Length; i++)
            {
                OperationDescription operationDescription = contractDescription.Operations[i];
                IWmiInstance operation = contract.NewInstance("Operation");
                FillOperation(operation, operationDescription);
                instanceArray[i] = operation;
            }
            contract.SetProperty("Operations", instanceArray);
            FillBehaviorsInfo(contract, contractDescription.Behaviors);
        }

        private static void FillOperation(IWmiInstance operation, OperationDescription operationDescription)
        {
            operation.SetProperty("Name", operationDescription.Name);
            operation.SetProperty("Action", FixWildcardAction(operationDescription.Messages[0].Action));
            if (operationDescription.Messages.Count > 1)
            {
                operation.SetProperty("ReplyAction", FixWildcardAction(operationDescription.Messages[1].Action));
            }
            operation.SetProperty("IsOneWay", operationDescription.IsOneWay);
            operation.SetProperty("IsInitiating", operationDescription.IsInitiating);
            operation.SetProperty("IsTerminating", operationDescription.IsTerminating);
            operation.SetProperty("AsyncPattern", null != operationDescription.BeginMethod);
            if (null != operationDescription.SyncMethod)
            {
                if (null != operationDescription.SyncMethod.ReturnType)
                {
                    operation.SetProperty("ReturnType", operationDescription.SyncMethod.ReturnType.Name);
                }
                operation.SetProperty("MethodSignature", operationDescription.SyncMethod.ToString());
                ParameterInfo[] parameters = operationDescription.SyncMethod.GetParameters();
                string[] strArray = new string[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    strArray[i] = parameters[i].ParameterType.ToString();
                }
                operation.SetProperty("ParameterTypes", strArray);
            }
            operation.SetProperty("IsCallback", operationDescription.Messages[0].Direction == MessageDirection.Output);
            FillBehaviorsInfo(operation, operationDescription.Behaviors);
        }

        private static string FixWildcardAction(string action)
        {
            if (action == null)
            {
                return "*";
            }
            return action;
        }

        internal static void RegisterContract(ContractDescription contract)
        {
            lock (knownContracts)
            {
                if (!knownContracts.ContainsKey(contract.Name))
                {
                    knownContracts.Add(contract.Name, contract);
                }
            }
        }

        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            int processId = AppDomainInfo.Current.ProcessId;
            int id = AppDomainInfo.Current.Id;
            lock (knownContracts)
            {
                UpdateContracts();
                foreach (ContractDescription description in knownContracts.Values)
                {
                    IWmiInstance contract = instances.NewInstance(null);
                    contract.SetProperty("ProcessId", processId);
                    contract.SetProperty("AppDomainId", id);
                    FillContract(contract, description);
                    instances.AddInstance(contract);
                }
            }
        }

        bool IWmiProvider.GetInstance(IWmiInstance contract)
        {
            bool flag = false;
            if ((((int) contract.GetProperty("ProcessId")) == AppDomainInfo.Current.ProcessId) && (((int) contract.GetProperty("AppDomainId")) == AppDomainInfo.Current.Id))
            {
                ContractDescription description;
                string property = (string) contract.GetProperty("Name");
                UpdateContracts();
                if (knownContracts.TryGetValue(property, out description))
                {
                    flag = true;
                    FillContract(contract, description);
                }
            }
            return flag;
        }

        private static void UpdateContracts()
        {
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                foreach (System.ServiceModel.Administration.EndpointInfo info2 in info.Endpoints)
                {
                    RegisterContract(info2.Contract);
                }
            }
        }
    }
}

