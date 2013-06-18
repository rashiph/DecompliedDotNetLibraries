namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class WsrmFeb2005Dictionary
    {
        public XmlDictionaryString Accept;
        public XmlDictionaryString AcknowledgementRange;
        public XmlDictionaryString AckRequested;
        public XmlDictionaryString AckRequestedAction;
        public XmlDictionaryString AcksTo;
        public XmlDictionaryString BufferRemaining;
        public XmlDictionaryString ConnectionLimitReached;
        public XmlDictionaryString CreateSequence;
        public XmlDictionaryString CreateSequenceAction;
        public XmlDictionaryString CreateSequenceRefused;
        public XmlDictionaryString CreateSequenceResponse;
        public XmlDictionaryString CreateSequenceResponseAction;
        public XmlDictionaryString Expires;
        public XmlDictionaryString FaultCode;
        public XmlDictionaryString Identifier;
        public XmlDictionaryString InvalidAcknowledgement;
        public XmlDictionaryString LastMessage;
        public XmlDictionaryString LastMessageAction;
        public XmlDictionaryString LastMessageNumberExceeded;
        public XmlDictionaryString Lower;
        public XmlDictionaryString MessageNumber;
        public XmlDictionaryString MessageNumberRollover;
        public XmlDictionaryString Nack;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString NETNamespace;
        public XmlDictionaryString NETPrefix;
        public XmlDictionaryString Offer;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Sequence;
        public XmlDictionaryString SequenceAcknowledgement;
        public XmlDictionaryString SequenceAcknowledgementAction;
        public XmlDictionaryString SequenceFault;
        public XmlDictionaryString SequenceTerminated;
        public XmlDictionaryString TerminateSequence;
        public XmlDictionaryString TerminateSequenceAction;
        public XmlDictionaryString UnknownSequence;
        public XmlDictionaryString Upper;

        public WsrmFeb2005Dictionary(ServiceModelDictionary dictionary)
        {
            this.Identifier = dictionary.CreateString("Identifier", 15);
            this.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/rm", 0x10);
            this.SequenceAcknowledgement = dictionary.CreateString("SequenceAcknowledgement", 0x17);
            this.AcknowledgementRange = dictionary.CreateString("AcknowledgementRange", 0x18);
            this.Upper = dictionary.CreateString("Upper", 0x19);
            this.Lower = dictionary.CreateString("Lower", 0x1a);
            this.BufferRemaining = dictionary.CreateString("BufferRemaining", 0x1b);
            this.NETNamespace = dictionary.CreateString("http://schemas.microsoft.com/ws/2006/05/rm", 0x1c);
            this.SequenceAcknowledgementAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/rm/SequenceAcknowledgement", 0x1d);
            this.Sequence = dictionary.CreateString("Sequence", 0x1f);
            this.MessageNumber = dictionary.CreateString("MessageNumber", 0x20);
            this.AckRequested = dictionary.CreateString("AckRequested", 0x148);
            this.AckRequestedAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/rm/AckRequested", 0x149);
            this.AcksTo = dictionary.CreateString("AcksTo", 330);
            this.Accept = dictionary.CreateString("Accept", 0x14b);
            this.CreateSequence = dictionary.CreateString("CreateSequence", 0x14c);
            this.CreateSequenceAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequence", 0x14d);
            this.CreateSequenceRefused = dictionary.CreateString("CreateSequenceRefused", 0x14e);
            this.CreateSequenceResponse = dictionary.CreateString("CreateSequenceResponse", 0x14f);
            this.CreateSequenceResponseAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequenceResponse", 0x150);
            this.Expires = dictionary.CreateString("Expires", 0x37);
            this.FaultCode = dictionary.CreateString("FaultCode", 0x151);
            this.InvalidAcknowledgement = dictionary.CreateString("InvalidAcknowledgement", 0x152);
            this.LastMessage = dictionary.CreateString("LastMessage", 0x153);
            this.LastMessageAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage", 340);
            this.LastMessageNumberExceeded = dictionary.CreateString("LastMessageNumberExceeded", 0x155);
            this.MessageNumberRollover = dictionary.CreateString("MessageNumberRollover", 0x156);
            this.Nack = dictionary.CreateString("Nack", 0x157);
            this.NETPrefix = dictionary.CreateString("netrm", 0x158);
            this.Offer = dictionary.CreateString("Offer", 0x159);
            this.Prefix = dictionary.CreateString("r", 0x15a);
            this.SequenceFault = dictionary.CreateString("SequenceFault", 0x15b);
            this.SequenceTerminated = dictionary.CreateString("SequenceTerminated", 0x15c);
            this.TerminateSequence = dictionary.CreateString("TerminateSequence", 0x15d);
            this.TerminateSequenceAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/rm/TerminateSequence", 350);
            this.UnknownSequence = dictionary.CreateString("UnknownSequence", 0x15f);
            this.ConnectionLimitReached = dictionary.CreateString("ConnectionLimitReached", 480);
        }
    }
}

