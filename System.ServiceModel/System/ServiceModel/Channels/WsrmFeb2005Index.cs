namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class WsrmFeb2005Index : WsrmIndex
    {
        private ActionHeader ackRequestedActionHeader;
        private AddressingVersion addressingVersion;
        private ActionHeader createSequenceActionHeader;
        private ActionHeader sequenceAcknowledgementActionHeader;
        private static MessagePartSpecification signedReliabilityMessageParts;
        private ActionHeader terminateSequenceActionHeader;

        internal WsrmFeb2005Index(AddressingVersion addressingVersion)
        {
            this.addressingVersion = addressingVersion;
        }

        protected override ActionHeader GetActionHeader(string element)
        {
            WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
            if (element == "AckRequested")
            {
                if (this.ackRequestedActionHeader == null)
                {
                    this.ackRequestedActionHeader = ActionHeader.Create(dictionary.AckRequestedAction, this.addressingVersion);
                }
                return this.ackRequestedActionHeader;
            }
            if (element == "CreateSequence")
            {
                if (this.createSequenceActionHeader == null)
                {
                    this.createSequenceActionHeader = ActionHeader.Create(dictionary.CreateSequenceAction, this.addressingVersion);
                }
                return this.createSequenceActionHeader;
            }
            if (element == "SequenceAcknowledgement")
            {
                if (this.sequenceAcknowledgementActionHeader == null)
                {
                    this.sequenceAcknowledgementActionHeader = ActionHeader.Create(dictionary.SequenceAcknowledgementAction, this.addressingVersion);
                }
                return this.sequenceAcknowledgementActionHeader;
            }
            if (!(element == "TerminateSequence"))
            {
                throw Fx.AssertAndThrow("Element not supported.");
            }
            if (this.terminateSequenceActionHeader == null)
            {
                this.terminateSequenceActionHeader = ActionHeader.Create(dictionary.TerminateSequenceAction, this.addressingVersion);
            }
            return this.terminateSequenceActionHeader;
        }

        internal static MessagePartSpecification SignedReliabilityMessageParts
        {
            get
            {
                if (signedReliabilityMessageParts == null)
                {
                    XmlQualifiedName[] headerTypes = new XmlQualifiedName[] { new XmlQualifiedName("Sequence", "http://schemas.xmlsoap.org/ws/2005/02/rm"), new XmlQualifiedName("SequenceAcknowledgement", "http://schemas.xmlsoap.org/ws/2005/02/rm"), new XmlQualifiedName("AckRequested", "http://schemas.xmlsoap.org/ws/2005/02/rm") };
                    MessagePartSpecification specification = new MessagePartSpecification(headerTypes);
                    specification.MakeReadOnly();
                    signedReliabilityMessageParts = specification;
                }
                return signedReliabilityMessageParts;
            }
        }
    }
}

