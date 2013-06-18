namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    public abstract class FaultConverter
    {
        protected FaultConverter()
        {
        }

        public static FaultConverter GetDefaultFaultConverter(MessageVersion version)
        {
            return new DefaultFaultConverter(version);
        }

        protected abstract bool OnTryCreateException(Message message, MessageFault fault, out Exception exception);
        protected abstract bool OnTryCreateFaultMessage(Exception exception, out Message message);
        public bool TryCreateException(Message message, MessageFault fault, out Exception exception)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (fault == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("fault");
            }
            bool flag = this.OnTryCreateException(message, fault, out exception);
            if (flag)
            {
                if (exception == null)
                {
                    Exception exception2 = new InvalidOperationException(System.ServiceModel.SR.GetString("FaultConverterDidNotCreateException", new object[] { base.GetType().Name }));
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2);
                }
                return flag;
            }
            if (exception != null)
            {
                Exception exception3 = new InvalidOperationException(System.ServiceModel.SR.GetString("FaultConverterCreatedException", new object[] { base.GetType().Name }), exception);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception3);
            }
            return flag;
        }

        public bool TryCreateFaultMessage(Exception exception, out Message message)
        {
            bool flag = this.OnTryCreateFaultMessage(exception, out message);
            if (flag)
            {
                if (message == null)
                {
                    Exception exception2 = new InvalidOperationException(System.ServiceModel.SR.GetString("FaultConverterDidNotCreateFaultMessage", new object[] { base.GetType().Name }));
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2);
                }
                return flag;
            }
            if (message != null)
            {
                Exception exception3 = new InvalidOperationException(System.ServiceModel.SR.GetString("FaultConverterCreatedFaultMessage", new object[] { base.GetType().Name }));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception3);
            }
            return flag;
        }

        private class DefaultFaultConverter : FaultConverter
        {
            private MessageVersion version;

            internal DefaultFaultConverter(MessageVersion version)
            {
                this.version = version;
            }

            protected override bool OnTryCreateException(Message message, MessageFault fault, out Exception exception)
            {
                bool isSenderFault;
                bool isReceiverFault;
                FaultCode subCode;
                exception = null;
                if ((string.Compare(fault.Code.Namespace, this.version.Envelope.Namespace, StringComparison.Ordinal) == 0) && (string.Compare(fault.Code.Name, "MustUnderstand", StringComparison.Ordinal) == 0))
                {
                    exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                    return true;
                }
                if (this.version.Envelope == EnvelopeVersion.Soap11)
                {
                    isSenderFault = true;
                    isReceiverFault = true;
                    subCode = fault.Code;
                }
                else
                {
                    isSenderFault = fault.Code.IsSenderFault;
                    isReceiverFault = fault.Code.IsReceiverFault;
                    subCode = fault.Code.SubCode;
                }
                if (subCode != null)
                {
                    if (subCode.Namespace == null)
                    {
                        return false;
                    }
                    if (isSenderFault && (string.Compare(subCode.Namespace, this.version.Addressing.Namespace, StringComparison.Ordinal) == 0))
                    {
                        if (string.Compare(subCode.Name, "ActionNotSupported", StringComparison.Ordinal) == 0)
                        {
                            exception = new ActionNotSupportedException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                            return true;
                        }
                        if (string.Compare(subCode.Name, "DestinationUnreachable", StringComparison.Ordinal) == 0)
                        {
                            exception = new EndpointNotFoundException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                            return true;
                        }
                        if (string.Compare(subCode.Name, "InvalidAddressingHeader", StringComparison.Ordinal) == 0)
                        {
                            if (((subCode.SubCode != null) && (string.Compare(subCode.SubCode.Namespace, this.version.Addressing.Namespace, StringComparison.Ordinal) == 0)) && (string.Compare(subCode.SubCode.Name, "InvalidCardinality", StringComparison.Ordinal) == 0))
                            {
                                exception = new MessageHeaderException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text, true);
                                return true;
                            }
                        }
                        else if (this.version.Addressing == AddressingVersion.WSAddressing10)
                        {
                            if (string.Compare(subCode.Name, "MessageAddressingHeaderRequired", StringComparison.Ordinal) == 0)
                            {
                                exception = new MessageHeaderException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                                return true;
                            }
                            if (string.Compare(subCode.Name, "InvalidAddressingHeader", StringComparison.Ordinal) == 0)
                            {
                                exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                                return true;
                            }
                        }
                        else
                        {
                            if (string.Compare(subCode.Name, "MessageInformationHeaderRequired", StringComparison.Ordinal) == 0)
                            {
                                exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                                return true;
                            }
                            if (string.Compare(subCode.Name, "InvalidMessageInformationHeader", StringComparison.Ordinal) == 0)
                            {
                                exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                                return true;
                            }
                        }
                    }
                    if ((isReceiverFault && (string.Compare(subCode.Namespace, this.version.Addressing.Namespace, StringComparison.Ordinal) == 0)) && (string.Compare(subCode.Name, "EndpointUnavailable", StringComparison.Ordinal) == 0))
                    {
                        exception = new ServerTooBusyException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                        return true;
                    }
                }
                return false;
            }

            protected override bool OnTryCreateFaultMessage(Exception exception, out Message message)
            {
                if (this.version.Addressing == AddressingVersion.WSAddressing10)
                {
                    if (exception is MessageHeaderException)
                    {
                        MessageHeaderException exception2 = exception as MessageHeaderException;
                        if (exception2.HeaderNamespace == AddressingVersion.WSAddressing10.Namespace)
                        {
                            message = exception2.ProvideFault(this.version);
                            return true;
                        }
                    }
                    else if (exception is ActionMismatchAddressingException)
                    {
                        message = (exception as ActionMismatchAddressingException).ProvideFault(this.version);
                        return true;
                    }
                }
                if ((this.version.Addressing != AddressingVersion.None) && (exception is ActionNotSupportedException))
                {
                    message = (exception as ActionNotSupportedException).ProvideFault(this.version);
                    return true;
                }
                if (exception is MustUnderstandSoapException)
                {
                    message = (exception as MustUnderstandSoapException).ProvideFault(this.version);
                    return true;
                }
                message = null;
                return false;
            }
        }
    }
}

