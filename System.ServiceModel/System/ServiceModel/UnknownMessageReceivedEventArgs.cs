namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    public sealed class UnknownMessageReceivedEventArgs : EventArgs
    {
        private System.ServiceModel.Channels.Message message;

        internal UnknownMessageReceivedEventArgs(System.ServiceModel.Channels.Message message)
        {
            this.message = message;
        }

        public System.ServiceModel.Channels.Message Message
        {
            get
            {
                return this.message;
            }
        }
    }
}

