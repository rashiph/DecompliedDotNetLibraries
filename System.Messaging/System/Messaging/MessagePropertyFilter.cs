namespace System.Messaging
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MessagePropertyFilter : ICloneable
    {
        internal const int ACKNOWLEDGE_TYPE = 4;
        internal const int ACKNOWLEDGEMENT = 1;
        internal const int ADMIN_QUEUE = 8;
        internal const int APP_SPECIFIC = 1;
        internal const int ARRIVED_TIME = 4;
        internal const int ATTACH_SENDER_ID = 8;
        internal const int AUTHENTICATED = 0x10;
        internal const int BODY = 0x10;
        internal int bodySize = 0x400;
        internal const int CONNECTOR_TYPE = 0x20;
        internal const int CORRELATION_ID = 0x40;
        internal const int CRYPTOGRAPHIC_PROVIDER_NAME = 0x80;
        internal const int CRYPTOGRAPHIC_PROVIDER_TYPE = 0x100;
        internal int data1;
        internal int data2;
        private const int defaultBodySize = 0x400;
        private const int defaultExtensionSize = 0xff;
        private const int defaultLabelSize = 0xff;
        internal const int DESTINATION_QUEUE = 0x8000;
        internal const int DIGITAL_SIGNATURE = 0x400;
        internal const int ENCRYPTION_ALGORITHM = 0x800;
        internal const int EXTENSION = 0x1000;
        internal int extensionSize = 0xff;
        internal const int FOREIGN_ADMIN_QUEUE = 0x2000;
        internal const int HASH_ALGORITHM = 0x4000;
        internal const int ID = 0x40;
        internal const int IS_FIRST_IN_TRANSACTION = 0x20000000;
        internal const int IS_LAST_IN_TRANSACTION = 0x40000000;
        internal const int IS_RECOVERABLE = 0x200;
        internal const int LABEL = 0x20;
        internal int labelSize = 0xff;
        internal const int LOOKUP_ID = 0x800;
        internal const int MESSAGE_TYPE = 0x200;
        internal const int PRIORITY = 0x10000;
        internal const int RESPONSE_QUEUE = 0x100;
        internal const int SECURITY_CONTEXT = 0x20000;
        internal const int SENDER_CERTIFICATE = 0x40000;
        internal const int SENDER_ID = 0x80000;
        internal const int SENT_TIME = 0x100000;
        internal const int SOURCE_MACHINE = 0x200000;
        internal const int SYMMETRIC_KEY = 0x400000;
        internal const int TIME_TO_BE_RECEIVED = 0x800000;
        internal const int TIME_TO_REACH_QUEUE = 0x1000000;
        internal const int TRANSACTION_ID = -2147483648;
        internal const int USE_AUTHENTICATION = 0x2000000;
        internal const int USE_DEADLETTER_QUEUE = 0x80;
        internal const int USE_ENCRYPTION = 0x4000000;
        internal const int USE_JOURNALING = 0x400;
        internal const int USE_TRACING = 0x8000000;
        internal const int VERSION = 0x10000000;

        public void ClearAll()
        {
            this.data1 = 0;
            this.data2 = 0;
        }

        public virtual object Clone()
        {
            return base.MemberwiseClone();
        }

        public void SetAll()
        {
            this.data1 = 0xffd;
            this.data2 = -3;
        }

        public void SetDefaults()
        {
            this.data1 = 0xffd;
            this.data2 = 0;
            this.DefaultBodySize = 0x400;
            this.DefaultExtensionSize = 0xff;
            this.DefaultLabelSize = 0xff;
        }

        [MessagingDescription("MsgAcknowledgeType"), DefaultValue(true)]
        public bool AcknowledgeType
        {
            get
            {
                return ((this.data1 & 4) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 4) : (this.data1 & -5);
            }
        }

        [DefaultValue(true), MessagingDescription("MsgAcknowledgement")]
        public bool Acknowledgment
        {
            get
            {
                return ((this.data1 & 1) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 1) : (this.data1 & -2);
            }
        }

        [DefaultValue(true), MessagingDescription("MsgAdministrationQueue")]
        public bool AdministrationQueue
        {
            get
            {
                return ((this.data1 & 8) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 8) : (this.data1 & -9);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgAppSpecific")]
        public bool AppSpecific
        {
            get
            {
                return ((this.data2 & 1) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 1) : (this.data2 & -2);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgArrivedTime")]
        public bool ArrivedTime
        {
            get
            {
                return ((this.data2 & 4) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 4) : (this.data2 & -5);
            }
        }

        [MessagingDescription("MsgAttachSenderId"), DefaultValue(false)]
        public bool AttachSenderId
        {
            get
            {
                return ((this.data2 & 8) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 8) : (this.data2 & -9);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgAuthenticated")]
        public bool Authenticated
        {
            get
            {
                return ((this.data2 & 0x10) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x10) : (this.data2 & -17);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgAuthenticationProviderName")]
        public bool AuthenticationProviderName
        {
            get
            {
                return ((this.data2 & 0x80) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x80) : (this.data2 & -129);
            }
        }

        [MessagingDescription("MsgAuthenticationProviderType"), DefaultValue(false)]
        public bool AuthenticationProviderType
        {
            get
            {
                return ((this.data2 & 0x100) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x100) : (this.data2 & -257);
            }
        }

        [DefaultValue(true), MessagingDescription("MsgBody")]
        public bool Body
        {
            get
            {
                return ((this.data1 & 0x10) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 0x10) : (this.data1 & -17);
            }
        }

        [MessagingDescription("MsgConnectorType"), DefaultValue(false)]
        public bool ConnectorType
        {
            get
            {
                return ((this.data2 & 0x20) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x20) : (this.data2 & -33);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgCorrelationId")]
        public bool CorrelationId
        {
            get
            {
                return ((this.data2 & 0x40) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x40) : (this.data2 & -65);
            }
        }

        [MessagingDescription("MsgDefaultBodySize"), DefaultValue(0x400)]
        public int DefaultBodySize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.bodySize;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DefaultSizeError"));
                }
                this.bodySize = value;
            }
        }

        [DefaultValue(0xff), MessagingDescription("MsgDefaultExtensionSize")]
        public int DefaultExtensionSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.extensionSize;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DefaultSizeError"));
                }
                this.extensionSize = value;
            }
        }

        [MessagingDescription("MsgDefaultLabelSize"), DefaultValue(0xff)]
        public int DefaultLabelSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.labelSize;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DefaultSizeError"));
                }
                this.labelSize = value;
            }
        }

        [MessagingDescription("MsgDestinationQueue"), DefaultValue(false)]
        public bool DestinationQueue
        {
            get
            {
                return ((this.data2 & 0x8000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x8000) : (this.data2 & -32769);
            }
        }

        [MessagingDescription("MsgDestinationSymmetricKey"), DefaultValue(false)]
        public bool DestinationSymmetricKey
        {
            get
            {
                return ((this.data2 & 0x400000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x400000) : (this.data2 & -4194305);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgDigitalSignature")]
        public bool DigitalSignature
        {
            get
            {
                return ((this.data2 & 0x400) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x400) : (this.data2 & -1025);
            }
        }

        [MessagingDescription("MsgEncryptionAlgorithm"), DefaultValue(false)]
        public bool EncryptionAlgorithm
        {
            get
            {
                return ((this.data2 & 0x800) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x800) : (this.data2 & -2049);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgExtension")]
        public bool Extension
        {
            get
            {
                return ((this.data2 & 0x1000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x1000) : (this.data2 & -4097);
            }
        }

        [MessagingDescription("MsgHashAlgorithm"), DefaultValue(false)]
        public bool HashAlgorithm
        {
            get
            {
                return ((this.data2 & 0x4000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x4000) : (this.data2 & -16385);
            }
        }

        [DefaultValue(true), MessagingDescription("MsgId")]
        public bool Id
        {
            get
            {
                return ((this.data1 & 0x40) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 0x40) : (this.data1 & -65);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgIsFirstInTransaction")]
        public bool IsFirstInTransaction
        {
            get
            {
                return ((this.data2 & 0x20000000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x20000000) : (this.data2 & -536870913);
            }
        }

        [MessagingDescription("MsgIsLastInTransaction"), DefaultValue(false)]
        public bool IsLastInTransaction
        {
            get
            {
                return ((this.data2 & 0x40000000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x40000000) : (this.data2 & -1073741825);
            }
        }

        [DefaultValue(true), MessagingDescription("MsgLabel")]
        public bool Label
        {
            get
            {
                return ((this.data1 & 0x20) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 0x20) : (this.data1 & -33);
            }
        }

        [MessagingDescription("MsgLookupId"), DefaultValue(false)]
        public bool LookupId
        {
            get
            {
                if (!MessageQueue.Msmq3OrNewer)
                {
                    throw new PlatformNotSupportedException(Res.GetString("PlatformNotSupported"));
                }
                return ((this.data1 & 0x800) != 0);
            }
            set
            {
                if (!MessageQueue.Msmq3OrNewer)
                {
                    throw new PlatformNotSupportedException(Res.GetString("PlatformNotSupported"));
                }
                this.data1 = value ? (this.data1 | 0x800) : (this.data1 & -2049);
            }
        }

        [DefaultValue(true), MessagingDescription("MsgMessageType")]
        public bool MessageType
        {
            get
            {
                return ((this.data1 & 0x200) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 0x200) : (this.data1 & -513);
            }
        }

        [MessagingDescription("MsgPriority"), DefaultValue(false)]
        public bool Priority
        {
            get
            {
                return ((this.data2 & 0x10000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x10000) : (this.data2 & -65537);
            }
        }

        [MessagingDescription("MsgRecoverable"), DefaultValue(false)]
        public bool Recoverable
        {
            get
            {
                return ((this.data2 & 0x200) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x200) : (this.data2 & -513);
            }
        }

        [MessagingDescription("MsgResponseQueue"), DefaultValue(true)]
        public bool ResponseQueue
        {
            get
            {
                return ((this.data1 & 0x100) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 0x100) : (this.data1 & -257);
            }
        }

        internal bool SecurityContext
        {
            get
            {
                return ((this.data2 & 0x20000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x20000) : (this.data2 & -131073);
            }
        }

        [MessagingDescription("MsgSenderCertificate"), DefaultValue(false)]
        public bool SenderCertificate
        {
            get
            {
                return ((this.data2 & 0x40000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x40000) : (this.data2 & -262145);
            }
        }

        [MessagingDescription("MsgSenderId"), DefaultValue(false)]
        public bool SenderId
        {
            get
            {
                return ((this.data2 & 0x80000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x80000) : (this.data2 & -524289);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgSenderVersion")]
        public bool SenderVersion
        {
            get
            {
                return ((this.data2 & 0x10000000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x10000000) : (this.data2 & -268435457);
            }
        }

        [MessagingDescription("MsgSentTime"), DefaultValue(false)]
        public bool SentTime
        {
            get
            {
                return ((this.data2 & 0x100000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x100000) : (this.data2 & -1048577);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgSourceMachine")]
        public bool SourceMachine
        {
            get
            {
                return ((this.data2 & 0x200000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x200000) : (this.data2 & -2097153);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgTimeToBeReceived")]
        public bool TimeToBeReceived
        {
            get
            {
                return ((this.data2 & 0x800000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x800000) : (this.data2 & -8388609);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgTimeToReachQueue")]
        public bool TimeToReachQueue
        {
            get
            {
                return ((this.data2 & 0x1000000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x1000000) : (this.data2 & -16777217);
            }
        }

        [MessagingDescription("MsgTransactionId"), DefaultValue(false)]
        public bool TransactionId
        {
            get
            {
                return ((this.data2 & -2147483648) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | -2147483648) : (this.data2 & 0x7fffffff);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgTransactionStatusQueue")]
        public bool TransactionStatusQueue
        {
            get
            {
                return ((this.data2 & 0x2000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x2000) : (this.data2 & -8193);
            }
        }

        [MessagingDescription("MsgUseAuthentication"), DefaultValue(false)]
        public bool UseAuthentication
        {
            get
            {
                return ((this.data2 & 0x2000000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x2000000) : (this.data2 & -33554433);
            }
        }

        [MessagingDescription("MsgUseDeadLetterQueue"), DefaultValue(true)]
        public bool UseDeadLetterQueue
        {
            get
            {
                return ((this.data1 & 0x80) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 0x80) : (this.data1 & -129);
            }
        }

        [DefaultValue(false), MessagingDescription("MsgUseEncryption")]
        public bool UseEncryption
        {
            get
            {
                return ((this.data2 & 0x4000000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x4000000) : (this.data2 & -67108865);
            }
        }

        [MessagingDescription("MsgUseJournalQueue"), DefaultValue(true)]
        public bool UseJournalQueue
        {
            get
            {
                return ((this.data1 & 0x400) != 0);
            }
            set
            {
                this.data1 = value ? (this.data1 | 0x400) : (this.data2 & -1025);
            }
        }

        [MessagingDescription("MsgUseTracing"), DefaultValue(false)]
        public bool UseTracing
        {
            get
            {
                return ((this.data2 & 0x8000000) != 0);
            }
            set
            {
                this.data2 = value ? (this.data2 | 0x8000000) : (this.data2 & -134217729);
            }
        }
    }
}

