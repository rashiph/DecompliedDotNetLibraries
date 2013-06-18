namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public abstract class TypedMessageConverter
    {
        protected TypedMessageConverter()
        {
        }

        public static TypedMessageConverter Create(System.Type messageContract, string action)
        {
            return Create(messageContract, action, null, TypeLoader.DefaultDataContractFormatAttribute);
        }

        public static TypedMessageConverter Create(System.Type messageContract, string action, DataContractFormatAttribute formatterAttribute)
        {
            return Create(messageContract, action, null, formatterAttribute);
        }

        public static TypedMessageConverter Create(System.Type messageContract, string action, XmlSerializerFormatAttribute formatterAttribute)
        {
            return Create(messageContract, action, null, formatterAttribute);
        }

        public static TypedMessageConverter Create(System.Type messageContract, string action, string defaultNamespace)
        {
            return Create(messageContract, action, defaultNamespace, TypeLoader.DefaultDataContractFormatAttribute);
        }

        public static TypedMessageConverter Create(System.Type messageContract, string action, string defaultNamespace, DataContractFormatAttribute formatterAttribute)
        {
            if (messageContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageContract"));
            }
            if (!messageContract.IsDefined(typeof(MessageContractAttribute), false))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxMessageContractAttributeRequired", new object[] { messageContract }), "messageContract"));
            }
            if (defaultNamespace == null)
            {
                defaultNamespace = "http://tempuri.org/";
            }
            return new XmlMessageConverter(GetOperationFormatter(messageContract, formatterAttribute, defaultNamespace, action));
        }

        public static TypedMessageConverter Create(System.Type messageContract, string action, string defaultNamespace, XmlSerializerFormatAttribute formatterAttribute)
        {
            if (messageContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageContract"));
            }
            if (defaultNamespace == null)
            {
                defaultNamespace = "http://tempuri.org/";
            }
            return new XmlMessageConverter(GetOperationFormatter(messageContract, formatterAttribute, defaultNamespace, action));
        }

        public abstract object FromMessage(Message message);
        private static OperationFormatter GetOperationFormatter(System.Type t, Attribute formatAttribute, string defaultNS, string action)
        {
            bool flag = formatAttribute is XmlSerializerFormatAttribute;
            MessageDescription description = new TypeLoader().CreateTypedMessageDescription(t, null, null, defaultNS, action, MessageDirection.Output);
            ContractDescription declaringContract = new ContractDescription("dummy_contract", defaultNS);
            OperationDescription operation = new OperationDescription(NamingHelper.XmlName(t.Name), declaringContract, false) {
                Messages = { description }
            };
            if (flag)
            {
                return XmlSerializerOperationBehavior.CreateOperationFormatter(operation, (XmlSerializerFormatAttribute) formatAttribute);
            }
            return new DataContractSerializerOperationFormatter(operation, (DataContractFormatAttribute) formatAttribute, null);
        }

        public abstract Message ToMessage(object typedMessage);
        public abstract Message ToMessage(object typedMessage, MessageVersion version);
    }
}

