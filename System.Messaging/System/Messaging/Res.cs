namespace System.Messaging
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class Res
    {
        internal const string ActiveXFormatter = "ActiveXFormatter";
        internal const string AmbiguousLabel = "AmbiguousLabel";
        internal const string ArrivedTimeNotSet = "ArrivedTimeNotSet";
        internal const string AsyncResultInvalid = "AsyncResultInvalid";
        internal const string AuthenticationNotSet = "AuthenticationNotSet";
        internal const string C00E0002 = "C00E0002";
        internal const string C00E0003 = "C00E0003";
        internal const string C00E0005 = "C00E0005";
        internal const string C00E0006 = "C00E0006";
        internal const string C00E0007 = "C00E0007";
        internal const string C00E0008 = "C00E0008";
        internal const string C00E0009 = "C00E0009";
        internal const string C00E000B = "C00E000B";
        internal const string C00E000D = "C00E000D";
        internal const string C00E0010 = "C00E0010";
        internal const string C00E0011 = "C00E0011";
        internal const string C00E0013 = "C00E0013";
        internal const string C00E0014 = "C00E0014";
        internal const string C00E0018 = "C00E0018";
        internal const string C00E0019 = "C00E0019";
        internal const string C00E001A = "C00E001A";
        internal const string C00E001B = "C00E001B";
        internal const string C00E001C = "C00E001C";
        internal const string C00E001D = "C00E001D";
        internal const string C00E001E = "C00E001E";
        internal const string C00E001F = "C00E001F";
        internal const string C00E0020 = "C00E0020";
        internal const string C00E0021 = "C00E0021";
        internal const string C00E0022 = "C00E0022";
        internal const string C00E0023 = "C00E0023";
        internal const string C00E0024 = "C00E0024";
        internal const string C00E0025 = "C00E0025";
        internal const string C00E0026 = "C00E0026";
        internal const string C00E0027 = "C00E0027";
        internal const string C00E0028 = "C00E0028";
        internal const string C00E002A = "C00E002A";
        internal const string C00E002B = "C00E002B";
        internal const string C00E002C = "C00E002C";
        internal const string C00E002D = "C00E002D";
        internal const string C00E002F = "C00E002F";
        internal const string C00E0030 = "C00E0030";
        internal const string C00E0033 = "C00E0033";
        internal const string C00E0035 = "C00E0035";
        internal const string C00E0036 = "C00E0036";
        internal const string C00E0037 = "C00E0037";
        internal const string C00E0038 = "C00E0038";
        internal const string C00E0039 = "C00E0039";
        internal const string C00E003A = "C00E003A";
        internal const string C00E003B = "C00E003B";
        internal const string C00E003C = "C00E003C";
        internal const string C00E003D = "C00E003D";
        internal const string C00E003E = "C00E003E";
        internal const string C00E003F = "C00E003F";
        internal const string C00E0040 = "C00E0040";
        internal const string C00E0041 = "C00E0041";
        internal const string C00E0042 = "C00E0042";
        internal const string C00E0043 = "C00E0043";
        internal const string C00E0044 = "C00E0044";
        internal const string C00E0045 = "C00E0045";
        internal const string C00E0046 = "C00E0046";
        internal const string C00E0048 = "C00E0048";
        internal const string C00E0049 = "C00E0049";
        internal const string C00E004A = "C00E004A";
        internal const string C00E004B = "C00E004B";
        internal const string C00E004C = "C00E004C";
        internal const string C00E004E = "C00E004E";
        internal const string C00E0050 = "C00E0050";
        internal const string C00E0051 = "C00E0051";
        internal const string C00E0055 = "C00E0055";
        internal const string C00E0056 = "C00E0056";
        internal const string C00E0058 = "C00E0058";
        internal const string C00E005A = "C00E005A";
        internal const string C00E005B = "C00E005B";
        internal const string C00E005C = "C00E005C";
        internal const string C00E005D = "C00E005D";
        internal const string C00E005E = "C00E005E";
        internal const string C00E005F = "C00E005F";
        internal const string C00E0060 = "C00E0060";
        internal const string C00E0061 = "C00E0061";
        internal const string C00E0062 = "C00E0062";
        internal const string C00E0063 = "C00E0063";
        internal const string C00E0064 = "C00E0064";
        internal const string C00E0065 = "C00E0065";
        internal const string C00E0066 = "C00E0066";
        internal const string C00E0067 = "C00E0067";
        internal const string C00E0068 = "C00E0068";
        internal const string C00E0069 = "C00E0069";
        internal const string C00E006A = "C00E006A";
        internal const string C00E006B = "C00E006B";
        internal const string C00E006C = "C00E006C";
        internal const string C00E006D = "C00E006D";
        internal const string C00E006E = "C00E006E";
        internal const string C00E006F = "C00E006F";
        internal const string C00E0070 = "C00E0070";
        internal const string C00E0071 = "C00E0071";
        internal const string C00E0072 = "C00E0072";
        internal const string C00E0073 = "C00E0073";
        internal const string C00E0074 = "C00E0074";
        internal const string C00E0075 = "C00E0075";
        internal const string C00E0076 = "C00E0076";
        internal const string C00E0077 = "C00E0077";
        internal const string C00E0078 = "C00E0078";
        internal const string C00E0079 = "C00E0079";
        internal const string C00E007A = "C00E007A";
        internal const string C00E007B = "C00E007B";
        internal const string C00E007C = "C00E007C";
        internal const string C00E007D = "C00E007D";
        internal const string C00E007E = "C00E007E";
        internal const string C00E007F = "C00E007F";
        internal const string C00E0080 = "C00E0080";
        internal const string C00E0081 = "C00E0081";
        internal const string C00E0082 = "C00E0082";
        internal const string CancelCaption = "CancelCaption";
        internal const string ClearingQueue = "ClearingQueue";
        internal const string CoertionType = "CoertionType";
        internal const string CouldntResolve = "CouldntResolve";
        internal const string CouldntResolveName = "CouldntResolveName";
        internal const string CreatingQueue = "CreatingQueue";
        internal const string CriteriaNotDefined = "CriteriaNotDefined";
        internal const string DefaultSizeError = "DefaultSizeError";
        internal const string DeletingQueue = "DeletingQueue";
        internal const string DestinationQueueNotSet = "DestinationQueueNotSet";
        internal const string FormatterMissing = "FormatterMissing";
        internal const string IdNotSet = "IdNotSet";
        internal const string IncompleteMQ = "IncompleteMQ";
        internal const string IncorrectNumberOfBytes = "IncorrectNumberOfBytes";
        internal const string InfiniteValue = "InfiniteValue";
        internal const string InvalidDateValue = "InvalidDateValue";
        internal const string InvalidId = "InvalidId";
        internal const string InvalidLabel = "InvalidLabel";
        internal const string InvalidMaxJournalSize = "InvalidMaxJournalSize";
        internal const string InvalidMaxQueueSize = "InvalidMaxQueueSize";
        internal const string InvalidParameter = "InvalidParameter";
        internal const string InvalidProperty = "InvalidProperty";
        internal const string InvalidQueuePathToCreate = "InvalidQueuePathToCreate";
        internal const string InvalidTrustee = "InvalidTrustee";
        internal const string InvalidTrusteeName = "InvalidTrusteeName";
        internal const string InvalidTypeDeserialization = "InvalidTypeDeserialization";
        internal const string InvalidTypeSerialization = "InvalidTypeSerialization";
        internal const string InvalidXmlFormat = "InvalidXmlFormat";
        private static Res loader;
        internal const string LongQueueName = "LongQueueName";
        internal const string LookupIdNotSet = "LookupIdNotSet";
        internal const string MessageNotFound = "MessageNotFound";
        internal const string MessageQueueBrowser = "MessageQueueBrowser";
        internal const string MessageQueueDesc = "MessageQueueDesc";
        internal const string MessageTypeNotSet = "MessageTypeNotSet";
        internal const string MissingProperty = "MissingProperty";
        internal const string MQ_Authenticate = "MQ_Authenticate";
        internal const string MQ_BasePriority = "MQ_BasePriority";
        internal const string MQ_CanRead = "MQ_CanRead";
        internal const string MQ_CanWrite = "MQ_CanWrite";
        internal const string MQ_Category = "MQ_Category";
        internal const string MQ_CreateTime = "MQ_CreateTime";
        internal const string MQ_DefaultPropertiesToSend = "MQ_DefaultPropertiesToSend";
        internal const string MQ_DenySharedReceive = "MQ_DenySharedReceive";
        internal const string MQ_EncryptionRequired = "MQ_EncryptionRequired";
        internal const string MQ_FormatName = "MQ_FormatName";
        internal const string MQ_Formatter = "MQ_Formatter";
        internal const string MQ_GuidId = "MQ_GuidId";
        internal const string MQ_Label = "MQ_Label";
        internal const string MQ_LastModifyTime = "MQ_LastModifyTime";
        internal const string MQ_MachineName = "MQ_MachineName";
        internal const string MQ_MaximumJournalSize = "MQ_MaximumJournalSize";
        internal const string MQ_MaximumQueueSize = "MQ_MaximumQueueSize";
        internal const string MQ_MessageReadPropertyFilter = "MQ_MessageReadPropertyFilter";
        internal const string MQ_MulticastAddress = "MQ_MulticastAddress";
        internal const string MQ_Path = "MQ_Path";
        internal const string MQ_PeekCompleted = "MQ_PeekCompleted";
        internal const string MQ_QueueName = "MQ_QueueName";
        internal const string MQ_ReadHandle = "MQ_ReadHandle";
        internal const string MQ_ReceiveCompleted = "MQ_ReceiveCompleted";
        internal const string MQ_SynchronizingObject = "MQ_SynchronizingObject";
        internal const string MQ_Transactional = "MQ_Transactional";
        internal const string MQ_UseJournalQueue = "MQ_UseJournalQueue";
        internal const string MQ_WriteHandle = "MQ_WriteHandle";
        internal const string MsgAcknowledgement = "MsgAcknowledgement";
        internal const string MsgAcknowledgeType = "MsgAcknowledgeType";
        internal const string MsgAdministrationQueue = "MsgAdministrationQueue";
        internal const string MsgAppSpecific = "MsgAppSpecific";
        internal const string MsgArrivedTime = "MsgArrivedTime";
        internal const string MsgAttachSenderId = "MsgAttachSenderId";
        internal const string MsgAuthenticated = "MsgAuthenticated";
        internal const string MsgAuthenticationProviderName = "MsgAuthenticationProviderName";
        internal const string MsgAuthenticationProviderType = "MsgAuthenticationProviderType";
        internal const string MsgBody = "MsgBody";
        internal const string MsgBodyStream = "MsgBodyStream";
        internal const string MsgBodyType = "MsgBodyType";
        internal const string MsgConnectorType = "MsgConnectorType";
        internal const string MsgCorrelationId = "MsgCorrelationId";
        internal const string MsgDefaultBodySize = "MsgDefaultBodySize";
        internal const string MsgDefaultExtensionSize = "MsgDefaultExtensionSize";
        internal const string MsgDefaultLabelSize = "MsgDefaultLabelSize";
        internal const string MsgDestinationQueue = "MsgDestinationQueue";
        internal const string MsgDestinationSymmetricKey = "MsgDestinationSymmetricKey";
        internal const string MsgDigitalSignature = "MsgDigitalSignature";
        internal const string MsgEncryptionAlgorithm = "MsgEncryptionAlgorithm";
        internal const string MsgExtension = "MsgExtension";
        internal const string MsgHashAlgorithm = "MsgHashAlgorithm";
        internal const string MsgId = "MsgId";
        internal const string MsgIsFirstInTransaction = "MsgIsFirstInTransaction";
        internal const string MsgIsLastInTransaction = "MsgIsLastInTransaction";
        internal const string MsgLabel = "MsgLabel";
        internal const string MsgLookupId = "MsgLookupId";
        internal const string MsgMessageType = "MsgMessageType";
        internal const string MsgPriority = "MsgPriority";
        internal const string MsgRecoverable = "MsgRecoverable";
        internal const string MsgResponseQueue = "MsgResponseQueue";
        internal const string MsgSenderCertificate = "MsgSenderCertificate";
        internal const string MsgSenderId = "MsgSenderId";
        internal const string MsgSenderVersion = "MsgSenderVersion";
        internal const string MsgSentTime = "MsgSentTime";
        internal const string MsgSourceMachine = "MsgSourceMachine";
        internal const string MsgTimeToBeReceived = "MsgTimeToBeReceived";
        internal const string MsgTimeToReachQueue = "MsgTimeToReachQueue";
        internal const string MsgTopObjectFormat = "MsgTopObjectFormat";
        internal const string MsgTransactionId = "MsgTransactionId";
        internal const string MsgTransactionStatusQueue = "MsgTransactionStatusQueue";
        internal const string MsgTypeFormat = "MsgTypeFormat";
        internal const string MsgUseAuthentication = "MsgUseAuthentication";
        internal const string MsgUseDeadLetterQueue = "MsgUseDeadLetterQueue";
        internal const string MsgUseEncryption = "MsgUseEncryption";
        internal const string MsgUseJournalQueue = "MsgUseJournalQueue";
        internal const string MsgUseTracing = "MsgUseTracing";
        internal const string MSMQInfoNotSupported = "MSMQInfoNotSupported";
        internal const string MSMQNotInstalled = "MSMQNotInstalled";
        internal const string NoCurrentMessage = "NoCurrentMessage";
        internal const string NoCurrentMessageQueue = "NoCurrentMessageQueue";
        internal const string NotAcknowledgement = "NotAcknowledgement";
        internal const string NotAMessageQueue = "NotAMessageQueue";
        internal const string NotImplemented = "NotImplemented";
        internal const string NotSet = "NotSet";
        internal const string PathNameDns = "PathNameDns";
        internal const string PathNotSet = "PathNotSet";
        internal const string PathSyntax = "PathSyntax";
        internal const string PermissionAllNull = "PermissionAllNull";
        internal const string PermissionPathOrCriteria = "PermissionPathOrCriteria";
        internal const string PlatformNotSupported = "PlatformNotSupported";
        internal const string PleaseWait = "PleaseWait";
        internal const string PropertyAddFailed = "PropertyAddFailed";
        internal const string PropertyOverflow = "PropertyOverflow";
        internal const string QueueExistsError = "QueueExistsError";
        internal const string QueueHelp = "QueueHelp";
        internal const string QueueNetworkProblems = "QueueNetworkProblems";
        internal const string QueueOk = "QueueOk";
        internal const string RefByFormatName = "RefByFormatName";
        internal const string RefByLabel = "RefByLabel";
        internal const string RefByPath = "RefByPath";
        internal const string ReferenceLabel = "ReferenceLabel";
        internal const string RemovingQueue = "RemovingQueue";
        private ResourceManager resources;
        internal const string RestoringQueue = "RestoringQueue";
        internal const string SelectLabel = "SelectLabel";
        internal const string SenderIdNotAttached = "SenderIdNotAttached";
        internal const string SenderIdNotSet = "SenderIdNotSet";
        internal const string SentTimeNotSet = "SentTimeNotSet";
        internal const string SourceMachineNotSet = "SourceMachineNotSet";
        internal const string StoredObjectsNotSupported = "StoredObjectsNotSupported";
        internal const string TooManyColumns = "TooManyColumns";
        internal const string toStringNone = "toStringNone";
        internal const string TransactionNotStarted = "TransactionNotStarted";
        internal const string TransactionStarted = "TransactionStarted";
        internal const string TypeListMissing = "TypeListMissing";
        internal const string TypeNotSupported = "TypeNotSupported";
        internal const string UnknownError = "UnknownError";
        internal const string VersionNotSet = "VersionNotSet";
        internal const string WinNTRequired = "WinNTRequired";
        internal const string XmlFormatter = "XmlFormatter";
        internal const string XmlMsgTargetTypeNames = "XmlMsgTargetTypeNames";
        internal const string XmlMsgTargetTypes = "XmlMsgTargetTypes";

        internal Res()
        {
            this.resources = new ResourceManager("System.Messaging", base.GetType().Assembly);
        }

        private static Res GetLoader()
        {
            if (loader == null)
            {
                Res res = new Res();
                Interlocked.CompareExchange<Res>(ref loader, res, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

