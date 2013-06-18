namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class Wsrm11Dictionary
    {
        public XmlDictionaryString AckRequestedAction;
        public XmlDictionaryString CloseSequence;
        public XmlDictionaryString CloseSequenceAction;
        public XmlDictionaryString CloseSequenceResponse;
        public XmlDictionaryString CloseSequenceResponseAction;
        public XmlDictionaryString CreateSequenceAction;
        public XmlDictionaryString CreateSequenceResponseAction;
        public XmlDictionaryString DiscardFollowingFirstGap;
        public XmlDictionaryString Endpoint;
        public XmlDictionaryString FaultAction;
        public XmlDictionaryString Final;
        public XmlDictionaryString IncompleteSequenceBehavior;
        public XmlDictionaryString LastMsgNumber;
        public XmlDictionaryString MaxMessageNumber;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString NoDiscard;
        public XmlDictionaryString None;
        public XmlDictionaryString SequenceAcknowledgementAction;
        public XmlDictionaryString SequenceClosed;
        public XmlDictionaryString TerminateSequenceAction;
        public XmlDictionaryString TerminateSequenceResponse;
        public XmlDictionaryString TerminateSequenceResponseAction;
        public XmlDictionaryString UsesSequenceSSL;
        public XmlDictionaryString UsesSequenceSTR;
        public XmlDictionaryString WsrmRequired;

        public Wsrm11Dictionary(XmlDictionary dictionary)
        {
            this.AckRequestedAction = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702/AckRequested");
            this.CloseSequence = dictionary.Add("CloseSequence");
            this.CloseSequenceAction = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequence");
            this.CloseSequenceResponse = dictionary.Add("CloseSequenceResponse");
            this.CloseSequenceResponseAction = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702/CloseSequenceResponse");
            this.CreateSequenceAction = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702/CreateSequence");
            this.CreateSequenceResponseAction = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702/CreateSequenceResponse");
            this.DiscardFollowingFirstGap = dictionary.Add("DiscardFollowingFirstGap");
            this.Endpoint = dictionary.Add("Endpoint");
            this.FaultAction = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702/fault");
            this.Final = dictionary.Add("Final");
            this.IncompleteSequenceBehavior = dictionary.Add("IncompleteSequenceBehavior");
            this.LastMsgNumber = dictionary.Add("LastMsgNumber");
            this.MaxMessageNumber = dictionary.Add("MaxMessageNumber");
            this.Namespace = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702");
            this.NoDiscard = dictionary.Add("NoDiscard");
            this.None = dictionary.Add("None");
            this.SequenceAcknowledgementAction = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702/SequenceAcknowledgement");
            this.SequenceClosed = dictionary.Add("SequenceClosed");
            this.TerminateSequenceAction = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequence");
            this.TerminateSequenceResponse = dictionary.Add("TerminateSequenceResponse");
            this.TerminateSequenceResponseAction = dictionary.Add("http://docs.oasis-open.org/ws-rx/wsrm/200702/TerminateSequenceResponse");
            this.UsesSequenceSSL = dictionary.Add("UsesSequenceSSL");
            this.UsesSequenceSTR = dictionary.Add("UsesSequenceSTR");
            this.WsrmRequired = dictionary.Add("WsrmRequired");
        }
    }
}

