namespace System.Messaging
{
    using System;
    using System.ComponentModel;
    using System.Messaging.Design;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DefaultPropertiesToSend
    {
        private MessageQueue cachedAdminQueue;
        private Message cachedMessage;
        private MessageQueue cachedResponseQueue;
        private MessageQueue cachedTransactionStatusQueue;
        private bool designMode;

        public DefaultPropertiesToSend()
        {
            this.cachedMessage = new Message();
        }

        internal DefaultPropertiesToSend(bool designMode)
        {
            this.cachedMessage = new Message();
            this.designMode = designMode;
        }

        private bool ShouldSerializeExtension()
        {
            return ((this.Extension != null) && (this.Extension.Length > 0));
        }

        private bool ShouldSerializeTimeToBeReceived()
        {
            if (this.TimeToBeReceived == Message.InfiniteTimeout)
            {
                return false;
            }
            return true;
        }

        private bool ShouldSerializeTimeToReachQueue()
        {
            if (this.TimeToReachQueue == Message.InfiniteTimeout)
            {
                return false;
            }
            return true;
        }

        [MessagingDescription("MsgAcknowledgeType"), DefaultValue(0)]
        public AcknowledgeTypes AcknowledgeType
        {
            get
            {
                return this.cachedMessage.AcknowledgeType;
            }
            set
            {
                this.cachedMessage.AcknowledgeType = value;
            }
        }

        [DefaultValue((string) null), MessagingDescription("MsgAdministrationQueue")]
        public MessageQueue AdministrationQueue
        {
            get
            {
                if (!this.designMode)
                {
                    return this.cachedMessage.AdministrationQueue;
                }
                if ((this.cachedAdminQueue != null) && (this.cachedAdminQueue.Site == null))
                {
                    this.cachedAdminQueue = null;
                }
                return this.cachedAdminQueue;
            }
            set
            {
                if (this.designMode)
                {
                    this.cachedAdminQueue = value;
                }
                else
                {
                    this.cachedMessage.AdministrationQueue = value;
                }
            }
        }

        [DefaultValue(0), MessagingDescription("MsgAppSpecific")]
        public int AppSpecific
        {
            get
            {
                return this.cachedMessage.AppSpecific;
            }
            set
            {
                this.cachedMessage.AppSpecific = value;
            }
        }

        [MessagingDescription("MsgAttachSenderId"), DefaultValue(true)]
        public bool AttachSenderId
        {
            get
            {
                return this.cachedMessage.AttachSenderId;
            }
            set
            {
                this.cachedMessage.AttachSenderId = value;
            }
        }

        internal Message CachedMessage
        {
            get
            {
                return this.cachedMessage;
            }
        }

        [DefaultValue(0x6602), MessagingDescription("MsgEncryptionAlgorithm")]
        public System.Messaging.EncryptionAlgorithm EncryptionAlgorithm
        {
            get
            {
                return this.cachedMessage.EncryptionAlgorithm;
            }
            set
            {
                this.cachedMessage.EncryptionAlgorithm = value;
            }
        }

        [Editor("System.ComponentModel.Design.ArrayEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), MessagingDescription("MsgExtension")]
        public byte[] Extension
        {
            get
            {
                return this.cachedMessage.Extension;
            }
            set
            {
                this.cachedMessage.Extension = value;
            }
        }

        [DefaultValue(0x8003), MessagingDescription("MsgHashAlgorithm")]
        public System.Messaging.HashAlgorithm HashAlgorithm
        {
            get
            {
                return this.cachedMessage.HashAlgorithm;
            }
            set
            {
                this.cachedMessage.HashAlgorithm = value;
            }
        }

        [MessagingDescription("MsgLabel"), DefaultValue("")]
        public string Label
        {
            get
            {
                return this.cachedMessage.Label;
            }
            set
            {
                this.cachedMessage.Label = value;
            }
        }

        [DefaultValue(3), MessagingDescription("MsgPriority")]
        public MessagePriority Priority
        {
            get
            {
                return this.cachedMessage.Priority;
            }
            set
            {
                this.cachedMessage.Priority = value;
            }
        }

        [MessagingDescription("MsgRecoverable"), DefaultValue(false)]
        public bool Recoverable
        {
            get
            {
                return this.cachedMessage.Recoverable;
            }
            set
            {
                this.cachedMessage.Recoverable = value;
            }
        }

        [MessagingDescription("MsgResponseQueue"), DefaultValue((string) null)]
        public MessageQueue ResponseQueue
        {
            get
            {
                if (this.designMode)
                {
                    return this.cachedResponseQueue;
                }
                return this.cachedMessage.ResponseQueue;
            }
            set
            {
                if (this.designMode)
                {
                    this.cachedResponseQueue = value;
                }
                else
                {
                    this.cachedMessage.ResponseQueue = value;
                }
            }
        }

        [TypeConverter(typeof(TimeoutConverter)), MessagingDescription("MsgTimeToBeReceived")]
        public TimeSpan TimeToBeReceived
        {
            get
            {
                return this.cachedMessage.TimeToBeReceived;
            }
            set
            {
                this.cachedMessage.TimeToBeReceived = value;
            }
        }

        [TypeConverter(typeof(TimeoutConverter)), MessagingDescription("MsgTimeToReachQueue")]
        public TimeSpan TimeToReachQueue
        {
            get
            {
                return this.cachedMessage.TimeToReachQueue;
            }
            set
            {
                this.cachedMessage.TimeToReachQueue = value;
            }
        }

        [DefaultValue((string) null), MessagingDescription("MsgTransactionStatusQueue")]
        public MessageQueue TransactionStatusQueue
        {
            get
            {
                if (this.designMode)
                {
                    return this.cachedTransactionStatusQueue;
                }
                return this.cachedMessage.TransactionStatusQueue;
            }
            set
            {
                if (this.designMode)
                {
                    this.cachedTransactionStatusQueue = value;
                }
                else
                {
                    this.cachedMessage.TransactionStatusQueue = value;
                }
            }
        }

        [DefaultValue(false), MessagingDescription("MsgUseAuthentication")]
        public bool UseAuthentication
        {
            get
            {
                return this.cachedMessage.UseAuthentication;
            }
            set
            {
                this.cachedMessage.UseAuthentication = value;
            }
        }

        [DefaultValue(false), MessagingDescription("MsgUseDeadLetterQueue")]
        public bool UseDeadLetterQueue
        {
            get
            {
                return this.cachedMessage.UseDeadLetterQueue;
            }
            set
            {
                this.cachedMessage.UseDeadLetterQueue = value;
            }
        }

        [MessagingDescription("MsgUseEncryption"), DefaultValue(false)]
        public bool UseEncryption
        {
            get
            {
                return this.cachedMessage.UseEncryption;
            }
            set
            {
                this.cachedMessage.UseEncryption = value;
            }
        }

        [DefaultValue(false), MessagingDescription("MsgUseJournalQueue")]
        public bool UseJournalQueue
        {
            get
            {
                return this.cachedMessage.UseJournalQueue;
            }
            set
            {
                this.cachedMessage.UseJournalQueue = value;
            }
        }

        [MessagingDescription("MsgUseTracing"), DefaultValue(false)]
        public bool UseTracing
        {
            get
            {
                return this.cachedMessage.UseTracing;
            }
            set
            {
                this.cachedMessage.UseTracing = value;
            }
        }
    }
}

