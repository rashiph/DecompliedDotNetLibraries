namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class AtomicTransactionExternalDictionary
    {
        public XmlDictionaryString Aborted;
        public XmlDictionaryString Commit;
        public XmlDictionaryString Committed;
        public XmlDictionaryString CompletionCoordinatorPortType;
        public XmlDictionaryString CompletionParticipantPortType;
        public XmlDictionaryString CoordinatorPortType;
        public XmlDictionaryString InconsistentInternalState;
        public XmlDictionaryString ParticipantPortType;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Prepare;
        public XmlDictionaryString Prepared;
        public XmlDictionaryString ReadOnly;
        public XmlDictionaryString Replay;
        public XmlDictionaryString Rollback;

        public AtomicTransactionExternalDictionary(ServiceModelDictionary dictionary)
        {
            this.Prefix = dictionary.CreateString("wsat", 0x17f);
            this.Prepare = dictionary.CreateString("Prepare", 0x183);
            this.Prepared = dictionary.CreateString("Prepared", 0x184);
            this.ReadOnly = dictionary.CreateString("ReadOnly", 0x185);
            this.Commit = dictionary.CreateString("Commit", 390);
            this.Rollback = dictionary.CreateString("Rollback", 0x187);
            this.Committed = dictionary.CreateString("Committed", 0x188);
            this.Aborted = dictionary.CreateString("Aborted", 0x189);
            this.Replay = dictionary.CreateString("Replay", 0x18a);
            this.CompletionCoordinatorPortType = dictionary.CreateString("CompletionCoordinatorPortType", 0x194);
            this.CompletionParticipantPortType = dictionary.CreateString("CompletionParticipantPortType", 0x195);
            this.CoordinatorPortType = dictionary.CreateString("CoordinatorPortType", 0x196);
            this.ParticipantPortType = dictionary.CreateString("ParticipantPortType", 0x197);
            this.InconsistentInternalState = dictionary.CreateString("InconsistentInternalState", 0x198);
        }
    }
}

