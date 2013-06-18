namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class AtomicTransactionExternal11Dictionary
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
        public XmlDictionaryString UnknownTransaction;
        public XmlDictionaryString Volatile2PCUri;

        public AtomicTransactionExternal11Dictionary(XmlDictionary dictionary)
        {
            this.Namespace = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06");
            this.CompletionUri = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Completion");
            this.Durable2PCUri = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Durable2PC");
            this.Volatile2PCUri = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Volatile2PC");
            this.CommitAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Commit");
            this.RollbackAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Rollback");
            this.CommittedAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Committed");
            this.AbortedAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Aborted");
            this.PrepareAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Prepare");
            this.PreparedAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Prepared");
            this.ReadOnlyAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/ReadOnly");
            this.ReplayAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/Replay");
            this.FaultAction = dictionary.Add("http://docs.oasis-open.org/ws-tx/wsat/2006/06/fault");
            this.UnknownTransaction = dictionary.Add("UnknownTransaction");
        }
    }
}

