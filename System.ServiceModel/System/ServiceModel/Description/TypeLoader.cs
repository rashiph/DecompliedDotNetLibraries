namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal class TypeLoader
    {
        private readonly Dictionary<System.Type, ContractDescription> contracts = new Dictionary<System.Type, ContractDescription>();
        internal const BindingFlags DefaultBindingFlags = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        internal static DataContractFormatAttribute DefaultDataContractFormatAttribute = new DataContractFormatAttribute();
        internal static XmlSerializerFormatAttribute DefaultXmlSerializerFormatAttribute = new XmlSerializerFormatAttribute();
        internal const string FaultSuffix = "Fault";
        private static System.Type[] formatterAttributes = new System.Type[] { typeof(XmlSerializerFormatAttribute), typeof(DataContractFormatAttribute) };
        private static System.Type[] knownTypesMethodParamType = new System.Type[] { typeof(ICustomAttributeProvider) };
        private static System.Type[] messageContractMemberAttributes = new System.Type[] { typeof(MessageHeaderAttribute), typeof(MessageBodyMemberAttribute), typeof(MessagePropertyAttribute) };
        private readonly Dictionary<System.Type, MessageDescriptionItems> messages = new Dictionary<System.Type, MessageDescriptionItems>();
        private static readonly System.Type OperationContractAttributeType = typeof(OperationContractAttribute);
        internal const string ResponseSuffix = "Response";
        internal const string ReturnSuffix = "Result";
        private readonly object thisLock = new object();

        private void AddBehaviors(ContractDescription contractDesc, System.Type implType, bool implIsCallback, ContractReflectionInfo reflectionInfo)
        {
            ServiceReflector.GetRequiredSingleAttribute<ServiceContractAttribute>(reflectionInfo.iface);
            for (int j = 0; j < contractDesc.Operations.Count; j++)
            {
                OperationDescription description = contractDesc.Operations[j];
                if (description.DeclaringContract == contractDesc)
                {
                    description.Behaviors.Add(new OperationInvokerBehavior());
                }
            }
            contractDesc.Behaviors.Add(new OperationSelectorBehavior());
            for (int k = 0; k < contractDesc.Operations.Count; k++)
            {
                ServiceInheritanceCallback<IOperationBehavior, KeyedByTypeCollection<IOperationBehavior>> callback = null;
                ServiceInheritanceCallback<IOperationBehavior, KeyedByTypeCollection<IOperationBehavior>> callback2 = null;
                OperationDescription opDesc = contractDesc.Operations[k];
                bool flag2 = opDesc.DeclaringContract != contractDesc;
                System.Type targetIface = implIsCallback ? opDesc.DeclaringContract.CallbackContractType : opDesc.DeclaringContract.ContractType;
                if ((implType == null) && !flag2)
                {
                    KeyedByTypeCollection<IOperationBehavior> types = this.GetIOperationBehaviorAttributesFromType(opDesc, targetIface, null);
                    for (int num3 = 0; num3 < types.Count; num3++)
                    {
                        opDesc.Behaviors.Add(types[num3]);
                    }
                }
                else
                {
                    if (callback == null)
                    {
                        callback = delegate (System.Type currentType, KeyedByTypeCollection<IOperationBehavior> behaviors) {
                            KeyedByTypeCollection<IOperationBehavior> types = this.GetIOperationBehaviorAttributesFromType(opDesc, targetIface, currentType);
                            for (int i = 0; i < types.Count; i++)
                            {
                                behaviors.Add(types[i]);
                            }
                        };
                    }
                    ApplyServiceInheritance<IOperationBehavior, KeyedByTypeCollection<IOperationBehavior>>(implType, opDesc.Behaviors, callback);
                    if (!flag2)
                    {
                        if (callback2 == null)
                        {
                            callback2 = delegate (System.Type currentType, KeyedByTypeCollection<IOperationBehavior> behaviors) {
                                KeyedByTypeCollection<IOperationBehavior> types = this.GetIOperationBehaviorAttributesFromType(opDesc, targetIface, null);
                                for (int i = 0; i < types.Count; i++)
                                {
                                    behaviors.Add(types[i]);
                                }
                            };
                        }
                        AddBehaviorsAtOneScope<IOperationBehavior, KeyedByTypeCollection<IOperationBehavior>>(targetIface, opDesc.Behaviors, callback2);
                    }
                }
            }
            for (int m = 0; m < contractDesc.Operations.Count; m++)
            {
                OperationDescription description2 = contractDesc.Operations[m];
                if (description2.Behaviors.Find<OperationBehaviorAttribute>() == null)
                {
                    OperationBehaviorAttribute item = new OperationBehaviorAttribute();
                    description2.Behaviors.Add(item);
                }
            }
            System.Type type = implIsCallback ? reflectionInfo.callbackiface : reflectionInfo.iface;
            AddBehaviorsAtOneScope<IContractBehavior, KeyedByTypeCollection<IContractBehavior>>(type, contractDesc.Behaviors, new ServiceInheritanceCallback<IContractBehavior, KeyedByTypeCollection<IContractBehavior>>(this.GetIContractBehaviorsFromInterfaceType));
            bool flag3 = false;
            for (int n = 0; n < contractDesc.Operations.Count; n++)
            {
                OperationDescription operation = contractDesc.Operations[n];
                bool flag4 = operation.DeclaringContract != contractDesc;
                System.Attribute formattingAttribute = GetFormattingAttribute(operation.OperationMethod, GetFormattingAttribute(operation.DeclaringContract.ContractType, DefaultDataContractFormatAttribute));
                DataContractFormatAttribute dataContractFormatAttribute = formattingAttribute as DataContractFormatAttribute;
                if (dataContractFormatAttribute != null)
                {
                    if (!flag4)
                    {
                        operation.Behaviors.Add(new DataContractSerializerOperationBehavior(operation, dataContractFormatAttribute, true));
                        operation.Behaviors.Add(new DataContractSerializerOperationGenerator());
                    }
                }
                else if ((formattingAttribute != null) && (formattingAttribute is XmlSerializerFormatAttribute))
                {
                    flag3 = true;
                }
            }
            if (flag3)
            {
                XmlSerializerOperationBehavior.AddBuiltInBehaviors(contractDesc);
            }
        }

        private static void AddBehaviorsAtOneScope<IBehavior, TBehaviorCollection>(System.Type type, TBehaviorCollection descriptionBehaviors, ServiceInheritanceCallback<IBehavior, TBehaviorCollection> callback) where IBehavior: class where TBehaviorCollection: KeyedByTypeCollection<IBehavior>
        {
            KeyedByTypeCollection<IBehavior> behaviors = new KeyedByTypeCollection<IBehavior>();
            callback(type, behaviors);
            for (int i = 0; i < behaviors.Count; i++)
            {
                IBehavior item = behaviors[i];
                if (!descriptionBehaviors.Contains(item.GetType()))
                {
                    if ((item is ServiceBehaviorAttribute) || (item is CallbackBehaviorAttribute))
                    {
                        descriptionBehaviors.Insert(0, item);
                    }
                    else
                    {
                        descriptionBehaviors.Add(item);
                    }
                }
            }
        }

        internal void AddBehaviorsFromImplementationType(ServiceEndpoint serviceEndpoint, System.Type implementationType)
        {
            foreach (IEndpointBehavior behavior in ServiceReflector.GetCustomAttributes(implementationType, typeof(IEndpointBehavior), false))
            {
                if (behavior is CallbackBehaviorAttribute)
                {
                    serviceEndpoint.Behaviors.Insert(0, behavior);
                }
                else
                {
                    serviceEndpoint.Behaviors.Add(behavior);
                }
            }
            foreach (IContractBehavior behavior2 in ServiceReflector.GetCustomAttributes(implementationType, typeof(IContractBehavior), false))
            {
                serviceEndpoint.Contract.Behaviors.Add(behavior2);
            }
            System.Type targetIface = serviceEndpoint.Contract.CallbackContractType;
            for (int j = 0; j < serviceEndpoint.Contract.Operations.Count; j++)
            {
                OperationDescription opDesc = serviceEndpoint.Contract.Operations[j];
                KeyedByTypeCollection<IOperationBehavior> descriptionBehaviors = new KeyedByTypeCollection<IOperationBehavior>();
                ApplyServiceInheritance<IOperationBehavior, KeyedByTypeCollection<IOperationBehavior>>(implementationType, descriptionBehaviors, delegate (System.Type currentType, KeyedByTypeCollection<IOperationBehavior> behaviors) {
                    KeyedByTypeCollection<IOperationBehavior> types = this.GetIOperationBehaviorAttributesFromType(opDesc, targetIface, currentType);
                    for (int m = 0; m < types.Count; m++)
                    {
                        behaviors.Add(types[m]);
                    }
                });
                for (int k = 0; k < descriptionBehaviors.Count; k++)
                {
                    IOperationBehavior item = descriptionBehaviors[k];
                    System.Type key = item.GetType();
                    if (opDesc.Behaviors.Contains(key))
                    {
                        opDesc.Behaviors.Remove(key);
                    }
                    opDesc.Behaviors.Add(item);
                }
            }
        }

        internal void AddBehaviorsSFx(ServiceEndpoint serviceEndpoint, System.Type contractType)
        {
            if (serviceEndpoint.Contract.IsDuplex() && (serviceEndpoint.Behaviors.Find<CallbackBehaviorAttribute>() == null))
            {
                serviceEndpoint.Behaviors.Insert(0, new CallbackBehaviorAttribute());
            }
        }

        private void AddSortedParts<T>(List<T> partDescriptionList, KeyedCollection<XmlQualifiedName, T> partDescriptionCollection) where T: MessagePartDescription
        {
            MessagePartDescription[] array = (MessagePartDescription[]) partDescriptionList.ToArray();
            if (array.Length > 1)
            {
                Array.Sort<MessagePartDescription>(array, new Comparison<MessagePartDescription>(TypeLoader.CompareMessagePartDescriptions));
            }
            foreach (T local in array)
            {
                if (partDescriptionCollection.Contains(new XmlQualifiedName(local.Name, local.Namespace)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageContractException(System.ServiceModel.SR.GetString("SFxDuplicateMessageParts", new object[] { local.Name, local.Namespace })));
                }
                partDescriptionCollection.Add(local);
            }
        }

        public static void ApplyServiceInheritance<IBehavior, TBehaviorCollection>(System.Type serviceType, TBehaviorCollection descriptionBehaviors, ServiceInheritanceCallback<IBehavior, TBehaviorCollection> callback) where IBehavior: class where TBehaviorCollection: KeyedByTypeCollection<IBehavior>
        {
            for (System.Type type = serviceType; type != null; type = type.BaseType)
            {
                AddBehaviorsAtOneScope<IBehavior, TBehaviorCollection>(type, descriptionBehaviors, callback);
            }
        }

        private void CheckDuplicateFaultContract(FaultDescriptionCollection faultDescriptionCollection, FaultDescription fault, string operationName)
        {
            foreach (FaultDescription description in faultDescriptionCollection)
            {
                if ((System.ServiceModel.Description.XmlName.IsNullOrEmpty(description.ElementName) && System.ServiceModel.Description.XmlName.IsNullOrEmpty(fault.ElementName)) && (description.DetailType == fault.DetailType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxFaultContractDuplicateDetailType", new object[] { operationName, fault.DetailType })));
                }
                if ((!System.ServiceModel.Description.XmlName.IsNullOrEmpty(description.ElementName) && !System.ServiceModel.Description.XmlName.IsNullOrEmpty(fault.ElementName)) && ((description.ElementName == fault.ElementName) && (description.Namespace == fault.Namespace)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxFaultContractDuplicateElement", new object[] { operationName, fault.ElementName, fault.Namespace })));
                }
            }
        }

        internal static int CompareMessagePartDescriptions(MessagePartDescription a, MessagePartDescription b)
        {
            int num = a.SerializationPosition - b.SerializationPosition;
            if (num != 0)
            {
                return num;
            }
            int num2 = string.Compare(a.Namespace, b.Namespace, StringComparison.Ordinal);
            if (num2 != 0)
            {
                return num2;
            }
            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }

        private ContractDescription CreateContractDescription(ServiceContractAttribute contractAttr, System.Type contractType, System.Type serviceType, out ContractReflectionInfo reflectionInfo, object serviceImplementation)
        {
            reflectionInfo = new ContractReflectionInfo();
            XmlQualifiedName name = NamingHelper.GetContractName(contractType, contractAttr.Name, contractAttr.Namespace);
            ContractDescription contractDescription = new ContractDescription(name.Name, name.Namespace) {
                ContractType = contractType
            };
            if (contractAttr.HasProtectionLevel)
            {
                contractDescription.ProtectionLevel = contractAttr.ProtectionLevel;
            }
            System.Type callbackContract = contractAttr.CallbackContract;
            EnsureCallbackType(callbackContract);
            EnsureSubcontract(contractAttr, contractType);
            reflectionInfo.iface = contractType;
            reflectionInfo.callbackiface = callbackContract;
            contractDescription.SessionMode = contractAttr.SessionMode;
            contractDescription.CallbackContractType = callbackContract;
            contractDescription.ConfigurationName = contractAttr.ConfigurationName ?? contractType.FullName;
            List<System.Type> inheritedContractTypes = ServiceReflector.GetInheritedContractTypes(contractType);
            List<System.Type> list2 = new List<System.Type>();
            for (int i = 0; i < inheritedContractTypes.Count; i++)
            {
                System.Type attrProvider = inheritedContractTypes[i];
                ServiceReflector.GetRequiredSingleAttribute<ServiceContractAttribute>(attrProvider);
                ContractDescription description2 = this.LoadContractDescriptionHelper(attrProvider, serviceType, serviceImplementation);
                foreach (OperationDescription description3 in description2.Operations)
                {
                    if (!contractDescription.Operations.Contains(description3))
                    {
                        foreach (OperationDescription description4 in contractDescription.Operations.FindAll(description3.Name))
                        {
                            if (description4.Messages[0].Direction == description3.Messages[0].Direction)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotInheritTwoOperationsWithTheSameName3", new object[] { description3.Name, description2.Name, description4.DeclaringContract.Name })));
                            }
                        }
                        contractDescription.Operations.Add(description3);
                    }
                }
                if (description2.CallbackContractType != null)
                {
                    list2.Add(description2.CallbackContractType);
                }
            }
            this.CreateOperationDescriptions(contractDescription, reflectionInfo, contractType, contractDescription, MessageDirection.Input);
            if ((callbackContract != null) && !list2.Contains(callbackContract))
            {
                this.CreateOperationDescriptions(contractDescription, reflectionInfo, callbackContract, contractDescription, MessageDirection.Output);
            }
            return contractDescription;
        }

        private FaultDescription CreateFaultDescription(FaultContractAttribute attr, XmlQualifiedName contractName, string contractNamespace, System.ServiceModel.Description.XmlName operationName)
        {
            System.ServiceModel.Description.XmlName name = new System.ServiceModel.Description.XmlName(attr.Name ?? (NamingHelper.TypeName(attr.DetailType) + "Fault"));
            FaultDescription description = new FaultDescription(NamingHelper.GetMessageAction(contractName, operationName.DecodedName + name.DecodedName, attr.Action, false));
            if (attr.Name != null)
            {
                description.SetNameAndElement(name);
            }
            else
            {
                description.SetNameOnly(name);
            }
            description.Namespace = attr.Namespace ?? contractNamespace;
            description.DetailType = attr.DetailType;
            if (attr.HasProtectionLevel)
            {
                description.ProtectionLevel = attr.ProtectionLevel;
            }
            return description;
        }

        private MessageDescription CreateMessageDescription(MethodInfo methodInfo, bool isAsync, System.ServiceModel.Description.XmlName returnValueName, string defaultNS, string action, System.ServiceModel.Description.XmlName wrapperName, string wrapperNamespace, MessageDirection direction)
        {
            MessageDescription description;
            string name = methodInfo.Name;
            if (returnValueName == null)
            {
                ParameterInfo[] inputParameters = ServiceReflector.GetInputParameters(methodInfo, isAsync);
                if ((inputParameters.Length == 1) && inputParameters[0].ParameterType.IsDefined(typeof(MessageContractAttribute), false))
                {
                    description = this.CreateTypedMessageDescription(inputParameters[0].ParameterType, null, null, defaultNS, action, direction);
                }
                else
                {
                    description = this.CreateParameterMessageDescription(inputParameters, null, null, null, name, defaultNS, action, wrapperName, wrapperNamespace, direction);
                }
            }
            else
            {
                ParameterInfo[] outputParameters = ServiceReflector.GetOutputParameters(methodInfo, isAsync);
                System.Type returnType = methodInfo.ReturnType;
                if (returnType.IsDefined(typeof(MessageContractAttribute), false) && (outputParameters.Length == 0))
                {
                    description = this.CreateTypedMessageDescription(returnType, methodInfo.ReturnTypeCustomAttributes, returnValueName, defaultNS, action, direction);
                }
                else
                {
                    description = this.CreateParameterMessageDescription(outputParameters, methodInfo.ReturnType, methodInfo.ReturnTypeCustomAttributes, returnValueName, name, defaultNS, action, wrapperName, wrapperNamespace, direction);
                }
            }
            bool flag = false;
            for (int i = 0; i < description.Headers.Count; i++)
            {
                MessageHeaderDescription description2 = description.Headers[i];
                if (description2.IsUnknownHeaderCollection)
                {
                    if (flag)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMultipleUnknownHeaders", new object[] { methodInfo, methodInfo.DeclaringType })));
                    }
                    flag = true;
                }
            }
            return description;
        }

        private MessageHeaderDescription CreateMessageHeaderDescription(System.Type headerParameterType, ICustomAttributeProvider attrProvider, System.ServiceModel.Description.XmlName defaultName, string defaultNS, int parameterIndex, int serializationPosition)
        {
            MessageHeaderDescription description = null;
            MessageHeaderAttribute requiredSingleAttribute = ServiceReflector.GetRequiredSingleAttribute<MessageHeaderAttribute>(attrProvider, messageContractMemberAttributes);
            System.ServiceModel.Description.XmlName name = requiredSingleAttribute.IsNameSetExplicit ? new System.ServiceModel.Description.XmlName(requiredSingleAttribute.Name) : defaultName;
            string ns = requiredSingleAttribute.IsNamespaceSetExplicit ? requiredSingleAttribute.Namespace : defaultNS;
            description = new MessageHeaderDescription(name.EncodedName, ns) {
                UniquePartName = defaultName.EncodedName
            };
            if (requiredSingleAttribute is MessageHeaderArrayAttribute)
            {
                if (!headerParameterType.IsArray || (headerParameterType.GetArrayRank() != 1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidMessageHeaderArrayType", new object[] { defaultName })));
                }
                description.Multiple = true;
                headerParameterType = headerParameterType.GetElementType();
            }
            description.Type = TypedHeaderManager.GetHeaderType(headerParameterType);
            description.TypedHeader = headerParameterType != description.Type;
            if (description.TypedHeader)
            {
                if ((requiredSingleAttribute.IsMustUnderstandSet || requiredSingleAttribute.IsRelaySet) || (requiredSingleAttribute.Actor != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxStaticMessageHeaderPropertiesNotAllowed", new object[] { defaultName })));
                }
            }
            else
            {
                description.Actor = requiredSingleAttribute.Actor;
                description.MustUnderstand = requiredSingleAttribute.MustUnderstand;
                description.Relay = requiredSingleAttribute.Relay;
            }
            description.SerializationPosition = serializationPosition;
            if (requiredSingleAttribute.HasProtectionLevel)
            {
                description.ProtectionLevel = requiredSingleAttribute.ProtectionLevel;
            }
            if (attrProvider is MemberInfo)
            {
                description.MemberInfo = (MemberInfo) attrProvider;
            }
            description.Index = parameterIndex;
            return description;
        }

        private MessagePartDescription CreateMessagePartDescription(System.Type bodyType, ICustomAttributeProvider attrProvider, System.ServiceModel.Description.XmlName defaultName, string defaultNS, int parameterIndex, int serializationIndex)
        {
            MessagePartDescription description = null;
            MessageBodyMemberAttribute singleAttribute = ServiceReflector.GetSingleAttribute<MessageBodyMemberAttribute>(attrProvider, messageContractMemberAttributes);
            if (singleAttribute == null)
            {
                description = new MessagePartDescription(defaultName.EncodedName, defaultNS) {
                    SerializationPosition = serializationIndex
                };
            }
            else
            {
                System.ServiceModel.Description.XmlName name = singleAttribute.IsNameSetExplicit ? new System.ServiceModel.Description.XmlName(singleAttribute.Name) : defaultName;
                string ns = singleAttribute.IsNamespaceSetExplicit ? singleAttribute.Namespace : defaultNS;
                description = new MessagePartDescription(name.EncodedName, ns) {
                    SerializationPosition = (singleAttribute.Order < 0) ? serializationIndex : singleAttribute.Order
                };
                if (singleAttribute.HasProtectionLevel)
                {
                    description.ProtectionLevel = singleAttribute.ProtectionLevel;
                }
            }
            if (attrProvider is MemberInfo)
            {
                description.MemberInfo = (MemberInfo) attrProvider;
            }
            description.Type = bodyType;
            description.Index = parameterIndex;
            return description;
        }

        private MessagePropertyDescription CreateMessagePropertyDescription(ICustomAttributeProvider attrProvider, System.ServiceModel.Description.XmlName defaultName, int parameterIndex)
        {
            MessagePropertyAttribute singleAttribute = ServiceReflector.GetSingleAttribute<MessagePropertyAttribute>(attrProvider, messageContractMemberAttributes);
            System.ServiceModel.Description.XmlName name = singleAttribute.IsNameSetExplicit ? new System.ServiceModel.Description.XmlName(singleAttribute.Name) : defaultName;
            MessagePropertyDescription description = new MessagePropertyDescription(name.EncodedName) {
                Index = parameterIndex
            };
            if (attrProvider is MemberInfo)
            {
                description.MemberInfo = (MemberInfo) attrProvider;
            }
            return description;
        }

        private OperationDescription CreateOperationDescription(ContractDescription contractDescription, MethodInfo methodInfo, MessageDirection direction, ContractReflectionInfo reflectionInfo, ContractDescription declaringContract)
        {
            OperationContractAttribute operationContractAttribute = ServiceReflector.GetOperationContractAttribute(methodInfo);
            if (operationContractAttribute == null)
            {
                return null;
            }
            if (ServiceReflector.HasEndMethodShape(methodInfo))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EndMethodsCannotBeDecoratedWithOperationContractAttribute", new object[] { methodInfo.Name, reflectionInfo.iface })));
            }
            bool isAsync = ServiceReflector.IsBegin(operationContractAttribute, methodInfo);
            System.ServiceModel.Description.XmlName operationName = NamingHelper.GetOperationName(ServiceReflector.GetLogicalName(methodInfo, isAsync), operationContractAttribute.Name);
            Collection<OperationDescription> collection = contractDescription.Operations.FindAll(operationName.EncodedName);
            for (int i = 0; i < collection.Count; i++)
            {
                OperationDescription description = collection[i];
                if (description.Messages[0].Direction == direction)
                {
                    if (isAsync && (description.BeginMethod != null))
                    {
                        string name = description.BeginMethod.Name;
                        string str2 = methodInfo.Name;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotHaveTwoOperationsWithTheSameName3", new object[] { name, str2, reflectionInfo.iface })));
                    }
                    if (!isAsync && (description.SyncMethod != null))
                    {
                        string str3 = description.SyncMethod.Name;
                        string str4 = methodInfo.Name;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotHaveTwoOperationsWithTheSameName3", new object[] { str3, str4, reflectionInfo.iface })));
                    }
                    contractDescription.Operations.Remove(description);
                    OperationDescription asyncOperation = this.CreateOperationDescription(contractDescription, methodInfo, direction, reflectionInfo, declaringContract);
                    asyncOperation.HasNoDisposableParameters = ServiceReflector.HasNoDisposableParameters(methodInfo);
                    if (isAsync)
                    {
                        description.BeginMethod = asyncOperation.BeginMethod;
                        description.EndMethod = asyncOperation.EndMethod;
                        this.VerifyConsistency(description, asyncOperation);
                        return description;
                    }
                    asyncOperation.BeginMethod = description.BeginMethod;
                    asyncOperation.EndMethod = description.EndMethod;
                    this.VerifyConsistency(asyncOperation, description);
                    return asyncOperation;
                }
            }
            OperationDescription description3 = new OperationDescription(operationName.EncodedName, declaringContract) {
                IsInitiating = operationContractAttribute.IsInitiating,
                IsTerminating = operationContractAttribute.IsTerminating,
                HasNoDisposableParameters = ServiceReflector.HasNoDisposableParameters(methodInfo)
            };
            if (operationContractAttribute.HasProtectionLevel)
            {
                description3.ProtectionLevel = operationContractAttribute.ProtectionLevel;
            }
            XmlQualifiedName contractName = new XmlQualifiedName(declaringContract.Name, declaringContract.Namespace);
            object[] knownTypeAttributes = ServiceReflector.GetCustomAttributes(methodInfo, typeof(FaultContractAttribute), false);
            if (operationContractAttribute.IsOneWay && (knownTypeAttributes.Length > 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OneWayAndFaultsIncompatible2", new object[] { methodInfo.DeclaringType.FullName, operationName.EncodedName })));
            }
            for (int j = 0; j < knownTypeAttributes.Length; j++)
            {
                FaultContractAttribute attr = (FaultContractAttribute) knownTypeAttributes[j];
                FaultDescription fault = this.CreateFaultDescription(attr, contractName, declaringContract.Namespace, description3.XmlName);
                this.CheckDuplicateFaultContract(description3.Faults, fault, operationName.EncodedName);
                description3.Faults.Add(fault);
            }
            knownTypeAttributes = ServiceReflector.GetCustomAttributes(methodInfo, typeof(ServiceKnownTypeAttribute), false);
            foreach (System.Type type in this.GetKnownTypes(knownTypeAttributes, methodInfo))
            {
                description3.KnownTypes.Add(type);
            }
            MessageDirection direction2 = direction;
            MessageDirection direction3 = MessageDirectionHelper.Opposite(direction);
            string action = NamingHelper.GetMessageAction(contractName, description3.CodeName, operationContractAttribute.Action, false);
            string str6 = NamingHelper.GetMessageAction(contractName, description3.CodeName, operationContractAttribute.ReplyAction, true);
            System.ServiceModel.Description.XmlName wrapperName = operationName;
            System.ServiceModel.Description.XmlName bodyWrapperResponseName = GetBodyWrapperResponseName(operationName);
            string wrapperNamespace = declaringContract.Namespace;
            MessageDescription item = this.CreateMessageDescription(methodInfo, isAsync, null, contractDescription.Namespace, action, wrapperName, wrapperNamespace, direction2);
            MessageDescription description6 = null;
            description3.Messages.Add(item);
            MethodInfo endMethod = methodInfo;
            if (!isAsync)
            {
                description3.SyncMethod = methodInfo;
            }
            else
            {
                endMethod = ServiceReflector.GetEndMethod(methodInfo);
                description3.EndMethod = endMethod;
                description3.BeginMethod = methodInfo;
            }
            if (!operationContractAttribute.IsOneWay)
            {
                System.ServiceModel.Description.XmlName returnValueName = GetReturnValueName(operationName);
                description6 = this.CreateMessageDescription(endMethod, isAsync, returnValueName, contractDescription.Namespace, str6, bodyWrapperResponseName, wrapperNamespace, direction3);
                description3.Messages.Add(description6);
            }
            else
            {
                if ((endMethod.ReturnType != typeof(void)) || ServiceReflector.HasOutputParameters(endMethod, isAsync))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ServiceOperationsMarkedWithIsOneWayTrueMust0")));
                }
                if (operationContractAttribute.ReplyAction != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OneWayOperationShouldNotSpecifyAReplyAction1", new object[] { operationName })));
                }
            }
            if (!operationContractAttribute.IsOneWay)
            {
                if (description6.IsVoid && (item.IsUntypedMessage || item.IsTypedMessage))
                {
                    description6.Body.WrapperName = (string) (description6.Body.WrapperNamespace = null);
                    return description3;
                }
                if (!item.IsVoid || (!description6.IsUntypedMessage && !description6.IsTypedMessage))
                {
                    return description3;
                }
                item.Body.WrapperName = (string) (item.Body.WrapperNamespace = null);
            }
            return description3;
        }

        private void CreateOperationDescriptions(ContractDescription contractDescription, ContractReflectionInfo reflectionInfo, System.Type contractToGetMethodsFrom, ContractDescription declaringContract, MessageDirection direction)
        {
            MessageDirectionHelper.Opposite(direction);
            if (!declaringContract.ContractType.IsAssignableFrom(contractDescription.ContractType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Bad contract inheritence. Contract {0} does not implement {1}", new object[] { declaringContract.ContractType.Name, contractDescription.ContractType.Name })));
            }
            foreach (MethodInfo info in contractToGetMethodsFrom.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (contractToGetMethodsFrom.IsInterface && (ServiceReflector.GetCustomAttributes(info, typeof(OperationBehaviorAttribute), false).Length != 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxOperationBehaviorAttributeOnlyOnServiceClass", new object[] { info.Name, contractToGetMethodsFrom.Name })));
                }
                ServiceReflector.ValidateParameterMetadata(info);
                OperationDescription item = this.CreateOperationDescription(contractDescription, info, direction, reflectionInfo, declaringContract);
                if (item != null)
                {
                    contractDescription.Operations.Add(item);
                }
            }
        }

        private MessageDescription CreateParameterMessageDescription(ParameterInfo[] parameters, System.Type returnType, ICustomAttributeProvider returnAttrProvider, System.ServiceModel.Description.XmlName returnValueName, string methodName, string defaultNS, string action, System.ServiceModel.Description.XmlName wrapperName, string wrapperNamespace, MessageDirection direction)
        {
            foreach (ParameterInfo info in parameters)
            {
                if (GetParameterType(info).IsDefined(typeof(MessageContractAttribute), false))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidMessageContractSignature", new object[] { methodName })));
                }
            }
            if ((returnType != null) && returnType.IsDefined(typeof(MessageContractAttribute), false))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidMessageContractSignature", new object[] { methodName })));
            }
            MessageDescription description = new MessageDescription(action, direction);
            MessagePartDescriptionCollection parts = description.Body.Parts;
            for (int i = 0; i < parameters.Length; i++)
            {
                MessagePartDescription item = CreateParameterPartDescription(new System.ServiceModel.Description.XmlName(parameters[i].Name), defaultNS, i, parameters[i], GetParameterType(parameters[i]));
                if (parts.Contains(new XmlQualifiedName(item.Name, item.Namespace)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageContractException(System.ServiceModel.SR.GetString("SFxDuplicateMessageParts", new object[] { item.Name, item.Namespace })));
                }
                description.Body.Parts.Add(item);
            }
            if (returnType != null)
            {
                description.Body.ReturnValue = CreateParameterPartDescription(returnValueName, defaultNS, 0, returnAttrProvider, returnType);
            }
            if (description.IsUntypedMessage)
            {
                description.Body.WrapperName = null;
                description.Body.WrapperNamespace = null;
                return description;
            }
            description.Body.WrapperName = wrapperName.EncodedName;
            description.Body.WrapperNamespace = wrapperNamespace;
            return description;
        }

        private static MessagePartDescription CreateParameterPartDescription(System.ServiceModel.Description.XmlName defaultName, string defaultNS, int index, ICustomAttributeProvider attrProvider, System.Type type)
        {
            MessageParameterAttribute singleAttribute = ServiceReflector.GetSingleAttribute<MessageParameterAttribute>(attrProvider);
            System.ServiceModel.Description.XmlName name = ((singleAttribute == null) || !singleAttribute.IsNameSetExplicit) ? defaultName : new System.ServiceModel.Description.XmlName(singleAttribute.Name);
            return new MessagePartDescription(name.EncodedName, defaultNS) { Type = type, Index = index, AdditionalAttributesProvider = attrProvider };
        }

        internal MessageDescription CreateTypedMessageDescription(System.Type typedMessageType, ICustomAttributeProvider returnAttrProvider, System.ServiceModel.Description.XmlName returnValueName, string defaultNS, string action, MessageDirection direction)
        {
            MessageDescription description;
            MessageDescriptionItems items;
            bool flag = false;
            MessageContractAttribute singleAttribute = ServiceReflector.GetSingleAttribute<MessageContractAttribute>(typedMessageType);
            if (this.messages.TryGetValue(typedMessageType, out items))
            {
                description = new MessageDescription(action, direction, items);
                flag = true;
            }
            else
            {
                description = new MessageDescription(action, direction, null);
            }
            description.MessageType = typedMessageType;
            description.MessageName = new System.ServiceModel.Description.XmlName(NamingHelper.TypeName(typedMessageType));
            if (singleAttribute.IsWrapped)
            {
                description.Body.WrapperName = GetWrapperName(singleAttribute.WrapperName, description.MessageName).EncodedName;
                description.Body.WrapperNamespace = singleAttribute.WrapperNamespace ?? defaultNS;
            }
            List<MemberInfo> list = new List<MemberInfo>();
            for (System.Type type = typedMessageType; ((type != null) && (type != typeof(object))) && (type != typeof(ValueType)); type = type.BaseType)
            {
                if (!type.IsDefined(typeof(MessageContractAttribute), false))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMessageContractBaseTypeNotValid", new object[] { type, typedMessageType })));
                }
                if (!description.HasProtectionLevel)
                {
                    MessageContractAttribute requiredSingleAttribute = ServiceReflector.GetRequiredSingleAttribute<MessageContractAttribute>(type);
                    if (requiredSingleAttribute.HasProtectionLevel)
                    {
                        description.ProtectionLevel = requiredSingleAttribute.ProtectionLevel;
                    }
                }
                if (!flag)
                {
                    foreach (MemberInfo info in type.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        if ((info.MemberType == MemberTypes.Field) || (info.MemberType == MemberTypes.Property))
                        {
                            PropertyInfo info2 = info as PropertyInfo;
                            if (info2 != null)
                            {
                                MethodInfo getMethod = info2.GetGetMethod(true);
                                if ((getMethod != null) && IsMethodOverriding(getMethod))
                                {
                                    continue;
                                }
                                MethodInfo setMethod = info2.GetSetMethod(true);
                                if ((setMethod != null) && IsMethodOverriding(setMethod))
                                {
                                    continue;
                                }
                            }
                            if ((info.IsDefined(typeof(MessageBodyMemberAttribute), false) || info.IsDefined(typeof(MessageHeaderAttribute), false)) || (info.IsDefined(typeof(MessageHeaderArrayAttribute), false) || info.IsDefined(typeof(MessagePropertyAttribute), false)))
                            {
                                list.Add(info);
                            }
                        }
                    }
                }
            }
            if (!flag)
            {
                List<MessagePartDescription> partDescriptionList = new List<MessagePartDescription>();
                List<MessageHeaderDescription> list3 = new List<MessageHeaderDescription>();
                for (int i = 0; i < list.Count; i++)
                {
                    System.Type propertyType;
                    MemberInfo attrProvider = list[i];
                    if (attrProvider.MemberType == MemberTypes.Property)
                    {
                        propertyType = ((PropertyInfo) attrProvider).PropertyType;
                    }
                    else
                    {
                        propertyType = ((FieldInfo) attrProvider).FieldType;
                    }
                    if (attrProvider.IsDefined(typeof(MessageHeaderArrayAttribute), false) || attrProvider.IsDefined(typeof(MessageHeaderAttribute), false))
                    {
                        list3.Add(this.CreateMessageHeaderDescription(propertyType, attrProvider, new System.ServiceModel.Description.XmlName(attrProvider.Name), defaultNS, i, -1));
                    }
                    else if (attrProvider.IsDefined(typeof(MessagePropertyAttribute), false))
                    {
                        description.Properties.Add(this.CreateMessagePropertyDescription(attrProvider, new System.ServiceModel.Description.XmlName(attrProvider.Name), i));
                    }
                    else
                    {
                        partDescriptionList.Add(this.CreateMessagePartDescription(propertyType, attrProvider, new System.ServiceModel.Description.XmlName(attrProvider.Name), defaultNS, i, -1));
                    }
                }
                if (returnAttrProvider != null)
                {
                    description.Body.ReturnValue = this.CreateMessagePartDescription(typeof(void), returnAttrProvider, returnValueName, defaultNS, 0, 0);
                }
                this.AddSortedParts<MessagePartDescription>(partDescriptionList, description.Body.Parts);
                this.AddSortedParts<MessageHeaderDescription>(list3, description.Headers);
                this.messages.Add(typedMessageType, description.Items);
            }
            return description;
        }

        internal static void EnsureCallbackType(System.Type callbackType)
        {
            if (((callbackType != null) && !callbackType.IsInterface) && !callbackType.IsMarshalByRef)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxInvalidCallbackContractType", new object[] { callbackType.Name })));
            }
        }

        private void EnsureNoInheritanceWithContractClasses(System.Type actualContractType)
        {
            if (actualContractType.IsClass)
            {
                for (System.Type type = actualContractType.BaseType; type != null; type = type.BaseType)
                {
                    if (ServiceReflector.GetSingleAttribute<ServiceContractAttribute>(type) != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxContractInheritanceRequiresInterfaces", new object[] { actualContractType, type })));
                    }
                }
            }
        }

        private void EnsureNoOperationContractsOnNonServiceContractTypes(System.Type actualContractType)
        {
            foreach (System.Type type in actualContractType.GetInterfaces())
            {
                this.EnsureNoOperationContractsOnNonServiceContractTypes_Helper(type);
            }
            for (System.Type type2 = actualContractType.BaseType; type2 != null; type2 = type2.BaseType)
            {
                this.EnsureNoOperationContractsOnNonServiceContractTypes_Helper(type2);
            }
        }

        private void EnsureNoOperationContractsOnNonServiceContractTypes_Helper(System.Type aParentType)
        {
            if (ServiceReflector.GetSingleAttribute<ServiceContractAttribute>(aParentType) == null)
            {
                foreach (MethodInfo info in aParentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    System.Type operationContractProviderType = ServiceReflector.GetOperationContractProviderType(info);
                    if (operationContractProviderType != null)
                    {
                        if (operationContractProviderType == OperationContractAttributeType)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxOperationContractOnNonServiceContract", new object[] { info.Name, aParentType.Name })));
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxOperationContractProviderOnNonServiceContract", new object[] { operationContractProviderType.Name, info.Name, aParentType.Name })));
                    }
                }
            }
        }

        internal static void EnsureSubcontract(ServiceContractAttribute svcContractAttr, System.Type contractType)
        {
            System.Type callbackContract = svcContractAttr.CallbackContract;
            List<System.Type> inheritedContractTypes = ServiceReflector.GetInheritedContractTypes(contractType);
            for (int i = 0; i < inheritedContractTypes.Count; i++)
            {
                System.Type attrProvider = inheritedContractTypes[i];
                ServiceContractAttribute requiredSingleAttribute = ServiceReflector.GetRequiredSingleAttribute<ServiceContractAttribute>(attrProvider);
                if (requiredSingleAttribute.CallbackContract != null)
                {
                    if (callbackContract == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InAContractInheritanceHierarchyIfParentHasCallbackChildMustToo", new object[] { attrProvider.Name, requiredSingleAttribute.CallbackContract.Name, contractType.Name })));
                    }
                    if (!requiredSingleAttribute.CallbackContract.IsAssignableFrom(callbackContract))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InAContractInheritanceHierarchyTheServiceContract3_2", new object[] { attrProvider.Name, contractType.Name })));
                    }
                }
            }
        }

        internal static System.ServiceModel.Description.XmlName GetBodyWrapperResponseName(System.ServiceModel.Description.XmlName operationName)
        {
            return new System.ServiceModel.Description.XmlName(operationName.EncodedName + "Response", true);
        }

        internal static System.ServiceModel.Description.XmlName GetBodyWrapperResponseName(string operationName)
        {
            return new System.ServiceModel.Description.XmlName(operationName + "Response");
        }

        internal static System.Attribute GetFormattingAttribute(ICustomAttributeProvider attrProvider, System.Attribute defaultFormatAttribute)
        {
            if (attrProvider != null)
            {
                if (attrProvider.IsDefined(typeof(XmlSerializerFormatAttribute), false))
                {
                    return ServiceReflector.GetSingleAttribute<XmlSerializerFormatAttribute>(attrProvider, formatterAttributes);
                }
                if (attrProvider.IsDefined(typeof(DataContractFormatAttribute), false))
                {
                    return ServiceReflector.GetSingleAttribute<DataContractFormatAttribute>(attrProvider, formatterAttributes);
                }
            }
            return defaultFormatAttribute;
        }

        private void GetIContractBehaviorsFromInterfaceType(System.Type interfaceType, KeyedByTypeCollection<IContractBehavior> behaviors)
        {
            foreach (IContractBehavior behavior in ServiceReflector.GetCustomAttributes(interfaceType, typeof(IContractBehavior), false))
            {
                behaviors.Add(behavior);
            }
        }

        private KeyedByTypeCollection<IOperationBehavior> GetIOperationBehaviorAttributesFromType(OperationDescription opDesc, System.Type targetIface, System.Type implType)
        {
            KeyedByTypeCollection<IOperationBehavior> result = new KeyedByTypeCollection<IOperationBehavior>();
            InterfaceMapping ifaceMap = new InterfaceMapping();
            bool useImplAttrs = false;
            if (implType != null)
            {
                if (!targetIface.IsAssignableFrom(implType) || !targetIface.IsInterface)
                {
                    return result;
                }
                ifaceMap = implType.GetInterfaceMap(targetIface);
                useImplAttrs = true;
            }
            MethodInfo operationMethod = opDesc.OperationMethod;
            this.ProcessOpMethod(operationMethod, true, opDesc, result, ifaceMap, useImplAttrs);
            if ((opDesc.SyncMethod != null) && (opDesc.BeginMethod != null))
            {
                this.ProcessOpMethod(opDesc.BeginMethod, false, opDesc, result, ifaceMap, useImplAttrs);
            }
            return result;
        }

        private IEnumerable<System.Type> GetKnownTypes(object[] knownTypeAttributes, ICustomAttributeProvider provider)
        {
            if (knownTypeAttributes.Length == 1)
            {
                ServiceKnownTypeAttribute attribute = (ServiceKnownTypeAttribute) knownTypeAttributes[0];
                if (!string.IsNullOrEmpty(attribute.MethodName))
                {
                    System.Type declaringType = attribute.DeclaringType;
                    if (declaringType == null)
                    {
                        declaringType = provider as System.Type;
                        if (declaringType == null)
                        {
                            declaringType = ((MethodInfo) provider).DeclaringType;
                        }
                    }
                    MethodInfo info = declaringType.GetMethod(attribute.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, knownTypesMethodParamType, null);
                    if (info == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxKnownTypeAttributeUnknownMethod3", new object[] { provider, attribute.MethodName, declaringType.FullName })));
                    }
                    if (!typeof(IEnumerable<System.Type>).IsAssignableFrom(info.ReturnType))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxKnownTypeAttributeReturnType3", new object[] { provider, attribute.MethodName, declaringType.FullName })));
                    }
                    return (IEnumerable<System.Type>) info.Invoke(null, new object[] { provider });
                }
            }
            List<System.Type> list = new List<System.Type>();
            for (int i = 0; i < knownTypeAttributes.Length; i++)
            {
                ServiceKnownTypeAttribute attribute2 = (ServiceKnownTypeAttribute) knownTypeAttributes[i];
                if (attribute2.Type == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxKnownTypeAttributeInvalid1", new object[] { provider.ToString() })));
                }
                list.Add(attribute2.Type);
            }
            return list;
        }

        internal static System.Type GetParameterType(ParameterInfo parameterInfo)
        {
            System.Type parameterType = parameterInfo.ParameterType;
            if (parameterType.IsByRef)
            {
                return parameterType.GetElementType();
            }
            return parameterType;
        }

        internal static System.ServiceModel.Description.XmlName GetReturnValueName(System.ServiceModel.Description.XmlName methodName)
        {
            return new System.ServiceModel.Description.XmlName(methodName.EncodedName + "Result", true);
        }

        internal static System.ServiceModel.Description.XmlName GetReturnValueName(string methodName)
        {
            return new System.ServiceModel.Description.XmlName(methodName + "Result");
        }

        internal static System.ServiceModel.Description.XmlName GetWrapperName(string wrapperName, System.ServiceModel.Description.XmlName defaultName)
        {
            if (string.IsNullOrEmpty(wrapperName))
            {
                return defaultName;
            }
            return new System.ServiceModel.Description.XmlName(wrapperName);
        }

        private static bool IsMethodOverriding(MethodInfo method)
        {
            return (method.IsVirtual && ((method.Attributes & MethodAttributes.NewSlot) == MethodAttributes.PrivateScope));
        }

        public ContractDescription LoadContractDescription(System.Type contractType)
        {
            return this.LoadContractDescriptionHelper(contractType, null, null);
        }

        public ContractDescription LoadContractDescription(System.Type contractType, System.Type serviceType)
        {
            return this.LoadContractDescriptionHelper(contractType, serviceType, null);
        }

        public ContractDescription LoadContractDescription(System.Type contractType, System.Type serviceType, object serviceImplementation)
        {
            return this.LoadContractDescriptionHelper(contractType, serviceType, serviceImplementation);
        }

        private ContractDescription LoadContractDescriptionHelper(System.Type contractType, System.Type serviceType, object serviceImplementation)
        {
            ContractDescription description;
            ServiceContractAttribute attribute;
            if (contractType == typeof(IOutputChannel))
            {
                return this.LoadOutputChannelContractDescription();
            }
            if (contractType == typeof(IRequestChannel))
            {
                return this.LoadRequestChannelContractDescription();
            }
            System.Type contractTypeAndAttribute = ServiceReflector.GetContractTypeAndAttribute(contractType, out attribute);
            lock (this.thisLock)
            {
                ContractReflectionInfo info;
                if (this.contracts.TryGetValue(contractTypeAndAttribute, out description))
                {
                    return description;
                }
                this.EnsureNoInheritanceWithContractClasses(contractTypeAndAttribute);
                this.EnsureNoOperationContractsOnNonServiceContractTypes(contractTypeAndAttribute);
                description = this.CreateContractDescription(attribute, contractTypeAndAttribute, serviceType, out info, serviceImplementation);
                if ((serviceImplementation != null) && (serviceImplementation is IContractBehavior))
                {
                    description.Behaviors.Add((IContractBehavior) serviceImplementation);
                }
                if (serviceType != null)
                {
                    UpdateContractDescriptionWithAttributesFromServiceType(description, serviceType);
                    foreach (ContractDescription description2 in description.GetInheritedContracts())
                    {
                        UpdateContractDescriptionWithAttributesFromServiceType(description2, serviceType);
                    }
                }
                this.UpdateOperationsWithInterfaceAttributes(description, info);
                this.AddBehaviors(description, serviceType, false, info);
                this.contracts.Add(contractTypeAndAttribute, description);
            }
            return description;
        }

        private ContractDescription LoadOutputChannelContractDescription()
        {
            System.Type contractType = typeof(IOutputChannel);
            XmlQualifiedName name = NamingHelper.GetContractName(contractType, null, "http://schemas.microsoft.com/2005/07/ServiceModel");
            ContractDescription declaringContract = new ContractDescription(name.Name, name.Namespace) {
                ContractType = contractType,
                ConfigurationName = contractType.FullName,
                SessionMode = SessionMode.NotAllowed
            };
            OperationDescription item = new OperationDescription("Send", declaringContract);
            MessageDescription description3 = new MessageDescription("*", MessageDirection.Input);
            item.Messages.Add(description3);
            declaringContract.Operations.Add(item);
            return declaringContract;
        }

        private ContractDescription LoadRequestChannelContractDescription()
        {
            System.Type contractType = typeof(IRequestChannel);
            XmlQualifiedName name = NamingHelper.GetContractName(contractType, null, "http://schemas.microsoft.com/2005/07/ServiceModel");
            ContractDescription declaringContract = new ContractDescription(name.Name, name.Namespace) {
                ContractType = contractType,
                ConfigurationName = contractType.FullName,
                SessionMode = SessionMode.NotAllowed
            };
            OperationDescription item = new OperationDescription("Request", declaringContract);
            MessageDescription description3 = new MessageDescription("*", MessageDirection.Input);
            MessageDescription description4 = new MessageDescription("*", MessageDirection.Output);
            item.Messages.Add(description3);
            item.Messages.Add(description4);
            declaringContract.Operations.Add(item);
            return declaringContract;
        }

        private void ProcessOpMethod(MethodInfo opMethod, bool canHaveBehaviors, OperationDescription opDesc, KeyedByTypeCollection<IOperationBehavior> result, InterfaceMapping ifaceMap, bool useImplAttrs)
        {
            MethodInfo attrProvider = null;
            if (useImplAttrs)
            {
                int index = Array.IndexOf<MethodInfo>(ifaceMap.InterfaceMethods, opMethod);
                if (index != -1)
                {
                    MethodInfo info2 = ifaceMap.TargetMethods[index];
                    if (info2 != null)
                    {
                        attrProvider = info2;
                    }
                }
                if (attrProvider == null)
                {
                    return;
                }
            }
            else
            {
                attrProvider = opMethod;
            }
            foreach (IOperationBehavior behavior in ServiceReflector.GetCustomAttributes(attrProvider, typeof(IOperationBehavior), false))
            {
                if (!canHaveBehaviors)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SyncAsyncMatchConsistency_Attributes6", new object[] { opDesc.SyncMethod.Name, opDesc.SyncMethod.DeclaringType, opDesc.BeginMethod.Name, opDesc.EndMethod.Name, opDesc.Name, behavior.GetType().FullName })));
                }
                result.Add(behavior);
            }
        }

        private static void UpdateContractDescriptionWithAttributesFromServiceType(ContractDescription description, System.Type serviceType)
        {
            ApplyServiceInheritance<IContractBehavior, KeyedByTypeCollection<IContractBehavior>>(serviceType, description.Behaviors, delegate (System.Type currentType, KeyedByTypeCollection<IContractBehavior> behaviors) {
                foreach (IContractBehavior behavior in ServiceReflector.GetCustomAttributes(currentType, typeof(IContractBehavior), false))
                {
                    IContractBehaviorAttribute attribute = behavior as IContractBehaviorAttribute;
                    if (((attribute == null) || (attribute.TargetContract == null)) || (attribute.TargetContract == description.ContractType))
                    {
                        behaviors.Add(behavior);
                    }
                }
            });
        }

        private void UpdateOperationsWithInterfaceAttributes(ContractDescription contractDesc, ContractReflectionInfo reflectionInfo)
        {
            object[] knownTypeAttributes = ServiceReflector.GetCustomAttributes(reflectionInfo.iface, typeof(ServiceKnownTypeAttribute), false);
            foreach (System.Type type in this.GetKnownTypes(knownTypeAttributes, reflectionInfo.iface))
            {
                foreach (OperationDescription description in contractDesc.Operations)
                {
                    if (!description.IsServerInitiated())
                    {
                        description.KnownTypes.Add(type);
                    }
                }
            }
            if (reflectionInfo.callbackiface != null)
            {
                knownTypeAttributes = ServiceReflector.GetCustomAttributes(reflectionInfo.callbackiface, typeof(ServiceKnownTypeAttribute), false);
                foreach (System.Type type2 in this.GetKnownTypes(knownTypeAttributes, reflectionInfo.callbackiface))
                {
                    foreach (OperationDescription description2 in contractDesc.Operations)
                    {
                        if (description2.IsServerInitiated())
                        {
                            description2.KnownTypes.Add(type2);
                        }
                    }
                }
            }
        }

        private void VerifyConsistency(OperationDescription syncOperation, OperationDescription asyncOperation)
        {
            ParameterInfo[] inputParameters = ServiceReflector.GetInputParameters(syncOperation.SyncMethod, false);
            ParameterInfo[] infoArray2 = ServiceReflector.GetInputParameters(syncOperation.BeginMethod, true);
            ParameterInfo[] outputParameters = ServiceReflector.GetOutputParameters(syncOperation.SyncMethod, false);
            ParameterInfo[] infoArray4 = ServiceReflector.GetOutputParameters(syncOperation.EndMethod, true);
            if ((inputParameters.Length != infoArray2.Length) || (outputParameters.Length != infoArray4.Length))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SyncAsyncMatchConsistency_Parameters5", new object[] { syncOperation.SyncMethod.Name, syncOperation.SyncMethod.DeclaringType, asyncOperation.BeginMethod.Name, asyncOperation.EndMethod.Name, syncOperation.Name })));
            }
            for (int i = 0; i < inputParameters.Length; i++)
            {
                if (inputParameters[i].ParameterType != infoArray2[i].ParameterType)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SyncAsyncMatchConsistency_Parameters5", new object[] { syncOperation.SyncMethod.Name, syncOperation.SyncMethod.DeclaringType, asyncOperation.BeginMethod.Name, asyncOperation.EndMethod.Name, syncOperation.Name })));
                }
            }
            for (int j = 0; j < outputParameters.Length; j++)
            {
                if (outputParameters[j].ParameterType != infoArray4[j].ParameterType)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SyncAsyncMatchConsistency_Parameters5", new object[] { syncOperation.SyncMethod.Name, syncOperation.SyncMethod.DeclaringType, asyncOperation.BeginMethod.Name, asyncOperation.EndMethod.Name, syncOperation.Name })));
                }
            }
            if (syncOperation.SyncMethod.ReturnType != syncOperation.EndMethod.ReturnType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SyncAsyncMatchConsistency_ReturnType5", new object[] { syncOperation.SyncMethod.Name, syncOperation.SyncMethod.DeclaringType, asyncOperation.BeginMethod.Name, asyncOperation.EndMethod.Name, syncOperation.Name })));
            }
            if (asyncOperation.Faults.Count != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SyncAsyncMatchConsistency_Attributes6", new object[] { syncOperation.SyncMethod.Name, syncOperation.SyncMethod.DeclaringType, asyncOperation.BeginMethod.Name, asyncOperation.EndMethod.Name, syncOperation.Name, typeof(FaultContractAttribute).Name })));
            }
            if (asyncOperation.KnownTypes.Count != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SyncAsyncMatchConsistency_Attributes6", new object[] { syncOperation.SyncMethod.Name, syncOperation.SyncMethod.DeclaringType, asyncOperation.BeginMethod.Name, asyncOperation.EndMethod.Name, syncOperation.Name, typeof(ServiceKnownTypeAttribute).Name })));
            }
            if (syncOperation.Messages.Count != asyncOperation.Messages.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SyncAsyncMatchConsistency_Property6", new object[] { syncOperation.SyncMethod.Name, syncOperation.SyncMethod.DeclaringType, asyncOperation.BeginMethod.Name, asyncOperation.EndMethod.Name, syncOperation.Name, "IsOneWay" })));
            }
            for (int k = 0; k < syncOperation.Messages.Count; k++)
            {
                if (syncOperation.Messages[k].Action != asyncOperation.Messages[k].Action)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SyncAsyncMatchConsistency_Property6", new object[] { syncOperation.SyncMethod.Name, syncOperation.SyncMethod.DeclaringType, asyncOperation.BeginMethod.Name, asyncOperation.EndMethod.Name, syncOperation.Name, (k == 0) ? "Action" : "ReplyAction" })));
                }
            }
        }

        private class ContractReflectionInfo
        {
            internal System.Type callbackiface;
            internal System.Type iface;
        }

        public delegate void ServiceInheritanceCallback<IBehavior, TBehaviorCollection>(System.Type currentType, KeyedByTypeCollection<IBehavior> behaviors);
    }
}

