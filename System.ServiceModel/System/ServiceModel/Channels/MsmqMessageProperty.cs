namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public sealed class MsmqMessageProperty
    {
        private int abortCount;
        private int acknowledge;
        private long lookupId;
        private string messageId;
        private int moveCount;
        public const string Name = "MsmqMessageProperty";

        internal MsmqMessageProperty(MsmqInputMessage msmqMessage)
        {
            if (msmqMessage == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("msmqMessage");
            }
            this.lookupId = msmqMessage.LookupId.Value;
            if (msmqMessage.AbortCount != null)
            {
                this.abortCount = msmqMessage.AbortCount.Value;
            }
            if (msmqMessage.MoveCount != null)
            {
                this.moveCount = msmqMessage.MoveCount.Value;
            }
            this.acknowledge = (ushort) msmqMessage.Class.Value;
            this.messageId = MsmqMessageId.ToString(msmqMessage.MessageId.Buffer);
        }

        public static MsmqMessageProperty Get(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (message.Properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message.Properties");
            }
            return (message.Properties["MsmqMessageProperty"] as MsmqMessageProperty);
        }

        private static System.ServiceModel.Channels.DeliveryFailure? TryGetDeliveryFailure(string messageId, int acknowledgment)
        {
            if ((0x8000 & acknowledgment) == 0)
            {
                return null;
            }
            int num2 = 0x4000 & acknowledgment;
            int num3 = -49153 & acknowledgment;
            if ((((num2 == 0) && (num3 >= 0)) && (num3 <= 10)) || (((num2 != 0) && (num3 >= 0)) && (num3 <= 2)))
            {
                return new System.ServiceModel.Channels.DeliveryFailure?((System.ServiceModel.Channels.DeliveryFailure) acknowledgment);
            }
            MsmqDiagnostics.UnexpectedAcknowledgment(messageId, acknowledgment);
            return 0;
        }

        public int AbortCount
        {
            get
            {
                return this.abortCount;
            }
            internal set
            {
                this.abortCount = value;
            }
        }

        public System.ServiceModel.Channels.DeliveryFailure? DeliveryFailure
        {
            get
            {
                return TryGetDeliveryFailure(this.messageId, this.acknowledge);
            }
        }

        public System.ServiceModel.Channels.DeliveryStatus? DeliveryStatus
        {
            get
            {
                System.ServiceModel.Channels.DeliveryFailure? deliveryFailure = this.DeliveryFailure;
                if (!deliveryFailure.HasValue)
                {
                    return null;
                }
                if ((System.ServiceModel.Channels.DeliveryFailure.ReachQueueTimeout != ((System.ServiceModel.Channels.DeliveryFailure) deliveryFailure.Value)) && (((System.ServiceModel.Channels.DeliveryFailure) deliveryFailure.Value) != System.ServiceModel.Channels.DeliveryFailure.Unknown))
                {
                    return 1;
                }
                return 0;
            }
        }

        internal long LookupId
        {
            get
            {
                return this.lookupId;
            }
        }

        internal string MessageId
        {
            get
            {
                return this.messageId;
            }
        }

        public int MoveCount
        {
            get
            {
                return this.moveCount;
            }
            internal set
            {
                this.moveCount = value;
            }
        }
    }
}

