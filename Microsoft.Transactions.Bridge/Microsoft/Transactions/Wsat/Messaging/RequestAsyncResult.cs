namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class RequestAsyncResult : AsyncResult
    {
        private UniqueId messageID;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private Message reply;

        public RequestAsyncResult(Message message, AsyncCallback callback, object state) : base(callback, state)
        {
            this.messageVersion = message.Version;
            this.messageID = message.Headers.MessageId;
        }

        public void End()
        {
            AsyncResult.End<Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult>(this);
        }

        public void Finished(Exception exception)
        {
            base.Complete(false, exception);
        }

        public void Finished(Message reply)
        {
            this.reply = reply;
            base.Complete(false);
        }

        public UniqueId MessageId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.messageID;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.messageVersion;
            }
        }

        public Message Reply
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.reply;
            }
        }
    }
}

