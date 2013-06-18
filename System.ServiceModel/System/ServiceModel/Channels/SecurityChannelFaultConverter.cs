namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal class SecurityChannelFaultConverter : FaultConverter
    {
        private IChannel innerChannel;

        internal SecurityChannelFaultConverter(IChannel innerChannel)
        {
            this.innerChannel = innerChannel;
        }

        protected override bool OnTryCreateException(Message message, MessageFault fault, out Exception exception)
        {
            if (this.innerChannel == null)
            {
                exception = null;
                return false;
            }
            FaultConverter property = this.innerChannel.GetProperty<FaultConverter>();
            if (property != null)
            {
                return property.TryCreateException(message, fault, out exception);
            }
            exception = null;
            return false;
        }

        protected override bool OnTryCreateFaultMessage(Exception exception, out Message message)
        {
            if (this.innerChannel == null)
            {
                message = null;
                return false;
            }
            FaultConverter property = this.innerChannel.GetProperty<FaultConverter>();
            if (property != null)
            {
                return property.TryCreateFaultMessage(exception, out message);
            }
            message = null;
            return false;
        }
    }
}

