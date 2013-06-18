namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class TransactionChannelFaultConverter<TChannel> : FaultConverter where TChannel: class, IChannel
    {
        private TransactionChannel<TChannel> channel;

        internal TransactionChannelFaultConverter(TransactionChannel<TChannel> channel)
        {
            this.channel = channel;
        }

        protected override bool OnTryCreateException(Message message, MessageFault fault, out Exception exception)
        {
            if (message.Headers.Action == "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions/fault")
            {
                exception = new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                return true;
            }
            if (fault.IsMustUnderstandFault)
            {
                MessageHeader emptyTransactionHeader = this.channel.Formatter.EmptyTransactionHeader;
                if (MessageFault.WasHeaderNotUnderstood(message.Headers, emptyTransactionHeader.Name, emptyTransactionHeader.Namespace))
                {
                    exception = new ProtocolException(System.ServiceModel.SR.GetString("SFxTransactionHeaderNotUnderstood", new object[] { emptyTransactionHeader.Name, emptyTransactionHeader.Namespace, this.channel.Protocol }));
                    return true;
                }
            }
            FaultConverter innerProperty = this.channel.GetInnerProperty<FaultConverter>();
            if (innerProperty != null)
            {
                return innerProperty.TryCreateException(message, fault, out exception);
            }
            exception = null;
            return false;
        }

        protected override bool OnTryCreateFaultMessage(Exception exception, out Message message)
        {
            FaultConverter innerProperty = this.channel.GetInnerProperty<FaultConverter>();
            if (innerProperty != null)
            {
                return innerProperty.TryCreateFaultMessage(exception, out message);
            }
            message = null;
            return false;
        }
    }
}

