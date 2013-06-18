namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Xml.Serialization;

    internal static class MessageBuilder
    {
        private static Type messageContractAttributeType;
        private static System.Xml.Serialization.XmlReflectionImporter xmlReflectionImporter;
        private static System.Runtime.Serialization.XsdDataContractExporter xsdDataContractExporter;

        public static void AddMessagePartDescription(OperationDescription operation, bool isResponse, MessageDescription message, Type type, SerializerOption serializerOption)
        {
            if (type != null)
            {
                string name;
                string str2;
                if (serializerOption == SerializerOption.DataContractSerializer)
                {
                    XmlQualifiedName rootElementName = XsdDataContractExporter.GetRootElementName(type);
                    if (rootElementName == null)
                    {
                        rootElementName = XsdDataContractExporter.GetSchemaTypeName(type);
                    }
                    if (!rootElementName.IsEmpty)
                    {
                        name = rootElementName.Name;
                        str2 = rootElementName.Namespace;
                    }
                    else
                    {
                        name = type.Name;
                        str2 = operation.DeclaringContract.Namespace;
                    }
                }
                else
                {
                    XmlTypeMapping mapping = XmlReflectionImporter.ImportTypeMapping(type);
                    name = mapping.ElementName;
                    str2 = mapping.Namespace;
                }
                MessagePartDescription item = new MessagePartDescription(NamingHelper.XmlName(name), str2) {
                    Index = 0,
                    Type = type
                };
                message.Body.Parts.Add(item);
            }
            if (isResponse)
            {
                SetReturnValue(message, operation);
            }
        }

        public static void AddMessagePartDescription(OperationDescription operation, bool isResponse, MessageDescription message, string[] argumentNames, Type[] argumentTypes)
        {
            string ns = operation.DeclaringContract.Namespace;
            for (int i = 0; i < argumentNames.Length; i++)
            {
                string name = argumentNames[i];
                MessagePartDescription item = new MessagePartDescription(NamingHelper.XmlName(name), ns) {
                    Index = i,
                    Type = argumentTypes[i]
                };
                message.Body.Parts.Add(item);
            }
            if (isResponse)
            {
                SetReturnValue(message, operation);
            }
        }

        public static void ClearWrapperNames(OperationDescription operation)
        {
            if (!operation.IsOneWay)
            {
                MessageDescription description = operation.Messages[0];
                MessageDescription description2 = operation.Messages[1];
                if (description2.IsVoid && (description.IsUntypedMessage || description.IsTypedMessage))
                {
                    description2.Body.WrapperName = null;
                    description2.Body.WrapperNamespace = null;
                }
                else if (description.IsVoid && (description2.IsUntypedMessage || description2.IsTypedMessage))
                {
                    description.Body.WrapperName = null;
                    description.Body.WrapperNamespace = null;
                }
            }
        }

        public static MessageDescription CreateEmptyMessageDescription(OperationDescription operation, bool isResponse, MessageDirection direction, string overridingAction)
        {
            return new MessageDescription(overridingAction ?? NamingHelper.GetMessageAction(operation, isResponse), direction) { Body = { WrapperName = null, WrapperNamespace = null } };
        }

        public static FaultDescription CreateFaultDescription(OperationDescription operation, Type faultType, string overridingAction)
        {
            string name = NamingHelper.TypeName(faultType) + "Fault";
            string action = overridingAction ?? (NamingHelper.GetMessageAction(operation, false) + name);
            FaultDescription description = new FaultDescription(action) {
                Namespace = operation.DeclaringContract.Namespace,
                DetailType = faultType
            };
            description.SetNameOnly(new System.ServiceModel.Description.XmlName(name));
            return description;
        }

        public static MessageDescription CreateFromMessageContract(OperationDescription operation, bool isResponse, MessageDirection direction, string overridingAction, Type messageContractType)
        {
            string action = overridingAction ?? NamingHelper.GetMessageAction(operation, isResponse);
            TypeLoader loader = new TypeLoader();
            return loader.CreateTypedMessageDescription(messageContractType, null, null, operation.DeclaringContract.Namespace, action, direction);
        }

        public static MessageDescription CreateMessageDescription(OperationDescription operation, bool isResponse, MessageDirection direction, string overridingAction, Type type, SerializerOption serializerOption)
        {
            if ((type != null) && IsMessageContract(type))
            {
                return CreateFromMessageContract(operation, isResponse, direction, overridingAction, type);
            }
            MessageDescription message = CreateEmptyMessageDescription(operation, isResponse, direction, overridingAction);
            AddMessagePartDescription(operation, isResponse, message, type, serializerOption);
            return message;
        }

        public static MessageDescription CreateMessageDescription(OperationDescription operation, bool isResponse, MessageDirection direction, string overridingAction, string[] argumentNames, Type[] argumentTypes)
        {
            MessageDescription description;
            if ((argumentTypes.Length == 1) && (argumentTypes[0] == MessageDescription.TypeOfUntypedMessage))
            {
                description = CreateEmptyMessageDescription(operation, isResponse, direction, overridingAction);
                AddMessagePartDescription(operation, isResponse, description, argumentNames, argumentTypes);
                return description;
            }
            if ((argumentTypes.Length == 1) && IsMessageContract(argumentTypes[0]))
            {
                return CreateFromMessageContract(operation, isResponse, direction, overridingAction, argumentTypes[0]);
            }
            description = CreateEmptyMessageDescription(operation, isResponse, direction, overridingAction);
            AddMessagePartDescription(operation, isResponse, description, argumentNames, argumentTypes);
            SetWrapperName(operation, isResponse, description);
            return description;
        }

        public static bool IsMessageContract(Type type)
        {
            if (type == null)
            {
                return false;
            }
            return type.IsDefined(MessageContractAttributeType, false);
        }

        private static void SetReturnValue(MessageDescription message, OperationDescription operation)
        {
            if (message.IsUntypedMessage)
            {
                message.Body.ReturnValue = message.Body.Parts[0];
                message.Body.Parts.RemoveAt(0);
            }
            else if (!message.IsTypedMessage)
            {
                message.Body.ReturnValue = new MessagePartDescription(operation.Name + "Result", operation.DeclaringContract.Namespace);
                message.Body.ReturnValue.Type = TypeHelper.VoidType;
            }
        }

        public static void SetWrapperName(OperationDescription operation, bool isResponse, MessageDescription message)
        {
            message.Body.WrapperName = operation.Name + (isResponse ? "Response" : string.Empty);
            message.Body.WrapperNamespace = operation.DeclaringContract.Namespace;
        }

        public static Type MessageContractAttributeType
        {
            get
            {
                if (messageContractAttributeType == null)
                {
                    messageContractAttributeType = typeof(MessageContractAttribute);
                }
                return messageContractAttributeType;
            }
        }

        private static System.Xml.Serialization.XmlReflectionImporter XmlReflectionImporter
        {
            get
            {
                if (xmlReflectionImporter == null)
                {
                    xmlReflectionImporter = new System.Xml.Serialization.XmlReflectionImporter();
                }
                return xmlReflectionImporter;
            }
        }

        private static System.Runtime.Serialization.XsdDataContractExporter XsdDataContractExporter
        {
            get
            {
                if (xsdDataContractExporter == null)
                {
                    xsdDataContractExporter = new System.Runtime.Serialization.XsdDataContractExporter();
                }
                return xsdDataContractExporter;
            }
        }
    }
}

