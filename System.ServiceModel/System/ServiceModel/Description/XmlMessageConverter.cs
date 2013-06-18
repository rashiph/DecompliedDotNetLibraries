namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class XmlMessageConverter : TypedMessageConverter
    {
        private OperationFormatter formatter;

        internal XmlMessageConverter(OperationFormatter formatter)
        {
            this.formatter = formatter;
        }

        public override object FromMessage(Message message)
        {
            if (((this.Action != null) && (message.Headers.Action != null)) && (message.Headers.Action != this.Action))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxActionMismatch", new object[] { this.Action, message.Headers.Action })));
            }
            object[] parameters = new object[1];
            this.formatter.DeserializeRequest(message, parameters);
            return parameters[0];
        }

        public override Message ToMessage(object typedMessage)
        {
            return this.ToMessage(typedMessage, MessageVersion.Soap12WSAddressing10);
        }

        public override Message ToMessage(object typedMessage, MessageVersion version)
        {
            if (typedMessage == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("typedMessage"));
            }
            return this.formatter.SerializeRequest(version, new object[] { typedMessage });
        }

        internal string Action
        {
            get
            {
                return this.formatter.RequestAction;
            }
        }
    }
}

