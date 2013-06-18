namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal class ComPlusTypeLoader : IContractResolver
    {
        private Dictionary<Guid, ContractDescription> contracts;
        private ServiceInfo info;
        private ITypeCacheManager interfaceResolver;
        private bool transactionFlow;

        public ComPlusTypeLoader(ServiceInfo info)
        {
            this.info = info;
            this.transactionFlow = (info.TransactionOption == TransactionOption.Required) || (info.TransactionOption == TransactionOption.Supported);
            this.interfaceResolver = new TypeCacheManager();
            this.contracts = new Dictionary<Guid, ContractDescription>();
        }

        private void ConfigureContractDescriptionBehaviors(ContractDescription contract)
        {
            contract.Behaviors.Add(new OperationSelectorBehavior());
            ComPlusContractBehavior item = new ComPlusContractBehavior(this.info);
            contract.Behaviors.Add(item);
        }

        private void ConfigureOperationDescriptionBehaviors(OperationDescription operation, IDataContractSurrogate contractSurrogate)
        {
            DataContractSerializerOperationBehavior item = new DataContractSerializerOperationBehavior(operation, TypeLoader.DefaultDataContractFormatAttribute);
            if (contractSurrogate != null)
            {
                item.DataContractSurrogate = contractSurrogate;
            }
            operation.Behaviors.Add(item);
            operation.Behaviors.Add(new OperationInvokerBehavior());
            if ((this.info.TransactionOption == TransactionOption.Supported) || (this.info.TransactionOption == TransactionOption.Required))
            {
                operation.Behaviors.Add(new TransactionFlowAttribute(TransactionFlowOption.Allowed));
            }
            OperationBehaviorAttribute attribute = new OperationBehaviorAttribute {
                TransactionAutoComplete = true,
                TransactionScopeRequired = false
            };
            operation.Behaviors.Add(attribute);
        }

        private ContractDescription CreateContractDescriptionInternal(Guid iid, Type type)
        {
            ComContractElement comContract = ConfigLoader.LookupComContract(iid);
            if (comContract == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("InterfaceNotFoundInConfig", new object[] { iid })));
            }
            if (string.IsNullOrEmpty(comContract.Name) || string.IsNullOrEmpty(comContract.Namespace))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("CannotHaveNullOrEmptyNameOrNamespaceForIID", new object[] { iid })));
            }
            ContractDescription contract = new ContractDescription(comContract.Name, comContract.Namespace) {
                ContractType = type,
                SessionMode = comContract.RequiresSession ? SessionMode.Required : SessionMode.Allowed
            };
            bool flag = false;
            List<Guid> list = new List<Guid>();
            foreach (ComPersistableTypeElement element2 in comContract.PersistableTypes)
            {
                Guid item = Fx.CreateGuid(element2.ID);
                list.Add(item);
            }
            IDataContractSurrogate contractSurrogate = null;
            if ((list.Count > 0) || comContract.PersistableTypes.EmitClear)
            {
                contractSurrogate = new DataContractSurrogateForPersistWrapper(list.ToArray());
            }
            foreach (ComMethodElement element3 in comContract.ExposedMethods)
            {
                flag = false;
                foreach (System.Reflection.MethodInfo info in type.GetMethods())
                {
                    if (info.Name == element3.ExposedMethod)
                    {
                        OperationDescription operation = this.CreateOperationDescription(contract, info, comContract, null != contractSurrogate);
                        this.ConfigureOperationDescriptionBehaviors(operation, contractSurrogate);
                        contract.Operations.Add(operation);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("MethodGivenInConfigNotFoundOnInterface", new object[] { element3.ExposedMethod, iid })));
                }
            }
            if (contract.Operations.Count == 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("NoneOfTheMethodsForInterfaceFoundInConfig", new object[] { iid })));
            }
            this.ConfigureContractDescriptionBehaviors(contract);
            return contract;
        }

        private MessageDescription CreateIncomingMessageDescription(ContractDescription contract, System.Reflection.MethodInfo methodInfo, string ns, string action, bool allowReferences)
        {
            ParameterInfo[] inputParameters = ServiceReflector.GetInputParameters(methodInfo, false);
            return this.CreateParameterMessageDescription(contract, inputParameters, null, null, null, methodInfo.Name, ns, action, MessageDirection.Input, allowReferences);
        }

        private MessagePartDescription CreateMessagePartDescription(Type bodyType, System.ServiceModel.Description.XmlName name, string ns, int index)
        {
            return new MessagePartDescription(name.EncodedName, ns) { SerializationPosition = index, MemberInfo = null, Type = bodyType, Index = index };
        }

        private OperationDescription CreateOperationDescription(ContractDescription contract, System.Reflection.MethodInfo methodInfo, ComContractElement config, bool allowReferences)
        {
            System.ServiceModel.Description.XmlName methodName = new System.ServiceModel.Description.XmlName(ServiceReflector.GetLogicalName(methodInfo));
            System.ServiceModel.Description.XmlName returnValueName = TypeLoader.GetReturnValueName(methodName);
            if (ServiceReflector.IsBegin(methodInfo))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.NoAsyncOperationsAllowed());
            }
            if (contract.Operations.FindAll(methodName.EncodedName).Count != 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.DuplicateOperation());
            }
            OperationDescription description = new OperationDescription(methodName.EncodedName, contract) {
                SyncMethod = methodInfo,
                IsInitiating = true,
                IsTerminating = false
            };
            description.KnownTypes.Add(typeof(Array));
            description.KnownTypes.Add(typeof(DBNull));
            description.KnownTypes.Add(typeof(CurrencyWrapper));
            description.KnownTypes.Add(typeof(ErrorWrapper));
            if (allowReferences)
            {
                description.KnownTypes.Add(typeof(PersistStreamTypeWrapper));
            }
            foreach (ComUdtElement element in config.UserDefinedTypes)
            {
                Type type;
                Guid typeLibId = Fx.CreateGuid(element.TypeLibID);
                TypeCacheManager.Provider.FindOrCreateType(typeLibId, element.TypeLibVersion, Fx.CreateGuid(element.TypeDefID), out type, false);
                this.info.AddUdt(type, typeLibId);
                description.KnownTypes.Add(type);
            }
            string ns = contract.Namespace;
            XmlQualifiedName contractName = new XmlQualifiedName(contract.Name, ns);
            string action = NamingHelper.GetMessageAction(contractName, methodName.DecodedName, null, false);
            string str3 = NamingHelper.GetMessageAction(contractName, methodName.DecodedName, null, true);
            MessageDescription item = this.CreateIncomingMessageDescription(contract, methodInfo, ns, action, allowReferences);
            MessageDescription description3 = this.CreateOutgoingMessageDescription(contract, methodInfo, returnValueName, ns, str3, allowReferences);
            description.Messages.Add(item);
            description.Messages.Add(description3);
            return description;
        }

        private MessageDescription CreateOutgoingMessageDescription(ContractDescription contract, System.Reflection.MethodInfo methodInfo, System.ServiceModel.Description.XmlName returnValueName, string ns, string action, bool allowReferences)
        {
            ParameterInfo[] outputParameters = ServiceReflector.GetOutputParameters(methodInfo, false);
            return this.CreateParameterMessageDescription(contract, outputParameters, methodInfo.ReturnType, methodInfo.ReturnTypeCustomAttributes, returnValueName, methodInfo.Name, ns, action, MessageDirection.Output, allowReferences);
        }

        private MessageDescription CreateParameterMessageDescription(ContractDescription contract, ParameterInfo[] parameters, Type returnType, ICustomAttributeProvider returnCustomAttributes, System.ServiceModel.Description.XmlName returnValueName, string methodName, string ns, string action, MessageDirection direction, bool allowReferences)
        {
            MessageDescription description = new MessageDescription(action, direction) {
                Body = { WrapperNamespace = ns }
            };
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameterInfo = parameters[i];
                Type parameterType = TypeLoader.GetParameterType(parameterInfo);
                if (!ComPlusTypeValidator.IsValidParameter(parameterType, parameterInfo, allowReferences))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("InvalidWebServiceParameter", new object[] { parameterInfo.Name, parameterType.Name, methodName, contract.Name })));
                }
                MessagePartDescription item = this.CreateMessagePartDescription(parameterType, new System.ServiceModel.Description.XmlName(parameterInfo.Name), ns, i);
                description.Body.Parts.Add(item);
            }
            System.ServiceModel.Description.XmlName operationName = new System.ServiceModel.Description.XmlName(methodName);
            if (returnType == null)
            {
                description.Body.WrapperName = operationName.EncodedName;
                return description;
            }
            description.Body.WrapperName = TypeLoader.GetBodyWrapperResponseName(operationName).EncodedName;
            if (!ComPlusTypeValidator.IsValidParameter(returnType, returnCustomAttributes, allowReferences))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("InvalidWebServiceReturnValue", new object[] { returnType.Name, methodName, contract.Name })));
            }
            MessagePartDescription description3 = this.CreateMessagePartDescription(returnType, returnValueName, ns, 0);
            description.Body.ReturnValue = description3;
            return description;
        }

        public ContractDescription ResolveContract(string contractTypeString)
        {
            Guid gUID;
            ContractDescription description;
            if ("IMetadataExchange" == contractTypeString)
            {
                gUID = typeof(IMetadataExchange).GUID;
            }
            else
            {
                if (!System.ServiceModel.DiagnosticUtility.Utility.TryCreateGuid(contractTypeString, out gUID))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("ContractTypeNotAnIID", new object[] { contractTypeString })));
                }
                this.ValidateInterface(gUID);
            }
            if (!this.contracts.TryGetValue(gUID, out description))
            {
                if (gUID != typeof(IMetadataExchange).GUID)
                {
                    Type type;
                    try
                    {
                        this.interfaceResolver.FindOrCreateType(this.info.ServiceType, gUID, out type, false, true);
                    }
                    catch (InvalidOperationException exception)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(exception.Message));
                    }
                    description = this.CreateContractDescriptionInternal(gUID, type);
                }
                else
                {
                    description = this.ResolveIMetadataExchangeToContract();
                }
                this.contracts.Add(gUID, description);
                ComPlusServiceHostTrace.Trace(TraceEventType.Verbose, 0x50003, "TraceCodeComIntegrationServiceHostCreatedServiceContract", this.info, description);
            }
            return description;
        }

        private ContractDescription ResolveIMetadataExchangeToContract()
        {
            TypeLoader loader = new TypeLoader();
            return loader.LoadContractDescription(typeof(IMetadataExchange));
        }

        private void ValidateInterface(Guid iid)
        {
            if (!ComPlusTypeValidator.IsValidInterface(iid))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("InvalidWebServiceInterface", new object[] { iid })));
            }
            bool flag = false;
            foreach (ContractInfo info in this.info.Contracts)
            {
                if (info.IID == iid)
                {
                    if (info.Operations.Count == 0)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("RequireConfiguredMethods", new object[] { iid })));
                    }
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("RequireConfiguredInterfaces", new object[] { iid })));
            }
        }
    }
}

