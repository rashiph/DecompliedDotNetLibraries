namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class InputChannelAcceptor : SingletonChannelAcceptor<IInputChannel, InputChannel, Message>
    {
        public InputChannelAcceptor(ChannelManagerBase channelManager) : base(channelManager)
        {
        }

        protected override InputChannel OnCreateChannel()
        {
            return new InputChannel(base.ChannelManager, null);
        }

        protected override void OnTraceMessageReceived(Message message)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40013, System.ServiceModel.SR.GetString("TraceCodeMessageReceived"), MessageTransmitTraceRecord.CreateReceiveTraceRecord(message), this, null);
            }
        }
    }
}

