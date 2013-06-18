namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [Serializable]
    public class CallbackContextMessageProperty : IMessageProperty
    {
        [NonSerialized]
        private EndpointAddress callbackAddress;
        private readonly IDictionary<string, string> context;
        [NonSerialized]
        private readonly EndpointAddress listenAddress;
        private const string PropertyName = "CallbackContextMessageProperty";

        public CallbackContextMessageProperty(IDictionary<string, string> context) : this((EndpointAddress) null, context)
        {
        }

        public CallbackContextMessageProperty(EndpointAddress callbackAddress)
        {
            if (callbackAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackAddress");
            }
            this.callbackAddress = callbackAddress;
        }

        public CallbackContextMessageProperty(EndpointAddress listenAddress, IDictionary<string, string> context)
        {
            if ((listenAddress != null) && (listenAddress.Headers.FindHeader("Context", "http://schemas.microsoft.com/ws/2006/05/context") != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ListenAddressAlreadyContainsContext"));
            }
            this.listenAddress = listenAddress;
            this.context = context;
        }

        public CallbackContextMessageProperty(string listenAddress, IDictionary<string, string> context) : this(new Uri(listenAddress), context)
        {
        }

        public CallbackContextMessageProperty(Uri listenAddress, IDictionary<string, string> context) : this(new EndpointAddress(listenAddress, new AddressHeader[0]), context)
        {
        }

        public void AddOrReplaceInMessage(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            this.AddOrReplaceInMessageProperties(message.Properties);
        }

        public void AddOrReplaceInMessageProperties(MessageProperties properties)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            properties["CallbackContextMessageProperty"] = this;
        }

        public EndpointAddress CreateCallbackAddress(Uri listenAddress)
        {
            if (listenAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listenAddress");
            }
            return CreateCallbackAddress(new EndpointAddress(listenAddress, new AddressHeader[0]), this.context);
        }

        private static EndpointAddress CreateCallbackAddress(EndpointAddress listenAddress, IDictionary<string, string> context)
        {
            if (listenAddress == null)
            {
                return null;
            }
            EndpointAddressBuilder builder = new EndpointAddressBuilder(listenAddress);
            if (context != null)
            {
                builder.Headers.Add(new ContextAddressHeader(context));
            }
            return builder.ToEndpointAddress();
        }

        public IMessageProperty CreateCopy()
        {
            if (this.callbackAddress != null)
            {
                return new CallbackContextMessageProperty(this.callbackAddress);
            }
            return new CallbackContextMessageProperty(this.listenAddress, this.context);
        }

        public void GetListenAddressAndContext(out EndpointAddress listenAddress, out IDictionary<string, string> context)
        {
            if (this.CallbackAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackaddress");
            }
            EndpointAddressBuilder builder = new EndpointAddressBuilder(this.CallbackAddress);
            AddressHeader header = null;
            int index = -1;
            for (int i = 0; i < builder.Headers.Count; i++)
            {
                if ((builder.Headers[i].Name == "Context") && (builder.Headers[i].Namespace == "http://schemas.microsoft.com/ws/2006/05/context"))
                {
                    if (header != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MultipleContextHeadersFoundInCallbackAddress")));
                    }
                    header = builder.Headers[i];
                    index = i;
                }
            }
            if (header != null)
            {
                builder.Headers.RemoveAt(index);
            }
            context = (header != null) ? ContextMessageHeader.ParseContextHeader(header.GetAddressHeaderReader()).Context : null;
            listenAddress = builder.ToEndpointAddress();
        }

        public static bool TryGet(Message message, out CallbackContextMessageProperty contextMessageProperty)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return TryGet(message.Properties, out contextMessageProperty);
        }

        public static bool TryGet(MessageProperties properties, out CallbackContextMessageProperty contextMessageProperty)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            object obj2 = null;
            if (properties.TryGetValue("CallbackContextMessageProperty", out obj2))
            {
                contextMessageProperty = obj2 as CallbackContextMessageProperty;
            }
            else
            {
                contextMessageProperty = null;
            }
            return (contextMessageProperty != null);
        }

        public EndpointAddress CallbackAddress
        {
            get
            {
                if ((this.callbackAddress == null) && (this.listenAddress != null))
                {
                    this.callbackAddress = CreateCallbackAddress(this.listenAddress, this.context);
                }
                return this.callbackAddress;
            }
        }

        public IDictionary<string, string> Context
        {
            get
            {
                return this.context;
            }
        }

        public static string Name
        {
            get
            {
                return "CallbackContextMessageProperty";
            }
        }
    }
}

