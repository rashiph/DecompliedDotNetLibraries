namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.ServiceModel.Channels;

    internal class MsmqIntegrationInputMessage : MsmqInputMessage
    {
        private NativeMsmqMessage.ByteProperty acknowledge;
        private NativeMsmqMessage.StringProperty adminQueue;
        private NativeMsmqMessage.IntProperty adminQueueLength;
        private NativeMsmqMessage.IntProperty appSpecific;
        private NativeMsmqMessage.IntProperty arrivedTime;
        private NativeMsmqMessage.ByteProperty authenticated;
        private NativeMsmqMessage.IntProperty bodyType;
        private NativeMsmqMessage.BufferProperty correlationId;
        private NativeMsmqMessage.StringProperty destinationQueue;
        private NativeMsmqMessage.IntProperty destinationQueueLength;
        private NativeMsmqMessage.BufferProperty extension;
        private NativeMsmqMessage.IntProperty extensionLength;
        private const int initialExtensionLength = 0;
        private const int initialLabelLength = 0x80;
        private const int initialQueueNameLength = 0x100;
        private NativeMsmqMessage.StringProperty label;
        private NativeMsmqMessage.IntProperty labelLength;
        private const int maxSize = 0x400000;
        private NativeMsmqMessage.ByteProperty priority;
        private NativeMsmqMessage.IntProperty privacyLevel;
        private NativeMsmqMessage.StringProperty responseFormatName;
        private NativeMsmqMessage.IntProperty responseFormatNameLength;
        private NativeMsmqMessage.IntProperty senderIdType;
        private NativeMsmqMessage.IntProperty sentTime;
        private NativeMsmqMessage.IntProperty timeToReachQueue;

        public MsmqIntegrationInputMessage() : this(0x400000)
        {
        }

        public MsmqIntegrationInputMessage(int maxBufferSize) : this(new MsmqInputMessage.SizeQuota(maxBufferSize))
        {
        }

        protected MsmqIntegrationInputMessage(MsmqInputMessage.SizeQuota bufferSizeQuota) : base(0x16, bufferSizeQuota)
        {
            this.acknowledge = new NativeMsmqMessage.ByteProperty(this, 6);
            this.adminQueue = new NativeMsmqMessage.StringProperty(this, 0x11, 0x100);
            this.adminQueueLength = new NativeMsmqMessage.IntProperty(this, 0x12, 0x100);
            this.appSpecific = new NativeMsmqMessage.IntProperty(this, 8);
            this.arrivedTime = new NativeMsmqMessage.IntProperty(this, 0x20);
            this.senderIdType = new NativeMsmqMessage.IntProperty(this, 0x16);
            this.authenticated = new NativeMsmqMessage.ByteProperty(this, 0x19);
            this.bodyType = new NativeMsmqMessage.IntProperty(this, 0x2a);
            this.correlationId = new NativeMsmqMessage.BufferProperty(this, 3, 20);
            this.destinationQueue = new NativeMsmqMessage.StringProperty(this, 0x3a, 0x100);
            this.destinationQueueLength = new NativeMsmqMessage.IntProperty(this, 0x3b, 0x100);
            this.extension = new NativeMsmqMessage.BufferProperty(this, 0x23, bufferSizeQuota.AllocIfAvailable(0));
            this.extensionLength = new NativeMsmqMessage.IntProperty(this, 0x24, 0);
            this.label = new NativeMsmqMessage.StringProperty(this, 11, 0x80);
            this.labelLength = new NativeMsmqMessage.IntProperty(this, 12, 0x80);
            this.priority = new NativeMsmqMessage.ByteProperty(this, 4);
            this.responseFormatName = new NativeMsmqMessage.StringProperty(this, 0x36, 0x100);
            this.responseFormatNameLength = new NativeMsmqMessage.IntProperty(this, 0x37, 0x100);
            this.sentTime = new NativeMsmqMessage.IntProperty(this, 0x1f);
            this.timeToReachQueue = new NativeMsmqMessage.IntProperty(this, 13);
            this.privacyLevel = new NativeMsmqMessage.IntProperty(this, 0x17);
        }

        private static Uri GetQueueName(string formatName)
        {
            if (string.IsNullOrEmpty(formatName))
            {
                return null;
            }
            return new Uri("msmq.formatname:" + formatName);
        }

        protected override void OnGrowBuffers(MsmqInputMessage.SizeQuota bufferSizeQuota)
        {
            base.OnGrowBuffers(bufferSizeQuota);
            this.adminQueue.EnsureValueLength(this.adminQueueLength.Value);
            this.responseFormatName.EnsureValueLength(this.responseFormatNameLength.Value);
            this.destinationQueue.EnsureValueLength(this.destinationQueueLength.Value);
            this.label.EnsureValueLength(this.labelLength.Value);
            bufferSizeQuota.Alloc(this.extensionLength.Value);
            this.extension.EnsureBufferLength(this.extensionLength.Value);
        }

        public void SetMessageProperties(MsmqIntegrationMessageProperty property)
        {
            property.AcknowledgeType = new AcknowledgeTypes?((AcknowledgeTypes) this.acknowledge.Value);
            property.Acknowledgment = new Acknowledgment?((Acknowledgment) base.Class.Value);
            property.AdministrationQueue = GetQueueName(this.adminQueue.GetValue(this.adminQueueLength.Value));
            property.AppSpecific = new int?(this.appSpecific.Value);
            property.ArrivedTime = new DateTime?(MsmqDateTime.ToDateTime(this.arrivedTime.Value).ToLocalTime());
            property.Authenticated = new bool?(this.authenticated.Value != 0);
            property.BodyType = new int?(this.bodyType.Value);
            property.CorrelationId = MsmqMessageId.ToString(this.correlationId.Buffer);
            property.DestinationQueue = GetQueueName(this.destinationQueue.GetValue(this.destinationQueueLength.Value));
            property.Extension = this.extension.GetBufferCopy(this.extensionLength.Value);
            property.Id = MsmqMessageId.ToString(base.MessageId.Buffer);
            property.Label = this.label.GetValue(this.labelLength.Value);
            if (base.Class.Value == 0)
            {
                property.MessageType = 2;
            }
            else if (base.Class.Value == 1)
            {
                property.MessageType = 3;
            }
            else
            {
                property.MessageType = 1;
            }
            property.Priority = new MessagePriority?((MessagePriority) this.priority.Value);
            property.ResponseQueue = GetQueueName(this.responseFormatName.GetValue(this.responseFormatNameLength.Value));
            property.SenderId = base.SenderId.GetBufferCopy(base.SenderIdLength.Value);
            property.SentTime = new DateTime?(MsmqDateTime.ToDateTime(this.sentTime.Value).ToLocalTime());
            property.InternalSetTimeToReachQueue(MsmqDuration.ToTimeSpan(this.timeToReachQueue.Value));
        }
    }
}

