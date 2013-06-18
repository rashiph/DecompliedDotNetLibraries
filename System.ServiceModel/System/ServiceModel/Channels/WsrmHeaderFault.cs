namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class WsrmHeaderFault : WsrmFault
    {
        private bool faultsInput;
        private bool faultsOutput;
        private UniqueId sequenceID;
        private string subcode;

        protected WsrmHeaderFault(FaultCode code, string subcode, FaultReason reason, bool faultsInput, bool faultsOutput) : base(code, subcode, reason)
        {
            this.subcode = subcode;
            this.faultsInput = faultsInput;
            this.faultsOutput = faultsOutput;
        }

        protected WsrmHeaderFault(bool isSenderFault, string subcode, string faultReason, string exceptionMessage, UniqueId sequenceID, bool faultsInput, bool faultsOutput) : base(isSenderFault, subcode, faultReason, exceptionMessage)
        {
            this.subcode = subcode;
            this.sequenceID = sequenceID;
            this.faultsInput = faultsInput;
            this.faultsOutput = faultsOutput;
        }

        protected WsrmHeaderFault(FaultCode code, string subcode, FaultReason reason, XmlDictionaryReader detailReader, ReliableMessagingVersion reliableMessagingVersion, bool faultsInput, bool faultsOutput) : this(code, subcode, reason, faultsInput, faultsOutput)
        {
            this.sequenceID = ParseDetail(detailReader, reliableMessagingVersion);
        }

        private static WsrmHeaderFault CreateWsrmHeaderFault(ReliableMessagingVersion reliableMessagingVersion, FaultCode code, string subcode, FaultReason reason, XmlDictionaryReader detailReader)
        {
            if (code.IsSenderFault)
            {
                if (subcode == "InvalidAcknowledgement")
                {
                    return new InvalidAcknowledgementFault(code, reason, detailReader, reliableMessagingVersion);
                }
                if (subcode == "MessageNumberRollover")
                {
                    return new MessageNumberRolloverFault(code, reason, detailReader, reliableMessagingVersion);
                }
                if (subcode == "UnknownSequence")
                {
                    return new UnknownSequenceFault(code, reason, detailReader, reliableMessagingVersion);
                }
                if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                {
                    if (subcode == "LastMessageNumberExceeded")
                    {
                        return new LastMessageNumberExceededFault(code, reason, detailReader, reliableMessagingVersion);
                    }
                }
                else if ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && (subcode == "SequenceClosed"))
                {
                    return new SequenceClosedFault(code, reason, detailReader, reliableMessagingVersion);
                }
            }
            if (!code.IsSenderFault && !code.IsReceiverFault)
            {
                return null;
            }
            return new SequenceTerminatedFault(code, reason, detailReader, reliableMessagingVersion);
        }

        protected override FaultCode Get11Code(FaultCode code, string subcode)
        {
            return code;
        }

        protected override bool Get12HasDetail()
        {
            return true;
        }

        private static void LookupDetailInformation(ReliableMessagingVersion reliableMessagingVersion, string subcode, out string detailName, out string detailNamespace)
        {
            detailName = null;
            detailNamespace = null;
            string namespaceString = WsrmIndex.GetNamespaceString(reliableMessagingVersion);
            bool flag = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
            bool flag2 = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
            if (subcode == "InvalidAcknowledgement")
            {
                detailName = "SequenceAcknowledgement";
                detailNamespace = namespaceString;
            }
            else if (((((subcode == "MessageNumberRollover") || (subcode == "SequenceTerminated")) || (subcode == "UnknownSequence")) || (flag && (subcode == "LastMessageNumberExceeded"))) || (flag2 && (subcode == "SequenceClosed")))
            {
                detailName = "Identifier";
                detailNamespace = namespaceString;
            }
            else
            {
                detailName = null;
                detailNamespace = null;
            }
        }

        protected override void OnFaultMessageCreated(MessageVersion version, Message message)
        {
            if (version.Envelope == EnvelopeVersion.Soap11)
            {
                WsrmSequenceFaultHeader header = new WsrmSequenceFaultHeader(base.GetReliableMessagingVersion(), this);
                message.Headers.Add(header);
            }
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            WsrmUtilities.WriteIdentifier(writer, base.GetReliableMessagingVersion(), this.sequenceID);
        }

        private static UniqueId ParseDetail(XmlDictionaryReader reader, ReliableMessagingVersion reliableMessagingVersion)
        {
            UniqueId id;
            try
            {
                id = WsrmUtilities.ReadIdentifier(reader, reliableMessagingVersion);
            }
            finally
            {
                reader.Close();
            }
            return id;
        }

        public static bool TryCreateFault11(ReliableMessagingVersion reliableMessagingVersion, Message message, MessageFault fault, int index, out WsrmHeaderFault wsrmFault)
        {
            string str2;
            string str3;
            if (index == -1)
            {
                wsrmFault = null;
                return false;
            }
            if (!fault.Code.IsSenderFault && !fault.Code.IsReceiverFault)
            {
                wsrmFault = null;
                return false;
            }
            string subcode = WsrmSequenceFaultHeader.GetSubcode(message.Headers.GetReaderAtHeader(index), reliableMessagingVersion);
            if (subcode == null)
            {
                wsrmFault = null;
                return false;
            }
            LookupDetailInformation(reliableMessagingVersion, subcode, out str2, out str3);
            XmlDictionaryReader detailReader = WsrmSequenceFaultHeader.GetReaderAtDetailContents(str2, str3, message.Headers.GetReaderAtHeader(index), reliableMessagingVersion);
            if (detailReader == null)
            {
                wsrmFault = null;
                return false;
            }
            wsrmFault = CreateWsrmHeaderFault(reliableMessagingVersion, fault.Code, subcode, fault.Reason, detailReader);
            if (wsrmFault != null)
            {
                message.Headers.UnderstoodHeaders.Add(message.Headers[index]);
                return true;
            }
            return false;
        }

        public static bool TryCreateFault12(ReliableMessagingVersion reliableMessagingVersion, Message message, MessageFault fault, out WsrmHeaderFault wsrmFault)
        {
            if (!fault.Code.IsSenderFault && !fault.Code.IsReceiverFault)
            {
                wsrmFault = null;
                return false;
            }
            if (((fault.Code.SubCode == null) || (fault.Code.SubCode.Namespace != WsrmIndex.GetNamespaceString(reliableMessagingVersion))) || !fault.HasDetail)
            {
                wsrmFault = null;
                return false;
            }
            string name = fault.Code.SubCode.Name;
            XmlDictionaryReader readerAtDetailContents = fault.GetReaderAtDetailContents();
            wsrmFault = CreateWsrmHeaderFault(reliableMessagingVersion, fault.Code, name, fault.Reason, readerAtDetailContents);
            return (wsrmFault != null);
        }

        public bool FaultsInput
        {
            get
            {
                return this.faultsInput;
            }
        }

        public bool FaultsOutput
        {
            get
            {
                return this.faultsOutput;
            }
        }

        public UniqueId SequenceID
        {
            get
            {
                return this.sequenceID;
            }
            protected set
            {
                this.sequenceID = value;
            }
        }
    }
}

