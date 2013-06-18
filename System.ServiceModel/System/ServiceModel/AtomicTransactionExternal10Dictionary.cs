namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class AtomicTransactionExternal10Dictionary
    {
        public XmlDictionaryString AbortedAction;
        public XmlDictionaryString CommitAction;
        public XmlDictionaryString CommittedAction;
        public XmlDictionaryString CompletionUri;
        public XmlDictionaryString Durable2PCUri;
        public XmlDictionaryString FaultAction;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString PrepareAction;
        public XmlDictionaryString PreparedAction;
        public XmlDictionaryString ReadOnlyAction;
        public XmlDictionaryString ReplayAction;
        public XmlDictionaryString RollbackAction;
        public XmlDictionaryString Volatile2PCUri;

        public AtomicTransactionExternal10Dictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat", 0x17e);
            this.CompletionUri = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Completion", 0x180);
            this.Durable2PCUri = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Durable2PC", 0x181);
            this.Volatile2PCUri = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Volatile2PC", 0x182);
            this.CommitAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Commit", 0x18b);
            this.RollbackAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Rollback", 0x18c);
            this.CommittedAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Committed", 0x18d);
            this.AbortedAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Aborted", 0x18e);
            this.PrepareAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepare", 0x18f);
            this.PreparedAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepared", 400);
            this.ReadOnlyAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/ReadOnly", 0x191);
            this.ReplayAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/Replay", 0x192);
            this.FaultAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wsat/fault", 0x193);
        }
    }
}

